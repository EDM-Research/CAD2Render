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
[CreateAssetMenu(fileName = "Untitled Dataset", menuName = "Cad2Render/Material randomizer Data/New manufacturing lines data")]
public class ManufacturingLinesData : ScriptableObject
{
    [Header("Manufacturing lines generation settings")]
    [Range(0.001f, 0.2f)]
    public float lineSpacing = 0.08f;
    [Tooltip("Red: polishing lines area, Green: line color diference, Blue: rust coeficient")]
    public Texture LineCreationZoneTexture;
}
