
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
    [Tooltip("Path to background images (relative to Resources dir)")]
    public string backgroundImagePath;

    public bool randomizeRotation = true;
    [Tooltip("set default rotation angle")]
    [Range(0.0f, 360.0f)]
    public float offsetRotationAngle = 0.0f;
    [Tooltip("set variation of rotation angle")]
    [Range(0.0f, 180.0f)]
    public float rotationAngle = 180.0f;

    [Tooltip("set to change the color of the background image")]
    public HSVOffsetData hsvOffsetData = null;
}