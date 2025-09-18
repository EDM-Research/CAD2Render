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
[CreateAssetMenu(fileName = "Untitled Dataset", menuName = "Cad2Render/Material randomizer Data/New HSV offset data")]
public class HSVOffsetData : ScriptableObject
{
    [Tooltip("Random Hue offset")]
    [Range(0.0f, 360.0f)]
    public float H_maxOffset = 0.0f;
    [Tooltip("Random Saturation offset")]
    [Range(0.0f, 100.0f)]
    public float S_maxOffset = 0.0f;
    [Tooltip("Random Value offset")]
    [Range(0.0f, 100.0f)]
    public float V_maxOffset = 0.0f;
}
