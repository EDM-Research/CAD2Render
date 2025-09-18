using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;


[AddComponentMenu("Cad2Render/View Randomize Handler")]
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
        randomizerType = MainRandomizerData.RandomizerTypes.View;
        this.LinkGui();
        up = new Vector3(0.0f, 1.0f, 0.0f);
        mainCamera.gateFit = Camera.GateFitMode.None;

        cameraMatrix = dataset.cameraMatrix;
        if (dataset.useCameraMatrix)
            SetCameraProperties();

    }

    private void SetCameraProperties()
    {
        float Fx, Fy;
        float Cx, Cy, shiftX, shiftY;
        float width, height;
        Fx = cameraMatrix[0, 0];
        Cx = cameraMatrix[0, 2];
        Fy = cameraMatrix[1, 1];
        Cy = cameraMatrix[1, 2];

        mainCamera.usePhysicalProperties = true;
        width = (float)mainCamera.pixelWidth;
        height = (float)mainCamera.pixelHeight;

        //float f = mainCamera.focalLength;
        Vector2 sensorSize = dataset.sensorSize;
        if (sensorSize.x <= 0 || sensorSize.y <= 0)
            sensorSize = mainCamera.sensorSize;

        float f = Fx * sensorSize.x / width;
        //f = Fy * sensorSize.y / height;
        shiftX = -(Cx - width / 2.0f) / width;
        shiftY = (Cy - height / 2.0f) / height;

        mainCamera.focalLength = f;                            // in mm, ax = f * mx, ay = f * my
        mainCamera.lensShift = new Vector2(shiftX, shiftY);    // W/2,H/w for (0,0), 1.0 shift in full W/H in image plane
        mainCamera.sensorSize = sensorSize;
    }

    //int i = 0;
    public override void Randomize(ref RandomNumberGenerator rng, SceneIteratorInterface sceneIterator = null)
    {
        if (dataset.importFromBOP && sceneIterator == null)
            Debug.LogWarning("Import camera pose from bop is enababled but the bob scene == null"); ;

        //rng calls are done first so the rng stays consitent between imported en randomized viewpoints
        float phi = rng.Range(dataset.minPhi, dataset.maxPhi);
        float theta = rng.Range(dataset.minTheta, dataset.maxTheta);
        //phi = (dataset.maxPhi - dataset.minPhi) / 120 * i + dataset.minPhi;
        //++i;
        float radius = GeometryUtils.convertMmToUnity(rng.Range(dataset.minRadius, dataset.maxRadius));
        if (dataset.randomYUp)
            up.y = rng.RandomSign();


        if (dataset.importFromBOP && sceneIterator != null)
        {
            mainCamera.transform.SetPositionAndRotation(sceneIterator.GetPose().worldToCam.GetTranslation(), sceneIterator.GetPose().worldToCam.GetRotation());

            cameraMatrix[0, 0] = sceneIterator.GetPose().projMat[0, 0];
            cameraMatrix[0, 2] = sceneIterator.GetPose().projMat[0, 2];
            cameraMatrix[1, 1] = sceneIterator.GetPose().projMat[1, 1];
            cameraMatrix[1, 2] = sceneIterator.GetPose().projMat[1, 2];
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

}
