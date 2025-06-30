//Copyright (c) 2020 Nick Michiels <nick.michiels@uhasselt.be>, Hasselt University, Belgium, All rights reserved.

using SneakySquirrelLabs.MinMaxRangeAttribute;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

//[HelpURL("Documentation/DatasetInformation.html")] // TODO
[CreateAssetMenu(fileName = "Untitled Dataset", menuName = "Cad2Render/Material randomizer Data/New Rust Generation data")]
public class RustGenerationData : ScriptableObject
{
    [Header("Rust generation settings")]
    //[Range(0.0f, 1.0f)]
    [MinMaxRange(0, 1,3)]
    [Tooltip("Amount of rust to be applied")]
    public Vector2 rustCoeficient = new Vector2(0.39f, 1.0f);
    [Tooltip("determines the size of rust spots")]
    [Range(0.0151f, 0.1f)]
    public float rustMaskZoom = 0.009f;
    [Tooltip("determines the size of color variations in rust spots")]
    [Range(0.0021f, 0.49f)]
    public float rustPaternZoom = 0.39f;

    [Tooltip("determines the detail in the rust mask and patern, comes with computational cost.")]
    [Range(1, 10)]
    public uint nrOfOctaves = 5;
    public Texture RustCreationZoneTexture;

    public Boolean changeColor = true;
    public Color rustColor1 = new Color(133.0f / 255, 60.0f / 255, 42.0f / 255, 1);
    public Color rustColor2 = new Color(65.0f / 255, 33.0f / 255, 15.0f / 255, 1);

    public Boolean changeMaskMap = true;
    public Boolean changeNormalMap = true;
}
