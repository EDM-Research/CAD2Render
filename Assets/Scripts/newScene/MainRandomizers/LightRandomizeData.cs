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
[CreateAssetMenu(fileName = "Untitled Dataset", menuName = "Cad2Render/New Light randomize Data", order = 2)]
public class LightRandomizeData : ScriptableObject {

    [Header("Input/output paths")]
    [Tooltip("Path to environment maps (relative to Resources dir)")]
    public string environmentsPath;
    [Tooltip("Light prefab used to create light sources")]
    public Light lightSourcePrefab;

    [Header("Environment Variations")]
    [Tooltip("Enable random environment maps.")]
    public bool environmentVariatons = true;
    [Tooltip("Enable random environment maps.")]
    public bool pickRandomEnvironment = true;
    [Tooltip("Enable random environment map rotation.")]
    public bool randomEnvironmentRotations = false;
    [Tooltip("Min environment angle")]
    [Range(0.0f, 360.0f)]
    public float minEnvironmentAngle = 0.0f;
    [Tooltip("Max environment angle")]
    [Range(0.0f, 360.0f)]
    public float maxEnvironmentAngle = 360.0f;
    [Tooltip("Enable random environment exposures. Scaling of environment map intensity.")]
    public bool randomExposuresEnvironment = false;
    [Tooltip("Min exposure")]
    [Range(-5.0f, 20.0f)]
    public float minExposure = 6.0f;
    [Tooltip("Max exposure")]
    [Range(-5.0f, 20.0f)]
    public float maxExposure = 8.0f;
    

    [Header("Light sources Variations")]
    [Tooltip("Enable random spawn of light sources")]
    public bool lightsourceVariatons = false;
    [Tooltip("Number of light sources to spawn")]
    [Range(0, 10)]
    public int numLightsources = 2;

    [Tooltip("min exponent modifier for the light intensity of the spawned lights")]
    [Range(-10, 10)]
    public float minIntensityModifier = 0;
    [Tooltip("Max exponent modifier for the light intensity of the spawned lights")]
    [Range(-10, 10)]
    public float maxIntensityModifier = 0;

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
    [Tooltip("Min radius in mm")]
    [Range(0.0f, 2000.0f)]
    public float minRadiusLight = 500.0f;
    [Tooltip("Max radius in mm")]
    [Range(0.0f, 2000.0f)]
    public float maxRadiusLight = 500.0f;
    [Space(15)]
    [Tooltip("Change projector texture.")]
    public bool applyProjectorVariations = false;
    [Tooltip("Path to projector textures (relative to Resources dir)")]
    public string projectorTexturePath;
}
