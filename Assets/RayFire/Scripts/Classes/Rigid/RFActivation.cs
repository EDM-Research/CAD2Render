using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RayFire
{
    [Serializable]
    public class RFActivation
    {
        public bool                local;
        public float               byOffset;
        public float               byVelocity;
        public float               byDamage;
        public bool                byActivator;
        public bool                byImpact;
        public bool                byConnectivity;
        public bool                unyielding;
        public bool                activatable;
        public bool                l;
        public int                 layer;
        
        public RayfireConnectivity connect; // TODO non serialized
        
        [NonSerialized] public int                 lb;
        [NonSerialized] public bool                activated;
        [NonSerialized] public bool                inactiveCorState;
        [NonSerialized] public bool                velocityCorState;
        [NonSerialized] public bool                offsetCorState;
        [NonSerialized] public IEnumerator         velocityEnum;
        [NonSerialized] public IEnumerator         offsetEnum;

        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////

        // Constructor
        public RFActivation()
        {
            byVelocity     = 0f;
            byOffset       = 0f;
            byDamage       = 0f;
            byActivator    = false;
            byImpact       = false;
            byConnectivity = false;
            unyielding     = false;
            activatable    = false;
            activated      = false;

            // unyList        = new List<int>();
            Reset();
        }

        // Copy from
        public void CopyFrom (RFActivation act)
        {
            byActivator    = act.byActivator;
            byImpact       = act.byImpact;
            byVelocity     = act.byVelocity;
            byOffset       = act.byOffset;
            local          = act.local;
            byDamage       = act.byDamage;
            byConnectivity = act.byConnectivity;
            unyielding     = act.unyielding;
            activatable    = act.activatable;
            l              = act.l;
            layer          = act.layer;
        }

        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////

        // Turn of all activation properties
        public void Reset()
        {
            activated        = false;
            inactiveCorState = false;
            velocityCorState = false;
            offsetCorState   = false;
            velocityEnum     = null;
            offsetEnum       = null;
        }

        // Connectivity check
        public void CheckConnectivity()
        {
            if (byConnectivity == true && connect != null)
            {
                connect.connectivityCheckNeed = true;
                connect = null;
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Coroutines
        /// /////////////////////////////////////////////////////////

        // Check velocity for activation
        public IEnumerator ActivationVelocityCor (RayfireRigid scr)
        {
            // Skip not activatable uny objects
            if (scr.activation.unyielding == true && scr.activation.activatable == false)
                yield break;
            
            // Stop if running 
            if (velocityCorState == true)
                yield break;

            // Set running state
            velocityCorState = true;
            
            // Check
            while (scr.activation.activated == false && scr.activation.byVelocity > 0)
            {
                if (scr.physics.rigidBody.velocity.magnitude > byVelocity)
                    scr.Activate();
                yield return null;
            }
            
            // Set state
            velocityCorState = false;
        }

        // Check offset for activation
        public IEnumerator ActivationOffsetCor (RayfireRigid scr)
        {
            // Skip not activatable uny objects
            if (scr.activation.unyielding == true && scr.activation.activatable == false)
                yield break;
            
            // Stop if running 
            if (offsetCorState == true)
                yield break;

            // Set running state
            offsetCorState = true;
           
            // Check
            while (scr.activation.activated == false && scr.activation.byOffset > 0)
            {
                if (scr.activation.local == true)
                {
                    if (Vector3.Distance (scr.transForm.localPosition, scr.physics.localPosition) > scr.activation.byOffset)
                        scr.Activate();
                }
                else
                {
                    if (Vector3.Distance (scr.transForm.position, scr.physics.initPosition) > scr.activation.byOffset)
                        scr.Activate();
                }

                yield return null;
            }
            
            // Set state
            offsetCorState = false;
        }

        // Exclude from simulation, move under ground, destroy
        public IEnumerator InactiveCor (RayfireRigid scr)
        {
            // Stop if running 
            if (inactiveCorState == true)
                yield break;

            // Set running state
            inactiveCorState = true;

            //scr.transForm.hasChanged = false;
            while (scr.simulationType == SimType.Inactive)
            {
                scr.physics.rigidBody.velocity        = Vector3.zero;
                scr.physics.rigidBody.angularVelocity = Vector3.zero;
                yield return null;
            }

            // Set state
            inactiveCorState = false;
        }

        // Activation by velocity and offset
        public IEnumerator InactiveCor  (RayfireRigidRoot scr)
        {
            // Stop if running 
            if (inactiveCorState == true)
                yield break;

            // Set running state
            inactiveCorState = true;
            
            while (scr.inactiveShards.Count > 0)
            {
                // Timestamp
                // float t1 = Time.realtimeSinceStartup;
                
                // Remove activated shards
                for (int i = scr.inactiveShards.Count - 1; i >= 0; i--)
                    if (scr.inactiveShards[i].tm == null || scr.inactiveShards[i].sm == SimType.Dynamic)
                        scr.inactiveShards.RemoveAt (i);

                // Velocity activation
                if (scr.activation.byVelocity > 0)
                {
                    for (int i = scr.inactiveShards.Count - 1; i >= 0; i--)
                    {
                        if (scr.inactiveShards[i].tm.hasChanged == true)
                            if (scr.inactiveShards[i].rb.velocity.magnitude > scr.activation.byVelocity)
                                if (ActivateShard (scr.inactiveShards[i], scr) == true)
                                    scr.inactiveShards.RemoveAt (i);
                    }

                    // Stop 
                    if (scr.inactiveShards.Count == 0)
                        yield break;
                }

                // Offset activation
                if (scr.activation.byOffset > 0)
                {
                    // By global offset
                    if (scr.activation.local == false)
                    {
                        for (int i = scr.inactiveShards.Count - 1; i >= 0; i--)
                        {
                            if (scr.inactiveShards[i].tm.hasChanged == true)
                                if (Vector3.Distance (scr.inactiveShards[i].tm.position, scr.inactiveShards[i].pos) > scr.activation.byOffset)
                                    if (ActivateShard (scr.inactiveShards[i], scr) == true)
                                        scr.inactiveShards.RemoveAt (i);
                        }
                    }
                    
                    // By local offset
                    else
                    {
                        for (int i = scr.inactiveShards.Count - 1; i >= 0; i--)
                        {
                            if (scr.inactiveShards[i].tm.hasChanged == true)
                                if (Vector3.Distance (scr.inactiveShards[i].tm.localPosition, scr.inactiveShards[i].los) > scr.activation.byOffset)
                                    if (ActivateShard (scr.inactiveShards[i], scr) == true)
                                        scr.inactiveShards.RemoveAt (i);
                        }
                    }

                    // Stop 
                    if (scr.inactiveShards.Count == 0)
                        yield break;
                }
                
                //Debug.Log (scr.inactiveShards.Count);
                
                // Stop velocity
                for (int i = scr.inactiveShards.Count - 1; i >= 0; i--)
                {
                    /*
                    if (scr.inactiveShards[i].rb == null)
                    {
                        Debug.Log (i);
                        Debug.Log (scr.inactiveShards[i].tm.name);
                    }
                    */
                    
                    scr.inactiveShards[i].rb.velocity        = Vector3.zero;
                    scr.inactiveShards[i].rb.angularVelocity = Vector3.zero;
                }
                
                // Debug.Log (Time.realtimeSinceStartup - t1);
                
                // TODO repeat 30 times per second, not every frame
                yield return null;
            }
            
            // Set state
            inactiveCorState = false;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Activate Rigid / Shard
        /// /////////////////////////////////////////////////////////

        // Activate inactive object
        public static void ActivateRigid (RayfireRigid scr, bool connCheck = true)
        {
            // Stop if excluded
            if (scr.physics.exclude == true)
                return;

            // Skip not activatable unyielding objects
            if (scr.activation.activatable == false && scr.activation.unyielding == true)
                return;

            // Initialize if not
            if (scr.initialized == false)
                scr.Initialize();

            // Turn convex if kinematic activation
            if (scr.simulationType == SimType.Kinematic)
            {
                MeshCollider meshCollider = scr.physics.meshCollider as MeshCollider;
                if (meshCollider != null)
                    meshCollider.convex = true;

                // Swap with animated object
                if (scr.physics.rec == true)
                {
                    // Set dynamic before copy
                    scr.simulationType                = SimType.Dynamic;
                    scr.physics.rigidBody.isKinematic = false;
                    scr.physics.rigidBody.useGravity  = scr.physics.useGravity;

                    // Create copy
                    GameObject inst = UnityEngine.Object.Instantiate (scr.gameObject);
                    inst.transform.position = scr.transForm.position;
                    inst.transform.rotation = scr.transForm.rotation;

                    // Save velocity
                    Rigidbody rBody = inst.GetComponent<Rigidbody>();
                    if (rBody != null)
                    {
                        rBody.velocity        = scr.physics.rigidBody.velocity;
                        rBody.angularVelocity = scr.physics.rigidBody.angularVelocity;
                    }

                    // Activate and init rigid
                    scr.gameObject.SetActive (false);
                }
            }

            // Connectivity check
            if (connCheck == true)
                scr.activation.CheckConnectivity();
            
            // Set layer
            SetActivationLayer (scr);
            
            // Set state
            scr.activation.activated = true;

            // Set props
            scr.simulationType                = SimType.Dynamic;
            scr.physics.rigidBody.isKinematic = false; // TODO error at manual activation of stressed connectivity structure
            scr.physics.rigidBody.useGravity  = scr.physics.useGravity;

            // Fade on activation
            if (scr.fading.onActivation == true) 
                scr.Fade();

            // Parent
            if (RayfireMan.inst.parent != null)
                scr.gameObject.transform.parent = RayfireMan.inst.parent.transform;

            // Init particles on activation
            RFParticles.InitActivationParticlesRigid (scr);

            // Activation sound
            RFSound.ActivationSound (scr.sound, scr.limitations.bboxSize);

            // Events
            scr.activationEvent.InvokeLocalEvent (scr);
            if (scr.meshRoot != null)
                scr.meshRoot.activationEvent.InvokeLocalEventMeshRoot (scr, scr.meshRoot);
            RFActivationEvent.InvokeGlobalEvent (scr);

            // Add initial rotation if still TODO put in ui
            if (scr.physics.rigidBody.angularVelocity == Vector3.zero)
            {
                float val = 0.3f;
                scr.physics.rigidBody.angularVelocity = new Vector3 (
                    Random.Range (-val, val), Random.Range (-val, val), Random.Range (-val, val));
            }
        }

        // Activate Rigid Root shard
        public static bool ActivateShard (RFShard shard, RayfireRigidRoot rigidRoot)
        {
            // Skip not activatable unyielding shards
            if (shard.act == false && shard.uny == true)
                return false;
            
            // Set dynamic sim state
            shard.sm = SimType.Dynamic;
            
            // Activate by Rigid if has rigid
            if (shard.rigid != null && shard.rigid.objectType == ObjectType.Mesh)
            {
                ActivateRigid (shard.rigid);
                return true;
            }

            // Physics ops
            if (shard.rb != null)
            {
                // Set props
                if (shard.rb.isKinematic == true)
                    shard.rb.isKinematic = false;

                // Turn On Gravity
                shard.rb.useGravity = rigidRoot.physics.useGravity;
                
                // Add initial rotation if still TODO put in ui
                float val = 0.3f;
                if (shard.rb.angularVelocity == Vector3.zero)
                    shard.rb.angularVelocity = new Vector3 (
                        Random.Range (-val, val), Random.Range (-val, val), Random.Range (-val, val));
            }
            
            // Set activation layer
            SetActivationLayer (shard, rigidRoot.activation);

            // Activation Fade TODO input Fade class by RigidRoot or MeshRoot
            if (rigidRoot.fading.onActivation == true)
                RFFade.FadeShard (rigidRoot, shard);

            // Parent
            if (RayfireMan.inst.parent != null)
                shard.tm.parent = RayfireMan.inst.parent.transform;

            // Connectivity check if shards was activated: TODO check only neibs of activated?
            if (rigidRoot.activation.byConnectivity == true && rigidRoot.activation.connect != null)
                rigidRoot.activation.connect.connectivityCheckNeed = true;

            // Init particles on activation
            RFParticles.InitActivationParticlesShard(rigidRoot, shard);
            
            // Activation sound
            RFSound.ActivationSound (rigidRoot.sound, rigidRoot.cluster.bound.size.magnitude);
            
            // Events
            rigidRoot.activationEvent.InvokeLocalEventRoot (shard, rigidRoot);
            RFActivationEvent.InvokeGlobalEventRoot (shard, rigidRoot);
            
            return true;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Rigid Activation Layer
        /// /////////////////////////////////////////////////////////
        
        // Set activation layer
        static void SetActivationLayer (RayfireRigid scr)
        {
            if (scr.activation.l == true)
                scr.gameObject.layer = scr.activation.layer;
        }
        
        // ReSet activation layer
        public static void RestoreActivationLayer (RayfireRigid scr)
        {
            if (scr.activation.l == true)
                scr.gameObject.layer = scr.activation.lb;
        }
        
        // Backup original layer in case rigid will change layer after activation
        public static void BackupActivationLayer (RayfireRigid scr)
        {
            if (scr.activation.l == true)
                scr.activation.lb = scr.gameObject.layer;
        }
        
        /// /////////////////////////////////////////////////////////
        /// RigidRoot Activation Layer
        /// /////////////////////////////////////////////////////////
                
        // Set activation layer
        static void SetActivationLayer (RFShard shard, RFActivation activation)
        {
            if (activation.l == true)
                shard.tm.gameObject.layer = activation.layer;
        }
        
        // Set activation layer
        public static void SetActivationLayer (List<RFShard> shards, RFActivation activation)
        {
            if (activation.l == true)
                for (int s = 0; s < shards.Count; s++)
                    shards[s].tm.gameObject.layer = activation.layer;
        }

        // ReSet layer for activated shards
        public static void RestoreActivationLayer (RayfireRigidRoot root)
        {
            if (root.activation.l == true)
                for (int i = 0; i < root.cluster.shards.Count; i++) 
                    root.cluster.shards[i].tm.gameObject.layer = root.cluster.shards[i].lb;
        }
        
        // Backup original layer in case shard will change layer after activation
        public static void BackupActivationLayer (RayfireRigidRoot root)
        {
            if (root.activation.l == true)
                for (int i = 0; i < root.cluster.shards.Count; i++)
                    root.cluster.shards[i].lb = root.cluster.shards[i].tm.gameObject.layer;
        }
    }
}