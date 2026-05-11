using System;
using System.Collections;
using UnityEngine;
using MyResourceManager = Assets.Scripts.io.MyResourceManager;


[RequireComponent(typeof(DefectTextureCCLHandler))]
[AddComponentMenu("Cad2Render/MaterialRandomizers/ReduceDefects")]
public class ReduceDefectHandler : MaterialRandomizerInterface
{
    //private RandomNumberGenerator rng;
    private ComputeShader ReduceDefectShader;
    public float reduceRation = 0.2f;
    public override int getPriority() { return -51; }//run after CCL


    public void Awake()
    {
        ReduceDefectShader = MyResourceManager.loadComputeShader("ReduceDefectShader");
    }

    public override void RandomizeSingleMaterial(MaterialTextures textures, ref RandomNumberGenerator rng)
    {
        if (textures.rend.material.shader.name != "HDRP/LayeredLit")
        {
            Debug.LogWarning("ReduceDefectHandler: only works with Layered Lit material.\nReduceDefectHandler is skipped.");
            return;
        }

        int kernelHandle = ReduceDefectShader.FindKernel("CSMain");

        //var layerMaskMap = textures.ensureExistence(MaterialTextures.MapTypes.layerMask, Color.white);
        //ReduceDefectShader.SetTexture(kernelHandle, "LayerMaskInOut", layerMaskMap);
        //ReduceDefectShader.SetInt("layerCount", textures.GetCurrentLinkedInt("_LayerCount"));

        var defectMap = textures.ensureExistence(MaterialTextures.MapTypes.defectMap, textures.falseColor != null ? textures.falseColor.falseColor : Color.black);
        ReduceDefectShader.SetTexture(kernelHandle, "DefectMapInOut", defectMap);

        ReduceDefectShader.SetFloat("reduceRatio", reduceRation);

        ReduceDefectShader.Dispatch(kernelHandle, textures.resolution.x / 8, textures.resolution.y / 8, 1);
    }
}