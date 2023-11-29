using UnityEditor;
using UnityEngine;
using Microsoft.Win32;
using System;
using ResourceManager = Assets.Scripts.io.ResourceManager;

public class TextureResampler
{
    private ComputeShader TextureSynthesizer;
    private TextureResamplerData dataset;
    private bool TdrDelay_registerFixed = false;
    private static bool TdrDelay_registerWarningSend = false;
    private Texture sampleTexture { get; set; }

    int generation;

    //int[] searchRadii = { 5, 20, 25, 30, 25, 20, 15, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 };
    //int[] searchRadii = { 64,48, 32, 24, 16, 12, 8, 7, 7, 6, 5, 4, 3, 2, 2, 1, 1, 1 };
    int[] searchRadii = { 32, 30, 28, 24, 20, 16, 12, 8, 7, 6, 5, 4, 3, 2, 2, 1, 1, 1 };
    //int[] searchRadii = { 64, 55, 48, 40, 32, 30, 28, 24, 20, 16, 12, 8, 7, 6, 5, 4, 3, 2, 2, 1, 1, 1 };
    //int[] searchRadii = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };

    [Obsolete("Use the new modular material randomizers.")]
    public TextureResampler(MatRandomizeData dataset)
        :this(new TextureResamplerData(dataset)){}

    public TextureResampler(TextureResamplerData dataset)
    {
        this.dataset = dataset;
        TextureSynthesizer = ResourceManager.loadShader("TextureSynthesizer");

        if (TdrDelay_registerWarningSend)
            return;
        TdrDelay_registerWarningSend = true;
        if (TdrDelay_registerFixed)
            return;

        //https://stackoverflow.com/a/70808736
        try
        {
            using (var key = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers", false)) // False to run without admin rights (read only)
            {
                if( key != null)
                {
                    Int32 TdrDdiDelay = (Int32) key.GetValue("TdrDdiDelay", 0);
                    Int32 TdrDelay = (Int32)key.GetValue("TdrDelay", 0);
                    if (TdrDelay < 30 || TdrDdiDelay < 30)
                    {
                        TdrDelay_registerFixed = !EditorUtility.DisplayDialog("TdrDelay", "The TdrDelay or TdrDdiDelay registry values at\n" + "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers" + "\nare not set or below 30(s).\nThis might cause unity to crash when using the texture resampler (Losing unsaved changes)", "Don't use the resampler", "I understand");
                    }
                    else
                        TdrDelay_registerFixed = true;
                }
            }
        }
        catch (Exception ex)
        {
            TdrDelay_registerFixed = !EditorUtility.DisplayDialog("TdrDelay", "The TdrDelay or TdrDdiDelay registry values at\n" + "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers" + "\nare not set or below 30(s).\nThis might cause unity to crash when using the texture resampler (Losing unsaved changes)", "Don't use the resampler", "I understand");
        }
    }

    public void ResampleTexture(MaterialTextures textures, Texture sampleTexture, MaterialTextures.MapTypes type, ref RandomNumberGenerator rng)
    {
        if (sampleTexture == null || !TdrDelay_registerFixed)
            return;

        generation = 0;
        this.sampleTexture = sampleTexture;
        ShuffleTexturePatches(ref rng, textures, type);
        BlendPatchBorders(ref rng, textures.get(type), textures.getResamplelocations(), dataset.nrResampleGenerations);
    }

    public void applyPreviousResample(MaterialTextures textures, MaterialTextures.MapTypes type)
    {
        textures.set(type, textures.get(type), new Color(0, 0, 0));
        int kernelHandle = TextureSynthesizer.FindKernel("ApplyResampleLocations");

        TextureSynthesizer.SetTexture(kernelHandle, "Input", sampleTexture);
        TextureSynthesizer.SetTexture(kernelHandle, "InputLocation", textures.getResamplelocations());
        TextureSynthesizer.SetTexture(kernelHandle, "Resampled", textures.get(type));

        TextureSynthesizer.Dispatch(kernelHandle, textures.resolutionX / 8, textures.resolutionY / 8, 1);
    }

    private void BlendPatchBorders(ref RandomNumberGenerator rng, RenderTexture subjectTexture, RenderTexture locationTexture, int repeatedUpdates = 1)
    {
        int kernelHandle = TextureSynthesizer.FindKernel("NeighbourSugestion");
        TextureSynthesizer.SetTexture(kernelHandle, "Resampled", subjectTexture);
        TextureSynthesizer.SetTexture(kernelHandle, "Input", sampleTexture);
        TextureSynthesizer.SetTexture(kernelHandle, "InputLocation", locationTexture);

        //int[] InputResolution = { sampleTexture.width, sampleTexture.height };
        //TextureSynthesizer.SetInts("resolutionInput", InputResolution);
        //int[] OutputResolution = { subjectTexture.width, subjectTexture.height };
        //TextureSynthesizer.SetInts("resolutionOutput", OutputResolution);

        TextureSynthesizer.SetInt("nrSamples", dataset.nrResampleSamples);
        TextureSynthesizer.SetInt("randSeed", rng.IntRange(0, int.MaxValue));

        for (int i = 0; i < repeatedUpdates; ++i)
        {
            TextureSynthesizer.SetInt("searchRadius", searchRadii[generation]);
            TextureSynthesizer.Dispatch(kernelHandle, subjectTexture.width / 8, subjectTexture.height / 8, 1);
            generation++;
        }
    }


    private void ShuffleTexturePatches(ref RandomNumberGenerator rng, MaterialTextures textures, MaterialTextures.MapTypes type)
    {
        int kernelHandle = TextureSynthesizer.FindKernel("Randomize");
        TextureSynthesizer.SetInt("randSeed", rng.IntRange(0, int.MaxValue));
        var currentLinkedTexture = textures.GetCurrentLinkedTexture(textures.getTextureName(type));
        if(currentLinkedTexture != null)
            textures.set(type, textures.GetCurrentLinkedTexture(textures.getTextureName(type)), new Color(0, 0, 0));
        else
            textures.set(type, textures.get(type), new Color(0, 0, 0));

        TextureSynthesizer.SetTexture(kernelHandle, "Resampled", textures.get(type));
        TextureSynthesizer.SetTexture(kernelHandle, "InputLocation", textures.getResamplelocations());
        TextureSynthesizer.SetTexture(kernelHandle, "Input", sampleTexture);

        //int[] InputResolution = { sampleTexture.width, sampleTexture.height };
        //TextureSynthesizer.SetInts("resolutionInput", InputResolution);
        //int[] OutputResolution = { textures.get(type).width, textures.get(type).height };
        //TextureSynthesizer.SetInts("resolutionOutput", OutputResolution);

        TextureSynthesizer.Dispatch(kernelHandle, textures.get(type).width / 8, textures.get(type).height / 8, 1);
    }
}
