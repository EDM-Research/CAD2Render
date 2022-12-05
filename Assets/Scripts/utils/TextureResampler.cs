using UnityEditor;
using UnityEngine;
using ResourceManager = Assets.Scripts.io.ResourceManager;

public class TextureResampler
{
    private ComputeShader TextureSynthesizer;
    private MaterialRandomizeData dataset;

    private Texture sampleTexture { get; set; }

    int generation;

    //int[] searchRadii = { 5, 20, 25, 30, 25, 20, 15, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 };
    //int[] searchRadii = { 64,48, 32, 24, 16, 12, 8, 7, 7, 6, 5, 4, 3, 2, 2, 1, 1, 1 };
    int[] searchRadii = { 32, 30, 28, 24, 20, 16, 12, 8, 7, 6, 5, 4, 3, 2, 2, 1, 1, 1 };
    //int[] searchRadii = { 64, 55, 48, 40, 32, 30, 28, 24, 20, 16, 12, 8, 7, 6, 5, 4, 3, 2, 2, 1, 1, 1 };
    //int[] searchRadii = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };

    public TextureResampler(MaterialRandomizeData dataset)
    {
        this.dataset = dataset;
        TextureSynthesizer = ResourceManager.loadShader("TextureSynthesizer");
    }

    /**
     * deprecated use TextureResampler(MaterialRandomizeData dataset) instead
     */
    [Obsolete]
    public TextureResampler(DatasetInformation data) : this(new MaterialRandomizeData(data)){}

    public void ResampleTexture(MaterialTextures textures, Texture sampleTexture, MaterialTextures.MapTypes type, ref RandomNumberGenerator rng)
    {
        if (sampleTexture == null)
            return;

        generation = 0;
        this.sampleTexture = sampleTexture;
        ResetGeneratedTexture(ref rng, textures, type);
        UpdateGeneratedTexture(ref rng, textures.get(type), textures.getResamplelocations(), dataset.nrResampleGenerations);
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

    private void UpdateGeneratedTexture(ref RandomNumberGenerator rng, RenderTexture subjectTexture, RenderTexture locationTexture, int repeatedUpdates = 1)
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


    private void ResetGeneratedTexture(ref RandomNumberGenerator rng, MaterialTextures textures, MaterialTextures.MapTypes type)
    {
        int kernelHandle = TextureSynthesizer.FindKernel("Randomize");
        TextureSynthesizer.SetInt("randSeed", rng.IntRange(0, int.MaxValue));
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
