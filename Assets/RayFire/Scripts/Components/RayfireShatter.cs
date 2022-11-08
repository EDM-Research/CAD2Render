using System.Linq;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

// Shatter fragment's output size filter wit low value to delete very small pieces

namespace RayFire
{
    [AddComponentMenu("RayFire/Rayfire Shatter")]
    [HelpURL("https://rayfirestudios.com/unity-online-help/components/unity-shatter-component/")]
    public class RayfireShatter : MonoBehaviour
    {
        public enum FragLastMode
        {
            New    = 0,
            ToLast = 1
        }
        
        public FragType    type      = FragType.Voronoi;
        public RFVoronoi   voronoi   = new RFVoronoi();
        public RFSplinters splinters = new RFSplinters();
        public RFSplinters slabs     = new RFSplinters();
        public RFRadial    radial    = new RFRadial();
        public RFCustom    custom    = new RFCustom();
        public RFMirrored  mirrored  = new RFMirrored();
        public RFSlice     slice     = new RFSlice();
        public RFBricks    bricks    = new RFBricks();
        public RFVoxels    voxels    = new RFVoxels();
        public RFTets      tets      = new RFTets();
        
        public FragmentMode     mode     = FragmentMode.Editor;
        public RFSurface        material = new RFSurface();
        public RFShatterCluster clusters = new RFShatterCluster();
        
        
        public RFShatterAdvanced advanced = new RFShatterAdvanced();

        // Export
        public RFMeshExport export = new RFMeshExport();

        // Center
        public bool       showCenter;
        public Vector3    centerPosition;
        public Quaternion centerDirection;

        // Components
        public Transform           transForm;
        public MeshFilter          meshFilter;
        public MeshRenderer        meshRenderer;
        public SkinnedMeshRenderer skinnedMeshRend;
        public List<MeshFilter>    meshFilters;

        // Vars
        public Mesh[]             meshes           = null;
        public Vector3[]          pivots           = null;
        public List<Transform>    rootChildList    = new List<Transform>();
        public List<GameObject>   fragmentsAll     = new List<GameObject>();
        public List<GameObject>   fragmentsLast    = new List<GameObject>();
        public List<RFDictionary> origSubMeshIdsRF = new List<RFDictionary>();
        public Material[]         materials        ;
        
        // Hidden
        public int     shatterMode  = 1;
        public bool    colorPreview;
        public bool    scalePreview = true;
        public float   previewScale;
        public float   size;
        public float   rescaleFix   = 1f;
        public Vector3 originalScale;
        [HideInInspector] public Bounds  bound;
        [HideInInspector] public bool    resetState;
        static                   float   minSize    = 0.01f;

        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////

        // Reset
        private void Reset()
        {
            ResetCenter();
        }

        // Set default vars before fragment
        void SetVariables()
        {
            size             = 0f;
            rescaleFix       = 1f;
            originalScale    = transForm.localScale;
            origSubMeshIdsRF = new List<RFDictionary>();
        }

        /// /////////////////////////////////////////////////////////
        /// Checks
        /// /////////////////////////////////////////////////////////
        
        // Basic proceed check
        bool MainCheck()
        {
            // Check if prefab
            if (gameObject.scene.rootCount == 0)
            {
                Debug.Log ("RayFire Shatter: " + name + " Can't fragment prefab because prefab unable to store Unity mesh. Fragment prefab in scene.", gameObject);
                return false;
            }

            // Single mesh mode
            if (advanced.combineChildren == false)
                if (SingleMeshCheck() == false)
                    return false;

            // Multiple mesh mode
            if (advanced.combineChildren == true)
            {
                // Has no children meshes
                if (meshFilters.Count == 1)
                    if (SingleMeshCheck() == false)
                        return false;
                
                // Remove no meshes
                if (meshFilters.Count > 0)
                    for (int i = meshFilters.Count - 1; i >= 0; i--)
                        if (meshFilters[i].sharedMesh == null)
                        {
                            Debug.Log ("RayFire Shatter: " + meshFilters[i].name + " MeshFilter has no Mesh, object excluded.", meshFilters[i].gameObject);
                            meshFilters.RemoveAt (i);
                        }
                
                // Remove no readable meshes
                if (meshFilters.Count > 0)
                    for (int i = meshFilters.Count - 1; i >= 0; i--)
                        if (meshFilters[i].sharedMesh.isReadable == false)
                        {
                            Debug.Log ("RayFire Shatter: " + meshFilters[i].name + " Mesh is not Readable, object excluded.", meshFilters[i].gameObject);
                            meshFilters.RemoveAt (i);
                        }

                // No meshes left
                if (meshFilters.Count == 0)
                    return false;
            }
            
            return true;
        }

        // Single mesh mode checks
        bool SingleMeshCheck()
        {
            // No mesh storage components
            if (meshFilter == null && skinnedMeshRend == null)
            { 
                Debug.Log ("RayFire Shatter: " + name + " Object has no mesh to fragment.", gameObject);
                return false;
            }

            // Has mesh filter
            if (meshFilter != null)
            { 
                // No shared mesh
                if (meshFilter.sharedMesh == null)
                { 
                    Debug.Log ("RayFire Shatter: " + name + " Object has no mesh to fragment.", gameObject);
                    return false;
                } 
                    
                // Not readable mesh
                if (meshFilter.sharedMesh.isReadable == false)
                {
                    Debug.Log ("RayFire Shatter: " + name + "Mesh is not readable. Open Import Settings and turn On Read/Write Enabled", gameObject); 
                    return false;
                }
            }
                
            // Has skinned mesh
            if (skinnedMeshRend != null && skinnedMeshRend.sharedMesh == null)
            {
                Debug.Log ("RayFire Shatter: " + name + " Object has no mesh to fragment.", gameObject);
                return false;
            } 
            
            return true;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////
        
        // Cache variables
        bool DefineComponents()
        {
            // Mesh storage 
            transForm       = GetComponent<Transform>();
            meshFilter      = GetComponent<MeshFilter>();
            meshRenderer    = GetComponent<MeshRenderer>();
            skinnedMeshRend = GetComponent<SkinnedMeshRenderer>();

            // Multymesh fragmentation
            meshFilters = new List<MeshFilter>();
            if (advanced.combineChildren == true)
                meshFilters = GetComponentsInChildren<MeshFilter>().ToList();

            // Basic proceed check
            if (MainCheck() == false)
                return false;
            
            // Mesh renderer
            if (skinnedMeshRend == null)
            {
                if (meshRenderer == null)
                    meshRenderer = gameObject.AddComponent<MeshRenderer>();
                bound = meshRenderer.bounds;
            }
            
            // Skinned mesh
            if (skinnedMeshRend != null)
                bound = skinnedMeshRend.bounds;
            
            return true;
        }

        // Get bounds
        public Bounds GetBound()
        {
            // Mesh renderer
            if (meshRenderer == null)
            {
                meshRenderer = GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                    return meshRenderer.bounds;
            }
            else
                return meshRenderer.bounds;
            
            // Skinned mesh
            if (skinnedMeshRend == null)
            {
                skinnedMeshRend = GetComponent<SkinnedMeshRenderer>();
                if (skinnedMeshRend != null)
                    return skinnedMeshRend.bounds;
            }

            return new Bounds();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////
        
        // Fragment this object by shatter properties  List<GameObject>
        public void Fragment(FragLastMode fragmentMode = FragLastMode.New)
        {
            // Cache variables
            if (DefineComponents() == false)
                return;
            
            // Cache default vars
            SetVariables();
            
            // Check if object is too small
            ScaleCheck();
            
            // Cache
            RFFragment.CacheMeshes(ref meshes, ref pivots, ref origSubMeshIdsRF, this);

            // Stop
            if (meshes == null)
                return;
            
            // Create fragments
            if (fragmentMode == FragLastMode.ToLast)
            {
                if (rootChildList[rootChildList.Count - 1] != null)
                    fragmentsLast = CreateFragments(rootChildList[rootChildList.Count - 1]);
                else
                    fragmentMode = FragLastMode.New;
            }
            
            // Create new fragments
            if (fragmentMode == FragLastMode.New)
                fragmentsLast = CreateFragments();

            // Limitation fragment
            SizeLimitation();
            VertexLimitation();
            TriangleLimitation();
            
            // Collect to all fragments
            fragmentsAll.AddRange(fragmentsLast);
            
            // Reset original object back if it was scaled
            transForm.localScale = originalScale;
        }
        
        // Create fragments by mesh and pivots array
        List<GameObject> CreateFragments(Transform root = null)
        {
            // No mesh were cached
            if (meshes == null)
                return null;

            // Clear array for new fragments
            GameObject[] fragArray = new GameObject[meshes.Length];
            
            // Create root object
            if (root == null)
            {
                GameObject rootGo         = new GameObject (gameObject.name + "_root");
                rootGo.transform.position = transForm.position;
                rootGo.transform.rotation = transForm.rotation;
                rootGo.tag                = gameObject.tag;
                rootGo.layer              = gameObject.layer;
                root                      = rootGo.transform;
                rootChildList.Add (root);
            }
            
            //Debug.Log (root.transform);
            
            // Create instance for fragments
            GameObject fragInstance;
            if (advanced.copyComponents == true)
            {
                fragInstance = Instantiate(gameObject);
                fragInstance.transform.rotation = Quaternion.identity;
                fragInstance.transform.localScale = Vector3.one;

                // Destroy shatter
                DestroyImmediate(fragInstance.GetComponent<RayfireShatter>());
            }
            else
            {
                fragInstance = new GameObject();
                fragInstance.AddComponent<MeshFilter>();
                fragInstance.AddComponent<MeshRenderer>();
            }
            
            // Get original mats. in case of combined meshes it is already defined in CombineShatter()
            if (advanced.combineChildren == false)
                materials = skinnedMeshRend != null
                    ? skinnedMeshRend.sharedMaterials
                    : meshRenderer.sharedMaterials;

            // Vars 
            string baseName = gameObject.name + "_sh_";
            
            // Create fragment objects
            for (int i = 0; i < meshes.Length; ++i)
            {
                // Rescale mesh
                if (rescaleFix != 1f)
                    RFFragment.RescaleMesh (meshes[i], rescaleFix);

                // Instantiate. IMPORTANT do not parent when Instantiate
                GameObject fragGo = Instantiate(fragInstance);
                fragGo.transform.localScale = Vector3.one;
                
                // Set multymaterial
                MeshRenderer targetRend = fragGo.GetComponent<MeshRenderer>();
                RFSurface.SetMaterial(origSubMeshIdsRF, materials, material, targetRend, i, meshes.Length);

                // Set fragment object name and tm
                fragGo.name               = baseName + (i + 1);
                fragGo.transform.position = root.transform.position + (pivots[i] / rescaleFix);
                fragGo.transform.parent   = root.transform;
                fragGo.tag                = gameObject.tag;
                fragGo.layer              = gameObject.layer;
                
                // Set fragment mesh
                MeshFilter mf = fragGo.GetComponent<MeshFilter>();
                mf.sharedMesh = meshes[i];
                mf.sharedMesh.name = fragGo.name;

                // Set mesh collider
                MeshCollider mc = fragGo.GetComponent<MeshCollider>();
                if (mc != null)
                    mc.sharedMesh = meshes[i];

                // Add in array
                fragArray[i] = fragGo;
            }

            // Root back to original parent
            root.transform.parent = transForm.parent;

            // Reset scale for mesh fragments. IMPORTANT: skinned mesh fragments root should not be rescaled 
            if (skinnedMeshRend == null)
                root.transform.localScale = Vector3.one;

            // Destroy instance
            DestroyImmediate(fragInstance);

            // Empty lists
            meshes = null;
            pivots = null;
            origSubMeshIdsRF = new List<RFDictionary>();

            return fragArray.ToList();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Deleting
        /// /////////////////////////////////////////////////////////

        // Delete fragments from last Fragment method
        public void DeleteFragmentsLast(int destroyMode = 0)
        {
            // Destroy last fragments
            if (destroyMode == 1)
                for (int i = fragmentsLast.Count - 1; i >= 0; i--)
                    if (fragmentsLast[i] != null)
                        DestroyImmediate (fragmentsLast[i]);

            // Clean fragments list pre
            fragmentsLast.Clear();
            for (int i = fragmentsAll.Count - 1; i >= 0; i--)
                if (fragmentsAll[i] == null)
                    fragmentsAll.RemoveAt (i);
            
            // Check for all roots
            for (int i = rootChildList.Count - 1; i >= 0; i--)
                if (rootChildList[i] == null)
                    rootChildList.RemoveAt (i);
            
            // No roots
            if (rootChildList.Count == 0)
                return;

            // Destroy with root
            if (destroyMode == 0)
            {
                // Destroy root with fragments
                DestroyImmediate (rootChildList[rootChildList.Count - 1].gameObject);

                // Remove from list
                rootChildList.RemoveAt (rootChildList.Count - 1);
            }

            // Clean all fragments list post
            for (int i = fragmentsAll.Count - 1; i >= 0; i--)
                if (fragmentsAll[i] == null)
                    fragmentsAll.RemoveAt (i);
        }

        // Delete all fragments and roots
        public void DeleteFragmentsAll()
        {
            // Clear lists
            fragmentsLast.Clear();
            fragmentsAll.Clear();
            
            // Check for all roots
            for (int i = rootChildList.Count - 1; i >= 0; i--)
                if (rootChildList[i] != null)
                    DestroyImmediate(rootChildList[i].gameObject);
            rootChildList.Clear();
        }

        // Reset center helper
        public void ResetCenter()
        {
            centerPosition = Vector3.zero;
            centerDirection = Quaternion.identity;

            Renderer rend = GetComponent<Renderer>();
            if (rend != null)
                centerPosition = transform.InverseTransformPoint (rend.bounds.center);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Scale
        /// /////////////////////////////////////////////////////////
        
        // Check if object is too small
        void ScaleCheck()
        {
            // Geе size from renderers
            if (meshRenderer != null)
                size = meshRenderer.bounds.size.magnitude;
            if (skinnedMeshRend != null)
                size = skinnedMeshRend.bounds.size.magnitude;
            
            // Get rescaleFix if too small
            if (size != 0f && size < minSize)
            {
                // Get rescaleFix factor
                rescaleFix = 1f / size;
                
                // Scale small object up to shatter
                Vector3 newScale = transForm.localScale * rescaleFix;
                transForm.localScale = newScale;
                
                // Warning
                Debug.Log ("Warning. Object " + name + " is too small.");
            }
        }
        
        // Reset original object and fragments scale
        public void ResetScale (float scaleValue)
        {
            // Reset scale
            if (resetState == true && scaleValue == 0f)
            {
                if (skinnedMeshRend != null)
                    skinnedMeshRend.enabled = true;

                if (meshRenderer != null)
                    meshRenderer.enabled = true;

                if (fragmentsLast.Count > 0)
                    foreach (GameObject fragment in fragmentsLast)
                        if (fragment != null)
                            fragment.transform.localScale = Vector3.one;

                resetState = false;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Copy
        /// /////////////////////////////////////////////////////////
        
        // Copy shatter component
        public static void CopyRootMeshShatter (RayfireRigid source, List<RayfireRigid> targets)
        {
            // No shatter
            if (source.meshDemolition.scrShatter == null)
                return;

            // Copy shatter
            for (int i = 0; i < targets.Count; i++)
            {
                targets[i].meshDemolition.scrShatter = targets[i].gameObject.AddComponent<RayfireShatter>();
                targets[i].meshDemolition.scrShatter.CopyFrom (source.meshDemolition.scrShatter);
            }
        }
        
        // Copy from
        void CopyFrom (RayfireShatter shatter)
        {
            type      = shatter.type;

            voronoi   = new RFVoronoi(shatter.voronoi);
            splinters = new RFSplinters(shatter.splinters);
            slabs     = new RFSplinters(shatter.slabs);
            radial    = new RFRadial(shatter.radial); 
            custom    = new RFCustom(shatter.custom);
            slice     = new RFSlice(shatter.slice);
            tets      = new RFTets(shatter.tets);

            mode     = shatter.mode;
            material.CopyFrom (shatter.material);
            clusters = new RFShatterCluster(shatter.clusters);
            advanced = new RFShatterAdvanced(shatter.advanced);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Limitations
        /// /////////////////////////////////////////////////////////
        
        // Size limitation
        void SizeLimitation()
        {
            if (advanced.sizeLimitation == true)
            {
                for (int i = fragmentsLast.Count - 1; i >= 0; i--)
                {
                    
                    MeshRenderer mr = fragmentsLast[i].GetComponent<MeshRenderer>();
                    if (mr.bounds.size.magnitude > advanced.sizeAmount)
                        LimitationFragment (i);
                }
            }
        }
        
        // Vertex limitation
        void VertexLimitation()
        {
            if (advanced.vertexLimitation == true)
                for (int i = fragmentsLast.Count - 1; i >= 0; i--)
                {
                    MeshFilter mf = fragmentsLast[i].GetComponent<MeshFilter>();
                    if (mf.sharedMesh.vertexCount > advanced.vertexAmount)
                        LimitationFragment (i);
                }
        }
        
        // Triangle limitation
        void TriangleLimitation()
        {
            if (advanced.triangleLimitation == true)
                for (int i = fragmentsLast.Count - 1; i >= 0; i--)
                {
                    MeshFilter mf = fragmentsLast[i].GetComponent<MeshFilter>();
                    if (mf.sharedMesh.triangles.Length / 3 > advanced.triangleAmount)
                        LimitationFragment (i);
                }
        }
        
        // Fragment by limitations
        void LimitationFragment(int ind)
        {
            RayfireShatter shat = fragmentsLast[ind].AddComponent<RayfireShatter>();
            shat.voronoi.amount = 4;
                        
            shat.Fragment ();

            if (shat.fragmentsLast.Count > 0)
            {
                fragmentsLast.AddRange (shat.fragmentsLast);
                DestroyImmediate (shat.gameObject);
                fragmentsLast.RemoveAt (ind);

                // Parent and destroy root
                foreach (var frag in shat.fragmentsLast)
                    frag.transform.parent = rootChildList[rootChildList.Count - 1];
                DestroyImmediate (shat.rootChildList[rootChildList.Count - 1].gameObject);
            }
        }
        
        
        
        /*
        enum PrefabMode
        {
        	Scene,
        	Asset,
        	PrefabEditingMode
        }
         
        // Get prefab mode
        PrefabMode GetPrefabMode (GameObject go)
        {
            // scene, prefab, mode
            // Debug.Log (go.scene.path); // fullpath.unity,  null, ""
            // Debug.Log (go.scene.name); // scene name, null, box_pf
            // Debug.Log (go.scene.rootCount); // 4, 0, 1
            // Debug.Log (go.scene.isLoaded); // true, false, true
            // Debug.Log (go.scene.IsValid()); // true, false, true
            // return PrefabMode.Asset;
            
            // Prefab is asset
            if (go.scene.path.EndsWith(".prefab"))
                return PrefabMode.Asset;
            
            // Prefab is in editing mode
            if (string.IsNullOrEmpty(go.scene.path))
                return PrefabMode.PrefabEditingMode;
            
            // Prefab is in scene
            return PrefabMode.Scene;
        }
        */
    }
}