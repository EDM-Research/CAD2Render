using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Demolition support
// Reset

namespace RayFire
{
    // Class to store animation cache
    [Serializable]
    public class RFCache
    {
        // Vars
        public string           name;
        public List<bool>       act;
        public List<Vector3>    pos;
        public List<Quaternion> rot;
        public Transform        transform;

        // Constructor
        public RFCache (Transform parent, Transform tm)
        {
            act       = new List<bool>();
            pos       = new List<Vector3>();
            rot       = new List<Quaternion>();
            transform = tm;
            name      = tm.name;
            if (tm.parent != parent)
            {
                Transform lastParent = tm.parent;
                while (parent != lastParent)
                {
                    name       = name.Insert (0, "/");
                    name       = name.Insert (0, lastParent.name);
                    lastParent = lastParent.parent;
                }
            }
        }
    }

    [SelectionBase]
    [DisallowMultipleComponent]
    [AddComponentMenu ("RayFire/Rayfire Recorder")]
    [HelpURL ("https://rayfirestudios.com/unity-online-help/components/unity-recorder-component/")]
    public class RayfireRecorder : MonoBehaviour
    {
        public enum AnimatorType
        {
            Disabled   = 0,
            Record = 2,
            Play   = 8
        }
        
        public AnimatorType              mode          = AnimatorType.Record;
        public bool                      recordOnStart = true;
        public string                    clipName;
        public float                     duration   = 5f;
        public int                       rate       = 15;
        public bool                      reduceKeys = true;
        public float                     threshold;
        public bool                      playOnStart;
        public AnimationClip             animationClip;
        public RuntimeAnimatorController controller;
        public bool                      setToKinematic = true;
        public bool                      recorder;
        public float                     recordedTime;

        private int             stateNameHash;
        private string          assetFolder;
        private string          clipFolder = "RayFireRecords/";
        private float           stepTime;
        private Animator        animator;
        private List<Transform> tmList;
        private List<RFCache>   cacheList;
        private List<float>     timeList;

        /// //////////////////////////////////////////////////
        /// Common
        /// //////////////////////////////////////////////////

        // Awake
        void Awake()
        {
            // Set vars
            SetVariables();
        }

        // Start
        void Start()
        {
            // Collect rigid
            SetRigid();

            // Start ops
            if (mode == AnimatorType.Record && recordOnStart == true)
                StartRecord();
            else if (mode == AnimatorType.Play && playOnStart == true)
                StartPlay();
        }

        // Set rigid props
        void SetRigid()
        {
            if (mode == AnimatorType.Play)
            {
                List<RayfireRigid> rigidList = gameObject.GetComponentsInChildren<RayfireRigid>().ToList();
                foreach (RayfireRigid rigid in rigidList)
                {
                    if (rigid.physics.exclude == false)
                    {
                        rigid.physics.rec = true;

                        // Check for kinematic state
                        if (setToKinematic == true)
                            if (rigid.simulationType != SimType.Static || rigid.simulationType == SimType.Kinematic)
                            {
                                rigid.simulationType = SimType.Kinematic;
                                RFPhysic.SetSimulationType (rigid.physics.rigidBody, rigid.simulationType, rigid.objectType, rigid.physics.useGravity, rigid.physics.solverIterations);
                            }
                    }
                }
            }
        }

        // Set vars
        void SetVariables()
        {
            if (mode != AnimatorType.Disabled)
            {
                animator = GetComponent<Animator>();

                // Get list of cached transforms
                tmList = gameObject.GetComponentsInChildren<Transform> (false).ToList();
                tmList.Remove (transform);

                // No children
                if (tmList.Count == 0)
                {
                    Debug.Log ("RayFire Record: " + gameObject.name + " Mode set to " + mode.ToString() + " but object has no children. Mode set to None.", gameObject);
                    mode = AnimatorType.Disabled;
                    return;
                }

                // Record set
                SetModeRecord();

                // Play set
                SetModePlay();
            }
        }

        // Record set
        void SetModeRecord()
        {
            if (mode == AnimatorType.Record)
            {
                // Null active controller 
                if (animator != null)
                    animator.runtimeAnimatorController = null;

                // Prepare cache list
                cacheList = new List<RFCache>();
                if (tmList.Count > 0)
                    for (int i = 0; i < tmList.Count; i++)
                        cacheList.Add (new RFCache (transform, tmList[i]));

                // Time list
                timeList = new List<float>();

                // Set time step
                stepTime = 1f / rate;

                // Clip folder
                assetFolder = "Assets/" + clipFolder;
            }
        }

        // Play set
        void SetModePlay()
        {
            if (mode == AnimatorType.Play)
            {
                // Check for null controller
                if (controller == null)
                {
                    Debug.Log ("RayFire Record: " + gameObject.name + " Mode set to " + mode.ToString() + " but controller is not defined. Mode set to None.", gameObject);
                    mode = AnimatorType.Disabled;
                    return;
                }

                // Check for null controller
                if (animationClip == null)
                {
                    Debug.Log ("RayFire Record: " + gameObject.name + " Mode set to " + mode.ToString() + " but animation clip is not defined. Mode set to None.", gameObject);
                    mode = AnimatorType.Disabled;
                    return;
                }

                // Check for clip in controller
                bool hasClip = false;
                foreach (var anim in controller.animationClips)
                    if (anim == animationClip)
                        hasClip = true;
                if (hasClip == false)
                {
                    Debug.Log ("RayFire Record: " + gameObject.name + " Mode set to " + mode.ToString() + " but animation clip is not defined in controller. Mode set to None.", gameObject);
                    mode = AnimatorType.Disabled;
                    return;
                }

                // Create animator
                if (animator == null)
                    animator = gameObject.AddComponent<Animator>();
                animator.updateMode = AnimatorUpdateMode.AnimatePhysics;

                // Set defined controller
                animator.runtimeAnimatorController = controller;
            }
        }

        // Reset
        void Reset()
        {
            clipName = gameObject.name;
        }

        // //////////////////////////////////////////////////
        // Record
        // //////////////////////////////////////////////////

        // Start record
        public void StartRecord()
        {
            // Stop
            if (cacheList.Count == 0)
                return;

            // Start recording cor
            StartCoroutine (RecordCor());
        }

        // Record tm every frame
        IEnumerator RecordCor()
        {
            recorder = true;
            while (recorder == true)
            {
                // Save data
                timeList.Add (recordedTime);
                for (int i = 0; i < tmList.Count; i++)
                {
                    if (tmList[i] != null)
                    {
                        cacheList[i].act.Add (tmList[i].gameObject.activeSelf);
                        cacheList[i].pos.Add (tmList[i].localPosition);
                        cacheList[i].rot.Add (tmList[i].localRotation);
                    }
                }

                // Set time
                recordedTime += stepTime;

                // Wait
                yield return new WaitForSeconds (stepTime);

                // Temp
                if (duration > 0 && recordedTime > duration)
                    StopRecord();
            }

            // Create clip  
#if UNITY_EDITOR
            RFRecorder.CreateAnimationClip (cacheList, timeList, threshold, rate, assetFolder, clipName, reduceKeys);
#endif
        }

        // Stop record
        public void StopRecord()
        {
            recorder = false;
        }

        // //////////////////////////////////////////////////
        // Play
        // //////////////////////////////////////////////////

        // Start play
        public void StartPlay()
        {
            if (mode == AnimatorType.Play)
                animator.Play (animationClip.name);
        }
    }
}