using UnityEngine;

// Settings shared by all light spawners. Placement settings live in the subclasses.
public abstract class LightSpawnRandomizeData : ScriptableObject
{
    [Header("Light sources")]
    [Tooltip("Light prefab used to create light sources")]
    public Light lightSourcePrefab;
    [Tooltip("Number of light sources to spawn")]
    [Range(0, 10)]
    public int numLightsources = 2;

    [Tooltip("Min exponent modifier for the light intensity of the spawned lights")]
    [Range(-10, 10)]
    public float minIntensityModifier = 0;
    [Tooltip("Max exponent modifier for the light intensity of the spawned lights")]
    [Range(-10, 10)]
    public float maxIntensityModifier = 0;

    [Header("Projector")]
    [Tooltip("Give each light a random projector texture and hue.")]
    public bool applyProjectorVariations = false;
    [Tooltip("Path to projector textures (relative to Resources dir)")]
    public string projectorTexturePath;
}
