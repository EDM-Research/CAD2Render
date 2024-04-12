using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Untitled Dataset", menuName = "Cad2Render/Material randomizer Data/New dent generation data")]
public class DentGenerationData : ScriptableObject
{
    [Tooltip("Size of the dent")]
    [Range(0.001f, 0.9f)]
    public float dentSize = 0.2f;
    [Tooltip("Strength of the dent")]
    [Range(0.0f, 5.0f)]
    public float dentStrength = 1.0f;
}
