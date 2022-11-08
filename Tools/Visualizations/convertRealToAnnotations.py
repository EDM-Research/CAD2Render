# Copyright (c) 2021 Nick Michiels <nick.michiels@uhasselt.be>, Hasselt University, Belgium, All rights reserved.


import cv2
import numpy as np
import json
import os


    
def main():
    #---------------------------------------------------------------
    # export paramters
    #---------------------------------------------------------------
    # undistort = False
    export_segmentation_masks = True
    export_images = True
    visualize_overlays = False # verbose debug window to show overlaying aligned vertices
    export_mesh = False  # this works but only for obj file without mtl file
    #---------------------------------------------------------------
    # dataset information
    #---------------------------------------------------------------
    path_to_input_dataset = 'example/pickit/real/'  # path to input data with real dataset (image1.jpg, image1_metadata.txt), (image2.jpg, image2_metadata.txt), ...
    path_to_output_dataset = 'example/pickit/test/' # path to output dataset with annotations.txt format
    mesh_path = path_to_input_dataset + "mesh/plaatje.obj" # required for segmentation export
    
    # MAKE SURE TO CHANGE FX,FY,CX,CY, width, height when resolution or camera fov changes
    number_of_images = 13
    width = 2456
    height = 2054
    fx = 3512.1296562711223
    fy = 3502.899166718069
    cx = 1219.6121235754144
    cy = 994.4110357352513
    dist = np.array([-0.20745834024853502, 1.0839913764255735, 0.003035240488715265, -0.0018433729080270813, -7.428167473039992], dtype=np.float64)

    model_name =  os.path.splitext(os.path.basename(mesh_path))[0]
    
    print("model_name", model_name)
    #---------------------------------------------------------------
    # exporting images
    #---------------------------------------------------------------
    if export_images:
        print("Copying %d images"%(number_of_images))
        import shutil
        ensureDir(path_to_output_dataset + 'images')
        for image_id in range(1,number_of_images+1):
            shutil.copy(path_to_input_dataset + 'image%d.jpg'%image_id, path_to_output_dataset + 'images/%d_img.jpg'%(image_id-1)) 
    
        # if undistort:
        # newCameraMatrix = None
        # mapx,mapy = cv2.initUndistortRectifyMap(projMat[0:3,0:3],dist,None,newCameraMatrix,(width,height),5)
        
        
    #---------------------------------------------------------------
    # exporting mesh
    #---------------------------------------------------------------
    if export_mesh:
        print("Exporting mesh with name " + model_name)
        ensureDir(path_to_output_dataset + 'mesh')
        shutil.copy(mesh_path, path_to_output_dataset + 'mesh/' + os.path.basename(mesh_path)) 
    
    
    #---------------------------------------------------------------
    # setup projection matrix and local transform of object between aruco markers
    #---------------------------------------------------------------
    projMat = np.zeros((4,4))
    projMat[0,0] = fx
    projMat[1,1] = fy
    projMat[0,2] = cx
    projMat[1,2] = cy
    projMat[2,2] = 1.0
    projMat[3,3] = 1.0
    
    # scalematrix to transform milimeter scale model into meter scale
    scaleMat = np.eye(4, order='F', dtype=np.float64)
    scaleMat[0,0] = 0.001
    scaleMat[1,1] = 0.001
    scaleMat[2,2] = 0.001
    
    # localToWorld is the localtransform matrix of the physical object into the mal
    locToWorld = np.eye(4, order='F', dtype=np.float64)
    #rotate 180 degrees
    locToWorld[0,0] = -1
    locToWorld[0,2] = 0
    locToWorld[2,0] = 0
    locToWorld[2,2] = -1

    # move center of object to center of aruco pattern
    locToWorld[0,3] =  -34.030003#*0.001
    locToWorld[1,3] =  -38.279999#*0.001
    locToWorld[2,3] =  0.000003#*0.001

    locToWorld = np.matmul(scaleMat, locToWorld)
    
    
    
    #---------------------------------------------------------------
    # exporting annotations
    #---------------------------------------------------------------
    annotations_file = open(path_to_output_dataset + 'annotations.txt', "w")
    
    
    if export_segmentation_masks:
        print("Exporting segmentation masks as well")
        ensureDir(path_to_output_dataset + 'segmentation')
        # make colors file
        colors_file = open(path_to_output_dataset + "colors.txt","w") 
        colors_file.write(model_name + ",255,0,0\n")
        colors_file.close()
        
    for image_id in range(1,number_of_images+1):
        print("exporting image %d/%d"%(image_id,number_of_images), end = "\r")
        rvec, tvec = load_real_camera_metadata(path_to_input_dataset + 'image%d_metadata.txt'%image_id)
        
        if rvec is None or tvec is None:
            print("ERROR: could not load rvec and tvec metadata for image %d"% image_id)
            return
            
            
        # get worldToCam and viewMat from rvec and tvec calibration poses
        #------------------------------------------------------------------------------------------------------------
        R, _ = cv2.Rodrigues(rvec)
        worldToCam = np.zeros((4,4), dtype=np.float64)
        worldToCam[0:3,0:3] = R
        worldToCam[0:3,3] = tvec[:]
        worldToCam[3,3] = 1.0
        viewMat = np.matmul(worldToCam,locToWorld)
        mvp = np.matmul(projMat,viewMat)
        
        
        # convert worldToCam and lockToWorld to Unity coordinate system before outputting to annotations file
        #------------------------------------------------------------------------------------------------------------
        # IDAEALLY REMOVE THIS CONVERSIONS SO INTERMEDIATE FORMAT IS ALREADY IN OPENCV COORDINATE SYSTEM
        
        flipXMatrix = np.zeros((4,4))
        flipXMatrix[0,0] = -1 
        flipXMatrix[1,1] = 1 
        flipXMatrix[2,2] = 1 
        flipXMatrix[3,3] = 1 
        locToWorld_Unity = np.matmul(locToWorld,flipXMatrix)
        
        unityToOpencvConversionMatrix = np.zeros((4,4))
        unityToOpencvConversionMatrix[0,0] = 1 
        unityToOpencvConversionMatrix[1,1] = -1 
        unityToOpencvConversionMatrix[2,2] = 1 
        unityToOpencvConversionMatrix[3,3] = 1 
        
        worldToCam_Unity = np.matmul(unityToOpencvConversionMatrix,worldToCam)
        
        
        # construct json data for this sample
        #------------------------------------------------------------------------------------------------------------
        json_data = {}
        json_data["id"] = image_id-1
        json_data["proj"] = projMat.flatten(order='F').tolist()
        json_data["worldToCam"] = worldToCam_Unity.flatten(order='F').tolist()
        json_data["models"] = []
        json_model = {}
        json_model["name"] = model_name
        json_model["instance"] = 0
        json_model["locToWorld"] = locToWorld_Unity.flatten(order='F').tolist()
        json_data["models"].append(json_model)
        
        json.dump(json_data, annotations_file)
        annotations_file.write("\n")
        annotations_file.flush()
        
        
        if export_segmentation_masks:
            seg = render_segmentation_map(mesh_path, projMat, viewMat, width, height)
            cv2.imwrite(path_to_output_dataset + 'segmentation/%d_seg.png'%(image_id-1), seg) 
            
            if visualize_overlays:
                cv2.namedWindow("segmentation", cv2.WINDOW_NORMAL)
                cv2.imshow("segmentation", seg)
            
        if visualize_overlays:
            debug_visualize_mesh_overlay(projMat, worldToCam, mvp, mesh_path, path_to_input_dataset + 'image%d.jpg'%(image_id))
    
    print("Done exporting %d images"%(number_of_images))
    annotations_file.close()


def debug_visualize_mesh_overlay(projMat, worldToCam, mvp, mesh_path, image_path):
    mvp_aruco = np.matmul(projMat,worldToCam)
    
    # load vertex data of model
    import pywavefront
    scene = pywavefront.Wavefront(mesh_path, create_materials=False, collect_faces=False)
    verts = np.asarray(scene.vertices)#np.array(scene.mesh_list[0].materials[0].vertices, dtype = np.float32)
    
    
    projected = cv2.imread(image_path)
    # if undistort:
        # projected = cv2.remap(projected,mapx,mapy,cv2.INTER_LINEAR)
        
    # project all vertices onto image
    for i in range(verts.shape[0]):
        # if useBlender:
            # # point = np.array([verts[i,0],-verts[i,2],verts[i,1], 1.0]) #TODO: blender objects require some magic coordinate flips
            # point = np.array([verts[i,0],verts[i,1],verts[i,2], 1.0]) #TODO: blender objects require some magic coordinate flips
        # else:
        
        point = np.array([verts[i,0],verts[i,1],verts[i,2], 1.0])
        projPoint = np.matmul(mvp,point)
        projPoint = projPoint / projPoint[2]
        
       
        
        if (int(projPoint[1]) >= 0 and int(projPoint[1]) < projected.shape[0] and int(projPoint[0]) >= 0 and int(projPoint[0]) < projected.shape[1]):
            projected = cv2.circle(projected, (int(projPoint[0]),int(projPoint[1])), 2, (0, 0, 255), 1) 
            # projected[int(projPoint[1]),int(projPoint[0]),:] = [0,0,1]
        # print(projPoint)
        
    point = np.array([0.0,0.0,0.0, 1.0])
    projPoint = np.matmul(mvp_aruco,point)
    projPoint = projPoint / projPoint[2]
    
    
    if (int(projPoint[1]) >= 0 and int(projPoint[1]) < projected.shape[0] and int(projPoint[0]) >= 0 and int(projPoint[0]) < projected.shape[1]):
        projected = cv2.circle(projected, (int(projPoint[0]),int(projPoint[1])), 7, (255, 0, 0), 1) 
        
    cv2.namedWindow("projected", cv2.WINDOW_NORMAL)
    cv2.imshow("projected", projected)
    cv2.waitKey(0)
            
def load_real_camera_metadata(path):
    file = open(path, 'r')
    lines = file.readlines()
    rvec = None
    tvec = None
    for line in lines:
        if line[0:7] == 'rvecMid':
            rvec = np.fromstring(line[11:-2], dtype=np.float64, sep=',')
        if line[0:7] == 'tvecMid':
            tvec = np.fromstring(line[11:-2], dtype=np.float64, sep=',')
    return rvec, tvec

def ensureDir(directory):
    import os
    if not os.path.exists(directory):
        os.makedirs(directory)
        
def render_segmentation_map(mesh_path, camera_mat, local_transform, width, height):
    import pyrender
    import trimesh
    mesh_data = trimesh.load(mesh_path)
    mesh = pyrender.Mesh.from_trimesh(mesh_data, smooth=False)
    
    scene = pyrender.Scene(ambient_light=[0.0, 0.0, 0.0], bg_color=[1.0, 1.0, 1.0])
    camera = pyrender.IntrinsicsCamera(fx = camera_mat[0,0],fy = camera_mat[1,1], cx = camera_mat[0,2], cy = camera_mat[1,2])
    #camera = pyrender.PerspectiveCamera( yfov=np.pi / 3.0)
    light = pyrender.PointLight(color=[1,1,1], intensity=2e2)
    local_transform = local_transform.copy()
    local_transform[1,:] = -local_transform[1,:]
    local_transform[2,:] = -local_transform[2,:]
    scene.add(mesh, pose=  local_transform)
    
    light_pos =  np.eye(4)
    light_pos[1,3] = 3
    # scene.add(light, pose= light_pos)
    
    camera_node = scene.add(camera, name="camera", pose=np.eye(4))
    
    # render scene
    r = pyrender.OffscreenRenderer(width, height)
    color, _ = r.render(scene)
    
    
    seg = color.copy()
    white_pixels_mask = np.all(seg != [255, 255, 255], axis=-1)
    seg[white_pixels_mask] = [0,0,255]
    
    return seg


if __name__ == '__main__':
    main()