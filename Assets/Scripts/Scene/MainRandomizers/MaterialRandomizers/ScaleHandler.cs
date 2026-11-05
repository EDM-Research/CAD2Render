using UnityEngine;


[AddComponentMenu("Cad2Render/MaterialRandomizers/ScaleHandler")]
public class ScaleHandler : MaterialRandomizerInterface
{
    private const string BASE_SCALE_TAG = "_BaseScale";

    public override int getPriority() { return 69; }

    public ScaleData dataset;
    [InspectorButton("TriggerCloneClicked")]
    public bool clone;
    private void TriggerCloneClicked()
    {
        RandomizerInterface.CloneDataset(ref dataset);
    }

    private Vector3 GetBaseScale(GameObject instance)
    {
        ScaleCache cache = instance.GetComponent<ScaleCache>();
        if (cache == null)
        {
            cache = instance.AddComponent<ScaleCache>();
            cache.baseScale = instance.transform.localScale;
        }
        return cache.baseScale;
    }

    private void RandomizeScale(GameObject instance, ref RandomNumberGenerator rng)
    {
        Vector3 baseScale = GetBaseScale(instance);

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
        instance.transform.localScale = new Vector3(
            baseScale.x * nextScale.x,
            baseScale.y * nextScale.y,
            baseScale.z * nextScale.z
            );
    }

    public override void RandomizeSingleInstance(GameObject instance, ref RandomNumberGenerator rng)
    {
        if (!dataset.applyOnInstance)
            return;
        RandomizeScale(instance, ref rng);
    }

    public override void RandomizeSingleMaterial(MaterialTextures textures, ref RandomNumberGenerator rng)
    {
        if(dataset.applyOnInstance)
            return;
        RandomizeScale(textures.rend.gameObject, ref rng);
    }
}
