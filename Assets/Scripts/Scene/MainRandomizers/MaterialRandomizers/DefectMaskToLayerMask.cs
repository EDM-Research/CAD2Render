using Assets.Scripts.io;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[AddComponentMenu("Cad2Render/MaterialRandomizers/DefectMask To LayerMask")]
public class DefectMaskToLayerMask : MaterialRandomizerInterface
{

    private ComputeShader DefectToLayerMaskShader;

    public override int getPriority() { return -25; }//run after defect generation but before CCL


    public void Awake()
    {
        DefectToLayerMaskShader = MyResourceManager.loadComputeShader("DefectToLayerMask");
    }

    public override void RandomizeSingleMaterial(MaterialTextures textures, ref RandomNumberGenerator rng)
    {
        int kernelHandle = DefectToLayerMaskShader.FindKernel("CSMain");

        var layerMaskMap = textures.ensureExistence(MaterialTextures.MapTypes.layerMask, Color.white);
        DefectToLayerMaskShader.SetTexture(kernelHandle, "LayerMaskInOut", layerMaskMap);

        var defectMap = textures.ensureExistence(MaterialTextures.MapTypes.defectMap, textures.falseColor != null ? textures.falseColor.falseColor : Color.black);
        DefectToLayerMaskShader.SetTexture(kernelHandle, "DefectMaskIn", defectMap);

        DefectToLayerMaskShader.SetInt("layerCount", textures.GetCurrentLinkedInt("_LayerCount"));

        DefectToLayerMaskShader.Dispatch(kernelHandle, textures.resolution.x / 8, textures.resolution.y / 8, 1);
    }
}