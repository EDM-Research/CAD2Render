// Generated with Claude

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;


[AddComponentMenu("Cad2Render/Collider Spawned View Randomize Handler")]
[RequireComponent(typeof(Collider))]
public class ColliderSpawnedViewRandomizeHandler : RandomizerInterface
{
    public ColliderSpawnedViewRandomizeData dataset;
    [InspectorButton("TriggerCloneClicked")]
    public bool clone;
    private void TriggerCloneClicked()
    {
        RandomizerInterface.CloneDataset(ref dataset);
    }

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
    private RandomNumberGenerator rng;

    public void Start()
    {
        randomizerType = MainRandomizerData.RandomizerTypes.View;
        this.LinkGui();

        Collider spawnZoneCollider = GetComponent<Collider>();
        if (spawnZoneCollider)
            spawnZoneCollider.isTrigger = true;
    }

    public override void Randomize(ref RandomNumberGenerator rng, SceneIteratorInterface sceneIterator = null)
    {
        this.rng = rng;

        if (dataset.importFromSceneIterator && sceneIterator == null)
            Debug.LogWarning("Import camera pose from scene iterator is enabled but the scene iterator == null");

        if (dataset.importFromSceneIterator && sceneIterator != null)
        {
            mainCamera.transform.SetPositionAndRotation(sceneIterator.GetPose().worldToCam.GetTranslation(), sceneIterator.GetPose().worldToCam.GetRotation());
        }
        else
        {
            Vector3 cameraPosition = RandomPointInSpawnZone();
            mainCamera.transform.position = cameraPosition;

            float rotX = rng.Range(dataset.minRotationX, dataset.maxRotationX);
            float rotY = rng.Range(dataset.minRotationY, dataset.maxRotationY);
            float rotZ = rng.Range(dataset.minRotationZ, dataset.maxRotationZ);

            mainCamera.transform.rotation = Quaternion.Euler(rotX, rotY, rotZ);
        }
        resetFrameAccumulation();
    }

    private Vector3 RandomPointInSpawnZone()
    {
        Collider spawnZoneCollider = GetComponent<Collider>();
        if (!spawnZoneCollider)
            Debug.LogError("No Collider component on collider spawned view randomizer");

        Bounds bounds = spawnZoneCollider.bounds;
        return bounds.center + new Vector3(
            (rng.Next() - 0.5f) * (bounds.size.x * dataset.spawnZoneXZScale),
            (rng.Next() - 0.5f) * bounds.size.y,
            (rng.Next() - 0.5f) * (bounds.size.z * dataset.spawnZoneXZScale)
        );
    }

    public override ScriptableObject getDataset()
    {
        return dataset;
    }
}
