using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using SimpleJSON;
using System.Text.RegularExpressions;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System.Linq;

// https://github.com/thodan/bop_toolkit/blob/master/docs/bop_datasets_format.md
public class BOPDatasetExporter
{
    [Serializable]
    private struct Matrix3x3Object
    {
        public UnityEngine.Matrix4x4 mat;

        public JSONNode Serialize()
        {
            JSONArray matrix = new JSONArray();
            for (int row = 0; row < 3; ++row)
                for (int col = 0; col < 3; ++col)
                    matrix.Add(mat[row, col]);

            return matrix;
        }
    }


    [Serializable]
    private struct VectorObject
    {
        public UnityEngine.Vector3 vector;

        public JSONNode Serialize()
        {
            JSONArray vec = new JSONArray();
            for (int col = 0; col < 3; ++col)
                vec.Add(vector[col]);

            return vec;
        }
    }


    [Serializable]
    private struct KeypointObject
    {
        public VectorObject loc_t_l2m;
        public Matrix3x3Object cam_R_m2c;
        public VectorObject cam_t_m2c;
        public Vector2Int screen_co;
        public bool isVisible;

        public JSONNode Serialize()
        {
            var n = new JSONObject();
            n["loc_t_l2m"] = loc_t_l2m.Serialize();
            n["cam_R_m2c"] = cam_R_m2c.Serialize();
            n["cam_t_m2c"] = cam_t_m2c.Serialize();
            JSONObject vec = new JSONObject();
            vec["x"] = screen_co.x;
            vec["y"] = screen_co.y;
            n["screen_co"] = vec;
            n["isVisible"] = isVisible.ToString();
            return n;
        }
    }
    [Serializable]
    private struct KeypointListObject
    {
        int fileId;
        public Dictionary<string, List<List<KeypointObject>>> keypoints_gt;

        public JSONNode Serialize()
        {
            var scene = new JSONObject();
            foreach (var keypointList in keypoints_gt)
            {
                var sceneKeypoints = new JSONObject();
                for (int i = 0; i < keypointList.Value.Count; ++i)
                {
                    JSONArray instanceKeypoints = new JSONArray();
                    foreach (KeypointObject keypoint in keypointList.Value[i])
                        instanceKeypoints.Add(keypoint.Serialize());

                    sceneKeypoints[i.ToString()] = instanceKeypoints;
                    
                }
                scene[keypointList.Key] = sceneKeypoints;
            }
            return scene;
        }
    }

    [Serializable]
    private struct PoseObject
    {
        public Matrix3x3Object cam_R_m2c;
        public VectorObject cam_t_m2c;
        public int obj_id;

        public JSONNode Serialize()
        {
            var n = new JSONObject();
            n["cam_R_m2c"] = cam_R_m2c.Serialize();
            n["cam_t_m2c"] = cam_t_m2c.Serialize();
            n["obj_id"] = obj_id;
            return n;
        }
    }
    [Serializable]
    private struct PoseListObject
    {
        public int id;
        public Dictionary<string, List<PoseObject>> scene_gt;

        public JSONNode Serialize()
        {
            var n = new JSONObject();
            //n["id"] = id;
            //var h = n["scene_gt"].AsObject;
            foreach (var v in scene_gt)
            {
                JSONArray poses = new JSONArray();
                foreach (var pose in v.Value)
                    poses.Add(pose.Serialize());

                n[v.Key] = poses;
            }
            return n;
        }
    }

    [Serializable]
    private struct CameraObject
    {
        public Matrix3x3Object cam_K;
        public Matrix3x3Object cam_R_w2c;
        public VectorObject cam_t_w2c;
        public float depth_scale;

        public JSONNode Serialize()
        {
            var n = new JSONObject();
            n["cam_K"] = cam_K.Serialize();
            n["cam_R_w2c"] = cam_R_w2c.Serialize();
            n["cam_t_w2c"] = cam_t_w2c.Serialize();
            n["depth_scale"] = depth_scale;
            return n;
        }
    }
    [Serializable]
    private struct CameraListObject
    {
        public Dictionary<string, CameraObject> camera_gt;

        public JSONNode Serialize()
        {
            var n = new JSONObject();
            //n["id"] = id;
            //var h = n["scene_gt"].AsObject;
            foreach (var v in camera_gt)
            {
                n[v.Key] = v.Value.Serialize();
            }
            return n;
        }
    }

    [Serializable]
    private struct SceneInfoObject
    {
        public int carrier_id;
        public int composition_type;
        public string light_id;
        public float exposure;
        public float environment_rotation;
        public int nr_raytrace_samples;

        public JSONNode Serialize()
        {
            var n = new JSONObject();
            n["carrier_id"] = carrier_id;
            n["composition_type"] = composition_type;
            n["light_id"] = light_id;
            n["exposure"] = exposure;
            n["environment_rotation"] = environment_rotation;
            n["nr_of_raytrace_samples"] = nr_raytrace_samples;
            return n;
        }
    }
    [Serializable]
    private struct SceneInfoListObject
    {
        public Dictionary<string, SceneInfoObject> scene_info;

        public JSONNode Serialize()
        {
            var n = new JSONObject();
            //n["id"] = id;
            //var h = n["scene_gt"].AsObject;
            foreach (var v in scene_info)
            {
                n[v.Key] = v.Value.Serialize();
            }
            return n;
        }
    }


    public struct Model
    {
        public UnityEngine.Matrix4x4 localToWorld;
        public int obj_id;
    }

    public class SceneIterator
    {
        private int id = 0;
        private Scene scene;

        public SceneIterator(Scene s)
        {
            scene = s;
        }

        public Pose GetPose()
        {
            Pose pose = scene.poses[id];
            return pose;
        }

        public void Next()
        {
            id += 1;
            if (id >= scene.poses.Count)
            {
                Debug.LogWarning("End of BOP dataset reached. Starting over from first pose.");
                id = 0;
            }
        }
        

    }

    public struct Scene
    {
        public List<Pose> poses;

    }

    public struct Pose
    {
        public UnityEngine.Matrix4x4 projMat;
        public UnityEngine.Matrix4x4 worldToCam;
        public List<Model> models;
    }
    

    static public UnityEngine.Matrix4x4 ConvertOpencvToUnityProjectionmatrix(UnityEngine.Matrix4x4 opencvProj, int width, int height, float near, float far )
    {
        float fx = opencvProj[0, 0];
        float fy = opencvProj[1, 1];
        float px = opencvProj[0, 2];
        float py = opencvProj[1, 2];

        UnityEngine.Matrix4x4 unityProj = UnityEngine.Matrix4x4.identity;

        unityProj[0, 0] = (2.0f * fx / width);
        unityProj[1, 1] = (2.0f * fy / height);
        unityProj[0, 2] = (1.0f - 2.0f * (px / width));
        unityProj[1, 2] = (-1.0f + (2.0f * py) / height);
        unityProj[2, 2] = ((far + near) / (near - far));
        unityProj[2, 3] = (2.0f * far * near / (near - far));
        unityProj[3, 2] = (-1.0f);
        unityProj[3, 3] = (0.0f);

        return unityProj;
    }

    public static Matrix4x4 ConvertOpenCVToUnity(Quaternion rotation, Vector3 translation)
    {
        var camera_bop = Matrix4x4.TRS(translation, rotation, new Vector3(1, 1, 1));
        var rotX90 = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.Euler(90, 0, 0), new Vector3(1, 1, 1));
        var rotatedBop = rotX90 * camera_bop;
        rotatedBop[1, 0] *= -1;
        rotatedBop[1, 1] *= -1;
        rotatedBop[1, 2] *= -1;
        rotatedBop[1, 3] *= -1;

        var flipY = Matrix4x4.identity;
        flipY[1, 1] *= -1;

        return rotatedBop * flipY;
    }

    static public Scene Load(string inputPath)
    {
        string scene_camera_text = File.ReadAllText(inputPath /*+ "bop/train_PBR/000001/" */+ "scene_camera.json");
        JSONNode scene_camera = JSON.Parse(scene_camera_text);

        string scene_gt_text = File.ReadAllText(inputPath /*+ "bop/train_PBR/000001/"*/ + "scene_gt.json");
        JSONNode scene_gt = JSON.Parse(scene_gt_text);

        Scene scene = new Scene();

        // Load camera poses
        scene.poses = new List<Pose>();
        foreach (var v in scene_camera)
        {
            Pose pose = new Pose();

            //load the intrinsic camera matrix  
            pose.projMat = UnityEngine.Matrix4x4.identity;
            var camK_array = v.Value["cam_K"];
            for(int row = 0; row < 3; ++row)
            for (int col = 0; col < 3; ++col)
                pose.projMat[row, col] = camK_array[row * 3 + col];


            pose.worldToCam = UnityEngine.Matrix4x4.identity;
            //load the rotation matrix of the camera
            var rotation = v.Value["cam_R_w2c"];
            for (int row = 0; row < 3; ++row)
                for (int col = 0; col < 3; ++col)
                    pose.worldToCam[row , col] = rotation[row * 3 + col];
            //add transtalions to the camera transformation matrix
            var translation = v.Value["cam_t_w2c"];
            for (int row = 0; row < 3; ++row)
                pose.worldToCam[row, 3] = GeometryUtils.convertMmToUnity((float)translation[row]);

            //convert the camera transformation matrix to unity coordinate system
            var t = pose.worldToCam.GetTranslation();
            var r = pose.worldToCam.GetRotation();
            pose.worldToCam = ConvertOpenCVToUnity(r, t);

            // load all model matrics for this view
            pose.models = new List<Model>();
            JSONNode models_obj = scene_gt[v.Key];
            foreach (JSONNode m in models_obj)
            {
                Model model = new Model();
                model.localToWorld = UnityEngine.Matrix4x4.identity;

                rotation = m["cam_R_m2c"];
                //load the rotation matrix (in camera space) of the object 
                for (int row = 0; row < 3; ++row)
                    for (int col = 0; col < 3; ++col)
                        model.localToWorld[row, col] = rotation[row * 3 + col];

                translation = m["cam_t_m2c"];
                //add the translations (in camera space) of the object to the transformation matrix
                for (int row = 0; row < 3; ++row)
                    model.localToWorld[row, 3] = GeometryUtils.convertMmToUnity((float)translation[row]);

                //undo the y flip that is applied on the camera matrix
                var flipY = Matrix4x4.identity;
                flipY[1, 1] *= -1;

                //convert the camera space coordinates of the object to world space coordinates
                model.localToWorld = pose.worldToCam * flipY * model.localToWorld;

                model.obj_id = m["obj_id"];
                pose.models.Add(model);
            }
            scene.poses.Add(pose);
        }
        return scene;
    }


    private static int sceneId = 1;
    static public void SetupExportPath(string outputPath, int sceneNumber, bool exportDepth = false, bool exportNormal = false, bool exportAlbedo = false)
    {
        sceneId = sceneNumber;
        ensureDir(outputPath + "bop/");
        ensureDir(outputPath + "bop/metaData/");
        ensureDir(outputPath + "bop/models/");
        ensureDir(outputPath + "bop/train_PBR/");
        ensureDir(outputPath + String.Format("bop/train_PBR/{0:000000}", sceneId));
        ensureDir(outputPath + String.Format("bop/train_PBR/{0:000000}/rgb", sceneId));
        ensureDir(outputPath + String.Format("bop/train_PBR/{0:000000}/mask", sceneId));
        ensureDir(outputPath + String.Format("bop/train_PBR/{0:000000}/mask_visib", sceneId));

        ensureDir(outputPath + String.Format("bop/train_PBR/{0:000000}/mask_defect", sceneId));
        ensureDir(outputPath + String.Format("bop/train_PBR/{0:000000}/mask_defect_visib", sceneId));

        if (exportAlbedo)
            ensureDir(outputPath + String.Format("bop/train_PBR/{0:000000}/albedo", sceneId));
        if(exportNormal)
            ensureDir(outputPath + String.Format("bop/train_PBR/{0:000000}/normal", sceneId));
        if(exportDepth)
            ensureDir(outputPath + String.Format("bop/train_PBR/{0:000000}/depth", sceneId));
    }
    static private void ensureDir(string path)
    {
        if (!Directory.Exists(path))
        {
            //if it doesn't, create it
            Directory.CreateDirectory(path);

        }
    }

    static private void appendToJSON(string filename, string text, bool first_element)
    {
        // Append frame metadata to json file
        // if first frame, create the data, otherwise append to json
        // this approach will truncate the last '}' out of the json file and appends an element to the dictionary by adding a ','

        if (first_element)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(text);
                sw.Flush();
            }
        }
        else
        {
            using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                StreamWriter sw = new StreamWriter(fs);
                long endPoint = fs.Length;
                // Set the stream position to the end of the file.   
                fs.Seek(endPoint - 1, SeekOrigin.Begin);
                sw.WriteLine(',');
                sw.Write(text.Substring(1));
                sw.Flush();
            }
        }

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

    static private void exportRenderTexture(RenderTexture renderTexture, int fileID, string outputPath, ImageSaver imageSaver) {
        imageSaver.Save(renderTexture, outputPath + String.Format("bop/train_PBR/{0:000000}/rgb/", sceneId)  + fileID.ToString("D6"), ImageSaver.Extension.jpg, true);
    }
    static public void exportDepthTexture(RenderTexture depthTexture, int fileID, string outputPath, ImageSaver imageSaver)
    {
        imageSaver.Save(depthTexture, outputPath + String.Format("bop/train_PBR/{0:000000}/depth/", sceneId) + fileID.ToString("D6"), ImageSaver.Extension.exr, false, true);
    }
    static public void exportAlbedoTexture(RenderTexture albedoTexture, int fileID, string outputPath, ImageSaver imageSaver)
    {
        imageSaver.Save(albedoTexture, outputPath + String.Format("bop/train_PBR/{0:000000}/albedo/", sceneId) + fileID.ToString("D6"), ImageSaver.Extension.jpg, true);
    }
    static public void exportNormalTexture(RenderTexture normalTexture, int fileID, string outputPath, ImageSaver imageSaver)
    {
        imageSaver.Save(normalTexture, outputPath + String.Format("bop/train_PBR/{0:000000}/normal/", sceneId) + fileID.ToString("D6"), ImageSaver.Extension.jpg, true);
    }

    static RenderTexture splitSegmentationTextures;
    static RenderTexture splitSegmentationDefectTextures;
    static RenderTexture splitSegmentationDefectVisibTextures;
    private static void ensureSplitTextureStack(ref RenderTexture saveLocation, int stackSize, int width, int height)
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
    static private void exportSegmentationTexture(List<UnityEngine.GameObject> instantiated_models, RenderTexture segmentationTexture, RenderTexture segmentationTextureArray, int fileID, string outputPath, ImageSaver imageSaver)
    {
        if (instantiated_models.Count == 0)
        {
            Debug.LogWarning("No objects are exported. Check the export settings of the main generator and the object spawners");
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
                colorData[i] = new Vector3(0,0,0);
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

        imageSaver.SaveArray(splitSegmentationTextures, instantiated_models.Count, outputPath + String.Format("bop/train_PBR/{0:000000}/mask_visib/", sceneId) + fileID.ToString("D6"), ImageSaver.Extension.jpg, false, true);
        imageSaver.SaveArray(splitSegmentationDefectVisibTextures, instantiated_models.Count, outputPath + String.Format("bop/train_PBR/{0:000000}/mask_defect_visib/", sceneId) + fileID.ToString("D6"), ImageSaver.Extension.jpg, false, true);
        imageSaver.SaveArray(splitSegmentationDefectTextures, instantiated_models.Count, outputPath + String.Format("bop/train_PBR/{0:000000}/mask_defect/", sceneId) + fileID.ToString("D6"), ImageSaver.Extension.jpg, false, true);
        imageSaver.SaveArray(segmentationTextureArray, instantiated_models.Count, outputPath + String.Format("bop/train_PBR/{0:000000}/mask/", sceneId) + fileID.ToString("D6"), ImageSaver.Extension.jpg, false, true);
    }


    public static Matrix4x4 ConvertUnityToOpenCV(Quaternion rotation, Vector3 translation)
    {
        var flipY = Matrix4x4.identity;
        flipY[1, 1] *= -1;
        var rotX90 = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.Euler(90, 0, 0), new Vector3(1, 1, 1));

        var camera_bop = Matrix4x4.TRS(translation, rotation, new Vector3(1, 1, 1)) * flipY.inverse;
        camera_bop[1, 0] *= -1;
        camera_bop[1, 1] *= -1;
        camera_bop[1, 2] *= -1;
        camera_bop[1, 3] *= -1;
        var rotatedBop = rotX90.inverse * camera_bop;

        return rotatedBop;
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

    static float depthScale = 1.0f;
    static public void setDepthScale(float newDepthScale) { depthScale = newDepthScale; }
    static private void exportCameraData(RenderTexture renderTexture, int fileID, string outputPath, Camera camera)
    {
        UnityEngine.Matrix4x4 worldToCam = ConvertUnityToOpenCV(camera.transform.rotation, camera.transform.position);
        // exporting scene camera metadata
        CameraListObject scene_camera_obj = new CameraListObject();
        scene_camera_obj.camera_gt = new Dictionary<string, CameraObject>();

        CameraObject camera_gt = new CameraObject();
        camera_gt.cam_K = new Matrix3x3Object();
        camera_gt.cam_K.mat = constructCameraMatrix(renderTexture.width, renderTexture.height, camera.fieldOfView, camera);
        camera_gt.cam_R_w2c = new Matrix3x3Object();
        camera_gt.cam_R_w2c.mat = worldToCam;
        camera_gt.cam_t_w2c = new VectorObject();
        camera_gt.cam_t_w2c.vector = GeometryUtils.convertUnityToMm(new UnityEngine.Vector3(worldToCam[0, 3], worldToCam[1, 3], worldToCam[2, 3]));
        camera_gt.depth_scale = depthScale;

        scene_camera_obj.camera_gt[fileID.ToString()] = camera_gt;
        string text_scene_camera = scene_camera_obj.Serialize().ToString();
        appendToJSON(outputPath + String.Format("bop/train_PBR/{0:000000}/", sceneId) + "scene_camera.json", text_scene_camera, fileID == 1);
    }

    public static void exportKeyPoints(List<GameObject> instantiated_models, RenderTexture depthTexture, int fileID, string outputPath, Camera camera)
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
                keypoint.cam_R_m2c = new Matrix3x3Object();
                keypoint.cam_R_m2c.mat = viewMat;
                keypoint.cam_t_m2c = new VectorObject();
                keypoint.cam_t_m2c.vector = GeometryUtils.convertUnityToMm(translation);
                keypoint.loc_t_l2m = new VectorObject();
                keypoint.loc_t_l2m.vector = keypointObject.localPosition;
                var screenCo = camera.WorldToScreenPoint(keypointObject.position);
                keypoint.screen_co = new Vector2Int((int) Math.Round(screenCo.x), (int) Math.Round(screenCo.y));
                if (keypoint.screen_co.x >= 0 && keypoint.screen_co.x < depthText.width
                    && keypoint.screen_co.y >= 0 && keypoint.screen_co.y < depthText.height) {
                    var depthDistance = GeometryUtils.convertMmToUnity(depthText.GetPixel(keypoint.screen_co.x, keypoint.screen_co.y).linear.r * depthScale);
                    keypoint.isVisible = depthDistance - translation.z > 0 && translation.z > 0;
                }
                else
                    keypoint.isVisible = false;
                instanceKeyPoints.Add(keypoint);
            }
            SceneKeyPoints.Add(instanceKeyPoints);

        }

        obj.keypoints_gt[fileID.ToString()] = SceneKeyPoints;
        string text = obj.Serialize().ToString();
        appendToJSON(outputPath + String.Format("bop/train_PBR/{0:000000}/", sceneId) + "keyPoints_gt.json", text, fileID == 1);
    }

    static private void exportObjectData(List<UnityEngine.GameObject> instantiated_models, int fileID, string outputPath, Camera camera)
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
            if (!idExportData.exported)
            {
                //TODO fix bug for complex meshes: exportModel(model, outputPath + String.Format("bop/models/{0:000000}.ply", idExportData.id));
                idExportData.exported = true;
                obj_name_to_id[model.name] = idExportData;
                exportModelId(model.name, idExportData.id, outputPath);
            }
            poses.Add(pose);
        }

        obj.scene_gt[fileID.ToString()] = poses;
        string text = obj.Serialize().ToString();
        appendToJSON(outputPath + String.Format("bop/train_PBR/{0:000000}/", sceneId) + "scene_gt.json", text, fileID == 1);
    }

    static private bool first = true;
    private static void exportModelId(string modelName, int id, string outputPath)
    {
        var n = new JSONObject();
        n[id.ToString()] = modelName;
        appendToJSON(outputPath + String.Format("bop/train_PBR/{0:000000}/", sceneId) + "model_id.json", n.ToString(), first);
        first = false;
    }

    public static void exportModel(GameObject model, String filename)
    {
        MeshFilter[] meshFilters = model.GetComponentsInChildren<MeshFilter>();
        List<CombineInstance> combine = new List<CombineInstance>();
        int i = 0;
        while (i < meshFilters.Length)
        {
            for(int j = 0; j < meshFilters[i].mesh.subMeshCount; ++j)
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
        mesh.CombineMeshes(combine.ToArray(), false,  false);
        try
        {
            PLYExporter.MeshToFile(mesh, filename);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Failed to export model " + model.name + " to ply format.\n" + e.Message);
        }
    }

    static private GameObject renderSettings;
    static public void setRenderSettingsObject(GameObject settings)
    {
        renderSettings = settings;
    }

    static int nr_raytracing_samples;
    static public void setNrOfRaytracingSamples(int samplecount) { nr_raytracing_samples = samplecount; }
    static private void exportSceneData(int fileID, string outputPath)
    {
        SceneInfoListObject obj = new SceneInfoListObject();
        obj.scene_info = new Dictionary<string, SceneInfoObject>();
        var sceneInfo = new SceneInfoObject();

        renderSettings.GetComponent<Volume>().profile.TryGet<HDRISky>(out var sky);
        sceneInfo.light_id = sky.hdriSky.value.name;
        sceneInfo.environment_rotation = sky.rotation.value;
        sceneInfo.exposure = sky.exposure.value;

        //specific for robojob paper
        sceneInfo.carrier_id = (sceneId / 100000) % 10;
        sceneInfo.composition_type = (sceneId / 1000) % 10;

        sceneInfo.nr_raytrace_samples = nr_raytracing_samples;

        obj.scene_info[fileID.ToString()] = sceneInfo;
        string text = obj.Serialize().ToString();
        appendToJSON(outputPath + String.Format("bop/train_PBR/{0:000000}/", sceneId) + "scene_info.json", text, fileID == 1);
    }

    static public void exportFrame(List<UnityEngine.GameObject> instantiated_models, RenderTexture renderTexture, RenderTexture segmentationTexture, RenderTexture segmentationTextureArray, int fileID, string outputPath, Camera camera, ImageSaver imageSaver) {
        exportRenderTexture(renderTexture, fileID, outputPath, imageSaver);
        exportSegmentationTexture(instantiated_models, segmentationTexture, segmentationTextureArray, fileID, outputPath, imageSaver);
        exportCameraData(renderTexture, fileID, outputPath, camera);
        exportObjectData(instantiated_models,  fileID, outputPath, camera);
        exportSceneData(fileID, outputPath);
    }
}
