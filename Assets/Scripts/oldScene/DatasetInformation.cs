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

[Obsolete("Used by the old scene, Use the new scene instead")]
[HelpURL("Documentation/DatasetInformation.html")] 
[CreateAssetMenu(fileName = "Untitled Dataset", menuName = "HDRPSyntheticDataGenerator/New Dataset (old dataset structure)", order = 2)]
public class DatasetInformation : ScriptableObject {
	public enum Parametersource { Forced, AutoDetect, TextFile }
	public Parametersource parametersource = Parametersource.Forced;

    [Header("General")]
    [Tooltip("Change the default render settings if set.")]
    public VolumeProfile renderProfile = null;
    [Tooltip("Change the default raytracing settings if set.")]
    public VolumeProfile rayTracingProfile = null;
    [Tooltip("Change the default post procesing settings if set.")]
    public VolumeProfile postProcesingProfile = null;
    [Tooltip("Width of generated images. Make sure you altered all render textures.")]
    public int resolutionWidth = 1024;
    [Tooltip("Height of generated images. Make sure you altered all render textures.")]
    public int resolutionHeight = 1024;
    public enum Extension { png, jpg };
    [Tooltip("Extension of generated images. Segmentation masks are always exported as png.")]
    public Extension outputExt = Extension.png;
    [Tooltip("Start id of first generated image.")]
    public int startFileCounter = 0;
    [Tooltip("Number of samples to generate (-1 for indefinite).")]
    public int numberOfSamples = -1;
    [Tooltip("Only export object info of game objects with the exportInstanceInfo tag.")]
    public bool exportModelsByTag = false;
    [Tooltip("Use predefined false color of the prefabs for the segmentation map.")]
    public bool exportPredefinedFalseColors = false;

    [Space(10)] // 15 pixels of spacing here.
    [Tooltip("Enable exporting sub-models.")]
    public bool exportSubModels = false;
    [Tooltip("Enable to export 2d image positions of sub models. Used for keypoints.")]
    public bool exportImagePosition = false;
    [Tooltip("Export Mitsuba Render files")]
    public bool exportToMitsuba = false;
    [Tooltip("Number of Mitsuba samples")]
    public int mitsubaSampleCount = 128;
    [Tooltip("Export BOP files")]
    public bool exportToBOP = false;
    [Tooltip("The scene id used when exporting to the bop format")]
    public int BOPSceneId;
    [Tooltip("Import camera poses model poses from BOP files")]
    public bool importFromBOP = false;

    [Space(10)] // 15 pixels of spacing here.
    [Tooltip("Enable gamma correction. Required to map linear scale rendered texture to gamma scale.")]
    public bool applyGammaCorrection = true;
    [Tooltip("Gamma correction gamma (pixel^(1/gamma)")]
    public float gammaCorrection = 2.2f;
    [Tooltip("Enable auto exposure of camera. Avoids too bright or too dark images.")]
    public bool autoCameraExposure = false;
    [Tooltip("Stop simumation time when rendering.")]
    public bool stopSimulationTimeCompletly = true;
    [Tooltip("Number of intermediate frames that are renderd before saving the image.")]
    public int numRenderFrames = 50;
    [Tooltip("Number of frames that the scene is renderd on a lower resolution to let newly spawned objects settle.")]
    public int numPhysicsFrames = 50;
    [Tooltip("Seed for random number generator.")]
    public int seed = 42;

    [Header("Input/output paths")]
    [Tooltip("Path to output dataset folder.")]
    public string outputPath = "../renderings/";
    [Tooltip("Path to prefabs of models (relative to Resources dir)")]
    public string modelsPath;
    [Tooltip("Path to environment maps (relative to Resources dir)")]
    public string environmentsPath;
    [Tooltip("Path to materials (relative to Resources dir)")]
    public string materialsPath;
    [Tooltip("Path to table materials (relative to Resources dir)")]
    public string tableMaterialsPath;
    [Tooltip("Path to material textures (relative to Resources dir)")]
    public string texturesPath;
    [Tooltip("Path to projector textures (relative to Resources dir)")]
    public string projectorTexturePath;
    [Tooltip("Name of annotations file. Relative to output path.")]
    public string annotationisFile = "annotations.txt";

    [Header("Model Variations")]
    [Tooltip("Enable model variations")]
    public bool modelVariatons = true;
    [Tooltip("Generate one model per unique prefab in modelsPath")]
    public bool uniqueObjects = true;
    [Tooltip("Number of random objects to create. Multiple objects of same class are possible.")]
    public int numRandomObjects = 3;
    [Tooltip("Enable random translations of models in POI")]
    public bool randomModelTranslations = true;
    [Tooltip("Enable random rotations of models in POI")]
    public bool randomModelRotations = true;
    [Tooltip("Enable random rotations offset around y axis")]
    public bool randomRotationOffset = false;
    [Tooltip("max degree of rotations around y axis, in both positive and negative direction")]
    public float randomRotationOffsetValue = 0.0f;
    [Tooltip("Enable random translations of submodel in prefab")]
    public bool randomSubModelTranslation = false;
    [Tooltip("Avoid Collisions")]
    public bool avoidCollisions = false;
    [Tooltip("Name of submodel in prefab to randomly translate")]
    public string subModelName = "";
    [Tooltip("Random submodel offset per axis")]
    public Vector3 subModelOffset = new Vector3(0, 0, 0);

    

    [Header("Viewpoint Variations")]
    [Tooltip("Enable viewpoint variations")]
    public bool viewpointVariatons = false;
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
    [Tooltip("Min radius")]
    [Range(0.0f, 20.0f)]
    public float minRadius = 1.0f;
    [Tooltip("Max radius")]
    [Range(0.0f, 20.0f)]
    public float maxRadius = 1.0f;

    [Header("Environment Variations")]
    [Tooltip("Enable random environment maps.")]
    public bool environmentVariatons = true;
    [Tooltip("Enable random environment map rotation.")]
    public bool randomEnvironmentRotations = false;
    [Tooltip("Enable random environment exposures. Scaling of environment map intensity.")]
    public bool randomExposuresEnvironment = false;
    [Tooltip("Min exposure")]
    [Range(-5.0f, 20.0f)]
    public float minExposure = 0.0f;
    [Tooltip("Max exposure")]
    [Range(-5.0f, 20.0f)]
    public float maxExposure = 8.0f;
    [Tooltip("Change projector texture.")]
    public bool applyProjectorVariations = false;
    [Tooltip("Enable random spawn of light sources")]
    public bool lightsourceVariatons = false;
    [Tooltip("Number of light sources to spawn")]
    public int numLightsources = 2;
    [Tooltip("Min theta")]
    [Range(-90.0f, 90.0f)]
    public float minThetaLight = 10.0f;
    [Tooltip("Max theta")]
    [Range(-90.0f, 90.0f)]
    public float maxThetaLight = 89.0f;
    [Tooltip("Min phi")]
    [Range(-360.0f, 360.0f)]
    public float minPhiLight = -45.0f;
    [Tooltip("Max phi")]
    [Range(-360.0f, 360.0f)]
    public float maxPhiLight = 45.0f;
    [Tooltip("Min radius")]
    [Range(0.0f, 20.0f)]
    public float minRadiusLight = 1.0f;
    [Tooltip("Max radius")]
    [Range(0.0f, 20.0f)]
    public float maxRadiusLight = 1.0f;


    [Header("Material Variations")]
    [Tooltip("Enable material variations")]
    public bool materialVariatons = false;
    [Tooltip("The resolution that generated texture wil have. when 0 it wil either default to 2048 or take the same resolution as the first input texture used for texture generation")]
    public Vector2Int generatedTextureResolution = new Vector2Int(0, 0);
    //public TextureDimension generatedTextureYResolution = 0;
    [Tooltip("Enable random HSV offets. Allows for indivudual variations of H, S or V. Offset is the maximum change in HSV. The variation is chosen randomly between 0 and offset.")]
    public bool applyRandomHSVOffset = false;
    [Tooltip("Random Hue offset")]
    [Range(0.0f, 180.0f)]
    public float H_maxOffset = 0.0f;
    [Tooltip("Random Saturation offset")]
    [Range(0.0f, 50.0f)]
    public float S_maxOffset = 0.0f;
    [Tooltip("Random Value offset")]
    [Range(0.0f, 50.0f)]
    public float V_maxOffset = 0.0f;
    [Tooltip("Enable random changes in normal map scales, uv scales and uv tilings")]
    public bool applyRandomUV = false;
    [Tooltip("Enable random changes albedo texture for material")]
    public bool applyRandomAlbedoTexture = false;

    [Space(15)] // 15 pixels of spacing here.
    [Tooltip("Enable random changes in the detail map")]
    public bool applyDetailMapVariations = false;
    [Range(0, 10)]
    public uint nrOfScratches = 3;
    [Range(1.0f, 10.0f)]
    public float scratchWidth = 5;
    [Range(1,8)]
    public uint nrAntiAliasSamples = 1;
    [Range(0, 10)]
    public uint nrOfDarkspots = 3;
    [Range(1.0f, 10.0f)]
    public float spotSize = 5;
    [Space(5)] // 5 pixels of spacing here
    [Range(0.0f, 2.0f)]
    public float detailAlbedoScale = 0.0f;
    [Range(0.0f, 2.0f)]
    public float detailNormalScale = 0.3f;
    [Range(0.0f, 2.0f)]
    public float detailSmoothnessScale = 0.0f;

    [Space(15)] // 10 pixels of spacing here.
    [Tooltip("Add rust to the material")]
    public bool applyRust = false;
    [Range(0.0f, 1.0f)]
    [Tooltip("Amount of rust to be applied")]
    public float rustCoeficient = 0.39f;
    [Tooltip("determines the size of rust spots")]
    [Range(0.0151f, 0.1f)]
    public float rustMaskZoom = 0.009f;
    [Tooltip("determines the size of color variations in rust spots")]
    [Range(0.0021f, 0.49f)]
    public float rustPaternZoom = 0.39f;
    public Color rustColor1 = new Color(133.0f / 255, 60.0f / 255, 42.0f / 255, 1);
    public Color rustColor2 = new Color( 65.0f / 255, 33.0f / 255, 15.0f / 255, 1);
    [Tooltip("determines the detail in the rust mask and patern, comes with computational cost.")]
    [Range(1, 10)]
    public uint nrOfOctaves = 5;

    [Space(15)] // 10 pixels of spacing here.
    [Tooltip("Add polishing lines to the material")]
    public bool applyManufacturingLines = false;
    [Range(0.001f, 0.2f)]
    public float lineSpacing = 0.08f;
    public Texture LineAndRustMask;

    [Header("Texture resampler (preview)")]
    public bool applyTextureResampling = false;
    public MaterialTextures.MapTypes[] resampleTextures = new MaterialTextures.MapTypes[0];
    [Range(1, 50)]
    public int nrResampleSamples = 9;
    [Range(5, 17)]
    public int nrResampleGenerations = 17;


    [Header("Override Model Material/Color")]
    [Tooltip("Substitutes color of model material with different random color.")]
    public bool overrideWithRandomMaterialColor = false;
    [Tooltip("Substitutes material of model with one specific material.")]
    public bool overrideWithRandomMaterial = false;
    [Tooltip("Material to subsitute.")]
    public Material varyingMaterial;
    [Tooltip("Min color range of random color.")]
    public Color minColor;
    [Tooltip("Max color range of random color.")]
    public Color maxColor;
    [Tooltip("Reset model material color to this backup color.")]
    public Color backupColor;

    [Header("Table variations")]
    [Tooltip("Enable random material assignment to table")]
    public bool tableVariatons = false;
    [Tooltip("Allows to vary the color of the table material with a random color.")]
    public bool overrideTableRandomMaterialColor = false;
    [Tooltip("Min color range of random color.")]
    public Color minColorTable;
    [Tooltip("Max color range of random color.")]
    public Color maxColorTable;
    [Tooltip("Reset table material color to this backup color.")]
    public Color backupColorTable;


    
    //[Header("Locations of the data, per device")]
    //public List<Location> locations = new List<Location>();

    //public string Path(DeviceManager currentDevice) {
    //	foreach(Location l in locations) {
    //           if (l.ID == currentDevice.devicename) {
    //               if (l.path.EndsWith("\\"))
    //                   return l.path;
    //               else
    //                   return l.path + "\\";
    //           }
    //	}
    //	throw new Exception("This dataset is not available on this device");
    //}

    //public string Extension(DeviceManager currentDevice) {
    //	foreach (Location l in locations) {
    //		if (l.ID == currentDevice.devicename)
    //			return l.extension;
    //	}
    //	throw new Exception("This dataset is not available on this device");
    //}

    //public string VideoFile(DeviceManager currentDevice) {
    //	foreach (Location l in locations) {
    //		if (l.ID == currentDevice.devicename)
    //			return l.videoFile;
    //	}
    //	throw new Exception("This dataset is not available on this device");
    //}

    [MenuItem("Assets/Activate This Dataset", true)]
    private static bool NewMenuOptionValidation()
    {
        return Selection.activeObject is DatasetInformation;
    }

    //[MenuItem("Assets/Activate This Dataset")]
    //private static void Activate()
    //{
    //    UseRenderingPlugin[] foundObjects = FindObjectsOfType<UseRenderingPlugin>();
    //    foreach (UseRenderingPlugin obj in foundObjects)
    //    {
    //        obj.dataset = Selection.activeObject as DatasetInformation;
    //    }
    //}
}
