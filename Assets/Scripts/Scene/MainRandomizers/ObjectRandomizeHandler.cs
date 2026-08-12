using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MyResourceManager = Assets.Scripts.io.MyResourceManager;
using UnityEngine.PlayerLoop;


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


    private RandomNumberGenerator rng;

    private GameObject[] models = new GameObject[0];
    private List<GameObject> submodels = new List<GameObject>();

    private List<GameObject> instantiatedModels = new List<GameObject>();
    private List<GameObject> instantiatedSubModels = new List<GameObject>();
    private MaterialRandomizeHandler materialRandomizeHandler;

    private bool updateMaterialRandomize = true;
    private bool updateObjectRandomize = true;

    public void Start()
    {
        randomizerType = MainRandomizerData.RandomizerTypes.Object;
        LinkGui();

        models = MyResourceManager.LoadAll<GameObject>(objectData.modelsPath);
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

    public override void Randomize(ref RandomNumberGenerator rng, SceneIteratorInterface sceneIterator = null)
    {
        this.rng = rng;

        if (updateObjectRandomize) { 
            DestroyModels();
            CreateModels(sceneIterator);
            if(materialRandomizeHandler != null)
                materialRandomizeHandler.RandomizeInstances(ref rng);
        }

        if (materialRandomizeHandler != null && updateMaterialRandomize)
            materialRandomizeHandler.RandomizeMaterials(ref rng);

        resetFrameAccumulation();
    }


    public GameObject FindModel(SceneIteratorInterface.C2RModel modelInfo)
    {
        string name = string.Format("obj_{0:000000}", modelInfo.obj_id);
        if (modelInfo.obj_name != "")
            name = modelInfo.obj_name;
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
    private void CreateModels(SceneIteratorInterface sceneIterator)
    {
        bool onlyConsumeRNG = false;
        if (sceneIterator != null && objectData.importFromBOP != ObjectRandomizeData.BopImportType.NoImport)
        {
            List<SceneIteratorInterface.C2RModel> bopModels = sceneIterator.GetPose().models;
            for (int i = 0; i < bopModels.Count; ++i)
            {
                GameObject model = FindModel(bopModels[i]);
                if (model != null)
                {
                    if (objectData.importFromBOP == ObjectRandomizeData.BopImportType.ModelAndPose)
                        SpawnModelAtExactPosition(model, i, bopModels[i].localToWorld.GetTranslation(), bopModels[i].localToWorld.GetRotation());
                    else // objectData.importFromBOP == ObjectRandomizeData.BopImportType.ModelOnly
                        SpawnModel(model, i);
                }
            }
            onlyConsumeRNG = true;
        }
        
        if (objectData.uniqueObjects)
        {
            for (int i = 0; i < models.Length; ++i)
            {
                GameObject model = models[i];
                SpawnModel(model, i, onlyConsumeRNG);
            }
        }
        else
        {
            for (int i = 0; i < objectData.numRandomObjects; ++i)
            {
                if (models.Length > 0)
                {
                    GameObject model = models[rng.IntRange(0, models.Length)];
                    SpawnModel(model, i, onlyConsumeRNG);
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
    private void SpawnModel(GameObject model, int index, bool onlyConsumeRNG = false)
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

        if (onlyConsumeRNG)
        {
            Destroy(clone);
            return;
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
        if(!updateMaterialRandomize)
            applyFalseColor(spawnObject);

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

    /** Applies the false color properties of the instance to the material property block of all renderers in the instance. This is needed to make sure the false color is applied correctly when randomizing materials on different frames then spawning objects. */
    private void applyFalseColor(GameObject instance)
    {
        FalseColor falseColor = instance.GetComponent<FalseColor>();
        if (falseColor == null)
            return;

        Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            for (int i = 0; i < renderer.materials.Length; ++i)
            {
                MaterialPropertyBlock tempPropertyBlock = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(tempPropertyBlock, i);
                falseColor.ApplyFalseColorProperties(tempPropertyBlock);
                renderer.SetPropertyBlock(tempPropertyBlock, i);
            }
        }
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

    //this function can alter the behavior of Randomize by disabeling the object span or material randomize
    public override bool updateCheck(uint currentUpdate, MainRandomizerData.RandomizerUpdateIntervals[] updateIntervals = null)
    {
        if (updateIntervals == null)
            return true;
        bool defaultTypeUpdate = true;//no default defined => randomize every update
        int updateMaterial = 0;
        int updateObject = 0;
        foreach (var updateInterval in updateIntervals)
        {
            if (updateInterval.randomizerType == MainRandomizerData.RandomizerTypes.Material)
                updateMaterial = 1 + ((currentUpdate + updateInterval.offset) % Math.Max(updateInterval.interval, 1) == 0 ? 1 : 0);
            if (updateInterval.randomizerType == MainRandomizerData.RandomizerTypes.Object)
                updateObject = 1 + ((currentUpdate + updateInterval.offset) % Math.Max(updateInterval.interval, 1) == 0 ? 1 : 0);

            if (updateInterval.randomizerType == MainRandomizerData.RandomizerTypes.Default)
                defaultTypeUpdate = (currentUpdate + updateInterval.offset) % Math.Max(updateInterval.interval, 1) == 0;
        }

        updateMaterialRandomize = updateMaterial == 2 || (updateMaterial == 0 && defaultTypeUpdate);
        updateObjectRandomize = updateObject == 2 || (updateObject == 0 && defaultTypeUpdate);

        return updateMaterialRandomize || updateObjectRandomize;
    }
}
