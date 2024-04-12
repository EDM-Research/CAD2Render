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
[CreateAssetMenu(fileName = "Untitled Dataset", menuName = "Cad2Render/New Material randomize Data", order = 2)]
public class MaterialRandomizeData : ScriptableObject
{
    [Tooltip("The resolution that generated texture wil have. when 0 it wil either default to 2048 or take the same resolution as the first input texture used for texture generation")]
    public Vector2Int generatedTextureResolution = new Vector2Int(0, 0);
}
