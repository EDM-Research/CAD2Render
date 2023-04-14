using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Collider))]
public class ObjectRandomizeHandler: RandomizerInterface
{
    public ObjectRandomizeData objectData;
    [InspectorButton("TriggerCloneClicked1")]
    public bool cloneObjectDataset;
    private void TriggerCloneClicked1()
    {
        RandomizerInterface.CloneDataset(ref objectData);
    }

    public MaterialRandomizeData materialData;
    [InspectorButton("TriggerCloneClicked2")]
    public bool cloneMaterialDataset;
    private void TriggerCloneClicked2()
    {
        RandomizerInterface.CloneDataset(ref materialData);
    }

    private RandomNumberGenerator rng;

    private GameObject[] models = new GameObject[0];
    private List<GameObject> submodels = new List<GameObject>();

    private List<GameObject> instantiatedModels = new List<GameObject>();
    private List<GameObject> InstantiatedSubModels = new List<GameObject>();
    private MatRandomizeHandler materialRandomizeHandler;

    public void Start()
    {
        this.LinkGui("ObjectRandomizerList");

        if (objectData.modelsPath != "")
        {
            models = Resources.LoadAll(objectData.modelsPath, typeof(GameObject)).Cast<GameObject>().ToArray();
            if (objectData.seperateSubmodels)
            {
                foreach (GameObject model in models)
                    foreach (Transform modelTransform in model.transform)
                        submodels.Add(modelTransform.gameObject);
                BOPDatasetExporter.addPrefabIds(submodels.ToArray());
            }
            else
                BOPDatasetExporter.addPrefabIds(models);//todo make compatible with submodels
        }
        else models = new GameObject[0];

        if (models.Length == 0)
            Debug.LogWarning("No models found in " + objectData.modelsPath);

        Collider spawnZoneCollider = this.GetComponent<Collider>();
        if (spawnZoneCollider)
            spawnZoneCollider.isTrigger = true;


        materialRandomizeHandler = this.gameObject.AddComponent<MatRandomizeHandler>();
        if (objectData.seperateSubmodels)
            materialRandomizeHandler.initialize(materialData, ref InstantiatedSubModels);
        else
            materialRandomizeHandler.initialize(materialData, ref instantiatedModels);
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

        materialRandomizeHandler.UpdateFalseColors();
        resetFrameAccumulation();
    }


    public GameObject FindModel(int id)
    {
        String name = String.Format("obj_{0:000000}", id);
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
            GameObject.Destroy(model);
        InstantiatedSubModels.Clear();
        instantiatedModels.Clear();
    }
    private void CreateModels(BOPDatasetExporter.SceneIterator bopSceneIterator)
    {
        if (bopSceneIterator != null && objectData.importFromBOP != ObjectRandomizeData.BopImportType.NoImport)
        {
            List<BOPDatasetExporter.Model> bopModels = bopSceneIterator.GetPose().models;
            for (int i = 0; i < bopModels.Count; ++i)
            {
                GameObject model = FindModel(bopModels[i].obj_id);
                if(model != null)
                {
                    if(objectData.importFromBOP == ObjectRandomizeData.BopImportType.ModelAndPose)
                        SpawnModelAtExactPosition(model, i, Utils.GetTranslation(bopModels[i].localToWorld), Utils.GetRotation(bopModels[i].localToWorld));
                    else // objectData.importFromBOP == ObjectRandomizeData.BopImportType.ModelOnly
                        SpawnModel(model, i);
                }
            }
        }
        else if (objectData.uniqueObjects)
        {
            for (int i = 0; i < models.Length; ++i)
            {
                GameObject model = (GameObject)models[i];
                SpawnModel(model, i);
            }
        }
        else
        {
            for (int i = 0; i < objectData.numRandomObjects; ++i)
            {
                if (models.Length > 0)
                {
                    GameObject model = (GameObject)models[rng.IntRange(0, models.Length)];
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
            InstantiatedSubModels.Add(childTransform.gameObject);
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
                childTrans.position = childTrans.position + Utils.convertMmToUnity( new Vector3(rng.Range(-objectData.subModelOffset.x, objectData.subModelOffset.x),
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
            InstantiatedSubModels.Add(childTransform.gameObject);
    }

    private Vector3 RandomPointInSpawnZone(float scale = 1.0f)
    {
        Collider spawnZoneCollider = this.GetComponent<Collider>();
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
        GameObject spawnObject = GameObject.Instantiate(prefab, spawnPosition, spawnRotation);

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

    public override bool updateCheck(uint currentUpdate, MainRandomizerData.RandomizerUpdateIntervals[] updateIntervals = null)
    {
        if (updateIntervals == null)
            return true;
        foreach (var pair in updateIntervals)
        {
            if (pair.randomizerType == MainRandomizerData.RandomizerTypes.Object)
            {
                return currentUpdate % Math.Max(pair.interval, 1) == 0;
            }
        }
        return base.updateCheck(currentUpdate, updateIntervals);
    }
}
