using System;
using System.Collections.Generic;
using UnityEngine;

// Spawns lights from a prefab each randomization. Subclasses decide where each light goes.
public abstract class LightSpawnRandomizeHandler : RandomizerInterface
{
    protected abstract LightSpawnRandomizeData SpawnData { get; }

    // Set the position and rotation of a freshly spawned light.
    protected abstract void PlaceLight(Light lightSource, ref RandomNumberGenerator rng);

    private UnityEngine.Object[] projectorMaps = new UnityEngine.Object[0];
    private readonly List<Light> instantiatedLights = new List<Light>();

    protected virtual void Start()
    {
        randomizerType = MainRandomizerData.RandomizerTypes.Light;
        this.LinkGui();

        if (SpawnData != null && SpawnData.projectorTexturePath != "")
            projectorMaps = Resources.LoadAll(SpawnData.projectorTexturePath, typeof(Texture));
    }

    protected virtual void OnDestroy()
    {
        ClearLights();
    }

    public override ScriptableObject getDataset()
    {
        return SpawnData;
    }

    public override void Randomize(ref RandomNumberGenerator rng, SceneIteratorInterface sceneIterator = null)
    {
        ClearLights();

        LightSpawnRandomizeData data = SpawnData;
        if (data.lightSourcePrefab == null)
        {
            Debug.LogWarning("No light prefab set on " + name);
            return;
        }

        for (int i = 0; i < data.numLightsources; ++i)
        {
            Light lightSource = Instantiate(data.lightSourcePrefab);
            PlaceLight(lightSource, ref rng);
            instantiatedLights.Add(lightSource);

            lightSource.intensity *= (float)Math.Pow(2, rng.Range(data.minIntensityModifier, data.maxIntensityModifier));

            if (data.applyProjectorVariations && projectorMaps.Length > 0)
            {
                Color.RGBToHSV(lightSource.color, out _, out float s, out _);
                lightSource.cookie = (Texture)projectorMaps[rng.IntRange(0, projectorMaps.Length)];
                lightSource.color = Color.HSVToRGB(rng.Next(), s, 1.0f);
            }
        }
        resetFrameAccumulation();
    }

    private void ClearLights()
    {
        foreach (Light lightSource in instantiatedLights)
        {
            if (lightSource != null)
                Destroy(lightSource.gameObject);
        }
        instantiatedLights.Clear();
    }
}
