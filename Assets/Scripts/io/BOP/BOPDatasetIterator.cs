using SimpleJSON;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using static Assets.Scripts.io.BOP.BOPDataset;

namespace Assets.Scripts.io.BOP
{
    public class BOPSceneIterator : SceneIteratorInterface
    {
        private int id = 0;
        private BOPScene scene;
        public BOPImportSettings dataset;

        private List<String> bopSceneDirectorys = new List<String>();
        private int bopSceneDirIndex = 0;

        public BOPSceneIterator(BOPScene s)
        {
            scene = s;
        }
        public BOPSceneIterator(string inputPath)
        {
            var dirInfo = new DirectoryInfo(dataset.inputPath);
            if (Regex.IsMatch(dirInfo.Name, @"[0-9][0-9][0-9][0-9][0-9][0-9]"))
                bopSceneDirectorys.Add(dirInfo.FullName + '/');
            else
            {
                foreach (DirectoryInfo subDirectory in dirInfo.GetDirectories())
                {
                    if (Regex.IsMatch(subDirectory.Name, @"[0-9][0-9][0-9][0-9][0-9][0-9]"))
                        bopSceneDirectorys.Add(subDirectory.FullName + '/');

                    else if (Regex.IsMatch(subDirectory.Name, @"[0-9][0-9][0-9][0-9][0-9][0-9]_[0-9][0-9]"))
                        bopSceneDirectorys.Add(subDirectory.FullName + '/');
                }
            }
            loadNextBopScene();
        }

        public override C2RPose GetPose()
        {
            C2RPose pose = scene.poses[id];
            return pose;
        }

        public override void Next()
        {
            id += 1;
            if (id >= scene.poses.Count)
            {
                loadNextBopScene();
                id = 0;
                raiseNewSceneLoaded();
            }
        }

        int rngSeed = 0;
        private void loadNextBopScene()
        {
            if (bopSceneDirIndex >= bopSceneDirectorys.Count)
            {
                bopSceneDirIndex = 0;
                Debug.LogWarning("End of BOP dataset reached. Starting over from first scene.");
                raiseLastSceneEnded();
            }
            string currentBopPath = bopSceneDirectorys[bopSceneDirIndex];

            var sceneDir = new DirectoryInfo(currentBopPath);
            if (sceneDir.Name.Length == 6 && Regex.IsMatch(sceneDir.Name, @"[0-9][0-9][0-9][0-9][0-9][0-9]"))
                rngSeed = Int32.Parse(new DirectoryInfo(currentBopPath).Name);
            else if (sceneDir.Name.Length == 9 && Regex.IsMatch(sceneDir.Name, @"[0-9][0-9][0-9][0-9][0-9][0-9]_[0-9][0-9]"))
                rngSeed = Int32.Parse(new DirectoryInfo(currentBopPath).Name.Substring(0, 6)) * 100 + Int32.Parse(new DirectoryInfo(currentBopPath).Name.Substring(7, 2));

            scene = Load(currentBopPath);

            ++bopSceneDirIndex;
        }

        private BOPScene Load(string inputPath)
        {
            string scene_camera_text = File.ReadAllText(inputPath /*+ "bop/train_PBR/000001/" */+ "scene_camera.json");
            JSONNode scene_camera = JSON.Parse(scene_camera_text);

            string scene_gt_text = File.ReadAllText(inputPath /*+ "bop/train_PBR/000001/"*/ + "scene_gt.json");
            JSONNode scene_gt = JSON.Parse(scene_gt_text);

            BOPScene scene = new BOPScene();

            // Load camera poses
            scene.poses = new List<SceneIteratorInterface.C2RPose>();
            foreach (var v in scene_camera)
            {
                SceneIteratorInterface.C2RPose pose = new SceneIteratorInterface.C2RPose();

                //load the intrinsic camera matrix  
                pose.projMat = UnityEngine.Matrix4x4.identity;
                var camK_array = v.Value["cam_K"];
                for (int row = 0; row < 3; ++row)
                    for (int col = 0; col < 3; ++col)
                        pose.projMat[row, col] = camK_array[row * 3 + col];


                pose.worldToCam = UnityEngine.Matrix4x4.identity;
                //load the rotation matrix of the camera
                var rotation = v.Value["cam_R_w2c"];
                for (int row = 0; row < 3; ++row)
                    for (int col = 0; col < 3; ++col)
                        pose.worldToCam[row, col] = rotation[row * 3 + col];
                //add transtalions to the camera transformation matrix
                var translation = v.Value["cam_t_w2c"];
                for (int row = 0; row < 3; ++row)
                    pose.worldToCam[row, 3] = GeometryUtils.convertMmToUnity((float)translation[row]);

                //convert the camera transformation matrix to unity coordinate system
                var t = pose.worldToCam.GetTranslation();
                var r = pose.worldToCam.GetRotation();
                pose.worldToCam = ConvertOpenCVToUnity(r, t);

                // load all model matrics for this view
                pose.models = new List<SceneIteratorInterface.C2RModel>();
                JSONNode models_obj = scene_gt[v.Key];
                foreach (JSONNode m in models_obj)
                {
                    SceneIteratorInterface.C2RModel model = new SceneIteratorInterface.C2RModel();
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
    }
}
