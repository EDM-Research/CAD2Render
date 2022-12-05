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
[CreateAssetMenu(fileName = "Untitled Dataset", menuName = "HDRPSyntheticDataGenerator/New Main Dataset", order = 2)]
public class MainRandomizerData : ScriptableObject {
    //public enum Parametersource { Forced, AutoDetect, TextFile }
    //public DatasetInformation.Parametersource parametersource = DatasetInformation.Parametersource.Forced;


    [Header("Input/output paths")]
    [Tooltip("Path to output dataset folder.")]
    public string outputPath = "../renderings/";
    [Tooltip("Name of annotations file. Relative to output path.")]
    public string annotationisFile = "annotations.txt";
    [Tooltip("Location of the bop file to import.")]
    public string BOPInputPath = "";
    [Tooltip("With which value the unity units need to be multiplied to get mm.")]
    [Range(0.001f, 10)]
    public float mmToUnityDistanceScale = 0.01f;

    [Header("Render settings")]
    [Tooltip("Resolution of generated images.")]
    public Vector2Int resolution = new Vector2Int(1024, 1024);
    [Tooltip("Change the default render settings if set.")]
    public VolumeProfile renderProfile = null;
    [Tooltip("Change the default raytracing settings if set.")]
    public VolumeProfile rayTracingProfile = null;
    [Tooltip("Change the default post procesing settings if set.")]
    public VolumeProfile postProcesingProfile = null;
    [Tooltip("Enable gamma correction. Required to map linear scale rendered texture to gamma scale.")]
    public bool applyGammaCorrection = true;
    //[Tooltip("Gamma correction gamma (pixel^(1/gamma)")]
    //public float gammaCorrection = 2.2f;
    [Tooltip("Enable auto exposure of camera. Avoids too bright or too dark images.")]
    public bool autoCameraExposure = false;
    [Tooltip("Stop simumation time when rendering.")]
    public bool stopSimulationTimeCompletly = true;
    [Tooltip("Number of intermediate frames that are renderd before saving the image.")]
    public int numRenderFrames = 50;
    [Tooltip("Number of frames that the scene is renderd on a lower resolution to let newly spawned objects settle.")]
    public int numPhysicsFrames = 50;

    [Header("Generation settings")]
    [Tooltip("Start id of first generated image.")]
    public int startFileCounter = 0;
    [Tooltip("Number of samples to generate (-1 for indefinite).")]
    public int numberOfSamples = -1;
    [Tooltip("Seed for random number generator.")]
    public int seed = 42;
    [Tooltip("Update the randomizers on diferent intervals.")]
    public bool separateUpdates = false;
    public enum RandomizerTypes {Default, View, Object, Light, Material }
    [System.Serializable]
    public class RandomizerUpdateIntervals
    {
        public RandomizerTypes randomizerType;
        [Tooltip("0 and 1 act the same way, randomizing every update.")]
        public uint interval = 1;
    }
    [Tooltip("Intervals to update the randomizers.")]
    public RandomizerUpdateIntervals[] updateIntervals = new RandomizerUpdateIntervals[0];//a dictionary would be better but the unity editor doesnt support this


    [Header("Export settings")]
    [Tooltip("Only export object info of game objects with the exportInstanceInfo tag.")]
    public bool exportModelsByTag = false;

    [Space(5)]
    [Tooltip("Export BOP files")]
    public bool exportToBOP = true;
    [Tooltip("The scene id used when exporting to the bop format")]
    public int BOPSceneId = 1;

    [Space(5)]
    [Tooltip("Export depth map")]
    public bool exportDepthTexture = false;
    [Tooltip("The max distance the depth in mm texture displays correctly, further away objects wil be sturated. Lower values mean more detailed depth texture")]
    public float maxDepthDistance = 1000.0f;
    [Tooltip("Export normal map")]
    public bool exportNormalTexture = false;
    [Tooltip("Export albedo map")]
    public bool exportAlbedoTexture = false;

    [Space(5)]
    [Tooltip("Export fm_format files (deprecated)")]
    public bool exportToFMFormat = false;
    [Tooltip("Extension of generated images. Segmentation masks are always exported as png.")]
    public ImageSaver.Extension outputExt = ImageSaver.Extension.png;
    [Tooltip("Enable exporting sub-models.")]
    public bool exportSubModels = false;
    [Tooltip("Enable to export 2d image positions of sub models. Used for keypoints.")]
    public bool exportImagePosition = false;
    [Space(5)]
    [Tooltip("Export Mitsuba Render files")]
    public bool exportToMitsuba = false;
    [Tooltip("Number of Mitsuba samples")]
    public int mitsubaSampleCount = 128;

    [Obsolete]
    public MainRandomizerData(DatasetInformation data)
    {
        resolution = new Vector2Int(data.resolutionWidth, data.resolutionHeight);
        //parametersource = data.parametersource;
        outputPath = data.outputPath;
        if (data.importFromBOP)
            BOPInputPath = data.outputPath;//no separate path exisits in the old data format
        else
            BOPInputPath = "";
        annotationisFile = data.annotationisFile;
        renderProfile = data.renderProfile;
        rayTracingProfile = data.rayTracingProfile;
        postProcesingProfile = data.postProcesingProfile;
        applyGammaCorrection = data.applyGammaCorrection;
        //gammaCorrection = data.gammaCorrection;
        autoCameraExposure = data.autoCameraExposure;
        stopSimulationTimeCompletly = data.stopSimulationTimeCompletly;
        numRenderFrames = data.numRenderFrames;
        numPhysicsFrames = data.numPhysicsFrames;
        startFileCounter = data.startFileCounter;
        numberOfSamples = data.numberOfSamples;
        seed = data.seed;
        exportModelsByTag = data.exportModelsByTag;
        outputExt = data.outputExt;
        exportSubModels = data.exportSubModels;
        exportImagePosition = data.exportImagePosition;
        exportToMitsuba = data.exportToMitsuba;
        mitsubaSampleCount = data.mitsubaSampleCount;
        exportToBOP = data.exportToBOP;
        BOPSceneId = data.BOPSceneId;
    }

}
