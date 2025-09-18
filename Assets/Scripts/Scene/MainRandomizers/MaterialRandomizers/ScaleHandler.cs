using UnityEngine;



[AddComponentMenu("Cad2Render/MaterialRandomizers/ScaleHandler")]
public class ScaleHandler : MaterialRandomizerInterface
{
    Vector3 previousScale;
    private void Start()
    {
        previousScale = new Vector3(1, 1, 1);
    }

    //public RotationData dataset;
    //[InspectorButton("TriggerCloneClicked")]
    //public bool clone;
    //private void TriggerCloneClicked()
    //{
    //    RandomizerInterface.CloneDataset(ref dataset);
    //}


    public override void RandomizeSingleMaterial(MaterialTextures textures, ref RandomNumberGenerator rng)
    {
        Vector3 nextScale = new Vector3(rng.Next()*0.3f + 0.85f, rng.Next() * 0.3f + 0.85f, rng.Next() * 0.3f + 0.85f);
        previousScale.x = 1 / previousScale.x;
        previousScale.y = 1 / previousScale.y;
        previousScale.z = 1 / previousScale.z;
        var currentScale = textures.rend.gameObject.transform.localScale;
        currentScale.Scale(previousScale);
        currentScale.Scale(nextScale);
        textures.rend.gameObject.transform.localScale = currentScale;
        previousScale = nextScale;
    }
}
