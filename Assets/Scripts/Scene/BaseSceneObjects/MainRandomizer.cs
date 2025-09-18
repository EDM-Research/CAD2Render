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


    //public RenderTexture depthTexture;
    private RenderTexture renderTexture = null;
    private RenderTexture segmentationTexture = null;
    private RenderTexture segmentationTextureArray = null;
    private RenderTexture albedoTexture = null;
    private RenderTexture normalTexture = null;
    private RenderTexture depthTexture = null;


    private Camera _mainCamera;
    private Camera mainCamera { get { if (_mainCamera == null) _mainCamera = Camera.main; return _mainCamera; } }
    private ExportDatasetInterface[] exporters;


    private SceneIteratorInterface sceneIterator = null;

    private RandomNumberGenerator rng;
    private int currentFrame = -2;
    private int fileCounter;
    bool capturing = false;

    static public GameObject renderSettings { get; private set; }
    static public GameObject raytracingSettings { get; private set; }
    static public GameObject postProcesingSettings { get; private set; }

    // Start is called before the first frame update
    void Start()
    {

        if (!checkDatasetSettings())
        {
            UnityEditor.EditorApplication.isPlaying = false;
            Application.Quit();
            return;
        }

        fileCounter = dataset.startFileCounter;
        rng = new RandomNumberGenerator(dataset.seed);
        GeometryUtils.setUnityScale(dataset.mmToUnityDistanceScale);

        exporters = GameObject.FindGameObjectWithTag("Exporter").GetComponentsInChildren<ExportDatasetInterface>();
        foreach (ExportDatasetInterface exporter in exporters)
            exporter.setupExportPath(dataset.outputPath, dataset.sceneId);

        sceneIterator = GetComponent<SceneIteratorInterface>();
        if (sceneIterator != null) {
            dataset.numberOfImages = -1;
            sceneIterator.NewSceneLoaded += () => { fileCounter = 0; };
            foreach (var exporter in exporters)
                sceneIterator.NewSceneLoaded += exporter.incrementOutputPath;
            sceneIterator.LastSceneEnded += () => { setRecording(false); };
        }

        var temp = GameObject.FindGameObjectWithTag("EnvironmentSettings");
        if (temp != null)
        {
            renderSettings = (GameObject)temp.transform.Find("Rendering Settings")?.gameObject;
            raytracingSettings = temp.transform.Find("Ray Tracing Settings")?.gameObject;
            postProcesingSettings = temp.transform.Find("PostProcessing")?.gameObject;
        }
        setRenderprofiles();

        Exposure exp = null;
        postProcesingSettings.GetComponent<Volume>().profile.TryGet<Exposure>(out exp);
        if (exp != null)
            exp.active = dataset.autoCameraExposure;
        else
            Debug.LogWarning("exposure component not found.");

        setupRenderTextures();
        setupGui();
    }

    private bool checkDatasetSettings()
    {
        if (dataset == null)
        {
            Debug.LogError("No dataset selected. Please link a dataset file to the main generator.");
            return false;
        }

        if (dataset.resolution.x <= 0)
            dataset.resolution.x = 1048;
        if (dataset.resolution.y <= 0)
            dataset.resolution.y = 1048;

        return true;
    }

    void Update()
    {
        if (currentFrame == -2)
        {
            currentFrame = 0;
            Randomize();
        }

        if (capturing)
        {
            if (currentFrame == 0)
            {
                Time.timeScale = 10.0f;
                mainCamera.enabled = false;
            }
            if (currentFrame == dataset.numPhysicsFrames)
            {
                Time.timeScale = dataset.stopSimulationTimeCompletly ? 0.0f : 1.0f;
                mainCamera.enabled = true;
                PathTracing raytraceSettings;
                raytracingSettings.GetComponent<Volume>().profile.TryGet<PathTracing>(out raytraceSettings);
                if (raytraceSettings != null)
                {
                    raytraceSettings.maximumSamples.overrideState = true;
                    raytraceSettings.maximumSamples.value = Math.Max(1, dataset.numRenderFrames - 1);
                }
            }

            if (currentFrame == dataset.numRenderFrames + dataset.numPhysicsFrames) {
                foreach (var exporter in exporters)
                {
                    StartCoroutine(exporter.exportFrame(getExportObjects(), mainCamera, fileCounter));
                }
                fileCounter++;

            }
            else if (currentFrame > dataset.numRenderFrames + dataset.numPhysicsFrames) // update randomize the frame after the save frame to make sure save is completed correctly
            {
                updateFileCounter();
                Randomize();
                currentFrame = 0;
                mainCamera.enabled = false;
                return;//dont start frame counter on 1
            }
            currentFrame++;
        }
        else if (currentFrame != -1)
        {
            Time.timeScale = 1.0f;
            mainCamera.enabled = true;
            currentFrame = -1;
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
                //child.Randomize(ref rng, bopSceneIterator);
                child.Randomize(ref rng, null);
        }

        Resources.UnloadUnusedAssets();
        System.GC.Collect();

        if (sceneIterator != null)
        {
            sceneIterator.Next();
        }
        update++;
        setupFalseColorStack();
    }


    public void setRenderprofiles()
    {
        if (renderSettings != null && dataset.renderProfile != null)
            renderSettings.GetComponent<Volume>().profile = dataset.renderProfile;

        if (raytracingSettings != null && dataset.rayTracingProfile != null)
            raytracingSettings.GetComponent<Volume>().profile = dataset.rayTracingProfile;

        if (postProcesingSettings != null && dataset.postProcesingProfile != null)
            postProcesingSettings.GetComponent<Volume>().profile = dataset.postProcesingProfile;
    }

    private void setupFalseColorStack()
    {
        var exportObjects = getExportObjects();
        int count = exportObjects.Count;

        //if (count <= 0 || count > BOPDatasetExporter.maxSegmentationObjects)
        //    return;
        if (segmentationTextureArray != null && segmentationTextureArray.volumeDepth == count)
            return;
        segmentationTextureArray = new RenderTexture(segmentationTexture.width, segmentationTexture.height, 24);
        segmentationTextureArray.dimension = UnityEngine.Rendering.TextureDimension.Tex2DArray;
        segmentationTextureArray.volumeDepth = count;
        segmentationTextureArray.enableRandomWrite = true;
        segmentationTextureArray.Create();

        var customPasses = GameObject.FindGameObjectWithTag("CustomPass");
        if (customPasses != null)
        {
            DrawSegmentationObjectsCustomPass segmentationMaskRenderer = (DrawSegmentationObjectsCustomPass)customPasses.GetComponent<CustomPassVolume>().customPasses.Find(pass => pass.name == "SegmentationPass");
            if (segmentationMaskRenderer != null)
            {
                segmentationMaskRenderer.targetTextureArray = segmentationTextureArray;
            }
            else Debug.LogWarning("Segmentation mask Array render (custom pass) was not found.");
        }
        else Debug.LogWarning("Custom pass object not found.");

        foreach (var exporter in exporters)
            exporter.segmentationTextureArray = segmentationTextureArray;
    }

    private void setupRenderTextures()
    {
        renderTexture = new RenderTexture(dataset.resolution.x, dataset.resolution.y, 24);
        segmentationTexture = new RenderTexture(dataset.resolution.x, dataset.resolution.y, 24);
        albedoTexture = new RenderTexture(dataset.resolution.x, dataset.resolution.y, 24);
        normalTexture = new RenderTexture(dataset.resolution.x, dataset.resolution.y, 24);
        depthTexture = new RenderTexture(dataset.resolution.x, dataset.resolution.y, 24, RenderTextureFormat.ARGBFloat);

        if (mainCamera != null)
        {
            mainCamera.targetTexture = renderTexture;
        }

        var customPasses = GameObject.FindGameObjectWithTag("CustomPass");
        if (customPasses != null)
        {
            DrawSegmentationObjectsCustomPass segmentationMaskRenderer = (DrawSegmentationObjectsCustomPass)customPasses.GetComponent<CustomPassVolume>().customPasses.Find(pass => pass.name == "SegmentationPass");
            if (segmentationMaskRenderer != null)
            {
                segmentationMaskRenderer.enabled = true;
                segmentationMaskRenderer.targetTexture = segmentationTexture;
                segmentationMaskRenderer.bakingCamera = mainCamera;
            }
            else Debug.LogWarning("Segmentation mask render (custom pass) was not found.");

            CustomShaderRenderToTexturePass NormalMapRenderer = (CustomShaderRenderToTexturePass)customPasses.GetComponent<CustomPassVolume>().customPasses.Find(pass => pass.name == "NormalsPass");
            if (NormalMapRenderer != null)
            {
                NormalMapRenderer.enabled = true;
                NormalMapRenderer.targetTexture = normalTexture;
                NormalMapRenderer.bakingCamera = mainCamera;
            }
            else Debug.LogWarning("Normal Map Renderer (custom pass) was not found.");

            mainCamera.depthTextureMode = DepthTextureMode.DepthNormals;
            CustomShaderRenderToTexturePass DepthRenderer = (CustomShaderRenderToTexturePass)customPasses.GetComponent<CustomPassVolume>().customPasses.Find(pass => pass.name == "DepthPass");
            if (DepthRenderer != null)
            {
                DepthRenderer.enabled = true;
                DepthRenderer.targetTexture = depthTexture;
                DepthRenderer.bakingCamera = mainCamera;
                DepthRenderer.overrideMaterial.SetFloat("_DepthMaxDistance", GeometryUtils.convertMmToUnity(dataset.maxDepthDistance));
            }
            else Debug.LogWarning("Depth Renderer (custom pass) was not found.");

        }
        else Debug.LogWarning("Custom pass object not found.");

        foreach (var exporter in exporters)
        {
            exporter.renderTexture = renderTexture;
            exporter.segmentationTexture = segmentationTexture;
            exporter.depthTexture = depthTexture;

            exporter.normalTexture = normalTexture;
            exporter.albedoTexture = albedoTexture;
        }
    }


    UIDocument UIDoc;
    Button recordButton;
    Label imageCounterLabel;
    private void setupGui()
    {
        var GUI = GameObject.FindGameObjectWithTag("GUI");
        if (!GUI)
        {
            Debug.LogWarning("GUI not found while linking buttons");
            return;
        }
        UIDoc = GUI.GetComponent<UIDocument>();
        if (!UIDoc)
        {
            Debug.LogWarning("UIDocument not found in the GUI while linking buttons");
            return;
        }
        UIDoc.panelSettings.clearColor = true;
        var item = new Image();
        item.image = renderTexture;
        //item.image = depthTexture;
        UIDoc.rootVisualElement.Q<VisualElement>("RenderDisplay").Add(item);

        item = new Image();
        item.image = segmentationTexture;
        UIDoc.rootVisualElement.Q<VisualElement>("SegmentationDisplay").Add(item);

        item = new Image();
        item.image = depthTexture;
        UIDoc.rootVisualElement.Q<VisualElement>("DepthDisplay").Add(item);

        recordButton = UIDoc.rootVisualElement.Q<Button>("RecordButton");
        recordButton.RegisterCallback<ClickEvent>(ev => recordButtonClicked());

        imageCounterLabel = UIDoc.rootVisualElement.Q<Label>("ImageCounter");
        updateFileCounter();

        UIDoc.rootVisualElement.Q<Button>("RandomizeAll").RegisterCallback<ClickEvent>(ev => Randomize());
    }

    private void OnDestroy()
    {
        UIDoc.panelSettings.clearColor = false;
    }

    public void recordButtonClicked()
    {
        setRecording(!capturing);
    }
    public void setRecording(bool record)
    {
        capturing = record;

        if (recordButton == null)
            return;
        recordButton.text = capturing ? "Stop recording" : "Start recording";
        recordButton.AddToClassList(capturing ? "RecordButton_Recording" : "RecordButton_NotRecording");
        recordButton.RemoveFromClassList(!capturing ? "RecordButton_Recording" : "RecordButton_NotRecording");
    }


    public void updateFileCounter()
    {
        if (imageCounterLabel != null)
            imageCounterLabel.text = $"Counter:\n{fileCounter}";

        if (fileCounter == dataset.numberOfImages && capturing)
            recordButtonClicked();
    }
}
