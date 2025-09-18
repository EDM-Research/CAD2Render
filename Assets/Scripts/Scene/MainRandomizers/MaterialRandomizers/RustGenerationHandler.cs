using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using ResourceManager = Assets.Scripts.io.ResourceManager;


[AddComponentMenu("Cad2Render/MaterialRandomizers/Rust generation")]
public class RustGenerationHandler : MaterialRandomizerInterface
{
    //private RandomNumberGenerator rng;
    public RustGenerationData dataset;
    [InspectorButton("TriggerCloneClicked")]
    public bool clone;

    private RenderTexture RustZoneTexture;
    private ComputeShader rustmapGenerationShader;
    private LocalKeyword changeMaskmap;
    private LocalKeyword changeNormalMap;
    private LocalKeyword changeColor;

    private void TriggerCloneClicked()
    {
        RandomizerInterface.CloneDataset(ref dataset);
    }

    public void Awake()
    {
        rustmapGenerationShader = ResourceManager.loadShader("rustMapGenerator");
        changeNormalMap = new LocalKeyword(rustmapGenerationShader, "changeNormalMap");
        changeMaskmap = new LocalKeyword(rustmapGenerationShader, "changeMaskMap");
        changeColor = new LocalKeyword(rustmapGenerationShader, "changeColor");
    }


    public override void RandomizeSingleMaterial(MaterialTextures textures, ref RandomNumberGenerator rng)
    {
        int kernelHandle = rustmapGenerationShader.FindKernel("CSMain");
        rustmapGenerationShader.SetInt("randSeed", rng.IntRange(128, Int32.MaxValue));
        rustmapGenerationShader.SetFloat("sharpness", dataset.sharpness);

        textures.set(MaterialTextures.MapTypes.maskMap, textures.GetCurrentLinkedTexture(MaterialTextures.MapTypes.maskMap), new Color(textures.GetCurrentLinkedFloat("_Metallic"), 1, 0,
                                                                                                                   textures.GetCurrentLinkedFloat("_Smoothness")));
        if (dataset.changeColor)
        {
            rustmapGenerationShader.EnableKeyword(changeColor);
            textures.set(MaterialTextures.MapTypes.colorMap, textures.GetCurrentLinkedTexture(MaterialTextures.MapTypes.colorMap), textures.GetCurrentLinkedColor("_Color"));
            rustmapGenerationShader.SetTexture(kernelHandle, "ColorMapInOut", textures.get(MaterialTextures.MapTypes.colorMap));
            rustmapGenerationShader.SetVector("colorRust1", dataset.rustColor1);
            rustmapGenerationShader.SetVector("colorRust2", dataset.rustColor2);
        }
        else
            rustmapGenerationShader.DisableKeyword(changeColor);

        if (dataset.changeMaskMap)
        {
            rustmapGenerationShader.EnableKeyword(changeMaskmap);
            rustmapGenerationShader.SetTexture(kernelHandle, "MaskMapInOut", textures.get(MaterialTextures.MapTypes.maskMap));
            rustmapGenerationShader.SetFloat("metalicnessOffset", dataset.metalicnessOffset);
        }
        else
            rustmapGenerationShader.DisableKeyword(changeMaskmap);

        if (dataset.changeNormalMap) { 
            rustmapGenerationShader.EnableKeyword(changeNormalMap);
            textures.set(MaterialTextures.MapTypes.normalMap, textures.GetCurrentLinkedTexture(MaterialTextures.MapTypes.normalMap) , new Color(0.5f, 0.5f, 1.0f, 1.0f));
            rustmapGenerationShader.SetTexture(kernelHandle, "NormalMapInOut", textures.get(MaterialTextures.MapTypes.normalMap));
            rustmapGenerationShader.SetFloat("dentModifier", dataset.dentModifier);
        }
        else
            rustmapGenerationShader.DisableKeyword(changeNormalMap);
        

        textures.set(MaterialTextures.MapTypes.defectMap, textures.get(MaterialTextures.MapTypes.defectMap), textures.falseColor.falseColor);
        rustmapGenerationShader.SetTexture(kernelHandle, "DefectMapInOut", textures.get(MaterialTextures.MapTypes.defectMap));
        updateRustZoneTexture(textures.resolutionX, textures.resolutionY);
        rustmapGenerationShader.SetTexture(kernelHandle, "rustMask", RustZoneTexture);


        rustmapGenerationShader.SetFloat("maskZoom", dataset.rustMaskZoom / textures.resolutionX * 100);
        rustmapGenerationShader.SetFloat("rustPaternZoom", dataset.rustPaternZoom / textures.resolutionY * 100);
        rustmapGenerationShader.SetFloat("xSkew", dataset.xSkew);
        rustmapGenerationShader.SetFloat("rustCoMin", dataset.rustCoeficient.x);
        rustmapGenerationShader.SetFloat("rustCoMax", dataset.rustCoeficient.y);
        rustmapGenerationShader.SetInt("nrOfOctaves", (int)dataset.nrOfOctaves);

        //execute shader
        rustmapGenerationShader.Dispatch(kernelHandle, textures.resolutionX / 8, textures.resolutionY / 8, 1);

        //set new calculated values
        textures.linkTexture(MaterialTextures.MapTypes.colorMap);
        textures.linkTexture(MaterialTextures.MapTypes.normalMap);
        textures.linkTexture(MaterialTextures.MapTypes.maskMap);
        textures.linkTexture(MaterialTextures.MapTypes.defectMap);
        if (dataset.changeNormalMap)
            textures.linkTexture(MaterialTextures.MapTypes.normalMap);

    }
    private void updateRustZoneTexture(int resolutionX, int resolutionY)
    {
        if (RustZoneTexture == null || RustZoneTexture.width != resolutionX || RustZoneTexture.height != resolutionY)
        {
            if (RustZoneTexture != null)
                RustZoneTexture.Release();
            RustZoneTexture = new RenderTexture(resolutionX, resolutionY, 0);
            RustZoneTexture.Create();

            if (dataset.RustCreationZoneTexture == null)
            {
                //if no LineAndRustMask was provided create a default mask (no lines, rust everywhere)
                RenderTexture rt = RenderTexture.active;
                RenderTexture.active = RustZoneTexture;
                GL.Clear(true, true, new Color(0, 0, 1)); //red = not used, green = not used, blue = rust zones (are being multilpied with the rust coeficient)
                RenderTexture.active = rt;
            }
            else
                Graphics.Blit(dataset.RustCreationZoneTexture, RustZoneTexture);
        }
    }

    public override ScriptableObject getDataset()
    {
        return dataset;
    }
}