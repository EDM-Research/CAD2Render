# Converts Synthetic Dataset Format V1 to bop dataset format
# Works on annotations generated for datasets between 06/2020 and 11/2020
# Requires the following input dataset structure:
# Dataset/
#   images/%d_img.jpg
#   segmentation/%d_seg.png
#   meshes/*.obj
#   annotations.txt (one json string line per sample, i.e. a dictionary containing id, proj, worldToCam and a list of models. Eeach model has a name, instance and locToWorld). Arrays are stored column first
#   colors.txt (contains a color per model name)


from bop_toolkit_lib import inout
import numpy as np
import json
import os
from pathlib import Path
import math

# PARAMETERS.
################################################################################
p = {
  # Folder containing input dataset
  'input_path': r'E:/Datasets/PILS/AtlasCopco/housing/AtlasCopco_housing_01_07_2021',
  'num_unique_objects': 3, # names of unique objects. This should be the number of meshes
  'im_size': (1024,1024),  #(2456, 2054) for symposium
  'v_fov': 60,   # use 32.80984 for high-res symposium dataset,
  
  
  # Output dataset name
  'dataset': 'BOB_format',
  
  
  # Folder containing the BOP datasets.
  'datasets_path': r'E:/Datasets/PILS/AtlasCopco/housing/AtlasCopco_housing_01_07_2021',
 

  # Dataset split. Options: 'train', 'val', 'test'.
  'dataset_split': 'train',

  # Dataset split type. None = default. See dataset_params.py for options.
  'dataset_split_type': 'PBR',
  
  # Enable to convert obj to ply with meshlabserver 2020
  'convert_obj_to_ply' : False,
  
  # Enable to copy images from source dataest to bop dataset. Disable if you want to rename them with different tool
  'copy_images' : False,
  
  # Enable to segmentation map to mask_visib (with occlusions)
  'convert_segmentation_to_mask_visib' : True,
  'segmentation_tolerance': 5,  # tolerance of assigning color to mask 
  
  
  # Enable to render non-occluded masks using bopt_toolkit_lib
  'render_masks' : False,
  'renderer_type': 'python'  # Options: 'cpp', 'python'.
  
  
  
}
################################################################################




def convertObjToPly(inputFile, outputFile):
    print("Converting \"%s\" to \"%s\"..."%(inputFile,outputFile))
    import subprocess
    list_files = subprocess.run(["meshlabserver", '-i', inputFile, '-o', outputFile, '-m', 'sa', 'vn', 'vc', 'vt', 'fn'])
    
    if list_files.returncode != 0:
        print("ERROR: failed converting OBJ to PLY: ", inputFile, " --> ", outputFile)
        
def ensure_dir(path):
    if not os.path.exists(path):
        os.makedirs(path)


def verticalToHorizontalFieldOfView(v_fov, aspect_ratio):
    #ensures that the horizontal focal length is equal to the vertical focal length
    h_fov = math.atan(math.tan(v_fov * math.pi /180.0 * 0.5) * aspect_ratio) * 2.0 * 180.0 / math.pi
    return h_fov
    



def loadUniqueModelNames(models_path):
    # search for all obj files in models path and assign unique id
    paths = Path(models_path).glob('**/*.obj')
    obj_id = 0
    modelsToID = {}
    idToModelName = {}
    for path in paths:
        modelName = path.stem
        modelsToID[modelName] = obj_id
        idToModelName[obj_id] = modelName
        obj_id += 1
    return modelsToID, idToModelName


    
mesh_path = None
if os.path.exists(p['input_path']+'/meshes'):
    mesh_path = p['input_path']+'/meshes'
if os.path.exists(p['input_path']+'/mesh'):
    mesh_path = p['input_path']+'/mesh'    
        
assert(mesh_path is not None)        
        

outputDatasetPath = p['datasets_path'] + '/' + p['dataset']
ensure_dir(outputDatasetPath)



# STEP 0 find unique models and assign object id, by iterating over all obj files
print('__________________________________________________________________________')    
print('STEP 0: FIND UNIQUE MODEL NAMES IN FIRST SAMPLE')

modelsToID, idToModelName = loadUniqueModelNames(mesh_path)
print("Found %d unique models:\n"%len(idToModelName), idToModelName)
#check if the number of loaded models is equal to the number of unique objects set in the paramters
assert(len(modelsToID) == p['num_unique_objects'])




# STEP 1 convert obj dataset to bop ply dataset format
if p['convert_obj_to_ply']:
    print('__________________________________________________________________________')
    print('STEP 1: CONVERTING OBJ TO PLY')

    ensure_dir(outputDatasetPath + '/models')
    if mesh_path is not None:
        for model, object_id in modelsToID.items():
            convertObjToPly(mesh_path + '/%s.obj'%model, outputDatasetPath+'/models/obj_%06d.ply'%object_id)
    else:
        print('ERROR: input mesh directory \"' + p['input_path']+'/meshes' + '\" does not exists')
        
else:
    print('__________________________________________________________________________')
    print('STEP 1 SKIPPED: CONVERTING OBJ TO PLY (NOT ENABLED)')
    


    
# STEP 2: build scene_camera and scene_gt
print('__________________________________________________________________________')
print('STEP 2: CONVERTING ANNOTATIONS TO SCENE_CAMERA AND SCENE_GT')


# For now, we cannot directly use projection matrix of unity, because it is in left handed coordinate system
# Manual override of projection matrix with custom opencv pinhole model
cam_K = np.zeros((3,3))
width = p['im_size'][0]
height = p['im_size'][1]
v_fov = p['v_fov']
aspect_ratio = float(width) / float(height)
h_fov = verticalToHorizontalFieldOfView(v_fov, aspect_ratio)
fy = (0.5*height) / math.tan(v_fov * math.pi /180.0 / 2.0)
fx = (0.5*width) / math.tan(h_fov * math.pi /180.0 / 2.0) # Unity ensures that fx == fy
cx = 0.5*width
cy = 0.5*height

cam_K[0,0] = fx
cam_K[1,1] = fy
cam_K[0,2] = cx
cam_K[1,2] = cy
cam_K[2,2] = 1.0


# The target dataset will only contain one scene with id 1
scene_id = 1
train_dir = outputDatasetPath + '/' + p['dataset_split'] + '_' + p['dataset_split_type'] + '/%06d/'%scene_id
ensure_dir(train_dir)


#iterate over all samples and build scene_camera and scene_gt
scene_camera = {}
scene_gt = {}
im_id = 0
with open(p['input_path'] + '/annotations.txt', 'r') as f:
    for line in f:
        data = json.loads(line)
        
        im_camera = {}
        im_camera['cam_K'] = cam_K
        im_camera['depth_scale'] = 1.0
        
        #load camera matrix
        worldToCam = data["worldToCam"]
        worldToCam = np.reshape(worldToCam, (4,4),order='F')
        
        # Just before projection we transform the coordinate in unity coordinates to opencv coordinates ( from unity to opencv requires a flip of y coordinat)
        #only left handed multiplication is required because all matrix operations are first done in unity coordinate system (left handed).
        unityToOpencvConversionMatrix = np.zeros((4,4))
        unityToOpencvConversionMatrix[0,0] = 1 
        unityToOpencvConversionMatrix[1,1] = -1 
        unityToOpencvConversionMatrix[2,2] = 1 
        unityToOpencvConversionMatrix[3,3] = 1 
        worldToCam = np.matmul(unityToOpencvConversionMatrix,worldToCam)
    

        im_camera['cam_R_w2c'] = worldToCam[0:3,0:3]
        im_camera['cam_t_w2c'] = worldToCam[0:3,3]
        
        im_gt = []
        for i in range(len(data["models"])):
            modelName = data["models"][i]["name"]
            if modelName == 'profiel':  #for know ignore profiel
                continue
                
            object_id = modelsToID[modelName]
      
            # Load localtransform of model
            
            locToWorld = data["models"][i]["locToWorld"]
            locToWorld = np.reshape(locToWorld, (4,4),order='F')

            # Unity magically flips x-asis of vertices in obj and fbx files
            flipXMatrix = np.zeros((4,4))
            flipXMatrix[0,0] = -1 
            flipXMatrix[1,1] = 1 
            flipXMatrix[2,2] = 1 
            flipXMatrix[3,3] = 1 
            locToWorld = np.matmul(locToWorld,flipXMatrix)

            viewMat = np.matmul(worldToCam,locToWorld)
            
            
            gt = {}
            gt['obj_id'] = object_id
            gt['cam_R_m2c'] = viewMat[0:3,0:3]
            gt['cam_t_m2c'] = viewMat[0:3,3]
            im_gt.append(gt)
            
        scene_camera[im_id] = im_camera
        scene_gt[im_id] = im_gt
        
        im_id+=1

inout.save_scene_camera(train_dir+'scene_camera.json', scene_camera)
inout.save_scene_gt(train_dir+'scene_gt.json', scene_gt)


 
# STEP 3: optionally copy and convert image data
if p['copy_images']:
    print('__________________________________________________________________________')
    print('STEP 3: COPYING IMAGE DATA')
    if not os.path.exists(train_dir + 'rgb'):
        os.makedirs(train_dir + 'rgb')
    import shutil
    for im_id in scene_gt.keys():
        src_file = p['input_path'] + '/images/%d_img.jpg'%im_id
        dst_file = train_dir + 'rgb/%06d.jpg'%im_id
        if im_id % 100 == 0:
            print("\rCopying image %06d/%06d"%(im_id+1,len(scene_gt)) , end =" ")
        shutil.copy(src_file, dst_file)
else:
    print('__________________________________________________________________________')
    print('STEP 3 SKIPPED: COPYING IMAGE DATA (NOT ENABLED)')



# STEP 4: convert segmentation masks to mask_visib
if p['convert_segmentation_to_mask_visib']:
    print('__________________________________________________________________________')
    print('STEP 4: CONVERTING SEGMENTATION MAPS TO MASK_VISIB')
    mask_visib_path = train_dir + 'mask_visib'
    if not os.path.exists(mask_visib_path):
        os.makedirs(mask_visib_path)
        
        
    # load object colors. Each color is an instance of an object. It does not need to be a unique object (there is no link with object id)
    # This x'th color in this list is the x'th object in the img_gt
    model_colors = []
    with open(p['input_path'] + '/colors.txt') as f:
        for line in f:
            line_data = line.strip().split(',')
            obj_id = modelsToID[line_data[0]]
            # model_colors[obj_id] = np.array([int(line_data[1]),int(line_data[2]),int(line_data[3])], dtype=np.uint8)
            model_colors.append(np.array([int(line_data[1]),int(line_data[2]),int(line_data[3]), 255], dtype=np.uint8))
    
    tol = p['segmentation_tolerance']
    for im_id, img_gt in scene_gt.items():
        if im_id % 100 == 0:
            print("\rConverting segmentation mask %06d/%06d"%(im_id+1,len(scene_gt)) , end =" ")
        # Load segmentation image
        seg_file = p['input_path'] + '/segmentation/%d_seg.png'%im_id
        seg_img = inout.load_im(seg_file)
        
        # Iterate over all models in image and extract visible mask by thresholding on object color
        for gt_index, model_gt in enumerate(img_gt):
            # obj_id = model_gt['obj_id']
            color = model_colors[gt_index]
            mask_visib = (seg_img >= color) & (seg_img <= color)
            mask_visib = mask_visib.all(-1).astype(dtype=np.uint8)*255
            
            # import cv2
            # cv2.imshow("mask", mask_visib)
            # cv2.waitKey(0)
            
            mask_visib_filename = mask_visib_path + '/%06d_%06d.png'%(im_id, gt_index)
            inout.save_im(mask_visib_filename, mask_visib)
else:
    print('__________________________________________________________________________')
    print('STEP 4 SKIPPED: CONVERTING SEGMENTATION MAPS TO MASK_VISIB (NOT ENABLED)')



# step 5: optionally generate mask data by projecting ply's using bop_toolkit_lib
if p['render_masks']:
    print('__________________________________________________________________________')
    print('STEP 5: RENDER MODELS TO MASKS')
    from calc_gt_masks import render_bop_masks
    obj_ids = []
    for model, object_id in modelsToID.items():
        obj_ids.append(object_id)
    render_bop_masks(p['datasets_path'], p['dataset'], p['dataset_split'],p['dataset_split_type'], p['renderer_type'], obj_ids, [scene_id], p['im_size'], delta = 15) 
else:
    print('__________________________________________________________________________')
    print('STEP 5 SKIPPED: RENDER MODELS TO MASKS (NOT ENABLED)')
