using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[AddComponentMenu("Cad2Render/MaterialRandomizers/Convert to LayeredLit")]
public class ConvertToLayeredLit : MaterialRandomizerInterface
{
    Shader layeredShader;

    public void Start()
    {
        layeredShader = Shader.Find("HDRP/LayeredLit");
    }


    public override int getPriority() { return 99; } //right after the materialModelRandomizer
    public override void RandomizeSingleMaterial(MaterialTextures textures, ref RandomNumberGenerator rng)
    {
        if (textures.rend.material.shader.name == "HDRP/LayeredLit")
            return;

        Material layered = new Material(layeredShader);
         
        // Copy Lit Layer 0
        layered.SetColor("_BaseColor0", textures.GetCurrentLinkedColor("_BaseColor"));
        layered.SetTexture("_BaseColorMap0", textures.GetCurrentLinkedTexture("_BaseColorMap"));
        layered.SetVector("_BaseColorMap_ST0", textures.GetCurrentLinkedVector("_BaseColorMap_ST"));

        layered.SetTexture("_NormalMap0", textures.GetCurrentLinkedTexture("_NormalMap"));
        layered.SetFloat("_NormalScale0", textures.GetCurrentLinkedFloat("_NormalScale"));
        layered.SetTexture("_BentNormalMap0", textures.GetCurrentLinkedTexture("_BentNormalMap"));

        layered.SetTexture("_MaskMap0", textures.GetCurrentLinkedTexture("_MaskMap"));
        layered.SetFloat("_Smoothness0", textures.GetCurrentLinkedFloat("_Smoothness"));
        layered.SetFloat("_Metallic0", textures.GetCurrentLinkedFloat("_Metallic"));

        // Add Layer 1
        layered.SetInt("_LayerCount", 2);
        HDMaterial.ValidateMaterial(layered);
        textures.rend.material = layered;
    }
}