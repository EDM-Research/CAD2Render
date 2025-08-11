using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Assets.Scripts.newScene
{
    public class Exporter
    {
        ImageSaver imageSaver;
        private MainRandomizerData dataset;
        private GameObject generator;

        private GameObject renderSettings; 
        private Camera mainCamera;
        public int fileCounter { get; private set; }
        public void resetFileCounter() { fileCounter = 0; }

        private RenderTexture _renderTexture;
        public RenderTexture renderTexture
        {
            get { return _renderTexture; }
            set
            {
                _renderTexture = value;
                SaveIntrinsicMatrix();
            }
        }
        public RenderTexture segmentationTexture { get; set; }
        public RenderTexture segmentationTextureArray { get; set; }
        public RenderTexture albedoTexture { get; set; }
        public RenderTexture normalTexture { get; set; }
        public RenderTexture depthTexture { get; set; }

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


        public Exporter(MainRandomizerData p_dataset, GameObject p_generator)
        {
            dataset = p_dataset;
            imageSaver = new ImageSaver(dataset.resolution.x, dataset.resolution.y);
            generator = p_generator;
            mainCamera = Camera.main;
            fileCounter = dataset.startFileCounter;

            setupOutputPaths();

            if (dataset.exportToFMFormat)
            {
                if (dataset.annotationisFile == "")
                    dataset.annotationisFile = "annotations.txt";
                // Create empty annotations file
                StreamWriter writer = new StreamWriter(dataset.outputPath + dataset.annotationisFile, false);
                writer.Close();
            }

            var temp = GameObject.FindGameObjectWithTag("EnvironmentSettings");
            if (temp != null)
            {
                renderSettings = temp.transform.Find("Rendering Settings")?.gameObject;
                BOPDatasetExporter.setRenderSettingsObject(renderSettings);
                //raytracingSettings = temp[0].transform.Find("Ray Tracing Settings")?.gameObject;
                //postProcesingSettings = temp[0].transform.Find("PostProcessing")?.gameObject;
            }
            ExportDatasetInfo();
        }

        private void setupOutputPaths()
        {
            if (dataset.exportToBOP)
                BOPDatasetExporter.SetupExportPath(dataset.outputPath, dataset.BOPSceneId, dataset.exportDepthTexture, dataset.exportNormalTexture, dataset.exportAlbedoTexture);
            if (dataset.exportToFMFormat)
            {
                if (!Directory.Exists(dataset.outputPath + "images/"))
                    Directory.CreateDirectory(dataset.outputPath + "images/");
                if (!Directory.Exists(dataset.outputPath + "segmentation/"))
                    Directory.CreateDirectory(dataset.outputPath + "segmentation/");
                if (!Directory.Exists(dataset.outputPath + "depth/") && dataset.exportDepthTexture)
                    Directory.CreateDirectory(dataset.outputPath + "depth/");
                if (!Directory.Exists(dataset.outputPath + "metaData/"))
                    Directory.CreateDirectory(dataset.outputPath + "metaData/");
            }
        }

        public void SaveIntrinsicMatrix()
        {
            double ImageSizeX = renderTexture.width;
            double ImageSizeY = renderTexture.height;
            double fovY = mainCamera.fieldOfView;
            double fovX = Camera.VerticalToHorizontalFieldOfView((float)fovY, (float)ImageSizeX / (float)ImageSizeY);

            double Fx, Fy, Cx, Cy;
            //Fx = (ImageSizeX / (2 * Math.Tan(fovX * Math.PI / 360)));
            //Fy = (ImageSizeY / (2 * Math.Tan(fovY * Math.PI / 360)));
            //Cx = ImageSizeX / 2;
            //Cy = ImageSizeY / 2;

            double f, pWidth, pHeight, sizeX, sizeY, shiftX, shiftY;
            f = mainCamera.focalLength;
            pWidth = mainCamera.pixelWidth;
            pHeight = mainCamera.pixelHeight;
            sizeX = mainCamera.sensorSize.x;
            sizeY = mainCamera.sensorSize.y;
            shiftX = mainCamera.lensShift.x;
            shiftY = mainCamera.lensShift.y;
            Fx = f * pWidth / sizeX;
            Fy = f * pHeight / sizeY;
            Cx = (-shiftX * pWidth) + pWidth / 2.0f;
            Cy = shiftY * pHeight + pHeight / 2.0f;

            StreamWriter writer = new StreamWriter(dataset.outputPath + "camera.txt", false);
            writer.WriteLine(Fx + ", 0, " + Cx);
            writer.WriteLine("0, " + Fy + ", " + Cy);
            writer.WriteLine("0, 0, 1");

            writer.Flush();
            writer.Close();
        }
        public void SaveMitsuba(List<GameObject> instantiatedModels)
        {
            HDRISky sky;
            renderSettings.GetComponent<Volume>().profile.TryGet<HDRISky>(out sky);

            string environmentMap = UnityEditor.AssetDatabase.GetAssetPath(sky.hdriSky.value);
            float env_exposure = 2.0f * (sky.exposure.value + 1.0f);
            float env_angle = sky.rotation.value;


            MitusbaExporter.SaveMitsuba(instantiatedModels, fileCounter, dataset.outputPath, mainCamera, renderTexture.width, renderTexture.height, dataset.mitsubaSampleCount, environmentMap, env_exposure, env_angle);
        }

        public IEnumerator Capture(List<GameObject> instantiatedModels)
        {
            yield return new WaitForEndOfFrame();

            if (dataset.exportToFMFormat)
            {
                CaptureFMFormatAnnotations(instantiatedModels);
                SaveFMFormat(fileCounter);
            }

            if (dataset.exportToMitsuba)
                SaveMitsuba(instantiatedModels);

            fileCounter++;//bop format starts at index 1 fm format starts with 0

            if (dataset.exportToBOP)
            {
                BOPDatasetExporter.exportFrame(instantiatedModels, renderTexture, segmentationTexture, segmentationTextureArray, fileCounter, dataset.outputPath, dataset.outputExtBOP, mainCamera, imageSaver);
                if (dataset.exportDepthTexture)
                    BOPDatasetExporter.exportDepthTexture(depthTexture, fileCounter, dataset.outputPath, imageSaver, dataset.depthMapExt);
                if (dataset.exportAlbedoTexture)
                    BOPDatasetExporter.exportAlbedoTexture(albedoTexture, fileCounter, dataset.outputPath, dataset.outputExtBOP, imageSaver);
                if (dataset.exportNormalTexture)
                    BOPDatasetExporter.exportNormalTexture(normalTexture, fileCounter, dataset.outputPath, dataset.outputExtBOP, imageSaver);
                if (dataset.exportKeyPoints)
                    BOPDatasetExporter.exportKeyPoints(instantiatedModels, depthTexture, fileCounter, dataset.outputPath, mainCamera);
            }
            //WaitForEndOfFrameAndSave(filenameExtension, fileCounter);//);
        }

        //private IEnumerator WaitForEndOfFrameAndSave(string filenameExtension, int fileCounter)
        //{
        //    yield return new WaitForEndOfFrame();
        //    Save(filenameExtension, fileCounter);
        //
        //
        //}

        private void SaveFMFormat(int fileCounter)
        {
            imageSaver.Save(renderTexture, dataset.outputPath + "images/" + fileCounter + "_img", dataset.outputExt, dataset.applyGammaCorrection);
            if (segmentationTexture)
                imageSaver.Save(segmentationTexture, dataset.outputPath + "segmentation/" + fileCounter + "_seg", dataset.outputExt, true);
            if (dataset.exportDepthTexture)
                imageSaver.Save(depthTexture, dataset.outputPath + "depth/" + fileCounter + "_depth", dataset.depthMapExt, true, true);
        }
        private void ExportDatasetInfo()
        {
            string metadataPath = "metaData/";
            if (dataset.exportToBOP)
                metadataPath = "bop/" + metadataPath;
            StreamWriter writer = new StreamWriter(dataset.outputPath + metadataPath + "versionInfo.json", false);
            
            var version = PlanetaGameLabo.UnityGitVersion.GitVersion.version;
            writer.WriteLine(JsonUtility.ToJson(version, true));
            writer.WriteLine();
            writer.Flush();
            writer.Close();

            if (dataset.renderProfile)
                System.IO.File.Copy(AssetDatabase.GetAssetPath(dataset.renderProfile), dataset.outputPath + metadataPath + dataset.renderProfile.name + ".asset", true);
            if (dataset.rayTracingProfile)
                System.IO.File.Copy(AssetDatabase.GetAssetPath(dataset.rayTracingProfile), dataset.outputPath + metadataPath + dataset.rayTracingProfile.name + ".asset", true);
            if (dataset.postProcesingProfile)
                System.IO.File.Copy(AssetDatabase.GetAssetPath(dataset.postProcesingProfile), dataset.outputPath + metadataPath + dataset.postProcesingProfile.name + ".asset", true);
            System.IO.File.Copy(AssetDatabase.GetAssetPath(dataset), dataset.outputPath + metadataPath + dataset.name + ".asset", true);

            foreach(RandomizerInterface randomizer in generator.GetComponentsInChildren<RandomizerInterface>())
                if(randomizer.getDataset() != null)
                    System.IO.File.Copy(AssetDatabase.GetAssetPath(randomizer.getDataset()), dataset.outputPath + metadataPath + randomizer.getDataset().name + ".asset", true);

            foreach (MaterialRandomizerInterface randomizer in generator.GetComponentsInChildren<MaterialRandomizerInterface>())
                if (randomizer.getDataset() != null)
                    System.IO.File.Copy(AssetDatabase.GetAssetPath(randomizer.getDataset()), dataset.outputPath + metadataPath + randomizer.getDataset().name + ".asset", true);
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
                hits = Physics.RaycastAll(worldPt, direction, GeometryUtils.convertMmToUnity(dataset.maxDepthDistance));

                bool vertexVisible = true;
                foreach (RaycastHit hit in hits)
                {
                    //Debug.Log(hit.collider.name);
                    if (hit.collider.name != child.name)
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

        private SaveModel populateSaveModel(Transform model, bool exportImagePosition)
        {
            SaveModel saveModel = new SaveModel();

            // Save column first
            saveModel.locToWorld = new float[16] {model.transform.localToWorldMatrix[0,0], model.transform.localToWorldMatrix[1, 0], model.transform.localToWorldMatrix[2, 0], model.transform.localToWorldMatrix[3, 0],
                                                  model.transform.localToWorldMatrix[0,1], model.transform.localToWorldMatrix[1, 1], model.transform.localToWorldMatrix[2, 1], model.transform.localToWorldMatrix[3, 1],
                                                  model.transform.localToWorldMatrix[0,2], model.transform.localToWorldMatrix[1, 2], model.transform.localToWorldMatrix[2, 2], model.transform.localToWorldMatrix[3, 2],
                                                  model.transform.localToWorldMatrix[0,3], model.transform.localToWorldMatrix[1, 3], model.transform.localToWorldMatrix[2, 3], model.transform.localToWorldMatrix[3, 3]};

            saveModel.name = model.name;

            if (exportImagePosition)
            {
                Vector3 screenPos = mainCamera.WorldToScreenPoint(model.position);
                saveModel.imgPos = new float[2] { screenPos.x, screenPos.y };
                saveModel.occluded = false;
                //writer.Write(fileCounter + "," + screenPos.x + "," + screenPos.y + "\n");// + "," + eachChild.position.x + "," + eachChild.position.y + "," + eachChild.position.z + "\n");
                //Debug.Log(child.name +  "target is " + screenPos.x + " pixels from the left and " + screenPos.y + " from the bottom.");

                if (model.GetComponent<MeshFilter>() != null)
                {
                    bool visible = checkKeypointVisibility(model.GetComponent<MeshFilter>().mesh, model);
                    saveModel.occluded = !visible;
                }
                
            }

            return saveModel;
        }
        private void CaptureFMFormatAnnotations(List<GameObject> instantiatedModels)
        {
            // open the annotations file
            StreamWriter writer = new StreamWriter(dataset.outputPath + dataset.annotationisFile, true);


            SaveObject AnnotationData = new SaveObject();
            AnnotationData.id = fileCounter;
            AnnotationData.proj = new float[16] { mainCamera.projectionMatrix[0,0], mainCamera.projectionMatrix[1, 0], mainCamera.projectionMatrix[2, 0], mainCamera.projectionMatrix[3, 0],
                                                  mainCamera.projectionMatrix[0,1], mainCamera.projectionMatrix[1, 1], mainCamera.projectionMatrix[2, 1], mainCamera.projectionMatrix[3, 1],
                                                  mainCamera.projectionMatrix[0,2], mainCamera.projectionMatrix[1, 2], mainCamera.projectionMatrix[2, 2], mainCamera.projectionMatrix[3, 2],
                                                  mainCamera.projectionMatrix[0,3], mainCamera.projectionMatrix[1, 3], mainCamera.projectionMatrix[2, 3], mainCamera.projectionMatrix[3, 3]};

            AnnotationData.worldToCam = new float[16] { mainCamera.transform.worldToLocalMatrix[0,0], mainCamera.transform.worldToLocalMatrix[1, 0], mainCamera.transform.worldToLocalMatrix[2, 0], mainCamera.transform.worldToLocalMatrix[3, 0],
                                                        mainCamera.transform.worldToLocalMatrix[0,1], mainCamera.transform.worldToLocalMatrix[1, 1], mainCamera.transform.worldToLocalMatrix[2, 1], mainCamera.transform.worldToLocalMatrix[3, 1],
                                                        mainCamera.transform.worldToLocalMatrix[0,2], mainCamera.transform.worldToLocalMatrix[1, 2], mainCamera.transform.worldToLocalMatrix[2, 2], mainCamera.transform.worldToLocalMatrix[3, 2],
                                                        mainCamera.transform.worldToLocalMatrix[0,3], mainCamera.transform.worldToLocalMatrix[1, 3], mainCamera.transform.worldToLocalMatrix[2, 3], mainCamera.transform.worldToLocalMatrix[3, 3]};


            AnnotationData.models = new List<SaveModel>();
            for (int i = 0; i < instantiatedModels.Count; ++i)
            {
                UnityEngine.GameObject model = instantiatedModels[i];
                //export main model
                SaveModel saveModel = populateSaveModel(model.transform, false);
                saveModel.instance = i;
                AnnotationData.models.Add(saveModel);

                //export all submodels
                if (dataset.exportSubModels)
                {
                    Transform[] allChildren = model.GetComponentsInChildren<Transform>();
                    foreach (Transform child in allChildren)
                    {
                        if (child == model)
                            continue;
                        SaveModel saveModelChild = populateSaveModel(child, dataset.exportImagePosition);
                        saveModelChild.instance = i;
                        //saveModelChild.name = saveModel.name + "." + saveModelChild.name;
                        AnnotationData.models.Add(saveModelChild);
                    }
                }
                //export only the keypoint submodels
                else if (dataset.exportImagePosition)
                {
                    Transform[] allChildren = model.GetComponentsInChildren<Transform>();
                    foreach (Transform child in allChildren)
                    {
                        if (child == model)
                            continue;
                        if (child.tag != "Keypoint")
                            continue;
                        SaveModel saveModelChild = populateSaveModel(child, true);
                        saveModelChild.instance = i;
                        //saveModelChild.name = saveModel.name + "." + saveModelChild.name;
                        AnnotationData.models.Add(saveModelChild);
                    }

                }
            }

            string json = JsonUtility.ToJson(AnnotationData);


            writer.WriteLine(json);
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
                        Color color = ColorEncoding.GetColorByIndex(subModelIdx);
                        Color32 writeColor = color;
                        ModelColor modelColor = new ModelColor(child.name, writeColor.r, writeColor.g, writeColor.b);
                        modelColors.modelColors.Add(modelColor);
                        writer_old.WriteLine(child.name + "," + writeColor.r + "," + writeColor.g + "," + writeColor.b);
                        subModelIdx += 1;
                    }
                }
                else
                {
                    Color color = ColorEncoding.GetColorByIndex(index);
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
}