using System.Collections;
using UnityEngine;


public class TextureResamplerHandler : MaterialRandomizerInterface
{
    //private RandomNumberGenerator rng;
    private TextureResampler texResampler;
    public TextureResamplerData dataset;

    public void Awake()
    {
        texResampler = new TextureResampler(dataset);
    }

    public override void RandomizeSingleMaterial(MaterialTextures textures, ref RandomNumberGenerator rng, BOPDatasetExporter.SceneIterator bopSceneIterator = null)
    {
        bool first = true;
        foreach (MaterialTextures.MapTypes type in dataset.resampleTextures)
        {
            if (first)
                texResampler.ResampleTexture(textures, textures.GetCurrentLinkedTexture(textures.getTextureName(type)), type, ref rng);
            else
                texResampler.applyPreviousResample(textures, type);
            first = false;
            textures.linkTexture(type);
        }
    }

    public override ScriptableObject getDataset()
    {
        return dataset;
    }
}