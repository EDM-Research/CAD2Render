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
    public void Start()
    {
        randomizerType = MainRandomizerData.RandomizerTypes.View;
        this.LinkGui();
        up = new Vector3(0.0f, 1.0f, 0.0f);
    }

    //int i = 0;
    public override void Randomize(ref RandomNumberGenerator rng, SceneIteratorInterface sceneIterator = null)
    {
        if (dataset.importFromSceneIterator && sceneIterator == null)
            Debug.LogWarning("Import camera pose from scene iterator is enabled but the scene iterator == null");

        //rng calls are done first so the rng stays consitent between imported en randomized viewpoints
        float phi = rng.Range(dataset.minPhi, dataset.maxPhi);
        float theta = rng.Range(dataset.minTheta, dataset.maxTheta);
        //phi = (dataset.maxPhi - dataset.minPhi) / 120 * i + dataset.minPhi;
        //++i;
        float radius = GeometryUtils.convertMmToUnity(rng.Range(dataset.minRadius, dataset.maxRadius));
        if (dataset.randomYUp)
            up.y = rng.RandomSign();


        if (dataset.importFromSceneIterator && sceneIterator != null)
        {
            mainCamera.transform.SetPositionAndRotation(sceneIterator.GetPose().worldToCam.GetTranslation(), sceneIterator.GetPose().worldToCam.GetRotation());
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
