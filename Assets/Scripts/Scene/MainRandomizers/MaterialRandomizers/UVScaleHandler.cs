using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;



[AddComponentMenu("Cad2Render/MaterialRandomizers/UVScaleHandler")]
public class UVScaleHandler : MaterialRandomizerInterface
{
    public UVScaleData dataset;
    [InspectorButton("TriggerCloneClicked")]
    public bool clone;
    private void TriggerCloneClicked()
    {
        RandomizerInterface.CloneDataset(ref dataset);
    }


    public override void RandomizeSingleMaterial(MaterialTextures textures, ref RandomNumberGenerator rng)
    {
        Vector4 newScale = new Vector4(rng.Range(dataset.scaleX[0], dataset.scaleX[1]),
                                       rng.Range(dataset.scaleY[0], dataset.scaleY[1]), 
                                       rng.Range(dataset.offsetX[0], dataset.offsetX[1]), 
                                       rng.Range(dataset.offsetY[0], dataset.offsetY[1]));
        if (dataset.syncScales)
            newScale.y = newScale.x;

        switch (textures.rend.material.shader.name)
        {
            case "HDRP/LayeredLit":
                textures.newProperties.SetVector("_LayerMaskMap_ST", newScale);
                textures.newProperties.SetVector($"_BaseColorMap{textures.GetCurrentLinkedInt("_LayerCount") - 1}_ST", newScale);
                break;

            case "HDRP/Lit":
            default:
                textures.newProperties.SetVector("_BaseColorMap_ST", newScale);
                break;

        }
    }
}
