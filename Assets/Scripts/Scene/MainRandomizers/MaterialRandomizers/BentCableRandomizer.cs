using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Cad2Render/MaterialRandomizers/Bent Cable Randomizer")]
public class BentCableRandomizer : MaterialRandomizerInterface
{
    [Min(0)] public int controlPointCount = 1;
    [SerializeField, HideInInspector] private Transform[] bones;
    [SerializeField, HideInInspector] private Vector3[] restJoints;
    [SerializeField, HideInInspector] private Quaternion[] restRotations;
    [SerializeField, HideInInspector] private Vector3[] restScales;
    [SerializeField, HideInInspector] private float[] arc;

    public void Initialize(Transform[] chain, Vector3 tip, float[] lengths)
    {
        if (chain.Length != lengths.Length || chain.Length == 0)
            throw new ArgumentException("Pipe bone and length counts must match.");
        bones = chain;
        restJoints = new Vector3[bones.Length + 1];
        restRotations = new Quaternion[bones.Length];
        restScales = new Vector3[bones.Length];
        arc = new float[bones.Length + 1];
        restJoints[bones.Length] = transform.InverseTransformPoint(tip);
        for (int i = 0; i < bones.Length; ++i)
        {
            if (!(lengths[i] > 0f) || float.IsInfinity(lengths[i]))
                throw new ArgumentException("Pipe bone lengths must be finite and positive.");
            // Blender rigs disable inherited scale. Sibling bones give Unity the
            // same behavior, avoiding shear from stretched ancestor transforms.
            bones[i].SetParent(transform, true);
            restJoints[i] = bones[i].localPosition;
            restRotations[i] = bones[i].localRotation;
            restScales[i] = bones[i].localScale;
            arc[i + 1] = arc[i] + lengths[i];
        }
    }

    public override void RandomizeSingleInstance(GameObject instance, ref RandomNumberGenerator rng)
    {
        if (bones == null || bones.Length == 0)
            return;

        var joints = (Vector3[])restJoints.Clone();
        var anchors = new SortedDictionary<int, Vector3>
        {
            { 0, Vector3.zero }, { bones.Length, Vector3.zero }
        };
        var candidates = new List<int>();
        for (int i = 1; i < bones.Length; ++i)
            if ((restJoints[i + 1] - restJoints[i - 1]).sqrMagnitude > 1e-12f)
                candidates.Add(i);

        for (int control = 0; control < controlPointCount && candidates.Count > 0; ++control)
        {
            int slot = rng.IntRange(0, candidates.Count);
            int index = candidates[slot];
            candidates.RemoveAt(slot);
            float nearest = float.PositiveInfinity;
            foreach (int anchor in anchors.Keys)
                nearest = Mathf.Min(nearest, Mathf.Abs(arc[index] - arc[anchor]));

            float magnitude = rng.Poisson(0.33f * nearest);
            Vector3 tangent = (restJoints[index + 1] - restJoints[index - 1]).normalized;
            Vector3 first = Vector3.Cross(tangent,
                Mathf.Abs(tangent.y) < 0.9f ? Vector3.up : Vector3.right).normalized;
            Vector3 second = Vector3.Cross(tangent, first);
            float angle = rng.Radian();
            // Convert the discrete Blender-unit sample into this rig's local units.
            float unitScale = (restJoints[index] - restJoints[index - 1]).magnitude
                / (arc[index] - arc[index - 1]);
            Vector3 offset = (first * Mathf.Cos(angle) + second * Mathf.Sin(angle))
                * (magnitude * unitScale);
            anchors.Add(index, joints[index] - restJoints[index] + offset);

            int left = 0;
            foreach (int right in anchors.Keys)
            {
                if (right == 0)
                    continue;
                for (int i = left; i < right; ++i)
                {
                    float factor = (arc[i] - arc[left]) / (arc[right] - arc[left]);
                    joints[i] = restJoints[i] + Vector3.Lerp(anchors[left], anchors[right], factor);
                }
                left = right;
            }
        }

        for (int i = 0; i < bones.Length; ++i)
        {
            Vector3 restDirection = restJoints[i + 1] - restJoints[i];
            Vector3 direction = joints[i + 1] - joints[i];
            bones[i].localPosition = joints[i];
            bones[i].localRotation = Quaternion.FromToRotation(restDirection, direction) * restRotations[i];
            // Exported Blender bones have their length along local Y.
            Vector3 scale = restScales[i];
            scale.y *= direction.magnitude / restDirection.magnitude;
            bones[i].localScale = scale;
        }
    }

    public override void RandomizeSingleMaterial(MaterialTextures textures, ref RandomNumberGenerator rng)
    { }
}
