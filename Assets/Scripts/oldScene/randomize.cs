//Copyright (c) 2020 Nick Michiels <nick.michiels@uhasselt.be>, Hasselt University, Belgium, All rights reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using System.IO;
using UnityEngine.UI;

using System;
using UnityEditor.UIElements;
using UnityEditor;
using System.Linq;

//using UnityEngine.Profiling;

[Obsolete("Used by the old scene, Use the new scene instead")]
public class randomize : MonoBehaviour
{
    [Header("Dataset")]
    [Tooltip("DatasetInformation containing settings for data generation.")]
    public DatasetInformation dataset;

    [Header("Scene Settings")]
    [Tooltip("Box colider defining 3D range where to spawn new objects.")]
    public GameObject spawnZone;
    private Collider spawnZoneCollider; 
    [Tooltip("Target position for camera lookat.")]
    public Transform poi;
    [Tooltip("Table or other additional/supporting object in scene.")]
    public GameObject table = null;
    [Tooltip("Projector light source to.")]
    public Light projector = null;

    //[Header("Target Render Textures To Save")]
    [HideInInspector] public RenderTexture renderTexture;
    [HideInInspector] public RenderTexture segmentationTexture;
    //public RenderTexture depthTexture;

    //[Header("Current Viewpoints")]
    //public float radius = 1.0f;
    //public float speed;
    //public float phi = 0.0f;
    //public float theta = 0.0f;

    //help classes to divide functionality
    private SceneHandler sceneHandler;
    private ExportHandler exportHandler;
    private MaterialRandomizeHandler materialRandomizeHandler;

    // List variation in textures, models, materials and table materials. Idea is to pick randomly from these lists
    private Cubemap[] cubeMaps = new Cubemap[0];
    private Texture[] projectorMaps = new Texture[0];
    private GameObject[] models = new GameObject[0];
    private Material[] materials = new Material[0];
    private Material[] materialsTable = new Material[0];
    //private UnityEngine.Object[] detailMaps;
    public bool randomFalseColors = true;

    public GameObject lightSourcePrefab;
    private List<GameObject> instantiatedLightSources;

    // Randomely generated models
    private List<GameObject> instantiatedModels;
    private List<GameObject> taggedModels;
    private List<string> instantiatedModelsNames;

    private BOPDatasetExporter.Scene bopScene;
    private BOPDatasetExporter.SceneIterator bopSceneIterator;

    private RandomNumberGenerator rng;
    private int currentFrame = 0;
    private Vector3 up = new Vector3(0.0f, 1.0f, 0.0f);
    //private Material tableMat;

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

    //functions linked to the gui
    public void SaveObjectColors() { if (dataset.exportModelsByTag) exportHandler.SaveObjectColors(taggedModels); else exportHandler.SaveObjectColors(instantiatedModels); }
    public void SaveMitsuba() { exportHandler.SaveMitsuba(instantiatedModels, table); }
    public void RandomizeMaterials() { materialRandomizeHandler.RandomizeMaterials(instantiatedModels, materials); materialRandomizeHandler.RandomizeMaterialObject(materials); }
    public void ToggleRecording() { exportHandler.ToggleRecording(); }

    // Start is called before the first frame update
    void Start()
    {
 
        rng = new RandomNumberGenerator(dataset.seed);


        if (!checkDatasetSettings())
        {
            UnityEditor.EditorApplication.isPlaying = false;
            return;
        }

        if (dataset.varyingMaterial)
            dataset.backupColor = dataset.varyingMaterial.GetColor("_BaseColor");


        //fileCounter = dataset.startFileCounter;

        if(dataset.environmentsPath != "")
            cubeMaps = Resources.LoadAll(dataset.environmentsPath, typeof(Cubemap)).Cast<Cubemap>().ToArray();
        if (dataset.projectorTexturePath != "")
            projectorMaps = Resources.LoadAll(dataset.projectorTexturePath, typeof(Texture)).Cast<Texture>().ToArray();
        if (dataset.modelsPath != "")
        {
            models = Resources.LoadAll(dataset.modelsPath, typeof(GameObject)).Cast<GameObject>().ToArray();
            BOPDatasetExporter.addPrefabIds(models);
        }
        if (dataset.materialsPath != "")
            materials = Resources.LoadAll(dataset.materialsPath, typeof(Material)).Cast<Material>().ToArray();
        if (dataset.tableMaterialsPath != "")
            materialsTable = Resources.LoadAll(dataset.tableMaterialsPath, typeof(Material)).Cast<Material>().ToArray();

        if (cubeMaps.Length == 0)
        {
            Debug.LogWarning("No environment maps found in " + dataset.environmentsPath);
        }
        if (models.Length == 0)
        {
            Debug.LogWarning("No models maps found in " + dataset.modelsPath);
        }
        if (materials.Length == 0)
        {
            Debug.LogWarning("No materials maps found in " + dataset.materialsPath);
        }
        if (materialsTable.Length == 0)
        {
            Debug.LogWarning("No materialsTable maps found in " + dataset.tableMaterialsPath);
        }

        if (dataset.importFromBOP)
        {
            bopScene = BOPDatasetExporter.Load(dataset.outputPath);
            dataset.numberOfSamples = bopScene.poses.Count;
            bopSceneIterator = new BOPDatasetExporter.SceneIterator(bopScene);
        }


        instantiatedModels = new List<GameObject>();
        if (dataset.exportModelsByTag)
            taggedModels = new List<GameObject>(GameObject.FindGameObjectsWithTag("ExportInstanceInfo"));
        
        instantiatedModelsNames = new List<string>();
        instantiatedLightSources = new List<GameObject>();
        
        materialRandomizeHandler = new MaterialRandomizeHandler(dataset);
        exportHandler = new ExportHandler(dataset);
        sceneHandler = new SceneHandler(dataset);
        sceneHandler.exporter = exportHandler;
        sceneHandler.RenderAtHighQuality(true);
        
        //if (dataset.autoCameraExposure)
        //{
        //    Exposure exp = null;
        //    postProcessing.profile.TryGet<Exposure>(out exp);
        //    exp.mode = new ExposureModeParameter(ExposureMode.Automatic, true);
        //}
        //else
        //{
        //    Exposure exp = null;
        //    postProcessing.profile.TryGet<Exposure>(out exp);
        //    exp.mode = new ExposureModeParameter(ExposureMode.Fixed, true);
        //}


        spawnZoneCollider = spawnZone.GetComponent<Collider>();

        up = new Vector3(0.0f, 1.0f, 0.0f);


        FullRandomize();
    }

    private bool checkDatasetSettings()
    {

        if (dataset == null)
        {
            Debug.LogError("No dataset selected. Please select a folder of the Resources directory.");
            return false;
        }


        if (string.IsNullOrEmpty(dataset.outputPath) || !Directory.Exists(dataset.outputPath))
        {
            if (!string.IsNullOrEmpty(dataset.outputPath) && EditorUtility.DisplayDialog("output path", "The output directory does not exists, do you want to create it?\nOutput path: " + Path.GetFullPath(dataset.outputPath), "Create directory", "Terminate program"))
            {
                try
                {
                    Directory.CreateDirectory(dataset.outputPath);
                }
                catch (Exception e)
                {
                    Debug.LogError("Output directory creation failed. " + e.Message);
                    return false;
                }
            }
            else
            {
                Debug.LogError("Output path for generated data not specified or does not exist");
                return false;
            }
        }

        if (string.IsNullOrEmpty(dataset.annotationisFile))
        {
            Debug.LogError("Annotations file is not set properly");
            return false;
        }


        if (!Directory.Exists(dataset.outputPath + "images/"))
        {
            Directory.CreateDirectory(dataset.outputPath + "images/");
        }

        if (!Directory.Exists(dataset.outputPath + "segmentation/"))
        {
            Directory.CreateDirectory(dataset.outputPath + "segmentation/");
        }

        if (!Directory.Exists(dataset.outputPath + "depth/"))
        {
            Directory.CreateDirectory(dataset.outputPath + "depth/");
        }

        if (!Directory.Exists(dataset.outputPath + "metaData/"))
        {
            Directory.CreateDirectory(dataset.outputPath + "metaData/");
        }

        return true;
    }

    //void LateUpdate()
    //{

    //    var buffer = new CommandBuffer();
    //    buffer.SetViewMatrix(bopScene.poses[0].worldToCam);
    //    mainCamera.AddCommandBuffer(CameraEvent.BeforeSkybox, buffer);

    //    mainCamera.worldToCameraMatrix = bopScene.poses[0].worldToCam;
    //}

    // Inputis camera to world transform, so inverse of view matrix or inverse of mainCamera.transform.worldToLocalMatrix
    private void SetTransformFromMatrix(Transform transform, UnityEngine.Matrix4x4 localToWorld)
    {
        transform.SetPositionAndRotation(Utils.GetTranslation(localToWorld), Utils.GetRotation(localToWorld));
    }

    void Update()
    {
        
        
        if (exportHandler != null && exportHandler.capturing)//export handler is not yet created if start script is waiting for user input (create directory popup)
        {
            if (currentFrame == 0)
                sceneHandler.RenderAtHighQuality(false, true);
            if (currentFrame == dataset.numPhysicsFrames)
            {
                sceneHandler.RenderAtHighQuality(true, dataset.stopSimulationTimeCompletly);
            }

            if (currentFrame == dataset.numRenderFrames + dataset.numPhysicsFrames)
            {
                if (!dataset.exportModelsByTag)
                    StartCoroutine(exportHandler.Capture(instantiatedModels, table));
                else
                    StartCoroutine(exportHandler.Capture(taggedModels, table));
            }
            else if (currentFrame > dataset.numRenderFrames + dataset.numPhysicsFrames) // update randomize the frame after the save frame to make sure save is completed correctly
            {
                FullRandomize();
                currentFrame = 0;
                sceneHandler.RenderAtHighQuality(false, true);//prevent first frame from being renderd on high resolution, otherwise the path tracer wil use this frame and give a blurry result
                return;//dont start frame counter on 1
            }
            currentFrame++;
        }
        else if (currentFrame != -1)
        {
            sceneHandler.RenderAtHighQuality(true, false);
            currentFrame = -1;
        }

    }

    public void FullRandomize()
    {
        if (dataset.modelVariatons)
            RandomizeModels();
        if (dataset.viewpointVariatons)
            RandomizeView();
        if (dataset.environmentVariatons)
            RandomizeEnvironment();
        if (dataset.lightsourceVariatons)
            RandomizeLightSources();
        if (dataset.materialVariatons)
            materialRandomizeHandler.RandomizeMaterials(instantiatedModels, materials);
        if (dataset.tableVariatons)
            RandomizeTable();
        if (dataset.overrideWithRandomMaterialColor)
            materialRandomizeHandler.RandomizeMaterialObject(materials);

        
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
        sceneHandler.setupFalseColorStack(instantiatedModels.Count);
        if (dataset.importFromBOP)
        {
            bopSceneIterator.Next();
        }
    }

    public GameObject FindModel(int id)
    {
        String name = String.Format("obj_{0:000000}", id);
        foreach (GameObject model in models)
        {
            Debug.Log(model.name);
            if (model.name == name)
                return model;
        }
        Debug.LogError("Model with name " + name + " not found.");
        return null;
        
    }
    public void RandomizeModels()
    {
        DestroyModels();

        if (dataset.importFromBOP)
        {
            List<BOPDatasetExporter.Model> bopModels = bopSceneIterator.GetPose().models;
            for (int i = 0; i < bopModels.Count; ++i) {
                GameObject model = FindModel(bopModels[i].obj_id);
             
                //model = (GameObject)models[0]; // TODO, support instances of different types, now only scenes with the same prefab are supported
                SpawnModelAtExactPosition(model, i, Utils.GetTranslation(bopModels[i].localToWorld), Utils.GetRotation(bopModels[i].localToWorld));
            }

            mainCamera.projectionMatrix = BOPDatasetExporter.ConvertOpencvToUnityProjectionmatrix(bopSceneIterator.GetPose().projMat, dataset.resolutionWidth, dataset.resolutionHeight, 0.01f, 1000.0f);
        }
        else if (dataset.uniqueObjects)
        {
            for (int i = 0; i < models.Length; ++i)
            {
                GameObject model = (GameObject)models[i];
                if(!SpawnModel(model, i))
                {
                    DestroyModels();
                    i = -1;
                }
            }
        }
        else
        {
            for (int i = 0; i < dataset.numRandomObjects; ++i)
            {
                if (models.Length > 0)
                {
                    GameObject model = models[rng.IntRange(0, models.Length)];
                    if (!SpawnModel(model, i))
                    {
                        DestroyModels();
                        i = -1;
                    }
                }
                else {
                    Debug.LogError("Spawning objects but no models are loaded. Check the model path in the dataset file");
                    break;
                }
            }
        }
    }

    void DestroyModels()
    {
        foreach (GameObject model in instantiatedModels)
        {
            //foreach (Transform child in model.transform)
            //{
            //    GameObject.Destroy(child.gameObject);
            //}

            GameObject.Destroy(model);



        }
        instantiatedModels.Clear();
        instantiatedModelsNames.Clear();
    }


    private void SpawnModelAtExactPosition(GameObject model, int index, Vector3 spawnPosition, Quaternion spawnRotation)
    {
        GameObject clone = createInstance(model, spawnPosition, spawnRotation);
        clone.name = model.name;
        if (!dataset.exportPredefinedFalseColors)
            addFalseColor(clone, index);
        instantiatedModels.Add(clone);
    }

    private bool SpawnModel(GameObject model, int index)
    {
        Vector3 spawnPosition;

        if (dataset.randomModelTranslations)
        {
            //int layerMask = LayerMask.GetMask("Prefabs");
            spawnPosition = model.transform.position + RandomPointInSpawnZone(); // generate random position in spawn zone    //spawnPosition = spawnPosition - model.transform.GetChild(0).GetComponent<Collider>().bounds.center; //translate center of object to position of spawn zone
            //while (true)
            //{
            //    if ((Physics2D.OverlapCircle(spawnPosition, 0.1f, layerMask, 0, 0)) == null)
            //    {
            //        break;
            //    }
            //    else
            //    {
            //        Debug.Log("iverlap");
            //        spawnPosition = model.transform.position + RandomPointInSpawnZone();
            //    }
            //}
        }
        else
            spawnPosition = model.transform.position;


        Quaternion spawnRotation;
        if (dataset.randomModelRotations)
            spawnRotation = rng.Rotation();
        else
            spawnRotation = model.transform.rotation;
        if (dataset.randomRotationOffset)
            spawnRotation *= Quaternion.AngleAxis(rng.Range(-dataset.randomRotationOffsetValue, dataset.randomRotationOffsetValue), Vector3.forward);

        GameObject clone = createInstance(model, spawnPosition, spawnRotation);
        if (!clone)
            return false;
        //var clonename = clone.name;
        clone.name = model.name;
        //var id = model.GetInstanceID();
        //var layer = model.layer;
        //var name = model.name;
        //var tag = model.tag;

        if (!dataset.exportPredefinedFalseColors)
            addFalseColor(clone, index);

        if (dataset.randomSubModelTranslation)
        {
            Transform childTrans = clone.transform.Find(dataset.subModelName);
            if (childTrans != null)
            {
                childTrans.position = childTrans.position + new Vector3(rng.Range(-dataset.subModelOffset.x, dataset.subModelOffset.x), rng.Range(-dataset.subModelOffset.y, dataset.subModelOffset.y), rng.Range(-dataset.subModelOffset.z, dataset.subModelOffset.z));
            }
            else
            {
                Debug.Log("Could not find submodel with name" + dataset.subModelName);
            }
        }

        //if (clone.GetComponent<MeshRenderer>())
        //{
        //    FalseColor falseColor = clone.AddComponent<FalseColor>();
        //    falseColor.falseColor = new Color(0f, 1.0f, 0f, 1f);
        //}
        //foreach (Transform child in clone.transform)
        //{
        //    if (child.gameObject.GetComponent<MeshRenderer>())
        //    {
        //        FalseColor falseColor = child.gameObject.AddComponent<FalseColor>();
        //        falseColor.falseColor = new Color(0f, 1.0f, 0f, 1f);
        //    }
        //}

        instantiatedModels.Add(clone);
        return true;
    }

    public void addFalseColor(GameObject subject, int index)
    {
        // add false color script to all children who has a 
        //Note however, that results from GetComponentsInChildren will also include that component from the object itself.So the name is slightly misleading - it should be thought of as "Get Components from Self And Children"!
        Transform[] allChildren = subject.GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren)
        {
            if (child.gameObject.GetComponent<MeshRenderer>())
            {
                if (randomFalseColors)
                {
                    FalseColor falseColor = child.gameObject.AddComponent<FalseColor>();
                    if (falseColor)
                    {
                        //Color color = ColorEncoding.EncodeIDAsColor(id);
                        Color color = ColorEncoding.EncodeColorByIndex(index);
                        //Color color = ColorEncoding.EncodeNameAsColor(clonename);//name);
                        //Color32 writeColor = color;
                        //writer.Write(name + "," + writeColor.r+ "," + writeColor.g+ "," + writeColor.b+ "\n");// + "," + eachChild.position.x + "," + eachChild.position.y + "," + eachChild.position.z + "\n");

                        //Color color = ColorEncoding.EncodeLayerAsColor(layer, false);
                        falseColor.SetColor(color);
                        falseColor.objectId = index;
                    }
                    else
                        Debug.Log("Could not attach false color to gameobject" + child.gameObject);
                }
                else
                {
                    if (child.gameObject.GetComponent<FalseColor>() == null)
                    {
                        FalseColor falseColor = child.gameObject.AddComponent<FalseColor>();
                        if (falseColor)
                        {
                            falseColor.objectId = index;
                            falseColor.SetColor(Color.black);
                        }
                        else
                            Debug.Log("Could not attach false color to gameobject" + child.gameObject);
                    }
                }



            }
        }
    }
    private Vector3 RandomPointInSpawnZone(float scale = 1.0f)
    {
        return spawnZoneCollider.bounds.center + new Vector3(
           (rng.Next() - 0.5f) * (spawnZoneCollider.bounds.size.x * scale),
           (rng.Next() - 0.5f) * spawnZoneCollider.bounds.size.y,
           (rng.Next() - 0.5f) * (spawnZoneCollider.bounds.size.z * scale)
        );
    }

    private GameObject createInstance(GameObject prefab, Vector3 spawnPosition, Quaternion spawnRotation)
    {
        GameObject spawnObject = (GameObject)Instantiate(prefab, spawnPosition, spawnRotation);//Quaternion.identity);
                                                                                               //Material mat = (Material)materials[rng.IntRange(0, materials.Length)];
                                                                                               //clone.GetComponent<Renderer>().material = mat;
                                                                                               //clone.layer = 12;
                                                                                               //foreach (Transform t in clone.transform)
                                                                                               //{
                                                                                               //    t.gameObject.layer = 12;
                                                                                               //}
        if (!dataset.avoidCollisions)
            return spawnObject;

        //spawnObject.AddComponent<collisionAvoidance>();
        //spawnObject.tag = "avoidCollision";
        
        
        BoxCollider collider = spawnObject.GetComponent<BoxCollider>();
        //int layerMask = LayerMask.GetMask("Prefabs");
        bool intersects = CheckIntersection(collider);
        //Debug.Log(intersects);
        int fails = 0;
        while (intersects)
        {
            spawnPosition = prefab.transform.position + RandomPointInSpawnZone();
            GameObject.DestroyImmediate(spawnObject);
            spawnObject = (GameObject)Instantiate(prefab, spawnPosition, spawnRotation);
        
            collider = spawnObject.GetComponent<BoxCollider>();
            intersects = CheckIntersection(collider) && fails < 50;
            fails++;
        }
        if (fails > 50)
        {
            GameObject.Destroy(spawnObject);
            return null;
        }
        return spawnObject;

        
        //    Collider[] colliders = Physics.OverlapBox(collider.bounds.center, collider.bounds.size * 0.5f, spawnRotation, layerMask);

        //Debug.Log(collider.bounds.center);
        //// Find new position if object collides with existing object (>1 ignores self collision)
        //if (colliders.Length > 1)
        //{
        //    spawnPosition = model.transform.position + RandomPointInSpawnZone();
        //    clone.transform.position = spawnPosition;

        //    colliders = Physics.OverlapBox(collider.bounds.center, collider.bounds.size * 0.5f, spawnRotation, layerMask);
        //    Debug.Log(collider.bounds.center);
        //    Debug.Log(colliders.Length);
    }

    private bool CheckIntersection(BoxCollider collider)
    {
        bool intersects = false;
        foreach (GameObject other in instantiatedModels)
        {
            BoxCollider other_collider = other.GetComponent<BoxCollider>();
            if (collider.bounds.Intersects(other_collider.bounds))
            {
                intersects = true;
                break;
            }
        }
        return intersects;
    }


    public void RandomizeLightSources()
    {
        foreach (GameObject lightsource in instantiatedLightSources)
        {
            GameObject.Destroy(lightsource);
        }
        instantiatedLightSources.Clear();


        for (int i = 0; i < dataset.numLightsources; ++i)
        {
            float phi = rng.Range(dataset.minPhiLight, dataset.maxPhiLight);
            float theta = rng.Range(dataset.minThetaLight, dataset.maxThetaLight);
            float radius = rng.Range(dataset.minRadiusLight, dataset.maxRadiusLight);

            Vector3 offset = SphericalCoordinates.SphericalToCartesian(phi * Mathf.PI / 180.0f, theta * Mathf.PI / 180.0f, radius);
            Vector3 spawnPosition = poi.position + offset;

            GameObject spawnObject = (GameObject)Instantiate(lightSourcePrefab);
            spawnObject.transform.position = spawnPosition;
            spawnObject.transform.LookAt(poi);
            instantiatedLightSources.Add(spawnObject);
        }
    }
    
    public void RandomizeView()
    {
        if (dataset.importFromBOP)
        {
            SetTransformFromMatrix(mainCamera.transform, bopSceneIterator.GetPose().worldToCam.inverse);
        }
        else
        {
            float phi = rng.Range(dataset.minPhi, dataset.maxPhi);
            float theta = rng.Range(dataset.minTheta, dataset.maxTheta);
            float radius = rng.Range(dataset.minRadius, dataset.maxRadius);

            if (dataset.randomYUp)
                up.y = rng.RandomSign();

            Vector3 offset = SphericalCoordinates.SphericalToCartesian(phi * Mathf.PI / 180.0f, theta * Mathf.PI / 180.0f, radius);
            mainCamera.transform.position = poi.position + offset;
            mainCamera.transform.LookAt(poi, up);
        }
    }

    public void RandomizeEnvironment()
    {
        HDRISky sky = null;
        if (SceneHandler.renderSettings != null)
            SceneHandler.renderSettings.GetComponent<Volume>()?.profile.TryGet<HDRISky>(out sky);

        // change texture on cube
        if (cubeMaps.Length > 0)
        {
            Cubemap texture = cubeMaps[rng.IntRange(0, cubeMaps.Length)];
            
            sky.hdriSky.value = texture;
        }
        

        if (dataset.randomEnvironmentRotations)
        {
            sky.rotation.value = rng.Angle();
        }

        if (dataset.randomExposuresEnvironment)
        {
            sky.exposure.value = rng.Range(dataset.minExposure, dataset.maxExposure);
        }

        if (dataset.applyProjectorVariations && projector && projectorMaps.Length > 0)
        {
            projector.cookie = (Texture) projectorMaps[rng.IntRange(0, projectorMaps.Length)];
            projector.color = Color.HSVToRGB(rng.Next(), rng.Next(),1.0f);
        }
    }

    public void RandomizeTable()
    {
        bool foundRenderer = false;
        if (table)
        {
            if(materialsTable.Length <= 0)
            {
                Debug.LogWarning("No materials loaded for the table");
                return;
            }
            Material mat = (Material)materialsTable[rng.IntRange(0, materialsTable.Length)];
            Renderer rend = table.GetComponent<Renderer>();

            if (rend != null)
            {
                rend.material = mat;
                foundRenderer = true;

                if (dataset.overrideTableRandomMaterialColor)
                {
                    MaterialPropertyBlock newProperties = new MaterialPropertyBlock();
                    rend.GetPropertyBlock(newProperties);

                    materialRandomizeHandler.ApplyHSVDOffsets(rend, newProperties, dataset.minColorTable, dataset.maxColorTable);
                    rend.SetPropertyBlock(newProperties);
                }
            }

            foreach (Transform child in table.transform)
            {
                Renderer rendChild = child.gameObject.GetComponent<Renderer>();
                if (rendChild != null)
                {
                    rendChild.material = mat; // change material to other material

                    if (dataset.overrideTableRandomMaterialColor)
                    {
                        MaterialPropertyBlock newProperties = new MaterialPropertyBlock();
                        rendChild.GetPropertyBlock(newProperties);

                        materialRandomizeHandler.ApplyHSVDOffsets(rend, newProperties, dataset.minColorTable, dataset.maxColorTable);
                        rendChild.SetPropertyBlock(newProperties);
                    }
                    foundRenderer = true;
                }
            }


            if(!foundRenderer)
            {
                Debug.LogWarning("could not find renderer component of table object");
            }
        }
    }

    void OnApplicationQuit()
    {
        if (dataset.varyingMaterial)
            dataset.varyingMaterial.SetColor("_BaseColor", dataset.backupColor);
    }

    void OnDestroy()
    {
        if (dataset.varyingMaterial)
            dataset.varyingMaterial.SetColor("_BaseColor", dataset.backupColor);
    }
}
