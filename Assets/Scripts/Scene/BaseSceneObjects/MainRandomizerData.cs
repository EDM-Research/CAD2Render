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
[CreateAssetMenu(fileName = "Untitled Dataset", menuName = "Cad2Render/New Main Dataset", order = 2)]
public class MainRandomizerData : ScriptableObject {
    //public enum Parametersource { Forced, AutoDetect, TextFile }
    //public DatasetInformation.Parametersource parametersource = DatasetInformation.Parametersource.Forced;

    [Tooltip("Location of the bop file to import.")]
    public string BOPInputPath = "";

    [Header("Render settings")]
    [Tooltip("Change the default render settings if set.")]
    public VolumeProfile renderProfile = null;
    [Tooltip("Change the default raytracing settings if set.")]
    public VolumeProfile rayTracingProfile = null;
    [Tooltip("Change the default post procesing settings if set.")]
    public VolumeProfile postProcesingProfile = null;
    [Tooltip("Enable auto exposure of camera. Avoids too bright or too dark images.")]
    public bool autoCameraExposure = false;

    [Header("Generation settings")]
    [Tooltip("Seed for random number generator.")]
    public int seed = 42;
    [Tooltip("Update the randomizers on diferent intervals.")]
    public bool separateUpdates = false;
    public enum RandomizerTypes {Default, View, Object, Light, Material }
    [System.Serializable]
    public class RandomizerUpdateIntervals
    {
        public RandomizerTypes randomizerType;
        [Tooltip("Interval to update the randomizer. <=1 means every frame, 2 means every 2 frames, etc.")]
        public uint interval = 1;
        [Tooltip("Offset the update of the randomizer.")]
        public uint offset = 0;
    }
    [Tooltip("Intervals to update the randomizers.")]
    public RandomizerUpdateIntervals[] updateIntervals = new RandomizerUpdateIntervals[0];//a dictionary would be better but the unity editor doesnt support this


}
