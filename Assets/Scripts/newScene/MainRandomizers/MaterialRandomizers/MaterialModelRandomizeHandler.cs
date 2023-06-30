using System.Collections;
using UnityEngine;
using ResourceManager = Assets.Scripts.io.ResourceManager;

public class MaterialModelRandomizeHandler : MaterialRandomizerInterface
{
    //private RandomNumberGenerator rng;
    public MaterialModelRandomizeData dataset;

    private Material[] materials = new Material[0];
    public void Awake()
    {
        materials = ResourceManager.LoadAll<Material>(dataset.materialsPath);

        if (materials.Length == 0)
            Debug.LogWarning("No materials found in " + dataset.materialsPath);
    }

    public override void RandomizeSingleMaterial(MaterialTextures textures, ref RandomNumberGenerator rng, BOPDatasetExporter.SceneIterator bopSceneIterator = null)
    {
        if (materials.Length == 0)
            return;
        var temp = textures.rend.materials;
        temp[textures.materialIndex] = materials[rng.IntRange(0, materials.Length)];
        textures.rend.materials = temp;
    }

    public override ScriptableObject getDataset()
    {
        return dataset;
    }
}