using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class ImageBackgroundRandomizeHandler : RandomizerInterface
{
    public ImageBackgroundRandomizeData dataset;
    [InspectorButton("TriggerCloneClicked")]
    public bool clone;
    private void TriggerCloneClicked()
    {
        RandomizerInterface.CloneDataset(ref dataset);
    }

    private Vector3 up = new Vector3(0.0f, 1.0f, 0.0f);
    private Camera _camera;
    private Camera mainCamera
    {
        get
        {
            if (!_camera)
            {
                _camera = Camera.main;
            }
            return _camera;
        }
    }

    private Texture2D[] backgroundTextures = new Texture2D[0];
    public void Start()
    {
        randomizerType = MainRandomizerData.RandomizerTypes.Light;
        this.LinkGui();

        if (dataset.backgroundImagePath != "")
            backgroundTextures = Resources.LoadAll(dataset.backgroundImagePath, typeof(Texture2D)).Cast<Texture2D>().ToArray(); ;
    }

    GameObject backgroundPlane = null;
    Material background = null;
    public override void Randomize(ref RandomNumberGenerator rng, BOPDatasetExporter.SceneIterator bopSceneIterator = null)
    {
        if (backgroundPlane == null)
            setupBackgroundPlane();

        if (dataset.randomizeRotation)
            backgroundPlane.transform.localEulerAngles = new Vector3(rng.Range(dataset.minRotationAngle, dataset.maxRotationAngle), -90, 90);

        if (backgroundTextures.Length > 1)
            background.mainTexture = backgroundTextures[rng.IntRange(0, backgroundTextures.Length)];

        resetFrameAccumulation();
    }

    private void setupBackgroundPlane()
    {
        float backgroundDistance = mainCamera.farClipPlane;
        backgroundPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        backgroundPlane.transform.parent = mainCamera.transform;
        backgroundPlane.transform.localPosition = new Vector3(0, 0, mainCamera.farClipPlane - 10);

        float scale = (float) Math.Tan(mainCamera.fieldOfView /2/ 180 * Math.PI) * mainCamera.farClipPlane / 2.5f;
        backgroundPlane.transform.localScale = new Vector3(scale, 1, scale);
        backgroundPlane.transform.localEulerAngles = new Vector3(0, -90, 90);

        Renderer backgroundRenderer = backgroundPlane.GetComponent<Renderer>();
        background = new Material(Shader.Find("HDRP/Unlit"));
        backgroundRenderer.material = background;
        if (backgroundTextures.Length > 0) 
            background.mainTexture = backgroundTextures[0];
        else
            Debug.LogWarning("No background images found");
    }

    public override ScriptableObject getDataset()
    {
        return dataset;
    }

}
