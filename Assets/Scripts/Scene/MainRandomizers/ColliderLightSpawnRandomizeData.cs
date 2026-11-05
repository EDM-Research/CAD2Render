using UnityEngine;

[CreateAssetMenu(fileName = "Untitled Dataset", menuName = "Cad2Render/Light settings/New Collider light spawn Data", order = 6)]
public class ColliderLightSpawnRandomizeData : LightSpawnRandomizeData
{
    public enum LightOrientation { LookAtTarget, RandomRotation }

    [Header("Collider placement (lights spawn inside the handler's collider)")]
    [Tooltip("LookAtTarget: point at the handler's target, or the collider centre if no target is set. RandomRotation: use the rotation ranges below.")]
    public LightOrientation orientation = LightOrientation.LookAtTarget;

    [Tooltip("Min rotation around X (degrees), used with RandomRotation")]
    [Range(-180.0f, 180.0f)]
    public float minRotationX = 30.0f;
    [Tooltip("Max rotation around X (degrees), used with RandomRotation")]
    [Range(-180.0f, 180.0f)]
    public float maxRotationX = 90.0f;
    [Tooltip("Min rotation around Y (degrees), used with RandomRotation")]
    [Range(-180.0f, 180.0f)]
    public float minRotationY = -180.0f;
    [Tooltip("Max rotation around Y (degrees), used with RandomRotation")]
    [Range(-180.0f, 180.0f)]
    public float maxRotationY = 180.0f;
    [Tooltip("Min rotation around Z (degrees), used with RandomRotation")]
    [Range(-180.0f, 180.0f)]
    public float minRotationZ = 0.0f;
    [Tooltip("Max rotation around Z (degrees), used with RandomRotation")]
    [Range(-180.0f, 180.0f)]
    public float maxRotationZ = 0.0f;
}
