using System;
using System.Collections;
using UnityEngine;
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

    private void TriggerCloneClicked()
    {
        RandomizerInterface.CloneDataset(ref dataset);
    }

    public void Awake()
    {
        rustmapGenerationShader = ResourceManager.loadShader("rustMapGenerator");
    }

    public override void RandomizeSingleMaterial(MaterialTextures textures, ref RandomNumberGenerator rng, BOPDatasetExporter.SceneIterator bopSceneIterator = null)
    {
        int kernelHandle = rustmapGenerationShader.FindKernel("CSMain");
        rustmapGenerationShader.SetInt("randSeed", rng.IntRange(128, Int32.MaxValue));
        rustmapGenerationShader.SetInt("applyRust", 1);

        textures.set(MaterialTextures.MapTypes.maskMap, textures.GetCurrentLinkedTexture("_MaskMap"), new Color(textures.GetCurrentLinkedFloat("_Metallic"), 1, 0,
                                                                                                                   textures.GetCurrentLinkedFloat("_Smoothness")));
        rustmapGenerationShader.SetTexture(kernelHandle, "MaskMapInOut", textures.get(MaterialTextures.MapTypes.maskMap));

        textures.set(MaterialTextures.MapTypes.colorMap, textures.GetCurrentLinkedTexture("_BaseColorMap"), textures.GetCurrentLinkedColor("_Color"));
        rustmapGenerationShader.SetTexture(kernelHandle, "ColorMapInOut", textures.get(MaterialTextures.MapTypes.colorMap));

        textures.set(MaterialTextures.MapTypes.defectMap, textures.get(MaterialTextures.MapTypes.defectMap), textures.falseColor.falseColor);
        rustmapGenerationShader.SetTexture(kernelHandle, "DefectMapInOut", textures.get(MaterialTextures.MapTypes.defectMap));

        var normalMap = textures.GetCurrentLinkedTexture("_NormalMap");
        textures.set(MaterialTextures.MapTypes.normalMap, normalMap, new Color(0.5f, 0.5f, 1.0f));
        rustmapGenerationShader.SetTexture(kernelHandle, "NormalMapInOut", textures.get(MaterialTextures.MapTypes.normalMap));
        if (normalMap != null)
            rustmapGenerationShader.SetInt("useNormalMapInput", 1);
        else
            rustmapGenerationShader.SetInt("useNormalMapInput", 0);

        updateRustZoneTexture(textures.resolutionX, textures.resolutionY);
        rustmapGenerationShader.SetTexture(kernelHandle, "rustMask", RustZoneTexture);


        rustmapGenerationShader.SetVector("colorRust1", dataset.rustColor1);
        rustmapGenerationShader.SetVector("colorRust2", dataset.rustColor2);
        rustmapGenerationShader.SetFloat("maskZoom", dataset.rustMaskZoom / textures.resolutionX * 100);
        rustmapGenerationShader.SetFloat("rustPaternZoom", dataset.rustPaternZoom / textures.resolutionY * 100);
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
        if (normalMap != null)
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