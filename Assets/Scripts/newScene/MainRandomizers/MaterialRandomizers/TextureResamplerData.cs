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
[CreateAssetMenu(fileName = "Untitled Dataset", menuName = "Cad2Render/Material randomizer Data/New Texture resampler data")]
public class TextureResamplerData : ScriptableObject {
    [Header("Texture resampler settings (preview)")]
    public MaterialTextures.MapTypes[] resampleTextures = new MaterialTextures.MapTypes[0];
    [Range(1, 50)]
    public int nrResampleSamples = 9;
    [Range(5, 17)]
    public int nrResampleGenerations = 17;

    [Obsolete("Use the new modular material randomizers.")]
    public TextureResamplerData(MatRandomizeData dataset)
    {
        nrResampleSamples = dataset.nrResampleSamples;
        nrResampleGenerations = dataset.nrResampleGenerations;
    }
}
