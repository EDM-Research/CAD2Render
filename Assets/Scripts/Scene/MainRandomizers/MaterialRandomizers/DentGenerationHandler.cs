using System;
using System.Collections;
using UnityEngine;
using ResourceManager = Assets.Scripts.io.ResourceManager;


[AddComponentMenu("Cad2Render/MaterialRandomizers/Dent generation")]
public class DentGenerationHandler : MaterialRandomizerInterface
{
    public DentGenerationData dataset;
    [InspectorButton("TriggerCloneClicked")]
    public bool clone;
    private void TriggerCloneClicked()
    {
        RandomizerInterface.CloneDataset(ref dataset);
    }

    //private RenderTexture RustZoneTexture;
    private ComputeShader DentedNormalGenerator;


    public void Awake()
    {
        DentedNormalGenerator = ResourceManager.loadShader("DentedNormalGenerator");
    }

    public override void RandomizeSingleMaterial(MaterialTextures textures, ref RandomNumberGenerator rng)
    {
        if (!textures.rend.material.IsKeywordEnabled("_NORMALMAP"))
            textures.rend.material.EnableKeyword("_NORMALMAP");
        int kernelHandle = DentedNormalGenerator.FindKernel("CSMain");
        DentedNormalGenerator.SetInt("randSeed", rng.IntRange(128, Int32.MaxValue));

        var normalMap = textures.GetCurrentLinkedTexture("_NormalMap");
        textures.set(MaterialTextures.MapTypes.normalMap, normalMap, new Color((float)Math.Sqrt(0.5), 0.5f, (float)Math.Sqrt(0.5), 0.5f));
        DentedNormalGenerator.SetTexture(kernelHandle, "NormalMapInOut", textures.get(MaterialTextures.MapTypes.normalMap));

        textures.set(MaterialTextures.MapTypes.colorMap, textures.GetCurrentLinkedTexture("_BaseColorMap"), textures.GetCurrentLinkedColor("_Color"));
        DentedNormalGenerator.SetTexture(kernelHandle, "ColorMapInOut", textures.get(MaterialTextures.MapTypes.colorMap));

        DentedNormalGenerator.SetInt("useNormalMapInput", 1);
        DentedNormalGenerator.SetFloat("dentStrength", dataset.dentStrength);
        DentedNormalGenerator.SetFloat("dentSize", dataset.dentSize);

        //execute shader
        DentedNormalGenerator.Dispatch(kernelHandle, textures.resolutionX / 8, textures.resolutionY / 8, 1);

        textures.get(MaterialTextures.MapTypes.colorMap).wrapMode = TextureWrapMode.Repeat;
        textures.linkTexture(MaterialTextures.MapTypes.colorMap);
        textures.linkTexture(MaterialTextures.MapTypes.normalMap);


    }

    //public override ScriptableObject getDataset()
    //{
    //    return dataset;
    //}
}