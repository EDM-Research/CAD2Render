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

        var layerMaskMap = textures.GetCurrentLinkedTexture("_LayerMaskMap");
        textures.set(MaterialTextures.MapTypes.layerMask, layerMaskMap, Color.white);
        DefectToLayerMaskShader.SetTexture(kernelHandle, "LayerMaskInOut", textures.get(MaterialTextures.MapTypes.layerMask));

        textures.set(MaterialTextures.MapTypes.defectMap, textures.get(MaterialTextures.MapTypes.defectMap), textures.falseColor != null ? textures.falseColor.falseColor : Color.black);
        DefectToLayerMaskShader.SetTexture(kernelHandle, "DefectMaskIn", textures.get(MaterialTextures.MapTypes.defectMap));

        DefectToLayerMaskShader.SetInt("layerCount", textures.GetCurrentLinkedInt("_LayerCount"));

        DefectToLayerMaskShader.Dispatch(kernelHandle, textures.resolution.x / 8, textures.resolution.y / 8, 1);

        textures.linkTexture(MaterialTextures.MapTypes.layerMask);
    }
}