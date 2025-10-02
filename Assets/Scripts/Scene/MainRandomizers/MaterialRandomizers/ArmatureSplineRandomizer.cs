using UnityEngine;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class ArmatureSplineRandomizer : MaterialRandomizerInterface
{
    public override void RandomizeSingleMaterial(MaterialTextures textures, ref RandomNumberGenerator rng)
    {
        return;
    }

    [Range(0f, 0.5f)] public float offsetScale = 0.25f;
    public override void RandomizeSingleInstance(GameObject instance, ref RandomNumberGenerator rng)
    {
        var skinnedMesh = instance.GetComponentInChildren<SkinnedMeshRenderer>();
        var bones = skinnedMesh.bones;
        Vector3 p0 = bones[0].position;
        Vector3 p3 = bones[bones.Length - 1].position;
        Vector3 dir = (p3 - p0);
        float length = dir.magnitude;
        dir.Normalize();

        // Base positions for control points (evenly spaced)
        Vector3 baseP1 = p0 + dir * (length / 3f);
        Vector3 baseP2 = p0 + dir * (2f * length / 3f);

        // Random perpendicular offsets
        Vector3 randomDir1 = Random.onUnitSphere;
        Vector3 randomDir2 = Random.onUnitSphere;

        Vector3 p1 = baseP1 + randomDir1 * length * offsetScale;
        Vector3 p2 = baseP2 + randomDir2 * length * offsetScale;


        for (int i = 0; i < bones.Length-1; i++)
        {
            float t = (float)i / (bones.Length - 1);

            // Simple Catmull-Rom spline evaluation
            Vector3 pos = CatmullRom(t, p0, p1, p2, p3);

            // Approx tangent by sampling ahead
            float delta = 1f / bones.Length;
            Vector3 posNext = CatmullRom(Mathf.Clamp01(t + delta), p0, p1, p2, p3);
            Vector3 tangent = (posNext - pos).normalized;

            //bones[i].position = pos;
            bones[i].rotation = Quaternion.LookRotation(tangent, Vector3.up);
            //if (i == 5) break;
        }
    }

    Vector3 CatmullRom(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * (t * t) +
            (-p0 + 3f * p1 - 3f * p2 + p3) * (t * t * t)
        );
    }
}
