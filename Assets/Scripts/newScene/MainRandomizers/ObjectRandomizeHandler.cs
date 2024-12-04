using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ResourceManager = Assets.Scripts.io.ResourceManager;


[AddComponentMenu("Cad2Render/Object Randomize Handler")]
[RequireComponent(typeof(Collider))]
public class ObjectRandomizeHandler : RandomizerInterface
{
    public ObjectRandomizeData objectData;
    [InspectorButton("TriggerCloneClicked1")]
    public bool cloneObjectDataset;
    private void TriggerCloneClicked1()
    {
        CloneDataset(ref objectData);
    }


    protected RandomNumberGenerator rng;

    private GameObject[] models = new GameObject[0];
    private List<GameObject> submodels = new List<GameObject>();

    protected List<GameObject> instantiatedModels = new List<GameObject>();
    protected List<GameObject> instantiatedSubModels = new List<GameObject>();
    private MaterialRandomizeHandler materialRandomizeHandler;

    public void Start()
    {
        randomizerType = MainRandomizerData.RandomizerTypes.Object;
        LinkGui();

        models = ResourceManager.LoadAll<GameObject>(objectData.modelsPath);
        if (objectData.seperateSubmodels)
        {
            foreach (GameObject model in models)
                foreach (Transform modelTransform in model.transform)
                    submodels.Add(modelTransform.gameObject);
        }

        if (models.Length == 0)
            Debug.LogWarning("No models found in " + objectData.modelsPath);

        Collider spawnZoneCollider = GetComponent<Collider>();
        if (spawnZoneCollider)
            spawnZoneCollider.isTrigger = true;

        TryGetComponent<MaterialRandomizeHandler>(out materialRandomizeHandler);
        if (materialRandomizeHandler != null)
        {
            if (materialRandomizeHandler.isActiveAndEnabled)
                materialRandomizeHandler.enabled = false;//prevent the main randomizer to call the material randomizer
            else
                materialRandomizeHandler = null;
            if (objectData.seperateSubmodels)
                materialRandomizeHandler.initialize(ref instantiatedSubModels);
            else
                materialRandomizeHandler.initialize(ref instantiatedModels);
        }
    }

    public override ScriptableObject getDataset()
    {
        return objectData;
    }

    public override void Randomize(ref RandomNumberGenerator rng, BOPDatasetExporter.SceneIterator bopSceneIterator = null)
    {
        DestroyModels();
        this.rng = rng;
        CreateModels(bopSceneIterator);

        if (materialRandomizeHandler != null)
            materialRandomizeHandler.Randomize(ref rng, bopSceneIterator);
        else
            resetFrameAccumulation();
    }


    public GameObject FindModel(int id)
    {
        string name = string.Format("obj_{0:000000}", id);
        foreach (GameObject model in models)
        {
            //Debug.Log(model.name);
            if (model.name == name)
                return model;
        }
        Debug.Log("Model with name " + name + " not found.");
        return null;

    }
    private void DestroyModels()
    {
        foreach (GameObject model in instantiatedModels)
            Destroy(model);
        instantiatedSubModels.Clear();
        instantiatedModels.Clear();
    }
    virtual protected void CreateModels(BOPDatasetExporter.SceneIterator bopSceneIterator)
    {
        if (bopSceneIterator != null && objectData.importFromBOP != ObjectRandomizeData.BopImportType.NoImport)
        {
            List<BOPDatasetExporter.Model> bopModels = bopSceneIterator.GetPose().models;
            for (int i = 0; i < bopModels.Count; ++i)
            {
                GameObject model = FindModel(bopModels[i].obj_id);
                if (model != null)
                {
                    if (objectData.importFromBOP == ObjectRandomizeData.BopImportType.ModelAndPose)
                        SpawnModelAtExactPosition(model, i, bopModels[i].localToWorld.GetTranslation(), bopModels[i].localToWorld.GetRotation());
                    else // objectData.importFromBOP == ObjectRandomizeData.BopImportType.ModelOnly
                        SpawnModel(model, i);
                }
            }
        }
        else if (objectData.uniqueObjects)
        {
            for (int i = 0; i < models.Length; ++i)
            {
                GameObject model = models[i];
                SpawnModel(model, i);
            }
        }
        else
        {
            for (int i = 0; i < objectData.numRandomObjects; ++i)
            {
                if (models.Length > 0)
                {
                    GameObject model = models[rng.IntRange(0, models.Length)];
                    SpawnModel(model, i);
                }
                else
                {
                    Debug.LogError("Spawning objects but no models are loaded. Check the model path in the dataset file");
                    break;
                }
            }
        }
    }

    private void SpawnModelAtExactPosition(GameObject model, int index, Vector3 spawnPosition, Quaternion spawnRotation)
    {
        GameObject clone = createInstance(model, spawnPosition, spawnRotation);
        clone.name = model.name;
        instantiatedModels.Add(clone);

        foreach (Transform childTransform in clone.transform)
            instantiatedSubModels.Add(childTransform.gameObject);
    }
    private void SpawnModel(GameObject model, int index)
    {
        Vector3 spawnPosition;

        if (objectData.randomModelTranslations)
            spawnPosition = model.transform.position + RandomPointInSpawnZone();
        else
            spawnPosition = model.transform.position;


        Quaternion spawnRotation;
        if (objectData.randomModelRotations)
            spawnRotation = rng.Rotation();
        else
            spawnRotation = model.transform.rotation;
        if (objectData.randomRotationOffset)
        {
            float angle = rng.Range(-objectData.randomRotationOffsetValue, objectData.randomRotationOffsetValue);
            Vector3 axis = objectData.randomRotationAxis != Vector3.zero ? objectData.randomRotationAxis : Vector3.forward;
            spawnRotation *= Quaternion.AngleAxis(angle, axis);
        }

        GameObject clone = createInstance(model, spawnPosition, spawnRotation);
        if (clone == null)
            return;//spawning failed
        clone.name = model.name;

        if (objectData.randomSubModelTranslation)
        {
            Transform childTrans = clone.transform.Find(objectData.subModelName);
            if (childTrans != null)
            {
                childTrans.position = childTrans.position + GeometryUtils.convertMmToUnity(new Vector3(rng.Range(-objectData.subModelOffset.x, objectData.subModelOffset.x),
                                                                        rng.Range(-objectData.subModelOffset.y, objectData.subModelOffset.y),
                                                                        rng.Range(-objectData.subModelOffset.z, objectData.subModelOffset.z)));
            }
            else
            {
                Debug.Log("Could not find submodel with name" + objectData.subModelName);
            }
        }

        instantiatedModels.Add(clone);
        foreach (Transform childTransform in clone.transform)
            instantiatedSubModels.Add(childTransform.gameObject);
    }

    private Vector3 RandomPointInSpawnZone(float scale = 1.0f)
    {
        Collider spawnZoneCollider = GetComponent<Collider>();
        if (!spawnZoneCollider)
            Debug.LogError("No Collider component on object randomizer");

        return spawnZoneCollider.bounds.center + new Vector3(
            (rng.Next() - 0.5f) * (spawnZoneCollider.bounds.size.x * scale),
            (rng.Next() - 0.5f) * spawnZoneCollider.bounds.size.y,
            (rng.Next() - 0.5f) * (spawnZoneCollider.bounds.size.z * scale)
        );
    }

    private GameObject createInstance(GameObject prefab, Vector3 spawnPosition, Quaternion spawnRotation)
    {
        GameObject spawnObject = Instantiate(prefab, spawnPosition, spawnRotation);

        if (!objectData.avoidCollisions || objectData.importFromBOP == ObjectRandomizeData.BopImportType.ModelAndPose)
            return spawnObject;

        Collider[] colliders = spawnObject.GetComponentsInChildren<Collider>();
        //int layerMask = LayerMask.GetMask("Prefabs");
        bool intersects = CheckIntersection(colliders);
        int fails = 0;
        while (intersects)
        {
            spawnObject.transform.position = prefab.transform.position + RandomPointInSpawnZone();
            intersects = CheckIntersection(colliders);
            fails++;
            if (fails >= 10)
            {
                DestroyImmediate(spawnObject);
                return null;
            }
        }
        return spawnObject;
    }

    private bool CheckIntersection(Collider[] colliders)
    {
        bool intersects = false;
        foreach (var collider in colliders)
        {
            foreach (GameObject other in instantiatedModels)
            {
                foreach (Collider other_collider in other.GetComponentsInChildren<Collider>())
                {
                    if (collider.bounds.Intersects(other_collider.bounds))
                    {
                        intersects = true;
                        break;
                    }
                }
            }
        }
        return intersects;
    }
}
