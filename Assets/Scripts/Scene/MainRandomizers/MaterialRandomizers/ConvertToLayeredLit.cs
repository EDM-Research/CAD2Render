using System.Drawing;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using static UnityEditor.ShaderUtil;

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
        layered.SetFloat("_DoubleSidedEnable", 1.0f);

        layered.SetColor("_BaseColor0", textures.GetCurrentLinkedColor("_BaseColor"));
        layered.SetColor("_BaseColor1", textures.GetCurrentLinkedColor("_BaseColor"));
        var baseColorMap = textures.GetCurrentLinkedTexture("_BaseColorMap");
        if (baseColorMap != null)
            baseColorMap.wrapMode = TextureWrapMode.Mirror;
        layered.SetTexture("_BaseColorMap0", baseColorMap);
        layered.SetTexture("_BaseColorMap1", baseColorMap);
        layered.SetVector("_BaseColorMap0_ST", textures.GetCurrentLinkedVector("_BaseColorMap_ST")); 
        layered.SetVector("_BaseColorMap1_ST", textures.GetCurrentLinkedVector("_BaseColorMap_ST"));

        layered.SetVector("_LayerMaskMap_ST", textures.GetCurrentLinkedVector("_BaseColorMap_ST"));

        var normalMap = textures.GetCurrentLinkedTexture("_NormalMap");
        //if (normalMap != null)
        //    normalMap.wrapMode = TextureWrapMode.Mirror;
        layered.SetTexture("_NormalMap0", normalMap);
        layered.SetTexture("_NormalMap1", normalMap);
        layered.SetFloat("_NormalScale0", textures.GetCurrentLinkedFloat("_NormalScale"));
        layered.SetFloat("_NormalScale1", textures.GetCurrentLinkedFloat("_NormalScale"));
        layered.SetTexture("_BentNormalMap0", textures.GetCurrentLinkedTexture("_BentNormalMap"));

        var maskMap = textures.GetCurrentLinkedTexture("_MaskMap");
        if (maskMap != null)
            maskMap.wrapMode = TextureWrapMode.Mirror;
        layered.SetTexture("_MaskMap0", maskMap);
        layered.SetTexture("_MaskMap1", maskMap);
        layered.SetFloat("_Smoothness0", textures.GetCurrentLinkedFloat("_Smoothness"));
        layered.SetFloat("_Smoothness1", textures.GetCurrentLinkedFloat("_Smoothness"));
        layered.SetFloat("_Metallic0", textures.GetCurrentLinkedFloat("_Metallic"));
        layered.SetFloat("_Metallic1", textures.GetCurrentLinkedFloat("_Metallic"));

        //make sure material randomizers that use [_BaseColor, _Smoothness, _Metallic] work with the layerelit shader
        textures.newProperties.SetColor("_BaseColor", textures.GetCurrentLinkedColor("_BaseColor"));
        textures.newProperties.SetFloat("_Smoothness", textures.GetCurrentLinkedFloat("_Smoothness"));
        textures.newProperties.SetFloat("_Metallic", textures.GetCurrentLinkedFloat("_Metallic"));

        // Add Layer 1
        layered.SetInt("_LayerCount", 2);
        HDMaterial.ValidateMaterial(layered);
        textures.rend.material = layered;


    }
}