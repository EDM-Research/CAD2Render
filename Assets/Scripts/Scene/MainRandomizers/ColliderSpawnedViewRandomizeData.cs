// Generated with Claude

//Copyright (c) 2020 Nick Michiels <nick.michiels@uhasselt.be>, Hasselt University, Belgium, All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[HelpURL("Documentation/DatasetInformation.html")]
[CreateAssetMenu(fileName = "Untitled Dataset", menuName = "Cad2Render/Camera settings/New Collider Spawned View Randomize Data", order = 5)]
public class ColliderSpawnedViewRandomizeData : ScriptableObject
{
    [Header("Viewpoint Variations")]
    [Tooltip("Use the scene iterator (e.g. a bop import) to determine the camera pose (use a Camera Parameters Handler for the intrinsics)")]
    [UnityEngine.Serialization.FormerlySerializedAs("importFromBOP")]
    public bool importFromSceneIterator = false;
    [Tooltip("Min camera rotation X (pitch, degrees)")]
    [Range(-180.0f, 180.0f)]
    public float minRotationX = -45.0f;
    [Tooltip("Max camera rotation X (pitch, degrees)")]
    [Range(-180.0f, 180.0f)]
    public float maxRotationX = 45.0f;
    [Tooltip("Min camera rotation Y (yaw, degrees)")]
    [Range(-180.0f, 180.0f)]
    public float minRotationY = -180.0f;
    [Tooltip("Max camera rotation Y (yaw, degrees)")]
    [Range(-180.0f, 180.0f)]
    public float maxRotationY = 180.0f;
    [Tooltip("Min camera rotation Z (roll, degrees)")]
    [Range(-180.0f, 180.0f)]
    public float minRotationZ = -15.0f;
    [Tooltip("Max camera rotation Z (roll, degrees)")]
    [Range(-180.0f, 180.0f)]
    public float maxRotationZ = 15.0f;

    [Header("Camera Spawn Zone")]
    [Tooltip("Scale the X and Z dimensions of the spawn zone")]
    [Range(0.1f, 2.0f)]
    public float spawnZoneXZScale = 1.0f;
}
