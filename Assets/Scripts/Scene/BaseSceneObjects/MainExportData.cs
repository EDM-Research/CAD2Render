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
[CreateAssetMenu(fileName = "Untitled Dataset", menuName = "Cad2Render/New export Settings", order = 2)]
public class MainExportData : ScriptableObject {
    //public enum Parametersource { Forced, AutoDetect, TextFile }
    //public DatasetInformation.Parametersource parametersource = DatasetInformation.Parametersource.Forced;

    [Header("Export settings")]
    [Tooltip("Location where the exported datasets are saved.")]
    public string outputPath = "";
    [Tooltip("The scene id")]
    public int sceneId = 1;
    [Tooltip("Start id of first generated image.")]
    public int startFileCounter = 0;
    [Tooltip("Number of samples to generate (-1 for indefinite).")]
    public int numberOfImages = -1;

    [Header("Render settings")]
    [Tooltip("Resolution of generated images.")]
    public Vector2Int resolution = new Vector2Int(1024, 1024);
    [Tooltip("Enable gamma correction. Required to map linear scale rendered texture to gamma scale.")]
    public bool applyGammaCorrection = true;
    [Tooltip("Stop simumation time when rendering.")]
    public bool stopSimulationTimeCompletly = true;
    [Tooltip("Number of intermediate frames that are renderd before saving the image.")]
    public int numRenderFrames = 50;
    [Tooltip("Number of frames that the scene is renderd on a lower resolution to let newly spawned objects settle.")]
    public int numPhysicsFrames = 50;

    [Header("Unit settings")]
    [Tooltip("With which value the unity units need to be multiplied to get mm.")]
    [Range(0.001f, 10)]
    public float mmToUnityDistanceScale = 0.01f;
    [Space(5)]
    [Tooltip("The max distance the depth in mm texture displays correctly, further away objects wil be sturated. Lower values mean more detailed depth texture")]
    public float maxDepthDistance = 1000.0f;

}
