using Assets.Scripts.io.BOP;
using GLTFast;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using static Assets.Scripts.io.FM.FMDataset;

namespace Assets.Scripts.io.FM
{
    public class FMDatasetExporter : ExportDatasetInterface
    {
        private ImageSaver imageSaver;
        public FMExportSettings dataset;
        [InspectorButton("TriggerCloneClicked")]
        public bool clone;
        private void TriggerCloneClicked()
        {
            RandomizerInterface.CloneDataset(ref dataset);
        }

        RenderTexture renderTexture;
        RenderTexture segmentationTexture;
        RenderTexture depthTexture;

        public override IEnumerator exportFrame(List<GameObject> instantiated_models, Camera camera, int fileID)
        {
            yield return new WaitForEndOfFrame(); 
            if (imageSaver == null) { imageSaver = new ImageSaver(camera.targetTexture.width, camera.targetTexture.height); }

            if (dataset.exportRender)
                exportRenderTexture(renderTexture, fileID);
            if (dataset.exportSegmentationMasks)
                exportSegmentationTexture(segmentationTexture, fileID);
            if (dataset.exportDepth)
                exportDepthTexture(depthTexture, fileID);

            if (dataset.exportCameraData)
                exportCameraData(camera, fileID);
            if (dataset.exportWorldposition)
                exportObjectData(instantiated_models, camera, fileID);
            if (dataset.exportColors)
                exportObjectColors(instantiated_models);
        }

        protected override void setupExportPath()
        {
            datasetPrefixPath = "fm/";
            if (dataset.exportRender)
                ensureDir(getFullPath() + "images/");
            if (dataset.exportSegmentationMasks)
                ensureDir(getFullPath() + "segmentation/");
            if (dataset.exportDepth)
                ensureDir(getFullPath() + "depth/");
        }
        protected override void setupCustomPasses(Camera mainCamera)
        {
            DrawSegmentationObjectsCustomPass segmentationMaskRenderer = (DrawSegmentationObjectsCustomPass)customPassVolume.customPasses.Find(pass => pass.name == "SegmentationPass");
            if (segmentationMaskRenderer == null)
            {
                segmentationTexture = new RenderTexture(mainCamera.targetTexture.width, mainCamera.targetTexture.height, 24);
                segmentationMaskRenderer = new DrawSegmentationObjectsCustomPass(mainCamera, segmentationTexture);
                segmentationMaskRenderer.name = "SegmentationPass";
                customPassVolume.customPasses.Add(segmentationMaskRenderer);
            }
            segmentationMaskRenderer.enabled = true;
            segmentationTexture = segmentationMaskRenderer.targetTexture;

            CustomShaderRenderToTexturePass DepthRenderer = (CustomShaderRenderToTexturePass)customPassVolume.customPasses.Find(pass => pass.name == "DepthPass");
            if(DepthRenderer == null)
            {
                depthTexture = new RenderTexture(mainCamera.targetTexture.width, mainCamera.targetTexture.height, 24, RenderTextureFormat.ARGBFloat);
                depthTexture.enableRandomWrite = true;
                depthTexture.Create();

                var depthMat = new Material(Shader.Find("Unlit/Depth"));
                depthMat.SetFloat("_DepthMaxDistance", GeometryUtils.convertMmToUnity(Math.Max(dataset.maxDepthDistance, 1.0f)));

                DepthRenderer = new CustomShaderRenderToTexturePass(depthMat, Color.black, mainCamera, depthTexture);
                DepthRenderer.name = "DepthPass";
                customPassVolume.customPasses.Add(DepthRenderer);
            }
            else
            {
                if (DepthRenderer.overrideMaterial.GetFloat("_DepthMaxDistance") != GeometryUtils.convertMmToUnity(dataset.maxDepthDistance))
                    Debug.LogWarning("Max depth distance for depth renderer is different between exporters. " + DepthRenderer.overrideMaterial.GetFloat("_DepthMaxDistance").ToString() + " is used.");
            }
            DepthRenderer.enabled = true;
            depthTexture = DepthRenderer.targetTexture;

            renderTexture = mainCamera.targetTexture;
        }
        public override List<(string, RenderTexture)> getTextureOutputs()
        {
            var list = new List<(string, RenderTexture)>();
            list.Add((datasetPrefixPath + "Main", renderTexture));
            list.Add((datasetPrefixPath + "Segmentation", segmentationTexture));
            if (dataset.exportDepth)
                list.Add((datasetPrefixPath + "Depth", depthTexture));

            return list;
        }

        private void exportDepthTexture(RenderTexture depthTexture, int fileID)
        {
            imageSaver.Save(depthTexture, getFullPath() + "depth/" + fileID + "_depth", dataset.depthMapExt, false, true);
        }


        private void exportObjectData(List<GameObject> instantiated_models, Camera camera, int fileID)
        {
            StreamWriter writer = new StreamWriter(getFullPath() + "annotations.txt", true);

            FMDataset.SaveObject AnnotationData = new FMDataset.SaveObject();
            AnnotationData.id = fileID;
            AnnotationData.proj = new float[16] { camera.projectionMatrix[0,0], camera.projectionMatrix[1, 0], camera.projectionMatrix[2, 0], camera.projectionMatrix[3, 0],
                                                  camera.projectionMatrix[0,1], camera.projectionMatrix[1, 1], camera.projectionMatrix[2, 1], camera.projectionMatrix[3, 1],
                                                  camera.projectionMatrix[0,2], camera.projectionMatrix[1, 2], camera.projectionMatrix[2, 2], camera.projectionMatrix[3, 2],
                                                  camera.projectionMatrix[0,3], camera.projectionMatrix[1, 3], camera.projectionMatrix[2, 3], camera.projectionMatrix[3, 3]};

            AnnotationData.worldToCam = new float[16] { camera.transform.worldToLocalMatrix[0,0], camera.transform.worldToLocalMatrix[1, 0], camera.transform.worldToLocalMatrix[2, 0], camera.transform.worldToLocalMatrix[3, 0],
                                                        camera.transform.worldToLocalMatrix[0,1], camera.transform.worldToLocalMatrix[1, 1], camera.transform.worldToLocalMatrix[2, 1], camera.transform.worldToLocalMatrix[3, 1],
                                                        camera.transform.worldToLocalMatrix[0,2], camera.transform.worldToLocalMatrix[1, 2], camera.transform.worldToLocalMatrix[2, 2], camera.transform.worldToLocalMatrix[3, 2],
                                                        camera.transform.worldToLocalMatrix[0,3], camera.transform.worldToLocalMatrix[1, 3], camera.transform.worldToLocalMatrix[2, 3], camera.transform.worldToLocalMatrix[3, 3]};
            
            AnnotationData.models = new List<SaveModel>();
            for (int i = 0; i < instantiated_models.Count; ++i)
            {
                GameObject model = instantiated_models[i];

                Transform[] allChildren = model.GetComponentsInChildren<Transform>();
                foreach (Transform child in allChildren)
                {
                    if ((child == model) || //always export the base model
                        (dataset.exportSubModels) ||
                        (dataset.exportKeypoints && child.tag == "Keypoint"))
                    {
                        SaveModel saveModel = new SaveModel();
                        saveModel.instance = i;
                        saveModel.name = model.name;
                        if (dataset.exportWorldposition)
                            addWorldPositionToSaveModel(saveModel, model.transform);
                        if (dataset.exportImagePosition)
                            addImagePositionToSaveModel(saveModel, model.transform, camera);

                        AnnotationData.models.Add(saveModel);
                    }
                }
            }

            string json = JsonUtility.ToJson(AnnotationData);

            writer.WriteLine(json);
            writer.Flush();
            writer.Close();

        }

        private SaveModel addWorldPositionToSaveModel(SaveModel saveModel, Transform model)
        {
            // Save column first
            saveModel.locToWorld = new float[16] {model.transform.localToWorldMatrix[0,0], model.transform.localToWorldMatrix[1, 0], model.transform.localToWorldMatrix[2, 0], model.transform.localToWorldMatrix[3, 0],
                                                  model.transform.localToWorldMatrix[0,1], model.transform.localToWorldMatrix[1, 1], model.transform.localToWorldMatrix[2, 1], model.transform.localToWorldMatrix[3, 1],
                                                  model.transform.localToWorldMatrix[0,2], model.transform.localToWorldMatrix[1, 2], model.transform.localToWorldMatrix[2, 2], model.transform.localToWorldMatrix[3, 2],
                                                  model.transform.localToWorldMatrix[0,3], model.transform.localToWorldMatrix[1, 3], model.transform.localToWorldMatrix[2, 3], model.transform.localToWorldMatrix[3, 3]};

            return saveModel;
        }
        private SaveModel addImagePositionToSaveModel(SaveModel saveModel, Transform model, Camera camera)
        {
            Vector3 screenPos = camera.WorldToScreenPoint(model.position);
            saveModel.imgPos = new float[2] { screenPos.x, screenPos.y };
            saveModel.occluded = false;


            RenderTexture currentActiveRT = RenderTexture.active;
            RenderTexture.active = depthTexture;
            Texture2D depthText = new Texture2D(depthTexture.width, depthTexture.height);
            depthText.ReadPixels(new Rect(0, 0, depthTexture.width, depthTexture.height), 0, 0);
            RenderTexture.active = currentActiveRT;

            if (screenPos.x >= 0 && screenPos.x < depthTexture.width &&
                screenPos.y >= 0 && screenPos.y < depthTexture.height)
            {
                var depthDistance = GeometryUtils.convertMmToUnity(depthText.GetPixel((int)screenPos.x, (int)screenPos.y).linear.r);
                float delta = GeometryUtils.convertMmToUnity(0.5f);//use 0.5mm of error margin on visibility check with the depth test
                saveModel.occluded = Vector3.Distance(camera.transform.position, model.position) - depthDistance > delta || screenPos.z < 0;
            }
            else
                saveModel.occluded = true;

            return saveModel;
        }

        private void exportCameraData(Camera camera, int fileID)
        {
            double ImageSizeX = camera.targetTexture.width;
            double ImageSizeY = camera.targetTexture.height;
            double fovY = camera.fieldOfView;
            double fovX = Camera.VerticalToHorizontalFieldOfView((float)fovY, (float)ImageSizeX / (float)ImageSizeY);

            double Fx, Fy, Cx, Cy;
            //Fx = (ImageSizeX / (2 * Math.Tan(fovX * Math.PI / 360)));
            //Fy = (ImageSizeY / (2 * Math.Tan(fovY * Math.PI / 360)));
            //Cx = ImageSizeX / 2;
            //Cy = ImageSizeY / 2;

            double f, pWidth, pHeight, sizeX, sizeY, shiftX, shiftY;
            f = camera.focalLength;
            pWidth = camera.pixelWidth;
            pHeight = camera.pixelHeight;
            sizeX = camera.sensorSize.x;
            sizeY = camera.sensorSize.y;
            shiftX = camera.lensShift.x;
            shiftY = camera.lensShift.y;
            Fx = f * pWidth / sizeX;
            Fy = f * pHeight / sizeY;
            Cx = (-shiftX * pWidth) + pWidth / 2.0f;
            Cy = shiftY * pHeight + pHeight / 2.0f;

            StreamWriter writer = new StreamWriter(getFullPath() + "camera.txt", false);
            writer.WriteLine(Fx + ", 0, " + Cx);
            writer.WriteLine("0, " + Fy + ", " + Cy);
            writer.WriteLine("0, 0, 1");

            writer.Flush();
            writer.Close();
        }

        private void exportSegmentationTexture(RenderTexture segmentationTexture, int fileID)
        {
            imageSaver.Save(segmentationTexture, getFullPath() + "segmentation/" + fileID + "_seg", dataset.outputExt, false);
        }

        private void exportRenderTexture(RenderTexture renderTexture, int fileID)
        {
            imageSaver.Save(renderTexture, getFullPath() + "images/" + fileID + "_img", dataset.outputExt, dataset.applyGammaCorrection);
        }

        public void exportObjectColors(List<GameObject> instantiatedModels)
        {
            StreamWriter writer = new StreamWriter(getFullPath() + "colors.json", true);

            FMDataset.ModelColors modelColors = new FMDataset.ModelColors();
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
                        subModelIdx += 1;
                    }
                }
                else
                {
                    Color color = ColorEncoding.GetColorByIndex(index);
                    Color32 writeColor = color;
                    ModelColor modelColor = new ModelColor(model.name, writeColor.r, writeColor.g, writeColor.b);
                    modelColors.modelColors.Add(modelColor);
                }
            }
            string json = JsonUtility.ToJson(modelColors);
            writer.WriteLine(json);

            writer.Close();
        }
    }
}