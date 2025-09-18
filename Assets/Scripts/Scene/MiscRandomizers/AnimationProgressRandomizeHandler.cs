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

    public override void Randomize(ref RandomNumberGenerator rng, SceneIteratorInterface sceneIterator = null)
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
        randomizerType = MainRandomizerData.RandomizerTypes.Object;
        animator = GetComponent<Animator>();
    }
}
