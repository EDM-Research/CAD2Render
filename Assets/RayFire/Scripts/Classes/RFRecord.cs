#if UNITY_EDITOR

using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;


// Namespace
namespace RayFire
{
    public static class RFRecorder
    {
        // Create animation clip
        public static void CreateAnimationClip(List<RFCache> cacheList, List<float> timeList, float threshold, int rate, string assetFolder, string clipName, bool optimizekeys)
        {
            // Stop
            if (timeList.Count == 0)
                return;
            
            // Create main clip
            AnimationClip clip  = new AnimationClip();
            clip.legacy = false;
            clip.frameRate = rate;
            clip.name = clipName + "_animation";
            
            // Create curves for each object
            foreach (RFCache cache in cacheList)
            {
                if (cache.transform != null)
                {
                    // active
                    // SetCurvePosition (ref clip, cache.pos, timeList, 0, cache.name, "localPosition.x", threshold, rate, optimizekeys);
                    
                    SetCurvePosition (ref clip, cache.pos, timeList, 0, cache.name, "localPosition.x", threshold, rate, optimizekeys);
                    SetCurvePosition (ref clip, cache.pos, timeList, 1, cache.name, "localPosition.y", threshold, rate, optimizekeys);
                    SetCurvePosition (ref clip, cache.pos, timeList, 2, cache.name, "localPosition.z", threshold, rate, optimizekeys);
                    
                    SetCurveRotation (ref clip, cache.rot, timeList, 0, cache.name, "localRotation.x", threshold, rate, optimizekeys);
                    SetCurveRotation (ref clip, cache.rot, timeList, 1, cache.name, "localRotation.y", threshold, rate, optimizekeys);
                    SetCurveRotation (ref clip, cache.rot, timeList, 2, cache.name, "localRotation.z", threshold, rate, optimizekeys);
                    SetCurveRotation (ref clip, cache.rot, timeList, 3, cache.name, "localRotation.w", threshold, rate, optimizekeys);
                }
            }

            // Set Folder
            if (Directory.Exists (assetFolder) == false)
                Directory.CreateDirectory(assetFolder);
            
            // Save clip asset
            string clipPath = assetFolder + clipName + "_animation.anim";
            AssetDatabase.CreateAsset(clip, clipPath);
            
            // Save controller
            string controllerPath = assetFolder + clipName + "_controller.controller";
            AnimatorController cont = AnimatorController.CreateAnimatorControllerAtPath (controllerPath);
            AnimatorStateMachine stateMachine = cont.layers[0].stateMachine;

             // Set clip and states
            SetStates (stateMachine, clip);
            
            // Asset ops
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        // Set clip and states
        static int SetStates (AnimatorStateMachine stateMachine, AnimationClip clip)
        {
            // Empty entry state
            AnimatorState emptyState = stateMachine.AddState ("EmptyState");
            emptyState.speed = 0;
            emptyState.writeDefaultValues = false;
            
            // Animation state
            AnimatorState recordState = stateMachine.AddState (clip.name);
            recordState.motion = clip;
            recordState.tag = clip.name + "Tag";
    
            // Exit transition
            AnimatorStateTransition exitTransition = recordState.AddExitTransition();
            exitTransition.duration = 0;
            exitTransition.hasExitTime = true;

            return recordState.nameHash;
        }
        
        // Set curve to clop
        static void SetCurvePosition (ref AnimationClip clip, List<Vector3> list, List<float> timeList, int ind, string nameVar, string track, float threshold, int rate, bool optimizeKeys)
        {
            // Create keys
            Keyframe[] keys = new Keyframe[timeList.Count];
            for (int i = 0; i < timeList.Count; i++)
                keys[i] = new Keyframe(timeList[i], list[i][ind], 0f, 0f, 0f, 0f);
            
            // Optimize
            if (optimizeKeys == true)
                keys = OptimizeKeys(keys, threshold, rate);

            // All keys was reduced
            if (keys.Length < 2)
                return;
            
            // Set keys to curve
            AnimationCurve curve = new AnimationCurve(keys);
            
            // Set key type
            for (int i = 0; i < curve.keys.Length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode (curve, i, AnimationUtility.TangentMode.Linear);
                AnimationUtility.SetKeyRightTangentMode (curve, i, AnimationUtility.TangentMode.Linear);
            }

            // Set curve to track
            clip.SetCurve(nameVar, typeof(Transform), track, curve);
        }
        
        // Set curve to clop
        static void SetCurveRotation (ref AnimationClip clip, List<Quaternion> list, List<float> timeList, int ind, string nameVar, string track, float threshold, int rate, bool optimizeKeys)
        {
            // Create keys
            Keyframe[] keys = new Keyframe[timeList.Count];
            for (int i = 0; i < timeList.Count; i++)
                keys[i] = new Keyframe (timeList[i], list[i][ind], 0f, 0f, 0f, 0f);

            // Optimize
            if (optimizeKeys == true)
                keys = OptimizeKeys(keys, threshold, rate);
            
            // All keys was reduced
            if (keys.Length < 2)
                return;
            
            // Set keys to curve
            AnimationCurve curve = new AnimationCurve(keys);

            // Set key type
            for (int i = 0; i < curve.keys.Length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode (curve, i, AnimationUtility.TangentMode.Linear);
                AnimationUtility.SetKeyRightTangentMode (curve, i, AnimationUtility.TangentMode.Linear);
            }

            // Set curve to track
            clip.SetCurve(nameVar, typeof(Transform), track, curve);
        }

        // Optimize keys
        static Keyframe[] OptimizeKeys(Keyframe[] keys, float threshold, int rate)
        {
            if (keys.Length <= 1)
                return keys;

            // Remove same keys
            List<int> removeInd = new List<int>();
            List<Keyframe> list = keys.ToList();
            
            // Collect indexes of all same keys between it's neibs
            for (int i = list.Count - 2; i > 1; i--)
                if (list[i].value - list[i - 1].value == 0 && list[i].value - list[i + 1].value == 0)
                    removeInd.Add (i);

            // Remove same keys
            for (int i = 0; i < removeInd.Count; i++)
                list.RemoveAt (removeInd[i]);
            if (list.Count == 1)
            {
                list.Clear();
                return list.ToArray();
            }
            
            // Remove by threshold
            if (threshold > 0 && list.Count > 6)
            {
                removeInd.Clear();
                float val = threshold / rate;
                for (int i = list.Count - 3; i > 3; i--)
                    if (Mathf.Abs (list[i].value - list[i - 1].value) > val && Mathf.Abs (list[i].value - list[i + 1].value) > val)
                        removeInd.Add (i);
                
                // Remove same keys
                for (int i = 0; i < removeInd.Count; i++)
                    list.RemoveAt (removeInd[i]);
                if (list.Count == 1)
                {
                    list.Clear();
                    return list.ToArray();
                }
            }
            
            return list.ToArray();
        }
    }
}

#endif
