//Copyright (c) 2020 Nick Michiels <nick.michiels@uhasselt.be>, Hasselt University, Belgium, All rights reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using System.IO;

using System;
using UnityEditor.UIElements;
using UnityEditor;
using UnityEngine.UIElements;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;

using Assets.Scripts.io;
using Assets.Scripts.io.BOP;

//using UnityEngine.Profiling;


[AddComponentMenu("Cad2Render/Main Randomizer")]
public class MainRandomizer : MonoBehaviour
{
    [Header("Dataset")]
    [Tooltip("DatasetInformation containing settings for data generation.")]
    public MainRandomizerData dataset;
    [InspectorButton("TriggerCloneClicked")]
    public bool clone;
    private void TriggerCloneClicked()
    {
        RandomizerInterface.CloneDataset(ref dataset);
    }



    private SceneIteratorInterface sceneIterator = null;

    private RandomNumberGenerator rng;

    static public Volume renderSettings { get; private set; }
    static public Volume raytracingSettings { get; private set; }
    static public Volume postProcesingSettings { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        rng = new RandomNumberGenerator(dataset.seed);

        var temp = GameObject.FindGameObjectWithTag("EnvironmentSettings");
        if (temp != null)
        {
            renderSettings = temp.transform.Find("Rendering Settings")?.gameObject?.GetComponent<Volume>();
            raytracingSettings = temp.transform.Find("Ray Tracing Settings")?.gameObject?.GetComponent<Volume>();
            postProcesingSettings = temp.transform.Find("PostProcessing")?.gameObject?.GetComponent<Volume>();
        }
        if (renderSettings != null && dataset.renderProfile != null)
            renderSettings.profile = dataset.renderProfile;

        if (raytracingSettings != null && dataset.rayTracingProfile != null)
            raytracingSettings.profile = dataset.rayTracingProfile;

        if (postProcesingSettings != null && dataset.postProcesingProfile != null)
        {
            postProcesingSettings.profile = dataset.postProcesingProfile;

            Exposure exp = null;
            postProcesingSettings.profile.TryGet<Exposure>(out exp);
            if (exp != null)
                exp.active = dataset.autoCameraExposure;
            else
                Debug.LogWarning("exposure component not found.");
        }
    }

    public List<GameObject> getExportObjects()
    {
        //return getVisibleExportObjects();

        return new List<GameObject>(GameObject.FindGameObjectsWithTag("ExportInstanceInfo"));
    }
    public List<GameObject> getVisibleExportObjects()
    {
        var fullList = new List<GameObject>(GameObject.FindGameObjectsWithTag("ExportInstanceInfo"));
        var filteredList = new List<GameObject>();

        foreach (var exportObject in fullList)
        {
            foreach (var renderer in exportObject.GetComponentsInChildren<Renderer>())
            {
                if (renderer.isVisible)
                {
                    filteredList.Add(exportObject);
                    break;
                }
            }
        }

        return filteredList;
    }

    private uint update = 0;
    public void Randomize()
    {
        ColorEncoding.resetGlobalColorIndex();
        foreach (RandomizerInterface child in this.GetComponentsInChildren<RandomizerInterface>())
        {
            if (!child.isActiveAndEnabled)
                continue;
            if (!dataset.separateUpdates || child.updateCheck(update, dataset.updateIntervals))
                child.Randomize(ref rng, sceneIterator);
        }

        Resources.UnloadUnusedAssets();
        System.GC.Collect();

        if (sceneIterator != null)
        {
            sceneIterator.Next();
        }
        update++;
    }

    public void setSceneIterator(SceneIteratorInterface newSceneIterator)
    {
        sceneIterator = newSceneIterator;
    }
}
