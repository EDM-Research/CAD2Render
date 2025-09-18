using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using static Assets.Scripts.io.BOP.BOPDataset;

namespace Assets.Scripts.io.BOP
{
    [System.Serializable]
    public class BOPDatasetExporter : ExportDatasetInterface
    {
        private ImageSaver imageSaver;

        public BOPExportSettings dataset;
        [InspectorButton("TriggerCloneClicked")]
        public bool clone;
        private void TriggerCloneClicked()
        {
            RandomizerInterface.CloneDataset(ref dataset);
        }

        public override IEnumerator exportFrame(List<GameObject> instantiated_models, Camera camera, int fileID)
        {
            yield return new WaitForEndOfFrame();
            if(imageSaver == null) { imageSaver = new ImageSaver(camera.targetTexture.width, camera.targetTexture.height); }
            fileID += 1;//bop starts indexing from 1

            if (dataset.exportRender)
                exportRenderTexture(fileID);
            if (dataset.exportSegmentationMasks)
                exportSegmentationTexture(instantiated_models, fileID);
            if (dataset.exportCameraData)
                exportCameraData(camera, fileID);
            if (dataset.exportObjectData)
                exportObjectData(instantiated_models, camera, fileID);
            if (dataset.exportKeypoints)
                exportKeyPoints(instantiated_models, camera, fileID);
            if (dataset.exportDepth)
                exportDepthTexture(fileID);
        }
        protected override void setupExportPath()
        {
            datasetPrefixPath = "bop/train_PBR/";
            ensureDir(getFullPath());

            ensureDir(getFullPath()+ "rgb/");
            ensureDir(getFullPath()+ "mask/");
            ensureDir(getFullPath()+ "mask_visib/");
                                  
            ensureDir(getFullPath()+ "mask_defect/");
            ensureDir(getFullPath()+ "mask_defect_visib/");
                                  
            ensureDir(getFullPath()+ "depth/");
            /*
            ensureDir(baseOutputPath + String.Format("bop/train_PBR/{0:000000}/albedo", sceneId));
            ensureDir(baseOutputPath + String.Format("bop/train_PBR/{0:000000}/normal", sceneId));
            */
        }

        struct idExportPair { public int id; public bool exported; }
        private static Dictionary<string, idExportPair> obj_name_to_id = new Dictionary<string, idExportPair>();
        /***
         * todo make compatible with a combination of bop style naming and random style naming
         * can contain conflicting id's now.
        */
        static private idExportPair getExportIdOfModel(GameObject model)
        {

            if (obj_name_to_id.ContainsKey(model.name) == false)
            {
                if (Regex.Match(model.name, @"obj_\d\d\d\d\d\d").Success && model.name.Length == 10)
                    obj_name_to_id[model.name] = new idExportPair { id = Int32.Parse(model.name.Substring(4)), exported = false };
                else
                    obj_name_to_id[model.name] = new idExportPair { id = obj_name_to_id.Count + 1, exported = false };
            }
            return obj_name_to_id[model.name];
        }

        public void exportRenderTexture(int fileID)
        {
            imageSaver.Save(renderTexture, getFullPath()+ "rgb/" + fileID.ToString("D6"), dataset.outputExt, true, true);
        }
        public void exportDepthTexture(int fileID)
        {
            imageSaver.Save(depthTexture, getFullPath()+ "depth/" + fileID.ToString("D6"), dataset.outputExt, false, true);
        }
        public void exportAlbedoTexture(RenderTexture albedoTexture, int fileID)
        {
            imageSaver.Save(albedoTexture, getFullPath()+ "albedo/" + fileID.ToString("D6"), dataset.outputExt, true);
        }
        public void exportNormalTexture(RenderTexture normalTexture, int fileID)
        {
            imageSaver.Save(normalTexture, getFullPath()+ "normal/"  + fileID.ToString("D6"), dataset.outputExt, true);
        }

        RenderTexture splitSegmentationTextures;
        RenderTexture splitSegmentationDefectTextures;
        RenderTexture splitSegmentationDefectVisibTextures;
        private void ensureSplitTextureStack(ref RenderTexture saveLocation, int stackSize, int width, int height)
        {
            //create a texture array to generate the segmenation image for each object
            if (saveLocation == null || saveLocation.volumeDepth < stackSize)
            {
                if (saveLocation != null)
                {
                    saveLocation.Release();
                }
                saveLocation = new RenderTexture(width, height, 0, RenderTextureFormat.RFloat);
                saveLocation.dimension = UnityEngine.Rendering.TextureDimension.Tex2DArray;
                saveLocation.volumeDepth = stackSize;
                saveLocation.enableRandomWrite = true;
                saveLocation.Create();
            }
        }
        public void exportSegmentationTexture(List<UnityEngine.GameObject> instantiated_models, int fileID)
        {
            if (instantiated_models.Count == 0)
            {
                Debug.LogWarning("No objects are exported. Check the export settings of the main generator and the object spawners");
                return;
            }
            if (instantiated_models.Count > dataset.maxSegmentationObjects)
            {
                imageSaver.Save(segmentationTexture, getFullPath()+ "mask_visib/" + fileID.ToString("D6"), ImageSaver.Extension.png, true, false);
                return;
            }

            ensureSplitTextureStack(ref splitSegmentationTextures, instantiated_models.Count, segmentationTexture.width, segmentationTexture.height);
            ensureSplitTextureStack(ref splitSegmentationDefectTextures, instantiated_models.Count, segmentationTexture.width, segmentationTexture.height);
            ensureSplitTextureStack(ref splitSegmentationDefectVisibTextures, instantiated_models.Count, segmentationTexture.width, segmentationTexture.height);

            //save the false collors of each object in an array
            Vector3[] colorData = new Vector3[instantiated_models.Count];
            int i = 0;
            foreach (GameObject instance in instantiated_models)
            {
                var temp = instance.GetComponent<FalseColor>();
                if (temp != null)
                    colorData[i] = new Vector3(temp.falseColor.linear.r, temp.falseColor.linear.g, temp.falseColor.linear.b);
                else
                {
                    Debug.LogError("No false color attached to export object: " + instance.name);
                    colorData[i] = new Vector3(0, 0, 0);
                }
                ++i;
            }

            ComputeShader SegmentationShader = (ComputeShader)Resources.Load("ComputeShaders/VisualMaskShader");
            int kernelHandle = SegmentationShader.FindKernel("CSMain");
            //upload the collor array to the gpu
            ComputeBuffer ColorBuffer = new ComputeBuffer(colorData.Length, sizeof(float) * 3);
            ColorBuffer.SetData(colorData);
            SegmentationShader.SetBuffer(kernelHandle, "FalseColors", ColorBuffer);

            //link the input and output texture for the visual mask shader
            SegmentationShader.SetTexture(kernelHandle, "segmentationTexture", segmentationTexture);
            SegmentationShader.SetTexture(kernelHandle, "ResultSegmentation", splitSegmentationTextures);
            SegmentationShader.SetTexture(kernelHandle, "ResultDefectVisib", splitSegmentationDefectVisibTextures);

            SegmentationShader.SetTexture(kernelHandle, "segmentationTextureArray", segmentationTextureArray);
            SegmentationShader.SetTexture(kernelHandle, "ResultDefect", splitSegmentationDefectTextures);

            //split the segmentation textures
            SegmentationShader.Dispatch(kernelHandle, segmentationTexture.width / 8, segmentationTexture.height / 8, instantiated_models.Count);
            ColorBuffer.Dispose();//clean disposel of the collor buffer (might want to edit this so the buffer is reused)

            imageSaver.SaveArray(splitSegmentationTextures, instantiated_models.Count, getFullPath()+ "mask_visib/" + fileID.ToString("D6"), dataset.outputExt, false, true);
            imageSaver.SaveArray(splitSegmentationDefectVisibTextures, instantiated_models.Count, getFullPath()+ "mask_defect_visib/" + fileID.ToString("D6"), dataset.outputExt, false, true);
            imageSaver.SaveArray(splitSegmentationDefectTextures, instantiated_models.Count, getFullPath()+ "mask_defect/"  + fileID.ToString("D6"), dataset.outputExt, false, true);
            imageSaver.SaveArray(segmentationTextureArray, instantiated_models.Count, getFullPath()+ "mask/" + fileID.ToString("D6"), dataset.outputExt, false, true);
        }


        static Matrix4x4 constructCameraMatrix(int width, int height, float v_fov, Camera mainCamera)
        {
            // CALCULATE PROJ MATRIX FROM VERTICAL FOV OF UNITY CAMERA
            //float aspect_ratio = (float)width / (float)height;
            //float h_fov = verticalToHorizontalFieldOfView(v_fov, aspect_ratio);
            //float pi = (float)Math.PI;
            //float fy = (0.5f * height) / (float)Math.Tan((float)(v_fov * pi / 180.0f / 2.0f));
            //float fx = (0.5f * width) / (float)Math.Tan((float)(h_fov * pi / 180.0f / 2.0f)); // Unity ensures that fx == fy
            //float cx = 0.5f * width;
            //float cy = 0.5f * height;

            // Updated calculation, inverse of ViewRandomizeHandler.cs code
            float Fx, Fy, Cx, Cy;
            float f, pWidth, pHeight, sizeX, sizeY, shiftX, shiftY;
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

            UnityEngine.Matrix4x4 cam_K = UnityEngine.Matrix4x4.identity;
            cam_K[0, 0] = Fx;
            cam_K[1, 1] = Fy;
            cam_K[0, 2] = Cx;
            cam_K[1, 2] = Cy;
            cam_K[2, 2] = 1.0f;

            return cam_K;
        }

        private float depthScale = 1.0f;
        public void setDepthScale(float newDepthScale) { depthScale = newDepthScale; }
        public void exportCameraData(Camera camera, int fileID)
        {
            UnityEngine.Matrix4x4 worldToCam = ConvertUnityToOpenCV(camera.transform.rotation, camera.transform.position);
            // exporting scene camera metadata
            CameraListObject scene_camera_obj = new BOPDataset.CameraListObject();
            scene_camera_obj.camera_gt = new Dictionary<string, CameraObject>();

            CameraObject camera_gt = new CameraObject();
            camera_gt.cam_K = new Matrix3x3Object();
            camera_gt.cam_K.mat = constructCameraMatrix(camera.targetTexture.width, camera.targetTexture.height, camera.fieldOfView, camera);
            camera_gt.cam_R_w2c = new Matrix3x3Object();
            camera_gt.cam_R_w2c.mat = worldToCam;
            camera_gt.cam_t_w2c = new VectorObject();
            camera_gt.cam_t_w2c.vector = GeometryUtils.convertUnityToMm(new UnityEngine.Vector3(worldToCam[0, 3], worldToCam[1, 3], worldToCam[2, 3]));
            camera_gt.depth_scale = depthScale;

            scene_camera_obj.camera_gt[fileID.ToString()] = camera_gt;
            string text_scene_camera = scene_camera_obj.Serialize().ToString();
            appendToJSON(getFullPath()+"scene_camera.json", text_scene_camera, fileID == 1);
        }

        public void exportKeyPoints(List<GameObject> instantiated_models, Camera camera, int fileID)
        {
            RenderTexture currentActiveRT = RenderTexture.active;
            RenderTexture.active = depthTexture;
            Texture2D depthText = new Texture2D(depthTexture.width, depthTexture.height);
            depthText.ReadPixels(new Rect(0, 0, depthTexture.width, depthTexture.height), 0, 0);
            RenderTexture.active = currentActiveRT;

            KeypointListObject obj = new KeypointListObject();
            obj.keypoints_gt = new Dictionary<string, List<List<KeypointObject>>>();
            List<List<KeypointObject>> SceneKeyPoints = new List<List<KeypointObject>>();

            for (int i = 0; i < instantiated_models.Count; ++i)
            {
                List<KeypointObject> instanceKeyPoints = new List<KeypointObject>();

                foreach (var keypointObject in instantiated_models[i].GetComponentsInChildren<Transform>().Where(child => child.CompareTag("Keypoint")))
                {
                    KeypointObject keypoint = new KeypointObject();
                    Matrix4x4 viewMat = GeometryUtils.getModelViewMatrix(keypointObject, camera);
                    Vector3 translation = new Vector3(viewMat[0, 3], viewMat[1, 3], viewMat[2, 3]);
                    //keypoint.cam_R_m2c = new Matrix3x3Object();
                    //keypoint.cam_R_m2c.mat = viewMat;
                    //keypoint.cam_t_m2c = new VectorObject();
                    //keypoint.cam_t_m2c.vector = GeometryUtils.convertUnityToMm(translation);
                    keypoint.loc_t_l2m = new VectorObject();
                    keypoint.loc_t_l2m.vector = keypointObject.localPosition;
                    var screenCo = camera.WorldToScreenPoint(keypointObject.position);
                    keypoint.screen_co = new Vector2Int((int)Math.Round(screenCo.x), (int)Math.Round(camera.pixelHeight - screenCo.y));
                    if (keypoint.screen_co.x >= 0 && keypoint.screen_co.x < depthText.width
                        && keypoint.screen_co.y >= 0 && keypoint.screen_co.y < depthText.height)
                    {
                        var depthDistance = GeometryUtils.convertMmToUnity(depthText.GetPixel(keypoint.screen_co.x, keypoint.screen_co.y).linear.r * depthScale);
                        float delta = GeometryUtils.convertMmToUnity(0.5f);//use 0.5mm of error margin on visibility check with the depth test
                        keypoint.isVisible = depthDistance - Vector3.Distance(keypointObject.position, camera.transform.position) > -delta && translation.z > 0;
                    }
                    else
                        keypoint.isVisible = false;
                    instanceKeyPoints.Add(keypoint);
                }
                SceneKeyPoints.Add(instanceKeyPoints);

            }

            obj.keypoints_gt[fileID.ToString()] = SceneKeyPoints;
            string text = obj.Serialize().ToString();
            appendToJSON(getFullPath()+ "keyPoints_gt.json" , text, fileID == 1);
        }

        public void exportObjectData(List<UnityEngine.GameObject> instantiated_models, Camera camera, int fileID)
        {
            // exporting scene ground truth metadata
            PoseListObject obj = new PoseListObject();
            obj.scene_gt = new Dictionary<string, List<PoseObject>>();
            List<PoseObject> poses = new List<PoseObject>();

            foreach (GameObject model in instantiated_models)
            {
                Matrix4x4 viewMat = GeometryUtils.getModelViewMatrix(model.transform, camera);
                Vector3 translation = new Vector3(viewMat[0, 3], viewMat[1, 3], viewMat[2, 3]);

                PoseObject pose = new PoseObject();
                pose.cam_R_m2c = new Matrix3x3Object();
                pose.cam_R_m2c.mat = viewMat;
                pose.cam_t_m2c = new VectorObject();
                pose.cam_t_m2c.vector = GeometryUtils.convertUnityToMm(translation);
                var idExportData = getExportIdOfModel(model);
                pose.obj_id = idExportData.id;

                if (instantiated_models.Count > dataset.maxSegmentationObjects)
                {
                    var temp = model.GetComponent<FalseColor>();
                    if (temp != null)
                        pose.falseColor = temp.falseColor;
                    else
                    {
                        Debug.LogError("No false color attached to export object: " + model.name);
                        pose.falseColor = Color.black;
                    }
                }
                else
                    pose.falseColor = Color.black;

                if (!idExportData.exported)
                {
                    //TODO fix bug for complex meshes: exportModel(model, outputPath + String.Format("bop/models/{0:000000}.ply", idExportData.id));
                    idExportData.exported = true;
                    obj_name_to_id[model.name] = idExportData;
                    exportModelId(model.name, idExportData.id, fileID == 1);
                }
                poses.Add(pose);
            }

            obj.scene_gt[fileID.ToString()] = poses;
            string text = obj.Serialize().ToString();
            appendToJSON(getFullPath()+ "scene_gt.json", text, fileID == 1);
        }

        private void exportModelId(string modelName, int id, bool first_element)
        {
            var n = new JSONObject();
            n[id.ToString()] = modelName;
            appendToJSON(getFullPath()+ "model_id.json", n.ToString(), first_element);
        }

        public static void exportModel(GameObject model, String filename)
        {
            MeshFilter[] meshFilters = model.GetComponentsInChildren<MeshFilter>();
            List<CombineInstance> combine = new List<CombineInstance>();
            int i = 0;
            while (i < meshFilters.Length)
            {
                for (int j = 0; j < meshFilters[i].mesh.subMeshCount; ++j)
                {
                    var instance = new CombineInstance();
                    instance.subMeshIndex = j;
                    instance.mesh = meshFilters[i].mesh;
                    instance.transform = Matrix4x4.identity;
                    combine.Add(instance);
                }

                i++;
            }
            Mesh mesh = new Mesh();
            mesh.CombineMeshes(combine.ToArray(), false, false);
            try
            {
                PLYExporter.MeshToFile(mesh, filename);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Failed to export model " + model.name + " to ply format.\n" + e.Message);
            }
        }
    }
}
