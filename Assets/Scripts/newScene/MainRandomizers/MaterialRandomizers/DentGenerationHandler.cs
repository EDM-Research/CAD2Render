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
    private ComputeShader DentedNormalGenerationShader;


    public void Awake()
    {
        DentedNormalGenerationShader = ResourceManager.loadShader("DentedNormalGenerationShader");
    }

    public override void RandomizeSingleMaterial(MaterialTextures textures, ref RandomNumberGenerator rng, BOPDatasetExporter.SceneIterator bopSceneIterator = null)
    {
        if (!textures.rend.material.IsKeywordEnabled("_NORMALMAP"))
            textures.rend.material.EnableKeyword("_NORMALMAP");
        int kernelHandle = DentedNormalGenerationShader.FindKernel("CSMain");
        DentedNormalGenerationShader.SetInt("randSeed", rng.IntRange(128, Int32.MaxValue));

        var normalMap = textures.GetCurrentLinkedTexture("_NormalMap");
        textures.set(MaterialTextures.MapTypes.normalMap, normalMap, new Color((float)Math.Sqrt(0.5), 0.5f, (float)Math.Sqrt(0.5), 0.5f));
        DentedNormalGenerationShader.SetTexture(kernelHandle, "NormalMapInOut", textures.get(MaterialTextures.MapTypes.normalMap));

        textures.set(MaterialTextures.MapTypes.colorMap, textures.GetCurrentLinkedTexture("_BaseColorMap"), textures.GetCurrentLinkedColor("_Color"));
        DentedNormalGenerationShader.SetTexture(kernelHandle, "ColorMapInOut", textures.get(MaterialTextures.MapTypes.colorMap));

        DentedNormalGenerationShader.SetInt("useNormalMapInput", 1);
        DentedNormalGenerationShader.SetFloat("dentStrength", dataset.dentStrength);
        DentedNormalGenerationShader.SetFloat("dentSize", dataset.dentSize);

        //execute shader
        DentedNormalGenerationShader.Dispatch(kernelHandle, textures.resolutionX / 8, textures.resolutionY / 8, 1);

        textures.get(MaterialTextures.MapTypes.colorMap).wrapMode = TextureWrapMode.Repeat;
        textures.linkTexture(MaterialTextures.MapTypes.colorMap);
        textures.linkTexture(MaterialTextures.MapTypes.normalMap);


    }

    //public override ScriptableObject getDataset()
    //{
    //    return dataset;
    //}
}