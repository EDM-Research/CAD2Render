using System;
using System.Collections;
using UnityEngine;
using ResourceManager = Assets.Scripts.io.ResourceManager;


[AddComponentMenu("Cad2Render/MaterialRandomizers/Scratch Generation")]
public class ScratchGenerationHandler : MaterialRandomizerInterface
{
    //public DentGenerationData dataset;
    //[InspectorButton("TriggerCloneClicked")]
    //public bool clone;
    //private void TriggerCloneClicked()
    //{
    //    RandomizerInterface.CloneDataset(ref dataset);
    //}

    //private RenderTexture RustZoneTexture;
    private ComputeShader ScratchGenerator;


    public void Awake()
    {
        ScratchGenerator = ResourceManager.loadShader("ScratchGenerator");
    }

    public override void RandomizeSingleMaterial(MaterialTextures textures, ref RandomNumberGenerator rng)
    {
        if (!textures.rend.material.IsKeywordEnabled("_NORMALMAP"))
            textures.rend.material.EnableKeyword("_NORMALMAP");
        int kernelHandle = ScratchGenerator.FindKernel("CSMain");
        ScratchGenerator.SetInt("randSeed", rng.IntRange(128, Int32.MaxValue));

        var normalMap = textures.GetCurrentLinkedTexture("_NormalMap");
        textures.set(MaterialTextures.MapTypes.normalMap, normalMap, new Color((float)Math.Sqrt(0.5), 0.5f, (float)Math.Sqrt(0.5), 0.5f));
        ScratchGenerator.SetTexture(kernelHandle, "NormalMapInOut", textures.get(MaterialTextures.MapTypes.normalMap));
        
        textures.set(MaterialTextures.MapTypes.defectMap, textures.get(MaterialTextures.MapTypes.defectMap), textures.falseColor.falseColor);
        ScratchGenerator.SetTexture(kernelHandle, "DefectMapInOut", textures.get(MaterialTextures.MapTypes.defectMap));


        ScratchGenerator.SetInt("nrScratches", 20);
        ScratchGenerator.SetFloat("scratchWidth", 2.5f);
        ScratchGenerator.SetInt("nrAASamples", 8);

        //execute shader
        ScratchGenerator.Dispatch(kernelHandle, textures.resolutionX / 8, textures.resolutionY / 8, 1);

        textures.linkTexture(MaterialTextures.MapTypes.normalMap);
        textures.linkTexture(MaterialTextures.MapTypes.defectMap);
    }

    //public override ScriptableObject getDataset()
    //{
    //    return dataset;
    //}
}