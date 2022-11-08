# Copyright (c) 2020 Nick Michiels <nick.michiels@uhasselt.be>, Hasselt University, Belgium, All rights reserved.


import cv2
import numpy as np
import json

#THIS SCRIPT ONLY WORKS FOR UNITY MODELS BASED ON OBJ OR FBX. It does not work for .blend files.
# REMARKS FOR USING BLENDER:
# - Make sure all objects are positioned at position (0,0,0) with no rotation!
# - When exporting to FBX, make sure to enable "!EXPERIMENTAL Apply Transform"


# interesting figure: https://medium.com/comerge/what-are-the-coordinates-225f1ec0dd78

pathToDataset = 'example/'
# pathToDataset = '../../renderings/businesscardholder_1obj_21001_seed1980/'
# pathToDataset = '../../renderings/multirob/'
#pathToDataset = '../../renderings/Compressor/'
#pathToDataset = '../../renderings/businessCardHolder/'
# pathToDataset = '/exampleAtlas/'

f = open(pathToDataset + 'annotations.txt', "r")

image_id = 1 # image id to extract out of annotations and apply projection on

jsonstring = ""
for i in range(image_id+1):
    jsonstring = f.readline()
data = json.loads(jsonstring)



for i in range(len(data["models"])):
    print(i, data["models"][i]["name"])
    
print("Please select a model to overlay (%d-%d):"%(0,len(data["models"])-1))
modelIdx = int(input())

if modelIdx < 0 or modelIdx >= len(data["models"]):
    print("Invalid model!")
    import sys
    sys.exit(0)


name = data["models"][modelIdx]["name"]
    

print("Visualizing", name)


# load projection matrix
projMat = data["proj"]
projMat = np.reshape(projMat, (4,4),order='F')

# For now, we cannot directly use projection matrix of unity, because it is in left handed coordinate system
# Manual override of projection matrix with custom opencv pinhole model
# MAKE SURE TO CHANGE FX,FY,CX,CY when resolution or camera fov changes
projMat = np.zeros((4,4))
projMat[0,0] = 1422.22222222222 #fx == (0.5*width) / tan(hfov / 2.0)
projMat[1,1] = 1422.22222222222 #fy == (0.5*height) / tan(vfov / 2.0)
projMat[0,2] = 512 #cx
projMat[1,2] = 512 #cy
projMat[2,2] = 1.0
projMat[3,3] = 1.0



# Load localtransform of model
locToWorld = data["models"][modelIdx]["locToWorld"]
locToWorld = np.reshape(locToWorld, (4,4),order='F')

#load camera matrix
worldToCam = data["worldToCam"]
worldToCam = np.reshape(worldToCam, (4,4),order='F')


# Unity magically flips x-asis of vertices in obj and fbx files
flipXMatrix = np.zeros((4,4))
flipXMatrix[0,0] = -1 
flipXMatrix[1,1] = 1 
flipXMatrix[2,2] = 1 
flipXMatrix[3,3] = 1 
locToWorld = np.matmul(locToWorld,flipXMatrix)

# calculate view matrix using localToWorld and cameramatrix
viewMat = np.matmul(worldToCam,locToWorld)

# Just before projection we transform the coordinate in unity coordinates to opencv coordinates ( from unity to opencv requires a flip of y coordinat)
#only left handed multiplication is required because all matrix operations are first done in unity coordinate system (left handed).
unityToOpencvConversionMatrix = np.zeros((4,4))
unityToOpencvConversionMatrix[0,0] = 1 
unityToOpencvConversionMatrix[1,1] = -1 
unityToOpencvConversionMatrix[2,2] = 1 
unityToOpencvConversionMatrix[3,3] = 1 
viewMat = np.matmul(unityToOpencvConversionMatrix,viewMat)


# calculate modelviewprojection matrix, this can then be used to project 3D point towards 2d pixel
mvp = np.matmul(projMat,viewMat)


# load vertex data of model
import pywavefront
scene = pywavefront.Wavefront(pathToDataset + "models/" + name + '.obj', create_materials=False, collect_faces=False)
verts = np.asarray(scene.vertices)#np.array(scene.mesh_list[0].materials[0].vertices, dtype = np.float32)


# initialize empty target image
projected = np.zeros((1024,1024,3))


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


#visualize segmentation mask and projected mesh
seg = cv2.imread(pathToDataset + "/segmentation/%d_seg.png"%image_id)

cv2.imshow("projected", projected)
cv2.imshow("segmentation", seg)
cv2.waitKey(0);

f.close()
