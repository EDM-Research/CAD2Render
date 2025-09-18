using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.io.BOP
{
    public class BOPDataset
    {

        [Serializable]
        public struct Matrix3x3Object
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
        public struct VectorObject
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
        public struct KeypointObject
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
                //n["cam_R_m2c"] = cam_R_m2c.Serialize();
                //n["cam_t_m2c"] = cam_t_m2c.Serialize();
                JSONObject vec = new JSONObject();
                vec["x"] = screen_co.x;
                vec["y"] = screen_co.y;
                n["screen_co"] = vec;
                n["isVisible"] = isVisible.ToString();
                return n;
            }
        }
        [Serializable]
        public struct KeypointListObject
        {
            int fileId;
            public Dictionary<string, List<List<KeypointObject>>> keypoints_gt;

            public JSONNode Serialize()
            {
                var scene = new JSONObject();
                foreach (var keypointList in keypoints_gt)
                {
                    var sceneKeypoints = new JSONArray();
                    for (int i = 0; i < keypointList.Value.Count; ++i)
                    {
                        JSONArray instanceKeypoints = new JSONArray();
                        foreach (KeypointObject keypoint in keypointList.Value[i])
                            instanceKeypoints.Add(keypoint.Serialize());
                        sceneKeypoints.Add(instanceKeypoints);
                        //sceneKeypoints[i.ToString()] = instanceKeypoints;

                    }
                    scene[keypointList.Key] = sceneKeypoints;
                }
                return scene;
            }
        }

        [Serializable]
        public struct PoseObject
        {
            public Matrix3x3Object cam_R_m2c;
            public VectorObject cam_t_m2c;
            public int obj_id;
            public Color falseColor;

            public JSONNode Serialize()
            {
                var n = new JSONObject();
                n["cam_R_m2c"] = cam_R_m2c.Serialize();
                n["cam_t_m2c"] = cam_t_m2c.Serialize();
                n["obj_id"] = obj_id;
                if (falseColor != Color.black)
                    n["mask_color"] = "#" + ColorUtility.ToHtmlStringRGB(falseColor);//falseColor.ToString("F7");
                return n;
            }
        }
        [Serializable]
        public struct PoseListObject
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
        public struct CameraObject
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
        public struct CameraListObject
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
        public struct SceneInfoObject
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
        public struct SceneInfoListObject
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


        public struct BOPScene
        {
            public List<SceneIteratorInterface.C2RPose> poses;
        }


        static public UnityEngine.Matrix4x4 ConvertOpencvToUnityProjectionmatrix(UnityEngine.Matrix4x4 opencvProj, int width, int height, float near, float far)
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
    }
}
