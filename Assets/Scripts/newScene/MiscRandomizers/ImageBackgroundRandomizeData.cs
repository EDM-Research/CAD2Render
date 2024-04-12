
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[HelpURL("Documentation/DatasetInformation.html")] // TODO
[CreateAssetMenu(fileName = "Untitled Dataset", menuName = "Cad2Render/New background image randomize Data", order = 2)]
public class ImageBackgroundRandomizeData : ScriptableObject
{
    [Header("Image background Variations")]
    [Tooltip("Enable image background variations")]
    public bool ImageBackgroundVariatons = false;
    [Tooltip("Path to background images (relative to Resources dir)")]
    public string backgroundImagePath;

    public bool randomizeRotation = true;
    [Tooltip("Min rotation angle")]
    [Range(0.0f, 360.0f)]
    public float minRotationAngle = 0.0f;
    [Tooltip("Max rotation angle")]
    [Range(0.0f, 360.0f)]
    public float maxRotationAngle = 360.0f;

}