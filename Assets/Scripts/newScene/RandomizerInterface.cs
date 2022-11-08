using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UIElements;

public abstract class RandomizerInterface : MonoBehaviour
{
    public abstract void Randomize(ref RandomNumberGenerator rng, BOPDatasetExporter.SceneIterator bopSceneIterator = null);
    public virtual List<GameObject> getExportObjects() { return new List<GameObject>(); }
    public abstract ScriptableObject getDataset();
    public bool addRandomizeButton = true;

    public virtual bool updateCheck(uint currentUpdate, MainRandomizerData.RandomizerUpdateIntervals[] updateIntervals = null)
    {
        if (updateIntervals == null)
            return true;
        foreach(var pair in updateIntervals)
        {
            if(pair.randomizerType == MainRandomizerData.RandomizerTypes.Default)
            {
                return currentUpdate % Math.Max(pair.interval, 1) == 0;
            }
        }
        return true;//no default defined => randomize every update
    }

    protected void resetFrameAccumulation()
    {
        HDRenderPipeline renderPipeline = RenderPipelineManager.currentPipeline as HDRenderPipeline;
        if (renderPipeline != null)
        {
            renderPipeline.ResetPathTracing();
        }
    }

    protected void LinkGui(string listName)
    {
        if (!addRandomizeButton)
            return;
        var GUI = GameObject.FindGameObjectWithTag("GUI");
        if (!GUI)
        {
            Debug.LogWarning("GUI not found while linking buttons");
            return;
        }
        var UIDoc = GUI.GetComponent<UIDocument>();
        if (!UIDoc)
        {
            Debug.LogWarning("UIDocument not found in the GUI while linking buttons");
            return;
        }
        ScrollView buttonList = UIDoc.rootVisualElement.Q<ScrollView>(listName);
        if (buttonList ==  null)
        {
            Debug.LogWarning(listName + " not found in the GUI while linking buttons");
            return;
        }

        Button button = new Button();
        button.text = this.name;
        RandomNumberGenerator rng = new RandomNumberGenerator((int)System.DateTime.Now.Ticks);
        button.RegisterCallback<ClickEvent>(ev => Randomize(ref rng));

        buttonList.Add(button);
    }
}
