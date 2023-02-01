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
using Assets.Scripts.newScene;
using UnityEngine.UIElements;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;

//using UnityEngine.Profiling;

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

    //help classes to divide functionality
    private Exporter exportHandler;
    private RandomNumberGenerator rng;

    private BOPDatasetExporter.Scene bopScene;
    private BOPDatasetExporter.SceneIterator bopSceneIterator = null;
    private List<String> bopSceneDirectorys = new List<String>();
    private int bopSceneDirIndex = -1;

    private int currentFrame = -2;
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

        if (dataset.BOPInputPath != "")
        {
            var dirInfo = new DirectoryInfo(dataset.BOPInputPath);
            if (Regex.IsMatch(dirInfo.Name, @"[0-9][0-9][0-9][0-9][0-9][0-9]"))
                bopSceneDirectorys.Add(dirInfo.FullName + '/');
            else
            {
                foreach (DirectoryInfo subDirectory in dirInfo.GetDirectories())
                {
                    if (Regex.IsMatch(subDirectory.Name, @"[0-9][0-9][0-9][0-9][0-9][0-9]"))
                        bopSceneDirectorys.Add(subDirectory.FullName + '/');

                    else if (Regex.IsMatch(subDirectory.Name, @"[0-9][0-9][0-9][0-9][0-9][0-9]_[0-9][0-9]"))
                        bopSceneDirectorys.Add(subDirectory.FullName + '/');
                }
            }
            loadNextBopScene();
            if (dataset.numberOfSamples < 0)
                dataset.numberOfSamples = bopScene.poses.Count;
            bopSceneIterator = new BOPDatasetExporter.SceneIterator(bopScene);
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


        exportHandler = new Exporter(dataset, this.gameObject);
        setupRenderTextures();
        BOPDatasetExporter.setNrOfRaytracingSamples(dataset.numRenderFrames);
        BOPDatasetExporter.setDepthScale(dataset.maxDepthDistance);

        setupGui();
    }

    private bool loadNextBopScene()
    {
        ++bopSceneDirIndex;
        if (bopSceneDirIndex >= bopSceneDirectorys.Count)
            return false;
        string currentBopPath = bopSceneDirectorys[bopSceneDirIndex];
        int sceneId = dataset.BOPSceneId;
        var sceneDir = new DirectoryInfo(currentBopPath);
        if (sceneDir.Name.Length == 6 && Regex.IsMatch(sceneDir.Name, @"[0-9][0-9][0-9][0-9][0-9][0-9]")) 
            sceneId += Int32.Parse(new DirectoryInfo(currentBopPath).Name);
        else if (sceneDir.Name.Length == 9 && Regex.IsMatch(sceneDir.Name, @"[0-9][0-9][0-9][0-9][0-9][0-9]_[0-9][0-9]"))
            sceneId += Int32.Parse(new DirectoryInfo(currentBopPath).Name.Substring(0,6)) * 100 + Int32.Parse(new DirectoryInfo(currentBopPath).Name.Substring(7, 2));

        BOPDatasetExporter.SetupExportPath(dataset.outputPath, sceneId, dataset.exportDepthTexture, dataset.exportNormalTexture, dataset.exportAlbedoTexture);
        rng = new RandomNumberGenerator(dataset.seed + sceneId);

        bopScene = BOPDatasetExporter.Load(currentBopPath);
        if (dataset.numberOfSamples < 0)
            dataset.numberOfSamples = bopScene.poses.Count;
        bopSceneIterator = new BOPDatasetExporter.SceneIterator(bopScene);
        return true;
    }

    private bool checkDatasetSettings()
    {
        if (dataset == null)
        {
            Debug.LogError("No dataset selected. Please select a folder of the Resources directory.");
            return false;
        }

        if(string.IsNullOrEmpty(dataset.outputPath))
        {
            Debug.LogError("Output path for generated data not specified");
            return false;
        }
        if (dataset.outputPath[dataset.outputPath.Length-1] != '/' && dataset.outputPath[dataset.outputPath.Length-1] != '\\')
            dataset.outputPath += "/";

        if (!Directory.Exists(dataset.outputPath))
        {
            if (!string.IsNullOrEmpty(dataset.outputPath) && EditorUtility.DisplayDialog("output path", "The output directory does not exists, do you want to create it?\nOutput path: " + Path.GetFullPath(dataset.outputPath), "Create directory", "Terminate program"))
            {
                try
                {
                    Directory.CreateDirectory(dataset.outputPath);
                }
                catch (Exception e)
                {
                    Debug.LogError("Output directory creation failed. " + e.Message);
                    return false;
                }
            }
            else
            {
                Debug.LogError("Output path for generated data not specified or does not exist");
                return false;
            }
        }

        rng = new RandomNumberGenerator(dataset.seed);
        Utils.setUnityScale(dataset.mmToUnityDistanceScale);

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

        if (exportHandler != null && capturing)//export handler is not yet created if start script is waiting for user input (create directory popup)
        {
            if (currentFrame == 0)
            {
                Time.timeScale = 10.0f; 
                mainCamera.enabled = false;
            }
            if (currentFrame == dataset.numPhysicsFrames)
            {
                Time.timeScale = dataset.stopSimulationTimeCompletly ? 0.0f :  1.0f;
                mainCamera.enabled = true;
                PathTracing raytraceSettings;
                raytracingSettings.GetComponent<Volume>().profile.TryGet<PathTracing>(out raytraceSettings);
                if (raytraceSettings != null)
                {
                    raytraceSettings.maximumSamples.overrideState = true;
                    raytraceSettings.maximumSamples.value = Math.Max(1, dataset.numRenderFrames - 1);
                }
            }

            if (currentFrame == dataset.numRenderFrames + dataset.numPhysicsFrames)
                StartCoroutine(exportHandler.Capture(getExportObjects()));
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
        if (dataset.exportModelsByTag)
            return new List<GameObject>(GameObject.FindGameObjectsWithTag("ExportInstanceInfo"));


        List<GameObject> instantiatedModels = new List<GameObject>();
        foreach (RandomizerInterface child in this.GetComponentsInChildren<RandomizerInterface>())
            instantiatedModels.AddRange(child.getExportObjects());
        return instantiatedModels;
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
                child.Randomize(ref rng, bopSceneIterator);
        }

        Resources.UnloadUnusedAssets();
        System.GC.Collect();

        if (bopSceneIterator !=  null)
        {
            bopSceneIterator.Next();
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
        if (dataset.exportModelsByTag) { 
            MatRandomizeHandler.AssignFalseColors(exportObjects, MaterialRandomizeData.FalseColorAssignmentType.globalIndex);
            BOPDatasetExporter.addPrefabIds(exportObjects.ToArray());
        }
        int count = exportObjects.Count;

        if (count <= 0)
            return;
        if (segmentationTextureArray != null && segmentationTextureArray.volumeDepth == count)
            return;
        segmentationTextureArray = new RenderTexture(segmentationTexture.width, segmentationTexture.height, 24);
        segmentationTextureArray.dimension = UnityEngine.Rendering.TextureDimension.Tex2DArray;
        segmentationTextureArray.volumeDepth = count;
        segmentationTextureArray.enableRandomWrite = true;
        segmentationTextureArray.Create();

        var customPasses = GameObject.FindGameObjectWithTag("CustomPass");
        if (customPasses != null) {
            DrawSegmentationObjectsCustomPass segmentationMaskRenderer = (DrawSegmentationObjectsCustomPass)customPasses.GetComponent<CustomPassVolume>().customPasses.Find(pass => pass.name == "Segmentation Mask");
            if (segmentationMaskRenderer != null)
            {
                segmentationMaskRenderer.targetTextureArray = segmentationTextureArray;
            }
            else Debug.LogWarning("Segmentation mask Array render (custom pass) was not found.");
        }
        else Debug.LogWarning("Custom pass object not found.");


        if (exportHandler != null)
            exportHandler.segmentationTextureArray = segmentationTextureArray;
        else Debug.LogError("Export handler not found");
    }

    private void setupRenderTextures()
    {
        renderTexture =       new RenderTexture(dataset.resolution.x, dataset.resolution.y, 24);
        segmentationTexture = new RenderTexture(dataset.resolution.x, dataset.resolution.y, 24);
        albedoTexture =       new RenderTexture(dataset.resolution.x, dataset.resolution.y, 24);
        normalTexture =       new RenderTexture(dataset.resolution.x, dataset.resolution.y, 24);
        depthTexture =        new RenderTexture(dataset.resolution.x, dataset.resolution.y, 24);

        if (mainCamera != null)
        {
            mainCamera.targetTexture = renderTexture;
        }

        var customPasses = GameObject.FindGameObjectWithTag("CustomPass");
        if (customPasses != null)
        {
            DrawSegmentationObjectsCustomPass segmentationMaskRenderer = (DrawSegmentationObjectsCustomPass)customPasses.GetComponent<CustomPassVolume>().customPasses.Find(pass => pass.name == "Segmentation Mask");
            if (segmentationMaskRenderer != null)
            {
                segmentationMaskRenderer.enabled = true;
                segmentationMaskRenderer.targetTexture = segmentationTexture;
                segmentationMaskRenderer.bakingCamera = mainCamera;
            }
            else Debug.LogWarning("Segmentation mask render (custom pass) was not found.");

            if (dataset.exportNormalTexture)
            {
                CustomShaderRenderToTexturePass NormalMapRenderer = (CustomShaderRenderToTexturePass)customPasses.GetComponent<CustomPassVolume>().customPasses.Find(pass => pass.name == "NormalsPass");
                if (NormalMapRenderer != null)
                {
                    NormalMapRenderer.enabled = true;
                    NormalMapRenderer.targetTexture = normalTexture;
                    NormalMapRenderer.bakingCamera = mainCamera;
                }
                else Debug.LogWarning("Normal Map Renderer (custom pass) was not found.");
            }

            if (dataset.exportDepthTexture)
            {
                mainCamera.depthTextureMode = DepthTextureMode.DepthNormals;
                CustomShaderRenderToTexturePass DepthRenderer = (CustomShaderRenderToTexturePass)customPasses.GetComponent<CustomPassVolume>().customPasses.Find(pass => pass.name == "DepthPass");
                if (DepthRenderer != null)
                {
                    DepthRenderer.enabled = true;
                    DepthRenderer.targetTexture = depthTexture;
                    DepthRenderer.bakingCamera = mainCamera;
                    DepthRenderer.overrideMaterial.SetFloat("_DepthMaxDistance", Utils.convertMmToUnity(dataset.maxDepthDistance));
                }
                else Debug.LogWarning("Depth Renderer (custom pass) was not found.");
            }
        }
        else Debug.LogWarning("Custom pass object not found.");

        if (exportHandler != null)
        {
            exportHandler.renderTexture = renderTexture;
            exportHandler.segmentationTexture = segmentationTexture;
            exportHandler.albedoTexture = albedoTexture;
            exportHandler.normalTexture = normalTexture;
            exportHandler.depthTexture = depthTexture;
        }
        else Debug.LogError("CExport handler not found");
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

        UIDoc.rootVisualElement.Q<Button>("ExportMitsubaButton").RegisterCallback<ClickEvent>(ev => exportHandler.SaveMitsuba(getExportObjects()));//todo get all objects instead
        UIDoc.rootVisualElement.Q<Button>("ExportFalseColorButton").RegisterCallback<ClickEvent>(ev => exportHandler.SaveObjectColors(getExportObjects()));
        UIDoc.rootVisualElement.Q<Button>("RandomizeAll").RegisterCallback<ClickEvent>(ev => Randomize());
    }

    private void OnDestroy()
    {
        UIDoc.panelSettings.clearColor = false;
    }

    public void recordButtonClicked()
    {
        capturing = !capturing;

        if (recordButton == null)
            return;
        recordButton.text = capturing ? "Stop recording" : "Start recording";
        recordButton.AddToClassList(capturing ? "RecordButton_Recording" : "RecordButton_NotRecording");
        recordButton.RemoveFromClassList(!capturing ? "RecordButton_Recording" : "RecordButton_NotRecording");
    }


    public void updateFileCounter()
    {
        if (imageCounterLabel != null)
            imageCounterLabel.text = $"Counter:\n{exportHandler.fileCounter}";

        if (exportHandler.fileCounter == dataset.numberOfSamples && capturing)
        {
            if (loadNextBopScene())
            {
                exportHandler.resetFileCounter();
                return;
            }
            recordButtonClicked();
        }
    }
}
