using UnityEngine;

[CreateAssetMenu(fileName = "Untitled Dataset", menuName = "Cad2Render/Light settings/New Sphere light spawn Data", order = 6)]
public class SphereLightSpawnRandomizeData : LightSpawnRandomizeData
{
    [Header("Sphere placement (around the handler, lights point at its centre)")]
    [Tooltip("Min theta")]
    [Range(-90.0f, 90.0f)]
    public float minThetaLight = 10.0f;
    [Tooltip("Max theta")]
    [Range(-90.0f, 90.0f)]
    public float maxThetaLight = 89.0f;
    [Tooltip("Min phi")]
    [Range(-360.0f, 360.0f)]
    public float minPhiLight = -45.0f;
    [Tooltip("Max phi")]
    [Range(-360.0f, 360.0f)]
    public float maxPhiLight = 45.0f;
    [Tooltip("Min radius in mm")]
    [Range(0.0f, 2000.0f)]
    public float minRadiusLight = 500.0f;
    [Tooltip("Max radius in mm")]
    [Range(0.0f, 2000.0f)]
    public float maxRadiusLight = 500.0f;
}
