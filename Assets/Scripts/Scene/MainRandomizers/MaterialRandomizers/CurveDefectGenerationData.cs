//Copyright (c) 2020 Nick Michiels <nick.michiels@uhasselt.be>, Hasselt University, Belgium, All rights reserved.

using SneakySquirrelLabs.MinMaxRangeAttribute;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

//[HelpURL("Documentation/DatasetInformation.html")] // TODO
[CreateAssetMenu(fileName = "Untitled Dataset", menuName = "Cad2Render/Material randomizer Data/New Curve Defect Generation data")]
public class CurveDefectGenerationData : ScriptableObject
{
    [Header("Rust generation settings")]
    //[Range(0.0f, 1.0f)]
    [MinMaxRange(0, 0.01f,3)]
    [Tooltip("")]
    public Vector2 defectWidth = new Vector2(0.0039f, 0.005f);
    [MinMaxRange(0, 0.5f, 3)]
    [Tooltip("")]
    public Vector2 defectLength = new Vector2(0.01f, 0.05f);
    [Tooltip("determines the tresshold for the noise map to consider an area to be a defect.")]
    [Range(0.0f, 1.0f)]
    public float defectCutoff = 0.4f;

    [Space(10)]
    public Boolean changeColor = true;
    public Color rustColor1 = new Color(133.0f / 255, 60.0f / 255, 42.0f / 255, 1);
    public Color rustColor2 = new Color(65.0f / 255, 33.0f / 255, 15.0f / 255, 1);

    [Space(10)]
    public Boolean changeMaskMap = true;
    [Range(-1.0f, 1.0f)]
    public float metalicnessOffset = -0.4f;

    [Space(10)]
    public Boolean changeNormalMap = true;
    [Range(-1.0f, 1.0f)]
    public float dentModifier = 1.0f;
    [Tooltip("Sharpness transition between clean and rusty surface.")]
    public float sharpness = 200.0f;
}
