using Assets.Scripts.io;
using UnityEngine;

public class importTextureDefectsHandler : MaterialRandomizerInterface
{
    public ImportTextureDefectsData dataset;
    [InspectorButton("TriggerCloneClicked")]
    public bool clone;
    private void TriggerCloneClicked()
    {
        RandomizerInterface.CloneDataset(ref dataset);
    }

    public override int getPriority() { return 100; }

    private ComputeShader maskToDefectShader;
    private Texture[] colorTextures = new Texture[0];
    private Texture[] defectTextures = new Texture[0];

    public void Awake()
    {
        maskToDefectShader = MyResourceManager.loadComputeShader("MaskToDefectShader");


        colorTextures = MyResourceManager.LoadAll<Texture>(dataset.TexturesPath);
        defectTextures = MyResourceManager.LoadAll<Texture>(dataset.DefectTexturesPath);

        if (colorTextures.Length == 0)
            Debug.LogWarning("No textures found in " + dataset.TexturesPath);

        if (defectTextures.Length == 0 || defectTextures.Length != colorTextures.Length)
            Debug.LogWarning("Error with defect texture importing (no files or wrong number): " + dataset.TexturesPath);
    }

    public override void RandomizeSingleMaterial(MaterialTextures textures, ref RandomNumberGenerator rng)
    {
        int index = rng.IntRange(0, colorTextures.Length);
        textures.set(MaterialTextures.MapTypes.colorMap, colorTextures[index], Color.white);

        int kernelHandle = maskToDefectShader.FindKernel("CSMain");

        var defectMap = textures.ensureExistence(MaterialTextures.MapTypes.defectMap, textures.falseColor != null ? textures.falseColor.falseColor : Color.black);
        maskToDefectShader.SetTexture(kernelHandle, "defectMask", defectMap);
        maskToDefectShader.SetTexture(kernelHandle, "defectInTexture", defectTextures[index]);

        maskToDefectShader.Dispatch(kernelHandle, textures.resolution.x / 8, textures.resolution.y / 8, 1);
    }
}
