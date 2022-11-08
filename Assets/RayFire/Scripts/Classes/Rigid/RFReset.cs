using System;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RayFire
{
    [Serializable]
    public class RFReset
    {
        public enum PostDemolitionType
        {
            DestroyWithDelay  = 0,
            DeactivateToReset = 1
        }
        
        public enum MeshResetType
        {
            Destroy              = 0,
            ReuseInputMesh       = 2,
            ReuseFragmentMeshes  = 4
        }
        
        public enum FragmentsResetType
        {
            Destroy     = 0,
            Reuse       = 2,
            Preserve    = 4
        }
        
        public bool               transform;
        public bool               damage;
        public bool               connectivity;
        public PostDemolitionType action;
        public float              destroyDelay;
        public MeshResetType      mesh;
        public FragmentsResetType fragments;

        [NonSerialized] public bool toBeDestroyed;

        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////

        // Constructor
        public RFReset()
        {
            action        = PostDemolitionType.DestroyWithDelay;
            destroyDelay  = 1;
            transform     = true;
            damage        = true;
            mesh          = MeshResetType.ReuseFragmentMeshes;
            fragments     = FragmentsResetType.Destroy;
            toBeDestroyed = false;
        }

        // Copy from
        public void CopyFrom (RFReset reset, ObjectType objectType)
        {
            transform    = reset.transform;
            damage       = reset.damage;
            action       = reset.action;
            destroyDelay = reset.destroyDelay;
            
            // Copy to initial object: mesh root copy
            if (objectType == ObjectType.MeshRoot)
            {
                mesh      = reset.mesh;
                fragments = reset.fragments;
            }

            // Copy to cluster shards
            else if (objectType == ObjectType.ConnectedCluster)
            {
                mesh      = reset.mesh;
                fragments = reset.fragments;
            }
            
            // Copy to demolished mesh fragments
            else if (objectType == ObjectType.Mesh)
            {
                mesh      = MeshResetType.Destroy;
                fragments = FragmentsResetType.Destroy;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Rigid Mesh
        /// /////////////////////////////////////////////////////////
        
        // Rigid 
        public static void ResetRigid (RayfireRigid scr)
        {
            // Object can't be reused
            if (ObjectReuseState (scr) == false)
                return;
            
            // Mesh Root reset
            if (MeshRootReset (scr) == true)
                return;
            
            // Save faded/demolished state before reset
            int faded = scr.fading.state;
            bool demolished = scr.limitations.demolished;

            // Reset tm
            if (scr.reset.transform == true)
                RestoreTransform(scr);
            
            // Reset activation TODO check if it was Kinematic
            if (scr.activation.activated == true)
                scr.simulationType = SimType.Inactive;
            
            // ReSet activation layer. IMPORTANT before Reset()
            RFActivation.RestoreActivationLayer (scr);
            
            // Reset rigid props
            Reset (scr);
            
            // Stop all cors in case object restarted
            scr.StopAllCoroutines();
            
            // Reset if object fading/faded
            if (faded >= 1)
                ResetFade(scr);
            
            // Demolished. Restore
            if (demolished == true)
                ResetMeshDemolition (scr);
            
            // Restore cluster even if it was not demolished
            ResetClusterDemolition (scr);
            
            // Reset sound
            ResetSound(scr.sound);
            
            // Remove particles
            DestroyRigidParticles (scr);
            
            // Enable Rigid because of cluster fade and reset
            if (scr.enabled == false)
                scr.enabled = true;
            
            // Activate if deactivated
            if (scr.gameObject.activeSelf == false)
                scr.gameObject.SetActive (true);

            // Start all coroutines
            scr.StartAllCoroutines();
        }

        // Reset if object fading/faded
        public static void ResetFade (RayfireRigid scr)
        {
            // Was excluded
            if (scr.fading.fadeType == FadeType.SimExclude)
            {
                // Null check because of Planar check fragments without collider
                if (scr.physics.meshCollider != null)
                    scr.physics.meshCollider.enabled = true;// TODO CHECK CLUSTER COLLIDERS
            }   
               
            // Was fall down
            else if (scr.fading.fadeType == FadeType.FallDown)
            {
                // Null check because of Planar check fragments without collider
                if (scr.physics.meshCollider != null)
                    scr.physics.meshCollider.enabled = true;// TODO CHECK CLUSTER COLLIDERS
                
                scr.gameObject.SetActive (true);
            } 
            
            // Was scaled down
            else if (scr.fading.fadeType == FadeType.ScaleDown)
            {
                scr.transForm.localScale = scr.physics.initScale;
                scr.gameObject.SetActive (true);
            }
            
            // Was moved down
            if (scr.fading.fadeType == FadeType.MoveDown)
            {
                // Null check because of Planar check fragments without collider
                if (scr.physics.meshCollider != null)
                    scr.physics.meshCollider.enabled = true; // TODO CHECK CLUSTER COLLIDERS

                // Reset gravity
                if (scr.simulationType != SimType.Inactive)
                    scr.physics.rigidBody.useGravity = scr.physics.useGravity;
                
                scr.gameObject.SetActive (true);
            }

            // Was destroyed
            else if (scr.fading.fadeType == FadeType.Destroy)
                scr.gameObject.SetActive (true);
            
            // Was set static
            if (scr.fading.fadeType == FadeType.SetStatic)
                scr.gameObject.SetActive (true);
            
            // Was set static
            if (scr.fading.fadeType == FadeType.SetKinematic)
                scr.gameObject.SetActive (true);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Rigid Mesh Root
        /// /////////////////////////////////////////////////////////

        // Mesh Root 
        static bool MeshRootReset (RayfireRigid scr)
        {
            // Not mesh root
            if (scr.objectType != ObjectType.MeshRoot)
                return false;

            // Cleanup destroyed/faded fragments
            if (MeshRootCleanup (scr) == false)
                return true;

            DestroyMeshRootParticles (scr);
            
            // Reset tm
            scr.physics.LoadInitTransform (scr.transform);

            // Reset fragments first
            foreach (var fragment in scr.fragments)
            {
                // Add rigid body to Rigid if it was deleted because of clustering
                if (fragment.physics.rigidBody == null)
                {
                 
                    Rigidbody r = fragment.gameObject.GetComponent<Rigidbody>();
                    if (r != null)
                        Debug.Log (r.name);
                    
                    fragment.physics.rigidBody = fragment.gameObject.AddComponent<Rigidbody>();
                }

                // Set object type back in case of clustering->demolition
                fragment.simulationType = scr.simulationType;

                // Set parent in case of clustering->demolition
                fragment.transForm.parent = scr.transForm;
                
                // Reset rigid
                ResetRigid (fragment);
                
                // Set density. After collider defined TODO save mass at first apply, reuse now
                RFPhysic.SetDensity (fragment);

                // Set drag properties
                RFPhysic.SetDrag (fragment);
                
                // Destroy parent connected cluster if rigid was clustered
                if (fragment.rootParent != null)
                    Object.Destroy (fragment.rootParent.gameObject);
                
                // TODO Test fragments reuse with transform state copied to fragments
            }
            
            // Reset uny data
            RayfireUnyielding.SetMeshRootUny (scr.transform, null);
            
            // Restore connectivity cluster
            RFBackupCluster.RestoreConnectivity (scr.activation.connect);
            
            return true;
        }

        // Cleanup and check for mesh root fragments
        static bool MeshRootCleanup (RayfireRigid scr)
        {
            // Cleanup destroyed/faded fragments
            for (int i = scr.fragments.Count - 1; i >= 0; i--)
                if (scr.fragments[i] == null)
                {
                    Debug.Log (scr.name + ": Mesh Root Fragment destroyed", scr.gameObject);
                    scr.fragments.RemoveAt (i);
                }

            // Check after cleanup
            if (scr.HasFragments == false)
                return false;

            return true;
        }

        // Destroy particles
        public static void DestroyMeshRootParticles (RayfireRigid scr)
        {
            if (scr.particleList.Count > 0)
            {
                for (int i = scr.particleList.Count - 1; i >= 0; i--)
                    if (scr.particleList[i] != null)
                        RayfireMan.DestroyGo (scr.particleList[i].gameObject);
                scr.particleList.Clear();
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Reset Rigid Root
        /// /////////////////////////////////////////////////////////

        // Reinit demolished mesh object
        public static void RigidRootReset (RayfireRigidRoot scr)
        {
            // Stop all cors in case object restarted
            scr.StopAllCoroutines();
            scr.corState                    = false;
            scr.activation.inactiveCorState = false;
            scr.fading.offsetCorState       = false;
            
            // Reset activation
            scr.activation.Reset();
            
            // TODO CHECK FOR RESET STATES
            // TODO CLEANUP
            
            // Destroy particle roots
            DestroyRigidRootParticles (scr);
            
            // Reset tm
            scr.transform.position   = scr.cluster.pos;
            scr.transform.rotation   = scr.cluster.rot;
            scr.transform.localScale = scr.cluster.scl;
           
            // Set object type back in case of clustering->demolition
            ResetSimType (scr);

            // ReSet parents for all shards
            ResetParentAndTm (scr);
            
            // Reset shards
            for (int i = 0; i < scr.cluster.shards.Count; i++)
            {
                // Shard faded
                if (scr.cluster.shards[i].fade != 0)
                {
                    // Enable collider
                    if (scr.cluster.shards[i].col.enabled == false)
                        scr.cluster.shards[i].col.enabled = true;
                    
                    // Reset fading
                    scr.cluster.shards[i].fade = 0;
                }
                
                // TODO Destroy parent connected cluster if rigid was clustered
                
                // Activate
                if (scr.cluster.shards[i].tm.gameObject.activeSelf == false)
                    scr.cluster.shards[i].tm.gameObject.SetActive (true);
            }

            // ReSet layer for activated shards
            RFActivation.RestoreActivationLayer (scr);
            
            // Set physics properties for shards
            RFPhysic.SetPhysics(scr.cluster.shards, scr.physics);
            
            /* TODO check if should be here
            // Reset shards with Rigid
            for (int i = 0; i < scr.cluster.shards.Count; i++)
                if (scr.cluster.shards[i].rigid != null)
                    scr.cluster.shards[i].rigid.ResetRigid();
                    */
            
            // Setup list for activation shards
            scr.SetInactiveList ();

            // Setup list with fade by offset shards
            RFFade.SetOffsetFadeList (scr);

            // Destroy child clusters if they were created
            DestroyClusters (scr);

            // Restore connectivity cluster
            RFBackupCluster.RestoreConnectivity (scr.activation.connect);
            
            // Reset sound
            ResetSound(scr.sound);
            
            // Start coroutines
            scr.StartAllCoroutines();
        }
        
        // ReSet parents and transform for all shards
        static void ResetParentAndTm(RayfireRigidRoot scr)
        {
            // TODO null checks
            for (int i = 0; i < scr.cluster.shards.Count; i++)
            {
                scr.cluster.shards[i].tm.SetParent (null);
                scr.cluster.shards[i].tm.SetPositionAndRotation (scr.cluster.shards[i].pos, scr.cluster.shards[i].rot);
                scr.cluster.shards[i].tm.SetParent (scr.parentList[i], true);
                scr.cluster.shards[i].tm.localScale = scr.cluster.shards[i].scl;
            }
        }
        
        // Set object type back in case of clustering->demolition
        static void ResetSimType(RayfireRigidRoot scr)
        {
            // Reset by RigidRoot and Rigid components
            for (int i = 0; i < scr.cluster.shards.Count; i++)
            {
                if (scr.cluster.shards[i].rigid == null)
                    scr.cluster.shards[i].sm = scr.simulationType;
                else 
                {
                    if (scr.cluster.shards[i].rigid.objectType == ObjectType.MeshRoot)
                        scr.cluster.shards[i].sm = scr.cluster.shards[i].rigid.simulationType;
                    else if (scr.cluster.shards[i].rigid.objectType == ObjectType.Mesh)
                        scr.cluster.shards[i].rigid.ResetRigid();
                }
            }
            
            // Reset uny states and sim state
            for (int i = 0; i < scr.unyList.Count; i++)
                scr.unyList[i].SetRigidRootUnyShardList();
        }
        
        // Destroy particles
        public static void DestroyRigidRootParticles (RayfireRigidRoot scr)
        {
            if (scr.particleList.Count > 0)
            {
                for (int i = scr.particleList.Count - 1; i >= 0; i--)
                    if (scr.particleList[i] != null)
                        RayfireMan.DestroyGo (scr.particleList[i].gameObject);
                scr.particleList.Clear();
            }
        }
        
        // Destroy clusters
        public static void DestroyClusters (RayfireRigidRoot scr)
        {
            for (int i = 0; i < scr.clusters.Count; i++)
                if (scr.clusters[i].tm != null)
                    RayfireMan.DestroyGo (scr.clusters[i].tm.gameObject);
            
            scr.clusters.Clear();
        }

        /// /////////////////////////////////////////////////////////
        /// Demolition reset
        /// /////////////////////////////////////////////////////////
        
        // Reinit demolished mesh object
        public static void ResetMeshDemolition (RayfireRigid scr)
        {
            // Edit meshes and fragments only if object was demolished
            if (scr.objectType == ObjectType.Mesh)
            {
                // Reset input shatter
                if (scr.reset.mesh != MeshResetType.ReuseInputMesh)
                    scr.meshDemolition.rfShatter = null;
                
                // Reset Meshes
                if (scr.reset.mesh != MeshResetType.ReuseFragmentMeshes)
                    scr.meshes = null;

                // Fragments need to be reused
                if (scr.reset.fragments == FragmentsResetType.Reuse)
                {
                    // Can be reused. Destroyed if can not
                    if (FragmentReuseState (scr) == true)
                        ReuseFragments (scr);
                    else
                        DestroyFragments (scr);
                }
                
                // Destroy fragments
                else if (scr.reset.fragments == FragmentsResetType.Destroy)
                    DestroyFragments (scr);
                
                // Fragments should be kept in scene. Forget about them
                else if (scr.reset.fragments == FragmentsResetType.Preserve)
                    PreserveFragments (scr);
            }
      
            // Activate
            scr.gameObject.SetActive (true);
        }
        
        // Destroy fragments and root
        static void DestroyFragments (RayfireRigid scr)
        {
            // Destroy fragments    
            if (scr.HasFragments == true)
            {
                // Get amount of fragments
                int fragmentNum = scr.fragments.Count (t => t != null);

                // Destroy fragments and root
                for (int i = scr.fragments.Count - 1; i >= 0; i--)
                {
                    if (scr.fragments[i] != null)
                    {
                        // Destroy particles
                        DestroyRigidParticles (scr.fragments[i]);
                        
                        // Destroy fragment
                        scr.fragments[i].gameObject.SetActive (false);
                        RayfireMan.DestroyGo (scr.fragments[i].gameObject);

                        // Destroy root
                        if (scr.fragments[i].rootParent != null)
                        {
                            scr.fragments[i].rootParent.gameObject.SetActive (false);
                            RayfireMan.DestroyGo (scr.fragments[i].rootParent.gameObject);
                        }
                    }
                }
                
                // Nullify
                scr.fragments = null;

                // Subtract amount of deleted fragments
                RayfireMan.inst.advancedDemolitionProperties.ChangeCurrentAmount (-fragmentNum);

                // Destroy descendants
                if (scr.limitations.descendants != null && scr.limitations.descendants.Count > 0)
                {
                    // Get amount of descendants
                    int descendantNum = scr.limitations.descendants.Count (t => t != null);
                    
                    // Destroy fragments and root
                    for (int i = 0; i < scr.limitations.descendants.Count; i++)
                    {
                        if (scr.limitations.descendants[i] != null)
                        {
                            // Destroy fragment
                            scr.limitations.descendants[i].gameObject.SetActive (false);
                            RayfireMan.DestroyGo (scr.limitations.descendants[i].gameObject);

                            // Destroy root
                            if (scr.limitations.descendants[i].rootParent != null)
                            {
                                scr.limitations.descendants[i].rootParent.gameObject.SetActive (false);
                                RayfireMan.DestroyGo (scr.limitations.descendants[i].rootParent.gameObject);
                            }
                        }
                    }
                    
                    // Clear
                    scr.limitations.descendants.Clear();
                    
                    // Subtract amount of deleted fragments
                    RayfireMan.inst.advancedDemolitionProperties.ChangeCurrentAmount (-descendantNum);
                }
            }
        }

        // Destroy particles
        public static void DestroyRigidParticles (RayfireRigid scr)
        {
            // Destroy debris
            if (scr.HasDebris == true)
                for (int d = 0; d < scr.debrisList.Count; d++)
                    if (scr.debrisList[d].hostTm != null)
                    {
                        scr.debrisList[d].hostTm.gameObject.SetActive (false);
                        RayfireMan.DestroyGo (scr.debrisList[d].hostTm.gameObject);
                    }

            // Destroy debris
            if (scr.HasDust == true)
                for (int d = 0; d < scr.dustList.Count; d++)
                    if (scr.dustList[d].hostTm != null)
                    {
                        scr.dustList[d].hostTm.gameObject.SetActive (false);
                        RayfireMan.DestroyGo (scr.dustList[d].hostTm.gameObject);
                    }
        }
        
        // Fragments need and can be reused
        static void ReuseFragments (RayfireRigid scr)
        {
            // Sub amount
            RayfireMan.inst.advancedDemolitionProperties.ChangeCurrentAmount (-scr.fragments.Count);
            
            // Activate root
            if (scr.rootChild != null)
            {
                scr.rootChild.gameObject.SetActive (false);
                scr.rootChild.position = scr.transForm.position;
                scr.rootChild.rotation = scr.transForm.rotation;
            }

            // Reset fragments tm
            for (int i = scr.fragments.Count - 1; i >= 0; i--)
            {
                // Destroy particles
                DestroyRigidParticles (scr.fragments[i]);
                
                scr.fragments[i].transForm.localScale = scr.fragments[i].physics.initScale;
                scr.fragments[i].transForm.position = scr.transForm.position + scr.pivots[i];
                scr.fragments[i].transForm.rotation = Quaternion.identity;

                // Reset activation TODO check if it was Kinematic
                if (scr.fragments[i].activation.activated == true)
                    scr.fragments[i].simulationType = SimType.Inactive;
                
                // Reset fading
                if (scr.fragments[i].fading.state >= 1)
                    ResetFade(scr.fragments[i]);
                
                // Reset rigid props
                Reset (scr.fragments[i]);
            }

            // Clear descendants
            scr.limitations.descendants.Clear();
        }
        
        // Preserve Fragments
        static void PreserveFragments (RayfireRigid scr)
        {
            scr.fragments = null;
            scr.rootChild = null;
            scr.limitations.descendants.Clear();
        }
          
        // Reinit demolished mesh object
        static void ResetClusterDemolition (RayfireRigid scr)
        {
            if (scr.objectType == ObjectType.ConnectedCluster || scr.objectType == ObjectType.NestedCluster)
            {
                RFBackupCluster.ResetRigidCluster (scr);
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Reuse state
        /// /////////////////////////////////////////////////////////          
        
        // Check fragments reuse state
        static bool ObjectReuseState (RayfireRigid scr)
        {
            // Mesh Root reset
            if (scr.objectType == ObjectType.MeshRoot)
                return true;
            
            // Excluded from sim
            if (scr.physics.exclude == true)
            {
                Debug.Log ("Demolished " + scr.objectType.ToString() + " reset not supported yet.");
                return false;
            }
            
            // Not mesh object type
            if (scr.objectType == ObjectType.Mesh 
                || scr.objectType == ObjectType.ConnectedCluster
                || scr.objectType == ObjectType.NestedCluster)
                return true;
            
            // Object can be reused
            return false;
        }
                
        // Check fragments reuse state
        static bool FragmentReuseState (RayfireRigid scr)
        {
            // Do not reuse reference demolition
            if (scr.demolitionType == DemolitionType.ReferenceDemolition)
                return false;
            
            // Fragments list null or empty
            if (scr.HasFragments == false)
                return false;

            // One of the fragment null
            if (scr.fragments.Any (t => t == null))
                return false;
            
            // One of the fragment going to be destroyed TODO make reusable
            if (scr.fragments.Any (t => t.reset.toBeDestroyed == true))
                return false;
            
            // One of the fragment demolished TODO make reusable
            if (scr.fragments.Any (t => t.limitations.demolished == true))
                return false;
  
            // Fragments can be reused
            return true;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Other
        /// /////////////////////////////////////////////////////////      
        
        // Restore transform or initial
        static void RestoreTransform (RayfireRigid scr)
        {
            // Restore tm
            scr.physics.LoadInitTransform (scr.transForm);
            scr.physics.velocity     = Vector3.zero;
            
            // Restore rigidbody TODO save initial velocity into vars and reset to them
            if (scr.physics.rigidBody != null)
            {
                scr.physics.rigidBody.velocity        = Vector3.zero;
                scr.physics.rigidBody.angularVelocity = Vector3.zero;
            }
        }
        
        // Restore rigid properties
        public static void Reset (RayfireRigid scr)
        {
            // Reset caching if it is on
            scr.meshDemolition.StopRuntimeCaching();
            
            scr.physics.Reset();
            scr.activation.Reset();
            if (scr.restriction != null)
                scr.restriction.Reset();
            scr.limitations.Reset();
            scr.meshDemolition.Reset();
            scr.clusterDemolition.Reset();
            scr.fading.Reset();
            if (scr.reset.damage == true)
                scr.damage.Reset();
            
            // Set physical simulation type. Important. Should after collider material define
            RFPhysic.SetSimulationType (scr.physics.rigidBody, scr.simulationType, scr.objectType, scr.physics.useGravity, scr.physics.solverIterations);
            
            // Set sleeping state TODO
            if (scr.simulationType == SimType.Sleeping)
            {
                scr.physics.velocity                  = Vector3.zero;
                scr.physics.rigidBody.velocity        = Vector3.zero;
                scr.physics.rigidBody.angularVelocity = Vector3.zero;
                scr.physics.rigidBody.Sleep();
            }
        }

        // Reset sound
        public static void ResetSound (RayfireSound scr)
        {
            if (scr != null)
            {
                scr.initialization.played = false;
                scr.activation.played = false;
                scr.demolition.played = false;
            }
        }
    }
}