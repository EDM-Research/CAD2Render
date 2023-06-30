using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public abstract class RandomizerInterface : MonoBehaviour
{
    public abstract void Randomize(ref RandomNumberGenerator rng, BOPDatasetExporter.SceneIterator bopSceneIterator = null);

    [Obsolete("exports are now selected by using the ExportInstanceInfo tag")]
    public virtual List<GameObject> getExportObjects() { return new List<GameObject>(); }
    public abstract ScriptableObject getDataset();
    public bool addRandomizeButton = true;

    protected MainRandomizerData.RandomizerTypes randomizerType = MainRandomizerData.RandomizerTypes.Default;
    public bool updateCheck(uint currentUpdate, MainRandomizerData.RandomizerUpdateIntervals[] updateIntervals = null)
    {
        if (updateIntervals == null)
            return true;
        foreach(var pair in updateIntervals)
        {
            if(pair.randomizerType == randomizerType)
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

    protected void LinkGui()
    {
        string listName = "";
        switch (this.randomizerType)
        {

            case MainRandomizerData.RandomizerTypes.View:
                listName = "ViewRandomizerList";
                break;
            case MainRandomizerData.RandomizerTypes.Object:
                listName = "ObjectRandomizerList";
                break;
            case MainRandomizerData.RandomizerTypes.Light:
                listName = "LightRandomizerList";
                break;
            case MainRandomizerData.RandomizerTypes.Material:
                listName = "MaterialRandomizerList";
                break;

            case MainRandomizerData.RandomizerTypes.Default:
            default:
                listName = "ViewRandomizerList";//misc randomizers are placed in view cause its the less crouded list.
                break;
        }

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

    public static void CloneDataset<T>(ref T dataset) where T : UnityEngine.Object
    {
        var newDataset = Instantiate(dataset);
        var result = Regex.Match(dataset.name, @"\d+$", RegexOptions.RightToLeft);
        if (result.Length == 0)
            newDataset.name = dataset.name + "_1";
        else
            newDataset.name = dataset.name.Substring(0, dataset.name.Length - result.Value.Length) + (Int32.Parse(result.Value) + 1);

        string newDatasetFile = Path.GetDirectoryName(SceneManager.GetActiveScene().path) + "/" + newDataset.name + ".asset";
        AssetDatabase.CreateAsset(newDataset, newDatasetFile);
        AssetDatabase.SaveAssets();
        dataset = newDataset;
    }
}
