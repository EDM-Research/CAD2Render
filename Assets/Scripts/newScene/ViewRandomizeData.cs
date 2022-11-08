//Copyright (c) 2020 Nick Michiels <nick.michiels@uhasselt.be>, Hasselt University, Belgium, All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

//[System.Serializable]
//public class Location {
//	public string ID;
//	public string path;
//	public string extension;
//	public string videoFile;
//}

[HelpURL("Documentation/DatasetInformation.html")] // TODO
[CreateAssetMenu(fileName = "Untitled Dataset", menuName = "HDRPSyntheticDataGenerator/New View randomize Data", order = 2)]
public class ViewRandomizeData : ScriptableObject {


    [Header("Viewpoint Variations")]
    [Tooltip("Enable viewpoint variations")]
    public bool viewpointVariatons = false;
    [Tooltip("Use the bop import file to determine the camera pose")]
    public bool importFromBOP = false;
    [Tooltip("Enable random y up vector between (0,1,0) and (0,-1,0). Required for changing viewpoint over poles, caused by limitations of spherical coordinates.")]
    public bool randomYUp = false;
    [Tooltip("Min theta")]
    [Range(-90.0f, 90.0f)]
    public float minTheta = 10.0f;
    [Tooltip("Max theta")]
    [Range(-90.0f, 90.0f)]
    public float maxTheta = 89.0f;
    [Tooltip("Min phi")]
    [Range(-360.0f, 360.0f)]
    public float minPhi = -45.0f;
    [Tooltip("Max phi")]
    [Range(-360.0f, 360.0f)]
    public float maxPhi = 45.0f;
    [Tooltip("Min radius in mm")]
    [Range(10.0f, 2000.0f)]
    public float minRadius = 1.0f;
    [Tooltip("Max radius in mm")]
    [Range(10f, 2000.0f)]
    public float maxRadius = 100.0f;

    public bool useCameraMatrix = false;
    public Matrix4x4 cameraMatrix = new Matrix4x4();
}
