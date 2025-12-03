using System.Data;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using Assets.Scripts.io.FM;
using Assets.Scripts.io;
using System;

public class MainExporter : MonoBehaviour
{
    public MainExportData dataset;
    [InspectorButton("TriggerCloneClicked")]
    public bool clone;
    private void TriggerCloneClicked()
    {
        RandomizerInterface.CloneDataset(ref dataset);
    }

    private Camera _mainCamera;
    private Camera mainCamera { get { if (_mainCamera == null) _mainCamera = Camera.main; return _mainCamera; } }
    //texture Of the maincamera
    private RenderTexture renderTexture = null;

    private int currentFrame = -2;
    public int fileCounter { get; private set; }
    public bool capturing { get; set; } = false;

    public ExportDatasetInterface[] exporters { get; private set; }
    private MainRandomizer randomizer = null;


    public event Action endOfDatasetGeneration;
    public event Action renderStart;
    public event Action renderEnd;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        exporters = GetComponentsInChildren<ExportDatasetInterface>();
        randomizer = GameObject.FindGameObjectWithTag("Generator").GetComponent<MainRandomizer>();

        if (!checkDatasetSettings())
        {
            UnityEditor.EditorApplication.isPlaying = false;
            Application.Quit();
            return;
        }

        fileCounter = dataset.startFileCounter;
        GeometryUtils.setUnityScale(dataset.mmToUnityDistanceScale);

        setupRenderTextures();
        foreach (ExportDatasetInterface exporter in exporters)
        {
            exporter.setup(mainCamera, dataset.outputPath, dataset.sceneId);
            renderStart += () => { exporter.onRenderStart(mainCamera, fileCounter); };
            renderEnd += () => { StartCoroutine(exporter.exportFrame(randomizer.getExportObjects(), mainCamera, fileCounter)); };
        }

        var sceneIterator = GetComponent<SceneIteratorInterface>();
        if (sceneIterator != null)
        {
            dataset.numberOfImages = -1;
            sceneIterator.NewSceneLoaded += () => { fileCounter = 0; };
            foreach (var exporter in exporters)
                sceneIterator.NewSceneLoaded += exporter.incrementSceneId;
            sceneIterator.LastSceneEnded += () => { endOfDatasetGeneration.Invoke(); };
        }
        randomizer.setSceneIterator(sceneIterator);
    }

    private bool checkDatasetSettings()
    {
        if (dataset == null)
        {
            Debug.LogError("No dataset selected. Please link a dataset file to the main generator.");
            return false;
        }

        if (dataset.resolution.x <= 0)
            dataset.resolution.x = 1024;
        if (dataset.resolution.y <= 0)
            dataset.resolution.y = 1024;

        return true;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentFrame == -2)
        {
            currentFrame = 0;
            randomizer.Randomize();
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
                renderStart.Invoke();

                Time.timeScale = dataset.stopSimulationTimeCompletly ? 0.0f : 1.0f;
                mainCamera.enabled = true;
                
                PathTracing raytraceSettings;
                MainRandomizer.raytracingSettings.TryGet<PathTracing>(out raytraceSettings);
                if (raytraceSettings != null)
                {
                    raytraceSettings.maximumSamples.overrideState = true;
                    raytraceSettings.maximumSamples.value = Math.Max(1, dataset.numRenderFrames - 1);
                }

            }
            if (currentFrame == dataset.numRenderFrames + dataset.numPhysicsFrames)
            {
                renderEnd.Invoke();
                fileCounter++;

            }
            else if (currentFrame > dataset.numRenderFrames + dataset.numPhysicsFrames) // update randomize the frame after the save frame to make sure save is completed correctly
            {
                randomizer.Randomize();
                currentFrame = 0;
                mainCamera.enabled = false;
                return;//dont start frame counter on 1
            }
            if(fileCounter == dataset.numberOfImages)
            {
                capturing = false;
                endOfDatasetGeneration.Invoke();
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
    private void setupRenderTextures()
    {
        renderTexture = new RenderTexture(dataset.resolution.x, dataset.resolution.y, 24);

        if (mainCamera != null)
        {
            mainCamera.targetTexture = renderTexture;
        }
    }
}
