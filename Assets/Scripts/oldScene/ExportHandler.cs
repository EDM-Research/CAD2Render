using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;
using UnityEditor;

[Obsolete("Used by the old scene, Use the new scene instead")]
public class ExportHandler
{
    ImageSaver imageSaver;
    private DatasetInformation dataset;
    public bool capturing {get; private set;}

    //private GameObject EnvironmentSettings;
    private GameObject renderSettings;
    //private GameObject raytracingSettings;
    //private GameObject postProcesingSettings;

    //gui references
    private Button recordButton;
    private Text text_captureID;

    private Camera mainCamera;
    public int fileCounter { get; private set; }

    private RenderTexture _renderTexture;
    public RenderTexture renderTexture { get { return _renderTexture; }
                                         set { _renderTexture = value;
                                               SaveIntrinsicMatrix();}
                                        }
    public RenderTexture segmentationTexture { get; set; }
    public RenderTexture segmentationTextureArray { get; set; }

    [Serializable]
    private struct SaveModel
    {

        public string name;
        public int instance;
        public float[] locToWorld;
        public float[] imgPos; // 2d image position
        public bool occluded;// waypoint occluded or not

    }
    [Serializable]
    private struct SaveObject
    {
        public int id;
        public float[] proj;
        public float[] worldToCam;
        public List<SaveModel> models;
    }


    public ExportHandler(DatasetInformation p_dataset)
    {
        dataset = p_dataset;
        imageSaver = new ImageSaver(dataset.resolutionWidth, dataset.resolutionHeight);
        LinkGUICaptureCounter();
        mainCamera = Camera.main;
        fileCounter = dataset.startFileCounter;
        UpdateFileCounterGUI(fileCounter);

        // Create empty annotations file
        StreamWriter writer = new StreamWriter(dataset.outputPath + dataset.annotationisFile, false);
        writer.Close();

        var temp = GameObject.FindGameObjectWithTag("EnvironmentSettings");
        if (temp != null)
        {
            renderSettings = temp.transform.Find("Rendering Settings")?.gameObject;
            //raytracingSettings = temp[0].transform.Find("Ray Tracing Settings")?.gameObject;
            //postProcesingSettings = temp[0].transform.Find("PostProcessing")?.gameObject;
        }
        ExportDatasetInfo();

        if (dataset.exportToBOP)
        {
            BOPDatasetExporter.SetupExportPath(dataset.outputPath, dataset.BOPSceneId);
        }
    }

    private void LinkGUICaptureCounter()
    {
        var capturingGameObject = GameObject.Find("Capturing");
        recordButton = capturingGameObject?.transform.Find("Capturing_Button")?.GetComponent<Button>();
        //if (recordButton)
        //    recordButton.onClick.AddListener(ToggleRecording);
        //else
        if (!recordButton)
            Debug.LogError("Record button not found");
        text_captureID = capturingGameObject?.transform.Find("Text_captureID")?.GetComponent<Text>();
        if(!text_captureID)
            Debug.LogError("Record id counter not found");
    }

    public void SaveIntrinsicMatrix()
    {
        double ImageSizeX = renderTexture.width;
        double ImageSizeY = renderTexture.height;
        double fovY = mainCamera.fieldOfView;
        double fovX = Camera.VerticalToHorizontalFieldOfView((float) fovY, (float) ImageSizeX / (float) ImageSizeY);

        double Fx, Fy, Cx, Cy;
        Fx = (ImageSizeX / (2 * Math.Tan(fovX * Math.PI / 360)));
        Fy = (ImageSizeY / (2 * Math.Tan(fovY * Math.PI / 360)));
        Cx = ImageSizeX / 2;
        Cy = ImageSizeY / 2;

        StreamWriter writer = new StreamWriter(dataset.outputPath + "camera.txt", false);
        writer.WriteLine(Fx + ", 0, " + Cx);
        writer.WriteLine("0, " + Fy + ", " + Cy);
        writer.WriteLine("0, 0, 1");

        writer.Flush();
        writer.Close();
    }
    public void SaveMitsuba(List<GameObject> instantiatedModels, GameObject table)
    {
        HDRISky sky;
        renderSettings.GetComponent<Volume>().profile.TryGet<HDRISky>(out sky);

        string environmentMap = UnityEditor.AssetDatabase.GetAssetPath(sky.hdriSky.value);
        float env_exposure = 2.0f * (sky.exposure.value + 1.0f);
        float env_angle = sky.rotation.value;


        MitusbaExporter.SaveMitsuba(instantiatedModels, fileCounter, dataset.outputPath, mainCamera, renderTexture.width, renderTexture.height, dataset.mitsubaSampleCount, environmentMap, env_exposure, env_angle, table);
    }

    public IEnumerator Capture(List<GameObject> instantiatedModels, GameObject table)
    {
        CaptureAnnotations(instantiatedModels);

        if (dataset.exportToMitsuba)
            SaveMitsuba(instantiatedModels, table);

        if (dataset.exportToBOP)
            BOPDatasetExporter.exportFrame(instantiatedModels, renderTexture, segmentationTexture, segmentationTextureArray, fileCounter, dataset.outputPath, mainCamera, imageSaver);

        //StartCoroutine(
        UpdateFileCounterGUI(fileCounter);
        yield return new WaitForEndOfFrame();
        //WaitForEndOfFrameAndSave(filenameExtension, fileCounter);//);
        Save(fileCounter);
        fileCounter++;

        if (fileCounter == dataset.numberOfSamples)
            ToggleRecording();
    }

    //private IEnumerator WaitForEndOfFrameAndSave(string filenameExtension, int fileCounter)
    //{
    //    yield return new WaitForEndOfFrame();
    //    Save(filenameExtension, fileCounter);
    //
    //
    //}

    private void Save(int fileCounter)
    {
        imageSaver.Save(mainCamera.targetTexture, dataset.outputPath + "images/" + fileCounter + "_img", dataset.outputExt, dataset.applyGammaCorrection);
        if (segmentationTexture)
            imageSaver.Save(segmentationTexture, dataset.outputPath + "segmentation/" + fileCounter + "_seg", dataset.outputExt, false);
        //if (depthTexture)
        //    ImageSaver.SaveDepth(depthTexture, dataset.outputPath + "depth/" + fileCounter + "_depth" + "." + "png", depthTexture.width, depthTexture.height, true, false, DatasetInformation.Extension.png);
    }

    private void UpdateFileCounterGUI(int id)
    {
        if (text_captureID)
            text_captureID.text = id.ToString();
    }

    public void ToggleRecording()
    {
        capturing = !capturing;

        string buttonText = "";
        Color buttonColor;

        if (capturing)
        {
            buttonText = "Stop\nRecording";
            buttonColor = Color.HSVToRGB(0 / 360.0f, 24 / 100.0f, 100 / 100.0f);
        }
        else
        {
            buttonText = "Start\nRecording";
            buttonColor = Color.HSVToRGB(102 / 360.0f, 24 / 100.0f, 100 / 100.0f);

        }

        if (recordButton)
        {
            recordButton.GetComponentInChildren<Text>().text = buttonText;
            ColorBlock colors = recordButton.colors;
            colors.normalColor = buttonColor;
            colors.selectedColor = buttonColor;
            recordButton.colors = colors;
        }


    }

	private void ExportDatasetInfo()
    {
        StreamWriter writer = new StreamWriter(dataset.outputPath + "metaData/versionInfo.json", false);
        var version = PlanetaGameLabo.UnityGitVersion.GitVersion.version;
        writer.WriteLine(JsonUtility.ToJson(version, true));
        writer.WriteLine();
        writer.Flush();
        writer.Close();

        if(dataset.renderProfile)
            System.IO.File.Copy(AssetDatabase.GetAssetPath(dataset.renderProfile), dataset.outputPath + "metaData/renderProfile.asset", true);
        if (dataset.rayTracingProfile)
            System.IO.File.Copy(AssetDatabase.GetAssetPath(dataset.rayTracingProfile), dataset.outputPath + "metaData/rayTracingProfile.asset", true);
        if (dataset.postProcesingProfile)
            System.IO.File.Copy(AssetDatabase.GetAssetPath(dataset.renderProfile), dataset.outputPath + "metaData/postProcesingProfile.asset", true);
        System.IO.File.Copy(AssetDatabase.GetAssetPath(dataset), dataset.outputPath + "metaData/dataset.asset", true);
    }

    private bool checkKeypointVisibility(Mesh keypoint, Transform child)
    {
        var vertices = keypoint.vertices;

        foreach (Vector3 vertex in vertices)
        {
            Vector3 worldPt = child.TransformPoint(vertex);

            //RaycastHit hit;
            // Calculate Ray direction
            Vector3 direction = mainCamera.transform.position - worldPt;

            RaycastHit[] hits;
            hits = Physics.RaycastAll(worldPt, direction, 100.0F);

            bool vertexVisible = true;
            foreach (RaycastHit hit in hits)
            {
                //Debug.Log(hit.collider.name);
                if (hit.collider.name == "default")
                {
                    vertexVisible = false;
                    break;
                }
                //Debug.Log(hit.collider.name);
            }
            if (vertexVisible)
            {
                return true;
            }
            //if (Physics.Raycast(worldPt, direction, out hit))
            //{
            //    Debug.Log(hit.collider.tag);
            //    if (hit.collider.tag == "MainCamera")
            //    {
            //        return true;
            //    }
            //}
        }

        return false;
    }

    private void CaptureAnnotations(List<GameObject> instantiatedModels)
    {
        // open the annotations file
        StreamWriter writer = new StreamWriter(dataset.outputPath + dataset.annotationisFile, true);


        SaveObject AnnotationData = new SaveObject();
        AnnotationData.id = fileCounter;
        AnnotationData.proj = new float[16] {   mainCamera.projectionMatrix[0,0], mainCamera.projectionMatrix[1, 0], mainCamera.projectionMatrix[2, 0], mainCamera.projectionMatrix[3, 0],
                                            mainCamera.projectionMatrix[0,1], mainCamera.projectionMatrix[1, 1], mainCamera.projectionMatrix[2, 1], mainCamera.projectionMatrix[3, 1],
                                            mainCamera.projectionMatrix[0,2], mainCamera.projectionMatrix[1, 2], mainCamera.projectionMatrix[2, 2], mainCamera.projectionMatrix[3, 2],
                                        mainCamera.projectionMatrix[0,3], mainCamera.projectionMatrix[1, 3], mainCamera.projectionMatrix[2, 3], mainCamera.projectionMatrix[3, 3]};

        //testObject.worldToCam = new float[16] {     Camera.worldToCameraMatrix[0,0], Camera.worldToCameraMatrix[1, 0], Camera.worldToCameraMatrix[2, 0], Camera.worldToCameraMatrix[3, 0],
        //                                            Camera.worldToCameraMatrix[0,1], Camera.worldToCameraMatrix[1, 1], Camera.worldToCameraMatrix[2, 1], Camera.worldToCameraMatrix[3, 1],
        //                                            Camera.worldToCameraMatrix[0,2], Camera.worldToCameraMatrix[1, 2], Camera.worldToCameraMatrix[2, 2], Camera.worldToCameraMatrix[3, 2],
        //                                            Camera.worldToCameraMatrix[0,3], Camera.worldToCameraMatrix[1, 3], Camera.worldToCameraMatrix[2, 3], Camera.worldToCameraMatrix[3, 3]};


        AnnotationData.worldToCam = new float[16] {     mainCamera.transform.worldToLocalMatrix[0,0], mainCamera.transform.worldToLocalMatrix[1, 0], mainCamera.transform.worldToLocalMatrix[2, 0], mainCamera.transform.worldToLocalMatrix[3, 0],
                                                    mainCamera.transform.worldToLocalMatrix[0,1], mainCamera.transform.worldToLocalMatrix[1, 1], mainCamera.transform.worldToLocalMatrix[2, 1], mainCamera.transform.worldToLocalMatrix[3, 1],
                                                    mainCamera.transform.worldToLocalMatrix[0,2], mainCamera.transform.worldToLocalMatrix[1, 2], mainCamera.transform.worldToLocalMatrix[2, 2], mainCamera.transform.worldToLocalMatrix[3, 2],
                                                    mainCamera.transform.worldToLocalMatrix[0,3], mainCamera.transform.worldToLocalMatrix[1, 3], mainCamera.transform.worldToLocalMatrix[2, 3], mainCamera.transform.worldToLocalMatrix[3, 3]};


        AnnotationData.models = new List<SaveModel>();
        for (int i = 0; i < instantiatedModels.Count; ++i)
        //foreach (GameObject model in instantiatedModels)
        {
            UnityEngine.GameObject model = instantiatedModels[i];

            // You have to choose between exporting submodels (children) or parent models
            if (dataset.exportSubModels)
            {
                Transform[] allChildren = model.GetComponentsInChildren<Transform>();
                foreach (Transform child in allChildren)
                {
                    SaveModel saveModelChild = new SaveModel();// (model.name);//, model.transform.localToWorldMatrix);



                    //Matrix4x4 modelTrans = recordButton.GetComponentInChildren<Renderer>().localToWorldMatrix;

                    // Save column first
                    saveModelChild.locToWorld = new float[16] {     child.transform.localToWorldMatrix[0,0], child.transform.localToWorldMatrix[1, 0], child.transform.localToWorldMatrix[2, 0], child.transform.localToWorldMatrix[3, 0],
                                                                    child.transform.localToWorldMatrix[0,1], child.transform.localToWorldMatrix[1, 1], child.transform.localToWorldMatrix[2, 1], child.transform.localToWorldMatrix[3, 1],
                                                                    child.transform.localToWorldMatrix[0,2], child.transform.localToWorldMatrix[1, 2], child.transform.localToWorldMatrix[2, 2], child.transform.localToWorldMatrix[3, 2],
                                                                    child.transform.localToWorldMatrix[0,3], child.transform.localToWorldMatrix[1, 3], child.transform.localToWorldMatrix[2, 3], child.transform.localToWorldMatrix[3, 3]};




                    //Matrix4x4 localToCam = Camera.worldToCameraMatrix * model.transform.localToWorldMatrix; // calculates relative transformations of local object in camera space

                    //saveModel.locToCam = new float[16] {   localToCam[0,0], localToCam[1, 0], localToCam[2, 0], localToCam[3, 0],
                    //                                       localToCam[0,1], localToCam[1, 1], localToCam[2, 1], localToCam[3, 1],
                    //                                       localToCam[0,2], localToCam[1, 2], localToCam[2, 2], localToCam[3, 2],
                    //                                       localToCam[0,3], localToCam[1, 3], localToCam[2, 3], localToCam[3, 3]};

                    saveModelChild.name = child.name;
                    saveModelChild.instance = i;


                    if (dataset.exportImagePosition)
                    {
                        Vector3 screenPos = mainCamera.WorldToScreenPoint(child.position);
                        saveModelChild.imgPos = new float[2] { screenPos.x, screenPos.y };
                        saveModelChild.occluded = false;
                        //writer.Write(fileCounter + "," + screenPos.x + "," + screenPos.y + "\n");// + "," + eachChild.position.x + "," + eachChild.position.y + "," + eachChild.position.z + "\n");
                        //Debug.Log(child.name +  "target is " + screenPos.x + " pixels from the left and " + screenPos.y + " from the bottom.");


                        if (child.name != "default")
                        {
                            if (child.GetComponent<MeshFilter>() != null)
                            {
                                bool visible = checkKeypointVisibility(child.GetComponent<MeshFilter>().mesh, child);
                                saveModelChild.occluded = !visible;
                            }

                            //if (visible)
                            //    Debug.Log(child.name + " is visible");
                            //else Debug.Log(child.name + " is not visible");
                            //var vertices = child.GetComponent<MeshFilter>().mesh.vertices;


                            //Debug.Log(vertices.Length);

                            //RaycastHit hit;
                            //// Calculate Ray direction
                            //Vector3 direction = mainCamera.transform.position - child.position;
                            //if (Physics.Raycast(child.position, direction, out hit))
                            //{
                            //    if (hit.collider.tag != "MainCamera") //hit something else before the camera
                            //    {
                            //        //do something here
                            //        Debug.Log(child.name + " is visible");
                            //    }
                            //    else Debug.Log(child.name + " is not visible (mc)");
                            //}
                            //else Debug.Log(child.name + " is not visible");

                            //Renderer m_Renderer = child.GetComponent<Renderer>();
                            //m_Renderer.enabled = true;
                            //if (m_Renderer.isVisible)
                            //{
                            //    Debug.Log(child.name + " is visible");
                            //}
                            //else Debug.Log(child.name + " is not visible");

                            //m_Renderer.enabled = false;
                        }
                    }

                    AnnotationData.models.Add(saveModelChild);

                }
            }
            else
            {

                SaveModel saveModel = new SaveModel();// (model.name);//, model.transform.localToWorldMatrix);


                //Matrix4x4 modelTrans = recordButton.GetComponentInChildren<Renderer>().localToWorldMatrix;

                // Save column first
                saveModel.locToWorld = new float[16] {   model.transform.localToWorldMatrix[0,0], model.transform.localToWorldMatrix[1, 0], model.transform.localToWorldMatrix[2, 0], model.transform.localToWorldMatrix[3, 0],
                                                     model.transform.localToWorldMatrix[0,1], model.transform.localToWorldMatrix[1, 1], model.transform.localToWorldMatrix[2, 1], model.transform.localToWorldMatrix[3, 1],
                                                     model.transform.localToWorldMatrix[0,2], model.transform.localToWorldMatrix[1, 2], model.transform.localToWorldMatrix[2, 2], model.transform.localToWorldMatrix[3, 2],
                                                     model.transform.localToWorldMatrix[0,3], model.transform.localToWorldMatrix[1, 3], model.transform.localToWorldMatrix[2, 3], model.transform.localToWorldMatrix[3, 3]};




                //Matrix4x4 localToCam = Camera.worldToCameraMatrix * model.transform.localToWorldMatrix; // calculates relative transformations of local object in camera space

                //saveModel.locToCam = new float[16] {   localToCam[0,0], localToCam[1, 0], localToCam[2, 0], localToCam[3, 0],
                //                                       localToCam[0,1], localToCam[1, 1], localToCam[2, 1], localToCam[3, 1],
                //                                       localToCam[0,2], localToCam[1, 2], localToCam[2, 2], localToCam[3, 2],
                //                                       localToCam[0,3], localToCam[1, 3], localToCam[2, 3], localToCam[3, 3]};

                saveModel.name = model.name;
                saveModel.instance = i;
                //saveModel.keypoint = new float[3] { 10.0f, 10.0f, 10.0f };
                AnnotationData.models.Add(saveModel);
            }
        }

        string json = JsonUtility.ToJson(AnnotationData);


        writer.WriteLine(json);


        //foreach (Transform eachChild in instantiatedModels[0].transform)
        //{
        //    if (eachChild.name == "Keypoint")
        //    {
        //        //Vector3 pos3d = instantiatedModels[0].transform.position;
        //        //pos3d += instantiatedModels[0].transform.position;
        //        Vector3 screenPos = Camera.WorldToScreenPoint(eachChild.position);
        //        writer.Write(fileCounter + "," + screenPos.x + "," + screenPos.y + "\n");// + "," + eachChild.position.x + "," + eachChild.position.y + "," + eachChild.position.z + "\n");
        //        //Debug.Log("target is " + screenPos.x + " pixels from the left and " + screenPos.y + " from the bottom.");
        //    }
        //}


        //int id = 0;
        //foreach (GameObject model in instantiatedModels)
        //{
        //    writer.Write(id + "," + model.transform.position.x + "," + model.transform.position.y + "," + model.transform.position.z + "\n");
        //    id++;
        //}


        writer.Flush();
        writer.Close();


    }


    [Serializable]
    private struct ModelColor
    {
        public ModelColor(string name, int r, int g, int b)
        {
            model = name;
            color = new int[3] { r, g, b };
        }

        public string model;
        public int[] color;
    }
    [Serializable]
    private struct ModelColors
    {
        public List<ModelColor> modelColors;
    }

    public void SaveObjectColors(List<GameObject> instantiatedModels)
    {
        StreamWriter writer = new StreamWriter(dataset.outputPath + "colors.json", false);
        StreamWriter writer_old = new StreamWriter(dataset.outputPath + "colors.txt", false);

        ModelColors modelColors = new ModelColors();
        modelColors.modelColors = new List<ModelColor>();
        for (int index = 0; index < instantiatedModels.Count; ++index)
        {
            UnityEngine.GameObject model = instantiatedModels[index];
            if (dataset.exportSubModels)
            {
                int subModelIdx = 0;
                foreach (Transform child in model.transform)
                {
                    var falseColor = child.gameObject.GetComponent<FalseColor>();
                    Color color = falseColor ? falseColor.falseColor : ColorEncoding.EncodeColorByIndex(subModelIdx);
                    Color32 writeColor = color;
                    ModelColor modelColor = new ModelColor(child.name, writeColor.r, writeColor.g, writeColor.b);
                    modelColors.modelColors.Add(modelColor);
                    writer_old.WriteLine(child.name + "," + writeColor.r + "," + writeColor.g + "," + writeColor.b);
                    subModelIdx += 1;
                }
            }
            else
            {
                var falseColor = model.GetComponent<FalseColor>();
                Color color = falseColor? falseColor.falseColor : ColorEncoding.EncodeColorByIndex(index);
                Color32 writeColor = color;
                ModelColor modelColor = new ModelColor(model.name, writeColor.r, writeColor.g, writeColor.b);
                modelColors.modelColors.Add(modelColor);
                writer_old.WriteLine(model.name + "," + writeColor.r + "," + writeColor.g + "," + writeColor.b);
            }
        }
        string json = JsonUtility.ToJson(modelColors);
        writer.WriteLine(json);

        writer.Close();
        writer_old.Close();
    }

    
}
