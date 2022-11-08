using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RayFire
{
    [Serializable]
    public class RFLimitations
    {
        public bool   byCollision;
        public float  solidity;
        public string tag;
        public int    depth;
        public float  time;
        public float  size;
        public bool   visible;
        public bool   sliceByBlade;

        // Non serialized
        [NonSerialized] public List<Vector3> slicePlanes;
        [NonSerialized] public ContactPoint  contactPoint;
        [NonSerialized] public Vector3       contactVector3;
        [NonSerialized] public Vector3       contactNormal;
        [NonSerialized] public bool          demolitionShould;
        [NonSerialized] public bool          demolished;
        [NonSerialized] public float         birthTime;
        [NonSerialized] public float         bboxSize;
        [NonSerialized] public int           currentDepth;
        [NonSerialized] public bool          demolishableCorState;
        
        // Blade props
        [NonSerialized] public float         sliceForce;
        [NonSerialized] public bool          affectInactive;
        
        // Family data. Do not nullify in Reset
        [NonSerialized] public RayfireRigid       ancestor;
        [NonSerialized] public List<RayfireRigid> descendants;
        
        // Hidden
        [HideInInspector] public Bounds bound;

        static string rootStr = "_root";
        public static string rigidStr = "RayFire Rigid: ";
        
        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////
        
        // Constructor
        public RFLimitations()
        {
            byCollision = true;
            solidity  = 0.1f;
            depth     = 1;
            time      = 0.2f;
            size      = 0.1f;
            tag       = "Untagged";
            visible   = false;
            
            sliceByBlade = false;
            sliceForce   = 0;
            
            currentDepth     = 0;
            birthTime        = 0f;
            bboxSize         = 0f;

            ancestor = null;
            descendants = null;
            
            Reset();
        }

        // Copy from
        public void CopyFrom (RFLimitations limitations)
        {
            byCollision = limitations.byCollision;
            solidity  = limitations.solidity;
            depth     = limitations.depth;
            time      = limitations.time;
            size      = limitations.size;
            tag       = limitations.tag;
            visible   = limitations.visible;
            
            sliceByBlade     = limitations.sliceByBlade;
            
            // Do not copy currentDepth. Set in other place
            
            Reset();
        }
        
        // Reset
        public void Reset()
        {
            slicePlanes          = new List<Vector3>();
            contactVector3       = Vector3.zero;
            contactNormal        = Vector3.down;
            demolitionShould     = false;
            demolished           = false;
            demolishableCorState = false;
            
            currentDepth = 0;
            birthTime    = 0f;
            sliceForce   = 0;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////
         
        // Cache velocity for fragments 
        public IEnumerator DemolishableCor(RayfireRigid scr)
        {
            // Stop if running 
            if (demolishableCorState == true)
                yield break;
            
            // Set running state
            demolishableCorState = true;
            
            while (scr.demolitionType != DemolitionType.None)
            {
                // Max depth reached
                if (scr.limitations.depth > 0 && scr.limitations.currentDepth >= scr.limitations.depth)
                    scr.demolitionType = DemolitionType.None;

                // Init demolition
                if (scr.limitations.demolitionShould == true)
                {
                    scr.Demolish();
                }

                // Check for slicing planes and init slicing
                else if (scr.limitations.sliceByBlade == true && scr.limitations.slicePlanes.Count > 1)
                    scr.Slice();
                
                yield return null;
            }
            
            // Set state
            demolishableCorState = false;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////
        
        // Check for user mistakes
        public static void Checks (RayfireRigid scr)
        {
   
            // TODO planar mesh -> set none collider
            
            // TODO static and cluster
            if (scr.objectType == ObjectType.Mesh)
            {
                                 
                
            }
            
            // ////////////////
            // Sim Type
            // ////////////////
            
            // Static and demolishable
            if (scr.simulationType == SimType.Static)
            {
                if (scr.demolitionType != DemolitionType.None)
                {
                    Debug.Log (rigidStr + scr.name + " Simulation Type set to " + scr.simulationType.ToString() + " but Demolition Type is not None. Static object can not be demolished. Demolition Type set to None.", scr.gameObject);
                    scr.demolitionType = DemolitionType.None;
                }
            }
            
            // Non static simulation but static property
            if (scr.simulationType != SimType.Static)
            {
                if (scr.gameObject.isStatic == true)
                {
                    Debug.Log (rigidStr + scr.name + " Simulation Type set to " + scr.simulationType.ToString() + " but object is Static. Turn off Static checkbox in Inspector.", scr.gameObject);
                }
            }
           
            // ////////////////
            // Object Type
            // ////////////////
            
            // Object can not be simulated as mesh
            if (scr.objectType == ObjectType.Mesh)
            {
                // Has no mesh
                if (scr.meshFilter == null || scr.meshFilter.sharedMesh == null)
                {
                    Debug.Log (rigidStr + scr.name + " Object Type set to " + scr.objectType.ToString() + " but object has no mesh. Object Excluded from simulation.", scr.gameObject);
                    scr.physics.exclude = true;
                }
                
                // Not readable mesh 
                if (scr.demolitionType != DemolitionType.None && scr.demolitionType != DemolitionType.ReferenceDemolition)
                {
                    if (scr.meshFilter != null && scr.meshFilter.sharedMesh != null && scr.meshFilter.sharedMesh.isReadable == false)
                    {
                        Debug.Log (rigidStr + scr.name + " Mesh is not readable. Demolition type set to None. Open Import Settings and turn On Read/Write Enabled property", scr.meshFilter.gameObject);
                        scr.demolitionType         = DemolitionType.None;
                        scr.meshDemolition.badMesh = 10;
                    }
                }
            }
            
            // Object can not be simulated as cluster
            else if (scr.objectType == ObjectType.NestedCluster || scr.objectType == ObjectType.ConnectedCluster)
            {
                if (scr.transForm.childCount == 0)
                {
                    Debug.Log (rigidStr + scr.name + " Object Type set to " + scr.objectType.ToString() + " but object has no children. Object Excluded from simulation.", scr.gameObject);
                    scr.physics.exclude = true;
                }
            }
            
            // Object can not be simulated as mesh
            else if (scr.objectType == ObjectType.SkinnedMesh)
            {
                if (scr.skinnedMeshRend == null)
                    Debug.Log (rigidStr + scr.name + " Object Type set to " + scr.objectType.ToString() + " but object has no SkinnedMeshRenderer. Object Excluded from simulation.", scr.gameObject);
                
                // Excluded from sim by default
                scr.physics.exclude = true;
            }
            
            // ////////////////
            // Demolition Type
            // ////////////////
            
            // Demolition checks
            if (scr.demolitionType != DemolitionType.None)
            {
                // // Static
                // if (scr.simulationType == SimType.Static)
                // {
                //     Debug.Log (rigidStr + scr.name + " Simulation Type set to " + scr.simulationType.ToString() + " but Demolition Type is " + scr.demolitionType.ToString() + ". Demolition Type set to None.", scr.gameObject);
                //     scr.demolitionType = DemolitionType.None;
                // }
                
                // Set runtime demolition for clusters and skinned mesh
                if (scr.objectType == ObjectType.SkinnedMesh ||
                    scr.objectType == ObjectType.NestedCluster ||
                    scr.objectType == ObjectType.ConnectedCluster)
                {
                    if (scr.demolitionType != DemolitionType.Runtime && scr.demolitionType != DemolitionType.ReferenceDemolition)
                    {
                        Debug.Log (rigidStr + scr.name + " Object Type set to " + scr.objectType.ToString() + " but Demolition Type is " + scr.demolitionType.ToString() + ". Demolition Type set to Runtime.", scr.gameObject);
                        scr.demolitionType = DemolitionType.Runtime;
                    }
                }
                
                // No Shatter component for runtime demolition with Use Shatter on
                if (scr.meshDemolition.scrShatter == null && scr.meshDemolition.useShatter == true)
                {
                    if (scr.demolitionType == DemolitionType.Runtime ||
                        scr.demolitionType == DemolitionType.AwakePrecache ||
                        scr.demolitionType == DemolitionType.AwakePrefragment)
                    {
                        Debug.Log (rigidStr + scr.name + "Demolition Type is " + scr.demolitionType.ToString() + ". Has no Shatter component, but Use Shatter property is On. Use Shatter property was turned Off.", scr.gameObject);
                        scr.meshDemolition.useShatter = false;
                    }
                }
            }
            
            // None check
            if (scr.demolitionType == DemolitionType.None)
            {
                if (scr.HasMeshes == true)
                {
                    Debug.Log (rigidStr + scr.name + " Demolition Type set to None. Had precached meshes which were destroyed.", scr.gameObject);
                    scr.DeleteCache();
                }

                if (scr.objectType == ObjectType.Mesh && scr.HasFragments == true)
                {
                    Debug.Log (rigidStr + scr.name + " Demolition Type set to None. Had prefragmented objects which were destroyed.", scr.gameObject);
                    scr.DeleteFragments();
                }

                if (scr.HasRfMeshes == true)
                {
                    Debug.Log (rigidStr + scr.name + " Demolition Type set to None. Had precached serialized meshes which were destroyed.", scr.gameObject);
                    scr.DeleteCache();
                }
            }

            // Runtime check
            else if (scr.demolitionType == DemolitionType.Runtime)
            {
                if (scr.HasMeshes == true)
                {
                    Debug.Log (rigidStr + scr.name + " Demolition Type set to Runtime. Had precached meshes which were destroyed.", scr.gameObject);
                    scr.DeleteCache();
                }

                if (scr.objectType == ObjectType.Mesh && scr.HasFragments == true)
                {
                    Debug.Log (rigidStr + scr.name + " Demolition Type set to Runtime. Had prefragmented objects which were destroyed.", scr.gameObject);
                    scr.DeleteFragments();
                }

                if (scr.HasRfMeshes == true)
                {
                    Debug.Log ("RayFire Rigid:" + scr.name + " Demolition Type set to Runtime. Had precached serialized meshes which were destroyed.", scr.gameObject);
                    scr.DeleteCache();
                }
                
                // No runtime caching for rigid with shatter with tets/slices/glue
                if (scr.meshDemolition.useShatter == true && scr.meshDemolition.runtimeCaching.type != CachingType.Disable)
                {
                    if (scr.meshDemolition.scrShatter.type == FragType.Decompose ||
                        scr.meshDemolition.scrShatter.type == FragType.Tets ||
                        scr.meshDemolition.scrShatter.type == FragType.Slices || 
                        scr.meshDemolition.scrShatter.clusters.enable == true)
                    {
                        Debug.Log (rigidStr + scr.name + " Demolition Type is Runtime, Use Shatter is On. Unsupported fragments type. Runtime Caching supports only Voronoi, Splinters, Slabs and Radial fragmentation types. Runtime Caching was Disabled.", scr.gameObject);
                        scr.meshDemolition.runtimeCaching.type = CachingType.Disable;
                    }
                }
            }

            // Awake precache check
            else if (scr.demolitionType == DemolitionType.AwakePrecache)
            {
                if (scr.HasMeshes == true)
                    Debug.Log (rigidStr + scr.name + " Demolition Type set to Awake Precache. Had manually precached Unity meshes which were overwritten.", scr.gameObject);
                
                if (scr.HasFragments == true)
                {
                    Debug.Log (rigidStr + scr.name + " Demolition Type set to Awake Precache. Had manually prefragmented objects which were destroyed.", scr.gameObject);
                    scr.DeleteFragments();
                }

                if (scr.HasRfMeshes == true)
                {
                    Debug.Log ("RayFire Rigid:" + scr.name + " Demolition Type set to Awake Precache. Has manually precached serialized meshes.", scr.gameObject);
                }
            }

            // Awake prefragmented check
            else if (scr.demolitionType == DemolitionType.AwakePrefragment)
            {
                if (scr.HasFragments == true)
                    Debug.Log (rigidStr + scr.name + " Demolition Type set to Awake Prefragment. Has manually prefragmented objects", scr.gameObject);
                if (scr.HasMeshes == true)
                    Debug.Log (rigidStr + scr.name + " Demolition Type set to Awake Prefragment. Has manually precached Unity meshes.", scr.gameObject);
                if (scr.HasRfMeshes == true)
                    Debug.Log (rigidStr + scr.name + " Demolition Type set to Awake Prefragment. Has manually precached serialized meshes.", scr.gameObject);
            }
            
            // Reference demolition
            else if (scr.demolitionType == DemolitionType.ReferenceDemolition) {}

            // TODO Tag and Layer check
        }
        
        // Set bound and size
        public static void SetBound (RayfireRigid scr)
        {
            if (scr.objectType == ObjectType.Mesh)
                scr.limitations.bound = scr.meshRenderer.bounds;
            else if (scr.objectType == ObjectType.SkinnedMesh)
                scr.limitations.bound = scr.skinnedMeshRend.bounds;
            else if (scr.objectType == ObjectType.NestedCluster || scr.objectType == ObjectType.ConnectedCluster)
                scr.limitations.bound = RFCluster.GetChildrenBound (scr.transForm);
            scr.limitations.bboxSize = scr.limitations.bound.size.magnitude;
        }
        
        // Set ancestor
        public static void SetAncestor (RayfireRigid scr)
        {
            // Set ancestor to this if it is ancestor
            if (scr.limitations.ancestor == null)
                for (int i = 0; i < scr.fragments.Count; i++)
                    scr.fragments[i].limitations.ancestor = scr;
            else
                for (int i = 0; i < scr.fragments.Count; i++) 
                    scr.fragments[i].limitations.ancestor = scr.limitations.ancestor;
        }
        
        // Set descendants 
        public static void SetDescendants (RayfireRigid scr)
        {
            if (scr.reset.action == RFReset.PostDemolitionType.DestroyWithDelay)
                return;
                
            if (scr.limitations.ancestor == null)
                scr.limitations.descendants.AddRange (scr.fragments);
            else
                scr.limitations.ancestor.limitations.descendants.AddRange (scr.fragments);
        }
        
        // Create root
        public static void CreateRoot (RayfireRigid rfScr)
        {
           GameObject root = new GameObject(rfScr.gameObject.name + rootStr);
           rfScr.rootChild          = root.transform;
           rfScr.rootChild.position = rfScr.transForm.position;
           rfScr.rootChild.rotation = rfScr.transForm.rotation;
           
           rfScr.rootChild.SetParent (rfScr.gameObject.transform.parent);
        }

    }
}