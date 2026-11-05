using UnityEngine;

[CreateAssetMenu(fileName = "Untitled Dataset", menuName = "Cad2Render/Light settings/New Environment randomize Data", order = 6)]
public class EnvironmentRandomizeData : ScriptableObject
{
    [Header("Input paths")]
    [Tooltip("Path to environment maps (relative to Resources dir)")]
    public string environmentsPath;

    [Header("Environment Variations")]
    [Tooltip("Pick a random environment map. When disabled, the maps are used in order.")]
    public bool pickRandomEnvironment = true;
    [Tooltip("Enable random environment map rotation.")]
    public bool randomEnvironmentRotations = false;
    [Tooltip("Min environment angle")]
    [Range(0.0f, 360.0f)]
    public float minEnvironmentAngle = 0.0f;
    [Tooltip("Max environment angle")]
    [Range(0.0f, 360.0f)]
    public float maxEnvironmentAngle = 360.0f;
    [Tooltip("Enable random environment and camera exposures.")]
    public bool randomExposuresEnvironment = false;
    [Tooltip("Min camera exposure")]
    [Range(-5.0f, 20.0f)]
    public float minCameraExposure = 6.0f;
    [Tooltip("Max camera exposure")]
    [Range(-5.0f, 20.0f)]
    public float maxCameraExposure = 8.0f;
    [Tooltip("Min environment exposure")]
    [Range(-5.0f, 20.0f)]
    public float minEnvironmentExposure = 6.0f;
    [Tooltip("Max environment exposure")]
    [Range(-5.0f, 20.0f)]
    public float maxEnvironmentExposure = 8.0f;
}
