using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationProgressRandomizeHandler : RandomizerInterface
{
    private Animator animator;

    public override ScriptableObject getDataset()
    {
        return null;
    }

    public override void Randomize(ref RandomNumberGenerator rng, BOPDatasetExporter.SceneIterator bopSceneIterator = null)
    {
        if(animator == null)
            animator = GetComponent<Animator>();
        animator.Play(0, 0, rng.Next());
        animator.speed = 0f;
        resetFrameAccumulation();
    }

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public override bool updateCheck(uint currentUpdate, MainRandomizerData.RandomizerUpdateIntervals[] updateIntervals = null)
    {
        if (updateIntervals == null)
            return true;
        foreach (var pair in updateIntervals)
        {
            if (pair.randomizerType == MainRandomizerData.RandomizerTypes.Object)//might also trigger with material randomize because of cascade trigger
            {
                return currentUpdate % Math.Max(pair.interval, 1) == 0;
            }
        }
        return base.updateCheck(currentUpdate, updateIntervals);
    }
}
