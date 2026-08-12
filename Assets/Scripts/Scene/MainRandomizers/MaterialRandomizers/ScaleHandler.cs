using UnityEngine;



[AddComponentMenu("Cad2Render/MaterialRandomizers/ScaleHandler")]
public class ScaleHandler : MaterialRandomizerInterface
{
    Vector3 previousScale;
    private void Awake()
    {
        previousScale = new Vector3(1, 1, 1);
    }
    public override int getPriority() { return 69; }

    public ScaleData dataset;
    [InspectorButton("TriggerCloneClicked")]
    public bool clone;
    private void TriggerCloneClicked()
    {
        RandomizerInterface.CloneDataset(ref dataset);
    }

    public override void RandomizeSingleInstance(GameObject instance, ref RandomNumberGenerator rng)
    {
        Vector3 nextScale = new Vector3(
            rng.Range(dataset.minScale.x, dataset.maxScale.x),
            rng.Range(dataset.minScale.y, dataset.maxScale.y),
            rng.Range(dataset.minScale.z, dataset.maxScale.z)
            );
        if (dataset.keepAspectRatio)
        {
            nextScale.y = nextScale.x;
            nextScale.z = nextScale.x;
        }
        previousScale.x = 1 / previousScale.x;
        previousScale.y = 1 / previousScale.y;
        previousScale.z = 1 / previousScale.z;
        var currentScale = instance.transform.localScale;
        currentScale.Scale(previousScale);
        currentScale.Scale(nextScale);
        instance.transform.localScale = currentScale;
        previousScale = nextScale;
    }

    public override void RandomizeSingleMaterial(MaterialTextures textures, ref RandomNumberGenerator rng)
    {
        return;
        Vector3 nextScale = new Vector3(
            rng.Range(dataset.minScale.x, dataset.maxScale.x), 
            rng.Range(dataset.minScale.y, dataset.maxScale.y), 
            rng.Range(dataset.minScale.z, dataset.maxScale.z)
            );
        if(dataset.keepAspectRatio) {
            nextScale.y = nextScale.x;
            nextScale.z = nextScale.x;
        }
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
