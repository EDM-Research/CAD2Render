using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;


// Cache setup props and recreate joints if different.
// Joints reset
// Break force by mass
// Different break variation at every init
// Runtime demolition jointing

// Joint management during sim:
// 1. ReCreate joint after few frames after break
// + time delay


// Preview for current force and iteration count, with color
// Label for total amount of joints at start / existing amount
// Set position based on size proportion

// TODO runtime joint creation for few frames to stick with neibs for better friction

// setup deform list at awake if Joints were precached


namespace RayFire
{
    [Serializable]
    public class RFJoint
    {
        public int id;
        public int df;  // Amount of happened deformations
        
        // Components
        public ConfigurableJoint jn;
        public Rigidbody         r1;
        public Rigidbody         r2;
        
        // Save props
        public int br;
        public int an;
        public int dm;

        public RFJoint(int Id, int Br, int An, int Dm)
        {
            id = Id;
            br = Br;
            an = An;
            dm = Dm;
        }
    }

    [Serializable]
    public class RFJointProperties
    {
        public enum RFJointBreakType
        {
            Breakable = 0,
            Unbreakable = 1
        }
        
        // Main properties
        public bool             enable;
        public RFJointBreakType breakType     = RFJointBreakType.Breakable;
        public int              breakForce    = 100;
        public int              breakForceVar = 10;
        public bool             forceByMass;
        public bool             varInAwake;
        public int              angleLimit    = 10;
        public int              angleLimitVar = 10;
        public int              damper        = 1000;

        // Deformation properties
        public bool  deformEnable = false;
        public int   deformCount  = 20;
        public float stiffFrc     = 0.75f;
        public int   stiffAbs     = 50;
        public int   bend         = 0;
        public int   percentage   = 100;
        public float weakening    = 0.8f;
        
        
        public int initAmount;
        
        // Lists
        [NonSerialized] 
        public List<RFJoint> deformList;
        public List<RFJoint> jointList = new List<RFJoint>();

        // Static
        public static SoftJointLimit       jointLimit = new SoftJointLimit();
        public static SoftJointLimitSpring spring     = new SoftJointLimitSpring();

        /// /////////////////////////////////////////////////////////
        /// Create
        /// ///////////////////////////////////////////////////////// 

        // Create joint
        public static void CreateJoints (RFCluster cluster, RFJointProperties joints)
        {
            // Skip if already has joints
            if (joints.HasJoints == true)
                return;
            
            // Prepare empty list
            joints.EmptyList ();
            
            // Set unchecked state
            RFShard.SetUnchecked (cluster.shards);
            
            // Create joints
            int id = 0;
            foreach (var shard in cluster.shards)
            {
                foreach (var neib in shard.neibShards)
                {
                    // Skip neib with created joints
                    if (neib.check == true)
                        continue;

                    // Create joint
                    RFJoint rfJoint = CreateJoint (shard, neib, joints, id++);
  
                    // Collect
                    joints.jointList.Add (rfJoint);
                    
                    /*
                    // Create joint
                    RFJoint rfJoint2 = CreateJoint (shard, neib, joints, id++);
  
                    // Collect
                    joints.jointList.Add (rfJoint2);
                    
                    // Create joint
                    RFJoint rfJoint3 = CreateJoint (shard, neib, joints, id++);
  
                    // Collect
                    joints.jointList.Add (rfJoint3);
                    */
                }
                shard.check = true;
            }

            // Save init amount
            joints.initAmount = joints.jointList.Count;
            
            // Set unchecked state
            RFShard.SetUnchecked (cluster.shards);
            
            // Angular Motion
            SetAngularMotion(joints.angleLimit, joints.angleLimitVar, joints.jointList);

            // Angular Spring
            if (joints.damper > 0)
                SetSpring(joints.damper, joints.jointList);
            
            // Break Force
            if (joints.breakType == RFJointBreakType.Breakable)
                SetBreakForce (joints.breakForce, joints.breakForceVar, joints.jointList, joints.forceByMass);
            
            // Save final props for reset
            SaveProperties(joints.jointList);
        }

        // Create joint
        public static RFJoint CreateJoint(RFShard shard, RFShard neib, RFJointProperties joints, int id)
        {
            // Create joint
            RFJoint rfJoint = new RFJoint(id, joints.breakForce, joints.angleLimit, joints.damper);
            rfJoint.jn = shard.tm.gameObject.AddComponent<ConfigurableJoint>();
            rfJoint.r1 = shard.rb;
            rfJoint.r2 = neib.rb;
            
            // Setup joint
            rfJoint.jn.connectedBody       = neib.rb;
            rfJoint.jn.enableCollision     = false;
            rfJoint.jn.enablePreprocessing = true;
                    
            // Set joint position and axis
            SetTransform (shard.tm, neib.tm, rfJoint.jn);
                    
            // Position Motion
            SetPositionMotion (rfJoint.jn);
            
            return rfJoint;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Motion
        /// ///////////////////////////////////////////////////////// 

        // Position Motion
        public static void SetPositionMotion (ConfigurableJoint joint)
        {
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
        }
        
        // Angular Motion
        public static void SetAngularMotion(float angleLimit, RFJoint joint)
        {
            if (angleLimit == 0)
            {
                joint.jn.angularXMotion = ConfigurableJointMotion.Locked;
                joint.jn.angularYMotion = ConfigurableJointMotion.Locked;
                joint.jn.angularZMotion = ConfigurableJointMotion.Locked;
            }
            else
            {
                joint.jn.angularXMotion = ConfigurableJointMotion.Limited;
                joint.jn.angularYMotion = ConfigurableJointMotion.Limited;
                joint.jn.angularZMotion = ConfigurableJointMotion.Limited;
                jointLimit.limit        = angleLimit;
                joint.jn.angularYLimit  = jointLimit;
                joint.jn.angularZLimit  = jointLimit;
            }
        }
        
        // Angular Motion
        public static void SetAngularMotion(float angleLimit, int var, List<RFJoint> jointList)
        {
            Random.InitState (0);
            if (jointList != null && jointList.Count > 0)
                for (int i = 0; i < jointList.Count; i++)
                    if (jointList[i] != null)
                        SetAngularMotion (angleLimit + (int)Random.Range(0, angleLimit * var / 100f), jointList[i]);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Damper
        /// ///////////////////////////////////////////////////////// 
        
        // Joint spring
        public static void SetSpring(int damper, ConfigurableJoint joint)
        {
            spring.damper              = damper;
            spring.spring              = damper;
            joint.angularYZLimitSpring = spring;
        }
        
        // Joint spring
        public static void SetSpring(int damper, List<RFJoint> jointList)
        {
            if (jointList != null && jointList.Count > 0)
                for (int i = 0; i < jointList.Count; i++)
                    if (jointList[i] != null)
                        SetSpring (damper, jointList[i].jn);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Break Force
        /// ///////////////////////////////////////////////////////// 
        
        // Single Joint break force
        public static void SetBreakForce(int force, ConfigurableJoint joint)
        {
            joint.breakForce = force;
        }
        
        // Joint List break force
        public static void SetBreakForce(int force, List<RFJoint> jointList)
        {
            if (jointList != null && jointList.Count > 0)
                for (int i = 0; i < jointList.Count; i++)
                    if (jointList[i] != null)
                        SetBreakForce (force, jointList[i].jn);
        }
        
        // Joint List break force variation
        public static void SetBreakForce(int force, int var, List<RFJoint> jointList, bool byMass)
        {
            if (byMass == false)
            {
                Random.InitState (0);
                if (jointList != null && jointList.Count > 0)
                    for (int i = 0; i < jointList.Count; i++)
                        if (jointList[i] != null)
                            SetBreakForce (force + Random.Range (0, var), jointList[i].jn);
            }
            else
            {
                
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Transform
        /// ///////////////////////////////////////////////////////// 

        // Set joint position and axis
        public static void SetTransform (Transform tm1, Transform tm2, ConfigurableJoint joint)
        {
            // Position
            Vector3 vector = (tm2.position - tm1.position) / 2f;
            joint.anchor = tm1.InverseTransformVector (vector);
            //joint.anchor = tm1.InverseTransformVector (vector) + new Vector3 (Random.Range (-0.2f, 0.2f), Random.Range (-0.2f, 0.2f), Random.Range (-0.2f, 0.2f));
            
            // Axis
            joint.axis = tm1.InverseTransformDirection (vector);
        }

        /// /////////////////////////////////////////////////////////
        /// List
        /// ///////////////////////////////////////////////////////// 

        public bool HasJoints { get { return jointList != null && jointList.Count > 0; } }
        public bool HasDeforms { get { return deformList != null && deformList.Count > 0; } }
        
        // Prepare empty list
        public void EmptyList () { if (jointList == null) jointList = new List<RFJoint>(); else jointList.Clear(); }

        // Destroy all existing joints
        public void DestroyJoints()
        {
            if (HasJoints == true)
            {
                for (int i = 0; i < jointList.Count; i++)
                    if (jointList[i] != null && jointList[i].jn)
                        Object.DestroyImmediate (jointList[i].jn);
                jointList.Clear();
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Properties
        /// ///////////////////////////////////////////////////////// 

        // Save properties in joint class to reset later
        public static void SaveProperties(List<RFJoint> joints)
        {
            for (int i = 0; i < joints.Count; i++)
            {
                joints[i].br = (int)joints[i].jn.breakForce;
                joints[i].an = (int)joints[i].jn.angularYLimit.limit;
                joints[i].dm = (int)joints[i].jn.angularYZLimitSpring.damper;
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Deformation
        /// ///////////////////////////////////////////////////////// 
        
        // Create joint
        public static RFJoint DeformJoint(RFJoint joint, RFJointProperties joints)
        {
            // ReSet joint position and axis
            SetTransform (joint.r1.transform, joint.r2.transform, joint.jn);
            
            // Bending
            if (joints.bend > 0)
                SetAngularMotion (joint.an + joints.bend, joint);

            // Weakening
            if (joint.br > 0 && joints.weakening > 0)
                SetBreakForce ((int)(joint.jn.breakForce * (1f - joints.weakening)), joint.jn);

            // Iterate deformation
            joint.df++;
            
            return joint;
        }
        
        // Prepare deformation
        public static void SetDeformation(RayfireConnectivity scr)
        {
            if (scr.joints.deformEnable == true)
            {
                if (scr.joints.HasJoints == true)
                {
                    scr.joints.deformList = new List<RFJoint>();
                    for (int i = 0; i < scr.joints.jointList.Count; i++)
                        if (scr.joints.percentage == 100 || scr.joints.percentage > Random.Range (1, 100))
                            scr.joints.deformList.Add (scr.joints.jointList[i]);
                    scr.StartCoroutine (scr.joints.DeformationCor());
                }
            }
        }
        
        // Joints deformation cor
        public IEnumerator DeformationCor()
        {
            // Deformation disabled
            if (deformEnable == false)
                yield break;
            
            // Has no joints
            if (HasJoints == false)
                yield break;

            // Repeat every frame
            while (deformEnable == true && deformList.Count > 0)
            {
                // Check joints current force
                for (int i = deformList.Count - 1; i >= 0; i--)
                {
                    // Joint broken
                    if (deformList[i].jn == null)
                    {
                        deformList.RemoveAt (i);
                        continue;
                    }
                    
                    // Skip if exceeded deformation count
                    if (deformList[i].df >= deformCount)
                    {
                        deformList.RemoveAt (i);
                        continue;
                    }
                    
                    // Stiffness check
                    if (breakType == RFJointBreakType.Breakable)
                    {
                        if (deformList[i].jn.currentForce.magnitude < breakForce * stiffFrc)
                            continue;
                    }
                    else
                    {
                        if (deformList[i].jn.currentForce.magnitude < stiffAbs)
                            continue;
                    }

                    // Reset joint tm
                    DeformJoint (deformList[i], this);
                }
                
                yield return null;
            }
        }
    }
}