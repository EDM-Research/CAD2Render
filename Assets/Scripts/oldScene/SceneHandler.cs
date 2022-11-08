using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;

[Obsolete("Used by the old scene, Use the new scene instead")]
public class SceneHandler
{
    private DatasetInformation dataset;
    
    private Camera mainCamera;
    private GameObject randomizer;
    private GameObject canvas;
    private GameObject gui;
    private GameObject customPasses;

    static public GameObject renderSettings { get; private set; }
    static public GameObject raytracingSettings { get; private set; }
    static public GameObject postProcesingSettings { get; private set; }

    public ExportHandler exporter { get; set; }

    public SceneHandler(DatasetInformation _dataset)
    {
        dataset = _dataset;

        mainCamera = Camera.main;
        gui = GameObject.Find("GUI");
        randomizer = GameObject.Find("SynthethicGenerator");
        canvas = GameObject.Find("Canvas");
        customPasses = GameObject.Find("Custom Pass");

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
  


        fullResRenderTex = new RenderTexture(dataset.resolutionWidth, dataset.resolutionHeight, 24);
        fullResSegmentationTex = new RenderTexture(dataset.resolutionWidth, dataset.resolutionHeight, 24);
    }

    public void setRenderprofiles()
    {
        if(renderSettings != null && dataset.renderProfile != null)
            renderSettings.GetComponent<Volume>().profile = dataset.renderProfile;

        if (raytracingSettings != null && dataset.rayTracingProfile != null)
            raytracingSettings.GetComponent<Volume>().profile = dataset.rayTracingProfile;

        if (postProcesingSettings != null && dataset.postProcesingProfile != null)
            postProcesingSettings.GetComponent<Volume>().profile = dataset.postProcesingProfile;
    }

    private RenderTexture fullResRenderTex = null;
    private RenderTexture fullResSegmentationTex = null;
    private RenderTexture segmentationTextureArray = null;
    //private RenderTexture lowResRenderTex = null;
    //private RenderTexture lowResSegmentationTex = null;
    public void RenderAtHighQuality(bool highQuality, bool stopTimeCompletly = false)
    {
        RenderTexture segmentationTexture;
        RenderTexture renderTexture;

        if (highQuality)
        {
            renderTexture = fullResRenderTex;
            segmentationTexture = fullResSegmentationTex;
            if (stopTimeCompletly)
                Time.timeScale = 0.0f;
            else
                Time.timeScale = 1.0f;
            mainCamera.enabled = true;//camera needs to be disabled so the path tracer doesnt start ghosting

        }
        else
        {
            //renderTexture = lowResRenderTex;
            //segmentationTexture = lowResSegmentationTex;
            Time.timeScale = 10.0f;
            mainCamera.enabled = false;//camera needs to be disabled so the path tracer doesnt start ghosting
            return;
        }



        if(mainCamera != null)
        {
            mainCamera.targetTexture = renderTexture;
        }
        if (customPasses != null)
        {
            DrawSegmentationObjectsCustomPass segmentationMaskRenderer = (DrawSegmentationObjectsCustomPass) customPasses.GetComponent<CustomPassVolume>().customPasses.Find(pass => pass.name == "Segmentation Mask");
            segmentationMaskRenderer.targetTexture = segmentationTexture;
        }

        if (randomizer != null)
        {
            randomizer.GetComponent<randomize>().renderTexture = renderTexture;
            randomizer.GetComponent<randomize>().segmentationTexture = segmentationTexture;
        }

        if (exporter != null)
        {
            exporter.renderTexture = renderTexture;
            exporter.segmentationTexture = segmentationTexture;
        }
        //canvas is disabled so this code is not run. if canvas is neded again this should update de render textures
        if (canvas != null)
        {
            canvas.transform.Find("Image").GetComponent<RawImage>().texture = renderTexture;
            canvas.transform.Find("Segmentation").GetComponent<RawImage>().texture = segmentationTexture;
        }
        //update the texture references in the gui to display the scene
        if (gui != null)
        {
            gui.transform.Find("Image").GetComponent<RawImage>().texture = renderTexture;
            gui.transform.Find("Segmentation").GetComponent<RawImage>().texture = segmentationTexture;
        }
    }
    public void setupFalseColorStack(int objectCount)
    {

        if (objectCount <= 0)
            return;
        if (segmentationTextureArray == null || segmentationTextureArray.volumeDepth != objectCount)
        {
            if(segmentationTextureArray != null)
                segmentationTextureArray.Release();
            segmentationTextureArray = new RenderTexture(dataset.resolutionWidth, dataset.resolutionHeight, 24);
            segmentationTextureArray.dimension = UnityEngine.Rendering.TextureDimension.Tex2DArray;
            segmentationTextureArray.volumeDepth = objectCount;
            segmentationTextureArray.enableRandomWrite = true;
            segmentationTextureArray.Create();
        }

        var customPasses = GameObject.FindGameObjectWithTag("CustomPass");
        if (customPasses != null)
        {
            DrawSegmentationObjectsCustomPass segmentationMaskRenderer = (DrawSegmentationObjectsCustomPass)customPasses.GetComponent<CustomPassVolume>().customPasses.Find(pass => pass.name == "Segmentation Mask");
            if (segmentationMaskRenderer != null)
            {
                segmentationMaskRenderer.targetTextureArray = segmentationTextureArray;
            }
            else Debug.LogWarning("Segmentation mask Array render (custom pass) was not found.");
        }
        else Debug.LogWarning("Custom pass object not found.");


        if (exporter != null)
            exporter.segmentationTextureArray = segmentationTextureArray;
        else Debug.LogError("Export handler not found");
    }
}
