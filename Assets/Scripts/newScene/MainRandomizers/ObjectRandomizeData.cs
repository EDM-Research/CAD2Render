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

//[HelpURL("Documentation/DatasetInformation.html")] // TODO
[CreateAssetMenu(fileName = "Untitled Dataset", menuName = "Cad2Render/New Object randomize Data", order = 2)]
public class ObjectRandomizeData : ScriptableObject
{
    [Header("Input/output paths")]
    [Tooltip("Path to prefabs of models (relative to Resources dir)")]
    public string modelsPath;

    public enum BopImportType { NoImport, ModelAndPose, ModelOnly }
    [Header("Model Variations")]
    [Tooltip("Use the bop import file to spawn objects")]
    public BopImportType importFromBOP = BopImportType.NoImport;
    [Tooltip("Generate one model per unique prefab in modelsPath")]
    public bool uniqueObjects = false;
    [Tooltip("Number of random objects to create. Multiple objects of same class are possible.")]
    public int numRandomObjects = 3;
    [Tooltip("Enable random translations of models in POI")]
    public bool randomModelTranslations = true;
    [Tooltip("Enable random rotations of models in POI")]
    public bool randomModelRotations = true;
    [Tooltip("Enable random rotations offset around y axis")]
    public bool randomRotationOffset = false;
    [Tooltip("max degree of rotations around y axis, in both positive and negative direction")]
    [Range(0, 180)]
    public float randomRotationOffsetValue = 0.0f;
    [Tooltip("Axis around the random rotation is done.")]
    public Vector3 randomRotationAxis = Vector3.forward;

    [Space(10)]
    [Tooltip("Handles all submodels seperatly as if they where there own prefab. (keeps undelying structure)")]
    public bool seperateSubmodels = false;
    [Tooltip("Enable random translations of submodel in prefab")]
    public bool randomSubModelTranslation = false;
    [Tooltip("Avoid Collisions")]
    public bool avoidCollisions = false;
    [Tooltip("Name of submodel in prefab to randomly translate")]
    public string subModelName = "";
    [Tooltip("Random submodel offset per axis")]
    public Vector3 subModelOffset = new Vector3(0, 0, 0);
}
