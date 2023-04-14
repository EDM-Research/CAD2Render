"""
Simple visualizer for EDM to quickly check if the dataset is correct
"""

import cv2
import os
import json
from plyfile import PlyData
import numpy as np
import matplotlib.pyplot as plt
import matplotlib.patches as patches

# Inputs
data_path = "E:\\Datasets\\PILS\\Barco\\bop"
scene_id = 0
DisplayAll = True

# model scale
model_scale = 100   # we assume a unit of mm

## Select the scene
all_scenes = os.listdir(os.path.join(data_path, 'train_PBR'))
scene_path = os.path.join(data_path, 'train_PBR', all_scenes[scene_id])
print("List of all scene folders: " + str(all_scenes))
print("Displaying results for folder: " + str(scene_path))

## get the model information
try:
    model_map_file = os.path.join(scene_path, "model_id.json")
    with open(model_map_file, 'r') as infile:
        model_map = json.load(infile)
except FileNotFoundError:
    model_map = {}
print("List of all models in the scene: " + str(model_map))

def getModelBB(object_id):

    ## read model PLY and sample FPS
    try:
        # full name in the model filename
        mesh_path = os.path.join(data_path, 'models', model_map[str(object_id)] + '.ply')
        scene = PlyData.read(open(mesh_path, 'rb'))
    except (KeyError, FileNotFoundError):
        # Model name type 0000001.ply
        mesh_path = os.path.join(data_path, 'models', "obj_" + str(object_id).zfill(6) + '.ply')
        scene = PlyData.read(open(mesh_path, 'rb'))

    num_verts = scene['vertex'].count
    vertices = np.zeros(shape=[num_verts, 3], dtype=np.float32)
    vertices[:, 0] = scene['vertex'].data['x']
    vertices[:, 1] = scene['vertex'].data['y']
    vertices[:, 2] = scene['vertex'].data['z']

    model = np.asarray(vertices)
    model = model*model_scale

    # Get Model Corners:
    min_x, max_x = np.min(model[:, 0]), np.max(model[:, 0])
    min_y, max_y = np.min(model[:, 1]), np.max(model[:, 1])
    min_z, max_z = np.min(model[:, 2]), np.max(model[:, 2])
    corners_3d = np.array([
        [min_x, min_y, min_z],
        [min_x, min_y, max_z],
        [min_x, max_y, min_z],
        [min_x, max_y, max_z],
        [max_x, min_y, min_z],
        [max_x, min_y, max_z],
        [max_x, max_y, min_z],
        [max_x, max_y, max_z],
    ])

    #center_3d = np.reshape(
    #            (np.max(corners_3d, 0) + np.min(corners_3d, 0)) / 2, (1, 3))
    #diameter = np.linalg.norm(np.asarray(
    #            corners_3d[0])-np.asarray(corners_3d[7]))
    #print(f'Model diameter: {diameter}')
    
    return corners_3d
#end def getModelBB

edgeColors = ['#ff0000', '#00ff00', '#0000ff', '#ffff00', '#ff00ff', '#00ffff', '#ffffff', '#000000']
def displayModel(instance_id, image_id, cam_K, gt_data, ax, color):
    # get rvec, tvec and object id from GT
    rvec = np.reshape(gt_data[image_id][instance_id]['cam_R_m2c'], [3, 3])
    tvec = np.array(gt_data[image_id][instance_id]["cam_t_m2c"])
    obj_id = np.array(gt_data[image_id][instance_id]["obj_id"])
    corners_3d = getModelBB(obj_id)


    # project corner onto image
    corners_2d, _ = cv2.projectPoints(corners_3d, rvec, tvec, cam_K, np.array([]))
    corners_2d = np.squeeze(corners_2d)

    ax.imshow(vis)
    ax.add_patch(patches.Polygon(xy=corners_2d[[0, 1, 3, 2, 0, 4, 6, 2]], fill=False, linewidth=1, edgecolor=color))
    ax.add_patch(patches.Polygon(xy=corners_2d[[5, 4, 6, 7, 5, 1, 3, 7]], fill=False, linewidth=1, edgecolor=color))
#end def displayModel

## read GT file
gt_file = os.path.join(scene_path, 'scene_gt.json')
with open(gt_file, 'r') as infile:
    gt_data = json.load(infile)

# read camera file:
cam_file = os.path.join(scene_path, 'scene_camera.json')
with open(cam_file, 'r') as infile:
    cam_data = json.load(infile)

for image_id, cam in zip(gt_data, cam_data):
    img_file = os.path.join(scene_path, 'rgb', str(image_id).zfill(6)+'.jpg')
    vis = cv2.imread(img_file, cv2.COLOR_BGR2RGB)
    
    # get the camera matrix
    cam_K = np.array(cam_data[image_id]["cam_K"]).reshape([3, 3]).astype('float')

    fig, ax = plt.subplots()
    
    for instance_id in range(len(gt_data[image_id])):
        edgeColor = "#777777"
        if(instance_id < len(edgeColors)):
            edgeColor = edgeColors[instance_id]
        displayModel(instance_id, image_id, cam_K, gt_data, ax, edgeColor)
        if(DisplayAll == False):
            break
    
    ax.set_title(str(image_id).zfill(6)+'.jpg')
    plt.show()
    
