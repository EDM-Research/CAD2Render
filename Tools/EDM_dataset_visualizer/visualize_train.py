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

# model scale
model_scale = 100   # we assume a unit of mm

# path to root of the dataset
data_path = "D:/Projects/PILS/renderings/proximus/bop"

## get the number of scenes
all_scenes = os.listdir(os.path.join(data_path, 'train_PBR'))
print(all_scenes)

# TODO: we assume only the first scene for development
scene_path = os.path.join(data_path, 'train_PBR', all_scenes[0])

## get the model information
try:
    model_map_file = os.path.join(scene_path, "model_id.json")
    with open(model_map_file, 'r') as infile:
        model_map = json.load(infile)
except FileNotFoundError:
    model_map = {"Model_name":"1","Model_id":1}
print(model_map)

## read model PLY and sample FPS
try:
    # full name in the model filename
    mesh_path = os.path.join(data_path, 'models', model_map['Model_name']+'.ply')
    scene = PlyData.read(open(mesh_path, 'rb'))
except FileNotFoundError:
    # Model name type 0000001.ply
    mesh_path = os.path.join(data_path, 'models', str(model_map['Model_id']).zfill(6)+'.ply')
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

center_3d = np.reshape(
            (np.max(corners_3d, 0) + np.min(corners_3d, 0)) / 2, (1, 3))
diameter = np.linalg.norm(np.asarray(
            corners_3d[0])-np.asarray(corners_3d[7]))

print(f'Model diameter: {diameter}')

## read GT file
gt_file = os.path.join(scene_path, 'scene_gt.json')
with open(gt_file, 'r') as infile:
    gt_data = json.load(infile)

# read camera file:
cam_file = os.path.join(scene_path, 'scene_camera.json')
with open(cam_file, 'r') as infile:
    cam_data = json.load(infile)

for im, cam in zip(gt_data, cam_data):
    img_file = os.path.join(scene_path, 'rgb', str(im).zfill(6)+'.jpg')
    vis = cv2.imread(img_file, cv2.COLOR_BGR2RGB)

    # get rvec and tvec from GT
    rvec = np.reshape(gt_data[im][0]['cam_R_m2c'], [3, 3])
    tvec = np.array(gt_data[im][0]["cam_t_m2c"])

    # get the camera matrix
    cam_K = np.array(cam_data[im]["cam_K"]).reshape([3, 3]).astype('float')

    # project corner onto image
    corners_2d, _ = cv2.projectPoints(corners_3d, rvec, tvec, cam_K, np.array([]))
    corners_2d = np.squeeze(corners_2d)

    fig, ax = plt.subplots()
    ax.imshow(vis)
    ax.add_patch(patches.Polygon(xy=corners_2d[[0, 1, 3, 2, 0, 4, 6, 2]], fill=False, linewidth=1, edgecolor='g'))
    ax.add_patch(patches.Polygon(xy=corners_2d[[5, 4, 6, 7, 5, 1, 3, 7]], fill=False, linewidth=1, edgecolor='g'))
    ax.set_title(str(im).zfill(6)+'.jpg')
    plt.show()
    
