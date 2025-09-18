using System.Collections;
using UnityEngine;


[AddComponentMenu("Cad2Render/MaterialRandomizers/Texture Resampler")]
public class TextureResamplerHandler : MaterialRandomizerInterface
{
    //private RandomNumberGenerator rng;
    private TextureResampler texResampler;
    public TextureResamplerData dataset;
    [InspectorButton("TriggerCloneClicked")]
    public bool clone;
    private void TriggerCloneClicked()
    {
        RandomizerInterface.CloneDataset(ref dataset);
    }

    public void Awake()
    {
        texResampler = new TextureResampler(dataset);
    }

    public override void RandomizeSingleMaterial(MaterialTextures textures, ref RandomNumberGenerator rng)
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