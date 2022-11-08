using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class HideRandomizeHandler : RandomizerInterface
{
    
    public void Start()
    {
        //this.LinkGui("ObjectRandomizerList");
    }
    public float hideChance = 0.5f;

    public override void Randomize(ref RandomNumberGenerator rng, BOPDatasetExporter.SceneIterator bopSceneIterator = null)
    {
        this.gameObject.SetActive(rng.Next() > hideChance);
        resetFrameAccumulation();
    }

    public override ScriptableObject getDataset()
    {
        return null;
    }

    public override bool updateCheck(uint currentUpdate, MainRandomizerData.RandomizerUpdateIntervals[] updateIntervals = null)
    {
        if (updateIntervals == null)
            return true;
        foreach (var pair in updateIntervals)
        {
            if (pair.randomizerType == MainRandomizerData.RandomizerTypes.Material)
            {
                return currentUpdate % Math.Max(pair.interval, 1) == 0;
            }
        }
        return base.updateCheck(currentUpdate, updateIntervals);
    }
}
