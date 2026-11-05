using UnityEngine;

// Spawns lights on a sphere around this object, all pointing at its centre.
[AddComponentMenu("Cad2Render/Sphere Light Spawn Handler")]
public class SphereLightSpawnRandomizeHandler : LightSpawnRandomizeHandler
{
    public SphereLightSpawnRandomizeData dataset;
    [InspectorButton("TriggerCloneClicked")]
    public bool clone;
    private void TriggerCloneClicked()
    {
        RandomizerInterface.CloneDataset(ref dataset);
    }

    protected override LightSpawnRandomizeData SpawnData => dataset;

    protected override void PlaceLight(Light lightSource, ref RandomNumberGenerator rng)
    {
        float phi = rng.Range(dataset.minPhiLight, dataset.maxPhiLight);
        float theta = rng.Range(dataset.minThetaLight, dataset.maxThetaLight);
        float radius = GeometryUtils.convertMmToUnity(rng.Range(dataset.minRadiusLight, dataset.maxRadiusLight));

        Vector3 offset = SphericalCoordinates.SphericalToCartesian(phi * Mathf.PI / 180.0f, theta * Mathf.PI / 180.0f, radius);
        lightSource.transform.position = transform.position + offset;
        lightSource.transform.LookAt(transform);
    }
}
