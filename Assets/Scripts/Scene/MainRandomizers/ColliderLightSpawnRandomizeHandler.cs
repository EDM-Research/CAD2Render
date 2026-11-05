using UnityEngine;

// Spawns lights at random points inside this object's collider.
[AddComponentMenu("Cad2Render/Collider Light Spawn Handler")]
[RequireComponent(typeof(Collider))]
public class ColliderLightSpawnRandomizeHandler : LightSpawnRandomizeHandler
{
    public ColliderLightSpawnRandomizeData dataset;
    [InspectorButton("TriggerCloneClicked")]
    public bool clone;
    private void TriggerCloneClicked()
    {
        RandomizerInterface.CloneDataset(ref dataset);
    }

    [Tooltip("What the lights point at when orientation is LookAtTarget. Empty = the collider centre.")]
    public Transform lookAtTarget;

    // Tries per light to find a point inside a non-box collider before using the last sample.
    private const int MaxSampleAttempts = 30;

    protected override LightSpawnRandomizeData SpawnData => dataset;

    protected override void Start()
    {
        Collider spawnZone = GetComponent<Collider>();
        if (spawnZone)
            spawnZone.isTrigger = true;
        base.Start();
    }

    protected override void PlaceLight(Light lightSource, ref RandomNumberGenerator rng)
    {
        Collider spawnZone = GetComponent<Collider>();
        lightSource.transform.position = RandomPointInCollider(spawnZone, ref rng);

        if (dataset.orientation == ColliderLightSpawnRandomizeData.LightOrientation.LookAtTarget)
        {
            Vector3 target = lookAtTarget ? lookAtTarget.position : spawnZone.bounds.center;
            lightSource.transform.LookAt(target);
        }
        else
        {
            lightSource.transform.rotation = Quaternion.Euler(
                rng.Range(dataset.minRotationX, dataset.maxRotationX),
                rng.Range(dataset.minRotationY, dataset.maxRotationY),
                rng.Range(dataset.minRotationZ, dataset.maxRotationZ));
        }
    }

    // Samples the collider's bounding box and keeps points that fall inside the collider itself.
    // Non-convex mesh colliders can't be tested, so they use the bounding box.
    private static Vector3 RandomPointInCollider(Collider spawnZone, ref RandomNumberGenerator rng)
    {
        Bounds bounds = spawnZone.bounds;
        bool canTestInside = !(spawnZone is MeshCollider meshCollider) || meshCollider.convex;

        Vector3 point = bounds.center;
        for (int attempt = 0; attempt < MaxSampleAttempts; ++attempt)
        {
            point = bounds.center + new Vector3(
                (rng.Next() - 0.5f) * bounds.size.x,
                (rng.Next() - 0.5f) * bounds.size.y,
                (rng.Next() - 0.5f) * bounds.size.z);

            // ClosestPoint returns the point itself when it lies inside the collider.
            if (!canTestInside || spawnZone.ClosestPoint(point) == point)
                break;
        }
        return point;
    }
}
