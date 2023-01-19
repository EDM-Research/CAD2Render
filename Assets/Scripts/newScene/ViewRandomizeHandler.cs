using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class ViewRandomizeHandler : RandomizerInterface
{
    public ViewRandomizeData dataset;
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
    private Matrix4x4 cameraMatrix;
    public void Start()
    {
        this.LinkGui("ViewRandomizerList");
        up = new Vector3(0.0f, 1.0f, 0.0f);
        mainCamera.gateFit = Camera.GateFitMode.None;

        cameraMatrix = dataset.cameraMatrix;
        if (dataset.useCameraMatrix)
            SetCameraProperties();

    }

    private void SetCameraProperties()
    {
        float Fx, Fy, sizeX, sizeY;
        float Cx, Cy, shiftX, shiftY;
        float width, height;
        Fx = cameraMatrix[0, 0];
        Cx = cameraMatrix[0, 2];
        Fy = cameraMatrix[1, 1];
        Cy = cameraMatrix[1, 2];

        width = (float)mainCamera.pixelWidth;
        height = (float)mainCamera.pixelHeight;

        float f = mainCamera.focalLength;
        sizeX = f * width / Fx;
        sizeY = f * height / Fy;
        shiftX = -(Cx - width / 2.0f) / width;
        shiftY = (Cy - height / 2.0f) / height;

        mainCamera.usePhysicalProperties = true;
        mainCamera.sensorSize = new Vector2(sizeX, sizeY);     // in mm, mx = 1000/x, my = 1000/y
        mainCamera.focalLength = f;                            // in mm, ax = f * mx, ay = f * my
        mainCamera.lensShift = new Vector2(shiftX, shiftY);    // W/2,H/w for (0,0), 1.0 shift in full W/H in image plane
    }

    public override void Randomize(ref RandomNumberGenerator rng, BOPDatasetExporter.SceneIterator bopSceneIterator = null)
    {
        if (dataset.importFromBOP && bopSceneIterator == null)
            Debug.LogWarning("Import camera pose from bop is enababled but the bob scene == null"); ;

        //rng calls are done first so the rng stays consitent between imported en randomized viewpoints
        float phi = rng.Range(dataset.minPhi, dataset.maxPhi);
        float theta = rng.Range(dataset.minTheta, dataset.maxTheta);
        float radius = Utils.convertMmToUnity(rng.Range(dataset.minRadius, dataset.maxRadius));
        if (dataset.randomYUp)
            up.y = rng.RandomSign();


        if (dataset.importFromBOP && bopSceneIterator != null)
        {
            mainCamera.transform.SetPositionAndRotation(bopSceneIterator.GetPose().worldToCam.GetTranslation(), bopSceneIterator.GetPose().worldToCam.GetRotation());

            cameraMatrix[0, 0] = bopSceneIterator.GetPose().projMat[0, 0];
            cameraMatrix[0, 2] = bopSceneIterator.GetPose().projMat[0, 2];
            cameraMatrix[1, 1] = bopSceneIterator.GetPose().projMat[1, 1];
            cameraMatrix[1, 2] = bopSceneIterator.GetPose().projMat[1, 2];
            SetCameraProperties();
        }
        else
        {
            Vector3 offset = SphericalCoordinates.SphericalToCartesian(phi * Mathf.PI / 180.0f, theta * Mathf.PI / 180.0f, radius);
            mainCamera.transform.position = this.transform.position + offset;
            mainCamera.transform.LookAt(this.transform, up);
        }
        resetFrameAccumulation();
    }

    public override ScriptableObject getDataset()
    {
        return dataset;
    }

    public override bool updateCheck(uint currentUpdate, MainRandomizerData.RandomizerUpdateIntervals[] updateIntervals = null)
    {
        if (updateIntervals == null)
            return true;
        foreach (var pair in updateIntervals)
        {
            if (pair.randomizerType == MainRandomizerData.RandomizerTypes.View)
            {
                return currentUpdate % Math.Max(pair.interval, 1) == 0;
            }
        }
        return base.updateCheck(currentUpdate, updateIntervals);
    }
}
