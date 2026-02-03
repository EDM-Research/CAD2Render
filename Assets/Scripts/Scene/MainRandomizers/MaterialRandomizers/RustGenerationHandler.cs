using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using MyResourceManager = Assets.Scripts.io.MyResourceManager;


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
        rustmapGenerationShader = MyResourceManager.loadComputeShader("rustMapGenerator");
        changeNormalMap = new LocalKeyword(rustmapGenerationShader, "changeNormalMap");
        changeMaskmap = new LocalKeyword(rustmapGenerationShader, "changeMaskMap");
        changeColor = new LocalKeyword(rustmapGenerationShader, "changeColor");
    }


    public override void RandomizeSingleMaterial(MaterialTextures textures, ref RandomNumberGenerator rng)
    {
        int kernelHandle = rustmapGenerationShader.FindKernel("CSMain");
        rustmapGenerationShader.SetInt("randSeed", rng.IntRange(128, Int32.MaxValue));
        rustmapGenerationShader.SetFloat("sharpness", dataset.sharpness);
        
        if (dataset.changeColor)
        {
            rustmapGenerationShader.EnableKeyword(changeColor);
            var colorMap = textures.ensureExistence(MaterialTextures.MapTypes.colorMap, textures.GetCurrentLinkedColor("_Color"));
            rustmapGenerationShader.SetTexture(kernelHandle, "ColorMapInOut", colorMap);
            rustmapGenerationShader.SetVector("colorRust1", dataset.rustColor1);
            rustmapGenerationShader.SetVector("colorRust2", dataset.rustColor2);
        }
        else
            rustmapGenerationShader.DisableKeyword(changeColor);

        if (dataset.changeMaskMap)
        {
            rustmapGenerationShader.EnableKeyword(changeMaskmap);

            var maskMap = textures.ensureExistence(MaterialTextures.MapTypes.maskMap, new Color(textures.GetCurrentLinkedFloat("_Metallic"), 1, 0,
                                                                                                textures.GetCurrentLinkedFloat("_Smoothness")));
            rustmapGenerationShader.SetTexture(kernelHandle, "MaskMapInOut", maskMap);
            rustmapGenerationShader.SetFloat("metalicnessOffset", dataset.metalicnessOffset);
        }
        else
            rustmapGenerationShader.DisableKeyword(changeMaskmap);

        if (dataset.changeNormalMap) { 
            rustmapGenerationShader.EnableKeyword(changeNormalMap);
            var normalMap = textures.ensureExistence(MaterialTextures.MapTypes.normalMap, new Color(0.5f, 0.5f, 1.0f, 1.0f));
            rustmapGenerationShader.SetTexture(kernelHandle, "NormalMapInOut", normalMap);
            rustmapGenerationShader.SetFloat("dentModifier", dataset.dentModifier);
        }
        else
            rustmapGenerationShader.DisableKeyword(changeNormalMap);


        var defectMap = textures.ensureExistence(MaterialTextures.MapTypes.defectMap, textures.falseColor != null ? textures.falseColor.falseColor : Color.black);
        rustmapGenerationShader.SetTexture(kernelHandle, "DefectMapInOut", defectMap);
        updateRustZoneTexture(textures.resolution.x, textures.resolution.y);
        rustmapGenerationShader.SetTexture(kernelHandle, "rustMask", RustZoneTexture);


        rustmapGenerationShader.SetFloat("maskZoom", dataset.rustMaskZoom / textures.resolution.x * 100);
        rustmapGenerationShader.SetFloat("rustPaternZoom", dataset.rustPaternZoom / textures.resolution.y * 100);
        rustmapGenerationShader.SetFloat("xSkew", dataset.xSkew);
        rustmapGenerationShader.SetFloat("rustCoMin", dataset.rustCoeficient.x);
        rustmapGenerationShader.SetFloat("rustCoMax", dataset.rustCoeficient.y);
        rustmapGenerationShader.SetFloat("sharpness", dataset.sharpness);
        rustmapGenerationShader.SetInt("nrOfOctaves", (int)dataset.nrOfOctaves);

        //execute shader
        rustmapGenerationShader.Dispatch(kernelHandle, textures.resolution.x / 8, textures.resolution.y / 8, 1);


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