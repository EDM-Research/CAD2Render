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
    [Header("Defect generation settings")]
    public int nrOfDefects = 10;
    [MinMaxRange(1.0f, 10.0f, 3)]
    [Tooltip("")]
    public Vector2 defectLength = new Vector2(2.0f, 5.0f);
    //[Range(0.0f, 1.0f)]
    [MinMaxRange(0.01f, 1.0f,3)]
    [Tooltip("")]
    public Vector2 defectWidth = new Vector2(0.03f, 0.05f);
    [MinMaxRange(-180, 180, 0)]
    public Vector2 defectAngle = new Vector2(-180, 180);
    [MinMaxRange(-2, 2, 2)]
    public Vector2 controlPointOffset = new Vector2(-1, 1);
    [Tooltip("Modifiers the defect width used for anotating the defect.")]
    [Range(0.5f, 5.0f)]
    public float defectAnnotationModifier = 1.0f;

    [Space(10)]
    public Boolean changeColor = true;
    public Color defectColor1 = new Color(133.0f / 255, 60.0f / 255, 42.0f / 255, 1);
    public Color defectColor2 = new Color(65.0f / 255, 33.0f / 255, 15.0f / 255, 1);

    [Space(10)]
    public Boolean changeMaskMap = true;
    [Range(-1.0f, 1.0f)]
    public float metalicnessOffset = -0.4f;

    [Space(10)]
    public Boolean changeNormalMap = true;
    [Tooltip("Modifiers the streghth with which the normal map is adjusted.")]
    [Range(-1.0f, 1.0f)]
    public float dentModifier = 1.0f;
    [Tooltip("Sharpness transition between defect and non defect surface.")]
    public float sharpness1 = 200.0f;
    public float sharpness2 = 200.0f;
}
