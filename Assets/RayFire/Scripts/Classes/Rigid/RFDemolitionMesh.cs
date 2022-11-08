using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

#if (UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID || UNITY_XBOXONE)
using RayFire.DotNet;
#endif

namespace RayFire
{
    [Serializable]
    public class RFDemolitionSkin
    {
        // rigid on root
        // get bones, get skins
        // on slice get bones and skins by sides using bounds
        // Dup bones sides + common bones
        // Sep skin by sides
        // Slice common skin, separate halfs, add skin, stick to bones
        // Create rigid doll for bones

        public List<Transform> bones;
        public List<SkinnedMeshRenderer> skins;

        public List<SkinnedMeshRenderer> skins0;
        public List<SkinnedMeshRenderer> skins1;
        public List<SkinnedMeshRenderer> skins2;
        
         //meshDemolition.skin.SetupSkin (this);
         //meshDemolition.skin.SeparateSkins(Vector3.up,Vector3.zero);
        
        public void SetupSkin(RayfireRigid rigid)
        {
            skins = rigid.GetComponentsInChildren<SkinnedMeshRenderer>().ToList();
            for (int i = 0; i < skins.Count; i++)
            {
                
            }
        }

        // Separate skins by plane
        public void SeparateSkins(Vector3 planeNormal, Vector3 planePoint)
        {
            Plane plane = new Plane(planeNormal, planePoint);
            for (int i = 0; i < skins.Count; i++)
            {
                bool sideMin = plane.GetSide (skins[i].bounds.min);
                bool sideMax = plane.GetSide (skins[i].bounds.max);
                if (sideMin == sideMax)
                {
                    if (sideMin == true)
                        skins1.Add (skins[i]);
                    else
                        skins2.Add (skins[i]);
                }
                else
                    skins0.Add (skins[i]);
            }
            
            // Fill "bones" array of your new SkinnedMeshRenderer object. Bones at each index should match bones that are listed in "boneWeights" array of your mesh.
            
            // Fill "bindposes" array of new mesh object. In my case I had to just copy it from body mesh to head mesh.
            
            //Mesh m = new Mesh();
            //m.boneWeights;
            //BoneWeight boneWeight = new BoneWeight();
            //boneWeight.weight0 = 1f;
            //m.bindposes;
            //skins[0].bones;

        }
    }

    [Serializable]
    public class RFDemolitionMesh
    {
        public enum MeshInputType
        {
            AtStart          = 3,
            AtInitialization = 6,
            AtDemolition     = 9
        }

        public int                  amount;
        public int                  variation;
        public float                depthFade;
        public float                contactBias;
        public int                  seed;
        public bool                 useShatter;
        public bool                 addChildren;
        public bool                 clusterize;
        public FragSimType          simType;
        public MeshInputType        meshInput;
        public RFFragmentProperties properties;
        public RFRuntimeCaching     runtimeCaching;
        public RayfireShatter       scrShatter;
        
        // Non serialized
        [NonSerialized] public int       badMesh;
        [NonSerialized] public int       shatterMode;
        [NonSerialized] public int       totalAmount;
        [NonSerialized] public int       innerSubId;
        [NonSerialized] public Mesh      mesh;
        [NonSerialized] public RFShatter rfShatter;
        
        // Hidden
        [HideInInspector] public Quaternion cacheRotationStart;
        
        static string fragmentStr = "_fr_";

        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////

        // Constructor
        public RFDemolitionMesh()
        {
            amount         = 15;
            variation      = 0;
            depthFade      = 0.5f;
            contactBias    = 0f;
            seed           = 1;
            useShatter     = false;
            addChildren    = true;
            clusterize     = false;
            simType        = FragSimType.Inherit;
            meshInput      = MeshInputType.AtDemolition;
            properties     = new RFFragmentProperties();
            runtimeCaching = new RFRuntimeCaching();

            Reset();

            shatterMode        = 1;
            innerSubId         = 0;
            cacheRotationStart = Quaternion.identity;

            mesh      = null;
            rfShatter = null;
        }

        // Copy from
        public void CopyFrom (RFDemolitionMesh demolition)
        {
            amount      = demolition.amount;
            variation   = demolition.variation;
            depthFade   = demolition.depthFade;
            seed        = demolition.seed;
            contactBias = demolition.contactBias;
            useShatter  = false;
            addChildren = demolition.addChildren;
            clusterize  = demolition.clusterize;
            simType     = demolition.simType;
            meshInput   = demolition.meshInput;
            meshInput   = MeshInputType.AtDemolition;

            properties.CopyFrom (demolition.properties);
            runtimeCaching = new RFRuntimeCaching();

            Reset();

            shatterMode        = 1;
            innerSubId         = 0;
            cacheRotationStart = Quaternion.identity;

            mesh      = null;
            rfShatter = null;
        }

        // Reset
        public void Reset()
        {
            badMesh     = 0;
            totalAmount = 0;
        }

        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////

        // Demolish single mesh to fragments
        public static bool DemolishMesh (RayfireRigid scr)
        {
            // Object demolition
            if (scr.objectType != ObjectType.Mesh && scr.objectType != ObjectType.SkinnedMesh)
                return true;

            // Skip if reference
            if (scr.demolitionType == DemolitionType.ReferenceDemolition)
                return true;

            // Already has fragments
            if (scr.HasFragments == true)
            {
                // Set tm 
                scr.rootChild.position = scr.transForm.position;
                scr.rootChild.rotation = scr.transForm.rotation;

                // Set parent
                RayfireMan.SetParentByManager (scr.rootChild.transform);

                // Activate root and fragments
                scr.rootChild.gameObject.SetActive (true);

                // Start all coroutines
                for (int i = 0; i < scr.fragments.Count; i++)
                    scr.fragments[i].StartAllCoroutines();

                scr.limitations.demolished = true;
                return true;
            }

            // Has serialized meshes but has no Unity meshes - convert to unity meshes
            if (scr.HasRfMeshes == true && scr.HasMeshes == false)
                RFMesh.ConvertRfMeshes (scr);

            // Has unity meshes - create fragments
            if (scr.HasMeshes == true)
            {
                scr.fragments              = CreateFragments (scr);
                scr.limitations.demolished = true;
                return true;
            }

            // Still has no Unity meshes - cache Unity meshes
            if (scr.HasMeshes == false)
            {
                // Cache unity meshes
                CacheRuntime (scr);

                // Caching in progress. Stop demolition
                if (scr.meshDemolition.runtimeCaching.inProgress == true)
                    return false;

                // Fragmentation on not supported platforms. approve and set dml to none
                if (scr.meshes == null)
                {
                    scr.limitations.demolished = false;
                    return true;
                }

                // Has unity meshes - create fragments
                if (scr.HasMeshes == true)
                {
                    scr.fragments              = CreateFragments (scr);
                    scr.limitations.demolished = true;
                    return true;
                }
            }

            return false;
        }

        // Clusterize runtime fragments
        public static bool ClusterizeFragments (RayfireRigid rigid)
        {
            // Clusterize disabled
            if (rigid.meshDemolition.clusterize == false)
                return false;

            // Not mesh demolition
            if (rigid.objectType != ObjectType.Mesh)
                return false;

            // Not runtime
            if (rigid.demolitionType == DemolitionType.None || rigid.demolitionType == DemolitionType.ReferenceDemolition)
                return false;

            // No fragments
            if (rigid.HasFragments == false)
                return false;

            // Create Connected cluster Rigid
            RayfireRigid scr = rigid.rootChild.gameObject.AddComponent<RayfireRigid>();
            rigid.CopyPropertiesTo (scr);

            // Set custom fragment simulation type if not inherited
            SetFragmentSimulationType (scr, rigid);
            
            // Set properties
            scr.objectType                = ObjectType.ConnectedCluster;
            scr.demolitionType            = DemolitionType.Runtime;
            scr.clusterDemolition.cluster = new RFCluster();
            
            /*
            // Copy Uny components
            if (scr.simulationType == SimType.Inactive || scr.simulationType == SimType.Kinematic)
            {
                RayfireUnyielding[] unyList = rigid.GetComponents<RayfireUnyielding>();
                if (unyList.Length > 0)
                {
                    for (int i = 0; i < unyList.Length; i++)
                    {
                        // scr.gameObject.AddComponent<RayfireUnyielding>();
                        // TODO Copy uny component
                    }
                }
            }
            */

            // Init
            scr.Initialize();
            
            // Set contact point for demolition
            scr.limitations.contactPoint   = rigid.limitations.contactPoint;
            scr.limitations.contactNormal  = rigid.limitations.contactNormal;
            scr.limitations.contactVector3 = rigid.limitations.contactVector3;

            // Inherit velocity
            scr.physics.velocity           = rigid.physics.velocity;
            scr.physics.rigidBody.velocity = rigid.physics.velocity;
            
            // Demolish cluster and get solo shards
            List<RFShard> detachShards = RFDemolitionCluster.DemolishConnectedCluster (scr);
           
            // Get has for all detached objects to keep their Rigid and rigidbody
            HashSet<Transform> detachTms    = new HashSet<Transform>();
            for (int i = 0; i < detachShards.Count; i++)
                detachTms.Add (detachShards[i].tm);
            
            // Destroy fragments rigid and rigidbody for not detached shards
            for (int i = rigid.fragments.Count - 1; i >= 0; i--)
            {
                if (detachTms.Contains (rigid.fragments[i].transForm) == false)
                {
                    // Debug.Log (rigid.fragments[i].name);
                    Object.Destroy (rigid.fragments[i].physics.rigidBody);
                    Object.Destroy (rigid.fragments[i]);
                    rigid.fragments.RemoveAt (i);
                }
            }

            // TODO add main and child clusters to fragments list. get them in scr.fragments
            
            // Delete if cluster was completely demolished
            if (scr.limitations.demolished == true)
                RayfireMan.DestroyFragment (scr, null);

            return true;
        }
        
        // Create fragments by mesh and pivots array
        public static List<RayfireRigid> CreateFragments (RayfireRigid scr)
        {
            // Fragments list
            List<RayfireRigid> scrArray = new List<RayfireRigid>();

            // Stop if has no any meshes
            if (scr.meshes == null)
                return scrArray;
            
            // Create RayFire manager if not created
            RayfireMan.RayFireManInit();
            
            // Create root object and parent
            RFLimitations.CreateRoot (scr);
            
            // Vars 
            int    baseLayer = scr.meshDemolition.properties.GetLayer(scr);
            string baseTag   = scr.meshDemolition.properties.GetTag(scr);
            string baseName  = scr.gameObject.name + fragmentStr;

            // Save original rotation
            // Quaternion originalRotation = rootChild.transform.rotation;
            
            // Set rotation to precache rotation
            if (scr.demolitionType == DemolitionType.AwakePrecache)
                scr.rootChild.transform.rotation = scr.cacheRotation;

            // Get original mats
            Material[] mats = scr.skinnedMeshRend != null
                ? scr.skinnedMeshRend.sharedMaterials
                : scr.meshRenderer.sharedMaterials;

            // Create fragment objects
            for (int i = 0; i < scr.meshes.Length; ++i)
            {
                // Get object from pool or create
                RayfireRigid rfScr = RayfireMan.inst == null
                    ? RFPoolingFragment.CreateRigidInstance()
                    : RayfireMan.inst.fragments.GetPoolObject(RayfireMan.inst.transForm);

                // Setup
                rfScr.transform.position    = scr.transForm.position + scr.pivots[i];
                rfScr.transform.parent      = scr.rootChild;
                rfScr.name                  = baseName + i;
                rfScr.gameObject.tag        = baseTag;
                rfScr.gameObject.layer      = baseLayer;
                rfScr.meshFilter.sharedMesh = scr.meshes[i];
                rfScr.rootParent            = scr.rootChild;
                
                // Copy properties from parent to fragment node
                scr.CopyPropertiesTo (rfScr);

                // Set custom fragment simulation type if not inherited
                SetFragmentSimulationType (rfScr, scr);
                
                // Copy particles
                RFParticles.CopyRigidParticles (scr, rfScr);
                
                // Set collider
                RFPhysic.SetFragmentCollider (rfScr, scr.meshes[i]);
                
                // Shadow casting
                if (RayfireMan.inst.advancedDemolitionProperties.sizeThreshold > 0 && 
                    RayfireMan.inst.advancedDemolitionProperties.sizeThreshold > scr.meshes[i].bounds.size.magnitude)
                    rfScr.meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
                
                // Turn on
                rfScr.gameObject.SetActive (true);

                // Update depth level and amount
                rfScr.limitations.currentDepth = scr.limitations.currentDepth + 1;
                rfScr.meshDemolition.amount    = (int)(rfScr.meshDemolition.amount * rfScr.meshDemolition.depthFade);
                if (rfScr.meshDemolition.amount < 3)
                    rfScr.meshDemolition.amount = 3;
                
                // Disable outer mat for depth fragments
                if (rfScr.limitations.currentDepth >= 1)
                    rfScr.materials.outerMaterial = null;
        
                // Set multymaterial
                RFSurface.SetMaterial (scr.subIds, mats, scr.materials, rfScr.meshRenderer, i, scr.meshes.Length);
                
                // Set mass by mass value accordingly to parent
                if (rfScr.physics.massBy == MassType.MassProperty)
                    RFPhysic.SetMassByParent (rfScr.physics, rfScr.limitations.bboxSize, scr.physics.mass, scr.limitations.bboxSize);
                
                // Add in array
                scrArray.Add (rfScr);
            }

            // Fix transform for precached fragments
            if (scr.demolitionType == DemolitionType.AwakePrecache)
                scr.rootChild.rotation = scr.transForm.rotation;

            // Fix runtime caching rotation difference. Get rotation difference and add to root
            if (scr.demolitionType == DemolitionType.Runtime && scr.meshDemolition.runtimeCaching.type != CachingType.Disable)
            {
                Quaternion cacheRotationDif = scr.transForm.rotation * Quaternion.Inverse (scr.meshDemolition.cacheRotationStart);
                scr.rootChild.rotation = cacheRotationDif * scr.rootChild.rotation;
            }
            
            // Reset scale for mesh fragments. IMPORTANT: skinned mesh fragments root should not be rescaled 
            if (scr.skinnedMeshRend == null)
                scr.rootChild.localScale = Vector3.one;
            
            // Set root to manager
            RayfireMan.SetParentByManager (scr.rootChild);
            
            // Ignore neib collisions
            RFPhysic.SetIgnoreColliders (scr.physics, scrArray);
            
            return scrArray;
        }
        
        // SLice mesh
        public static void SliceMesh(RayfireRigid scr)
        {
            // Empty lists
            scr.DeleteCache();
            scr.DeleteFragments();
    
            // SLice
            RFFragment.SliceMeshes (ref scr.meshes, ref scr.pivots, ref scr.subIds, scr, scr.limitations.slicePlanes);
            
            // TODO check if has slicePlanes
            
            // Remove plane info 
            Plane forcePlane = new Plane (scr.limitations.slicePlanes[1], scr.limitations.slicePlanes[0]);
            scr.limitations.slicePlanes.Clear();
            
            // Stop
            if (scr.HasMeshes == false)
                return;

            // Get fragments
            scr.fragments = CreateSlices(scr);

            // Check for sliced inactive/kinematic with unyielding
            RayfireUnyielding.SetUnyieldingFragments (scr);
            
            // TODO check for fragments
            
            // Set demolition 
            scr.limitations.demolished = true;
            
            // Fragments initialisation
            scr.InitMeshFragments();
            
            // Add force
            if (scr.limitations.sliceForce != 0)
            {
                foreach (var frag in scr.fragments)
                {
                    // Skip inactive fragments
                    if (scr.limitations.affectInactive == false && frag.simulationType == SimType.Inactive)
                        continue;
                    
                    // Apply force
                    Vector3 closestPoint = forcePlane.ClosestPointOnPlane (frag.transform.position);
                    Vector3 normalVector = (frag.transform.position - closestPoint).normalized;
                    frag.physics.rigidBody.AddForce (normalVector * scr.limitations.sliceForce, ForceMode.VelocityChange);

                    /* TODO force to spin fragments based on blades direction
                    normalVector = new Vector3 (-1, 0, 0);
                    frag.physics.rigidBody.AddForceAtPosition (normalVector * scr.limitations.sliceForce, closestPoint, ForceMode.VelocityChange);
                    */
                }
            }
        }
        
         // Create slices by mesh and pivots array
        public static List<RayfireRigid> CreateSlices (RayfireRigid scr)
        {
            // Fragments list
            List<RayfireRigid> scrArray = new List<RayfireRigid>();

            // Stop if has no any meshes
            if (scr.meshes == null)
                return scrArray;
            
            // Create RayFire manager if not created
            RayfireMan.RayFireManInit();
            
            // Create root object and parent
            RFLimitations.CreateRoot (scr);
            
            // Vars 
            int    baseLayer = scr.meshDemolition.properties.GetLayer(scr);
            string baseTag   = scr.meshDemolition.properties.GetTag(scr);
            string baseName  = scr.gameObject.name + "_";

            // Get original mats
            Material[] mats = scr.skinnedMeshRend != null
                ? scr.skinnedMeshRend.sharedMaterials
                : scr.meshRenderer.sharedMaterials;
            
            // Create fragment objects
            for (int i = 0; i < scr.meshes.Length; ++i)
            {
                // Get object from pool or create
                RayfireRigid rfScr = RayfireMan.inst == null
                    ? RFPoolingFragment.CreateRigidInstance()
                    : RayfireMan.inst.fragments.GetPoolObject(RayfireMan.inst.transForm);

                // Setup
                rfScr.transform.position    = scr.transForm.position + scr.pivots[i];
                rfScr.transform.parent      = scr.rootChild;
                rfScr.name                  = baseName + i;
                rfScr.gameObject.tag        = baseTag;
                rfScr.gameObject.layer      = baseLayer;
                rfScr.meshFilter.sharedMesh = scr.meshes[i];
                rfScr.rootParent            = scr.rootChild;

                // Copy properties from parent to fragment node
                scr.CopyPropertiesTo (rfScr);
                
                // Set custom fragment simulation type if not inherited
                SetFragmentSimulationType (rfScr, scr);
                
                // Copy particles
                RFParticles.CopyRigidParticles (scr, rfScr);
                
                // Set collider
                RFPhysic.SetFragmentCollider (rfScr, scr.meshes[i]);
                
                // Shadow casting
                if (RayfireMan.inst.advancedDemolitionProperties.sizeThreshold > 0 && 
                    RayfireMan.inst.advancedDemolitionProperties.sizeThreshold > scr.meshes[i].bounds.size.magnitude)
                    rfScr.meshRenderer.shadowCastingMode = ShadowCastingMode.Off;

                // Turn on
                rfScr.gameObject.SetActive (true);

                // Update depth level and amount
                rfScr.limitations.currentDepth = scr.limitations.currentDepth + 1;

                // Disable outer mat for depth fragments.
                if (rfScr.limitations.currentDepth >= 1)
                    rfScr.materials.outerMaterial = null;
                
                // Set multymaterial
                RFSurface.SetMaterial (scr.subIds, mats, scr.materials, rfScr.meshRenderer, i, scr.meshes.Length);

                // Set mass by mass value accordingly to parent
                if (rfScr.physics.massBy == MassType.MassProperty)
                    RFPhysic.SetMassByParent (rfScr.physics, rfScr.limitations.bboxSize, scr.physics.mass, scr.limitations.bboxSize);
                
                // Add in array
                scrArray.Add (rfScr);
            }
            
            // Set root to manager
            RayfireMan.SetParentByManager (scr.rootChild);
            
            // Empty lists
            scr.DeleteCache();

            return scrArray;
        }

        // Set custom fragment simulation type if not inherited
        static void SetFragmentSimulationType (RayfireRigid frag, RayfireRigid scr)
        {
            frag.simulationType = scr.simulationType;
            if (frag.meshDemolition.simType != FragSimType.Inherit)
                frag.simulationType = (SimType)frag.meshDemolition.simType;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Caching
        /// /////////////////////////////////////////////////////////
        
        // Start cache fragment meshes. Instant or runtime
        static void CacheRuntime (RayfireRigid scr)
        {
            // Reuse existing cache
            if (scr.reset.action == RFReset.PostDemolitionType.DeactivateToReset && scr.reset.mesh == RFReset.MeshResetType.ReuseFragmentMeshes)
                if (scr.HasMeshes == true)
                    return;

            // Clear all mesh data
            scr.DeleteCache();

            // Cache meshes
            if (scr.meshDemolition.runtimeCaching.type == CachingType.Disable)
                CacheInstant(scr);
            else
                scr.CacheFrames();
        }
        
        // Instant caching into meshes
        static void CacheInstant (RayfireRigid scr)
        {
            // Input mesh, setup
            if (RFFragment.InputMesh (scr) == false)
                return;

            // Create fragments
            RFFragment.CacheMeshesInst (ref scr.meshes, ref scr.pivots, ref scr.subIds, scr);
        }       
        
        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////

        // Cor to fragment mesh over several frames
        public IEnumerator RuntimeCachingCor (RayfireRigid scr)
        {
            // Object should be demolished when cached all meshes but not during caching
            bool demolitionShouldLocal = scr.limitations.demolitionShould == true;
            scr.limitations.demolitionShould = false;
            
            // Input mesh, setup, record time
            float t1 = Time.realtimeSinceStartup;
            if (RFFragment.InputMesh (scr) == false)
                yield break;
                        
            // Set list with amount of mesh for every frame
            List<int> batchAmount = runtimeCaching.type == CachingType.ByFrames
                ? RFRuntimeCaching.GetBatchByFrames(runtimeCaching.frames, totalAmount)
                : RFRuntimeCaching.GetBatchByFragments(runtimeCaching.fragments, totalAmount);
            
            // Caching in progress
            runtimeCaching.inProgress = true;

            // Wait next frame if input took too much time or long batch
            float t2 = Time.realtimeSinceStartup - t1;
            if (t2 > 0.025f || batchAmount.Count > 5)
                yield return null;

            // Save tm for multi frame caching
            GameObject tmRefGo = RFRuntimeCaching.CreateTmRef (scr);

            // Start rotation
            cacheRotationStart = scr.transForm.rotation;
            
            // Iterate every frame. Calc local frame meshes
            List<Mesh>         meshesList = new List<Mesh>();
            List<Vector3>      pivotsList = new List<Vector3>();
            List<RFDictionary> subList    = new List<RFDictionary>();
            for (int i = 0; i < batchAmount.Count; i++)
            {
                // Check for stop
                if (runtimeCaching.stop == true)
                {
                    ResetRuntimeCaching(scr, tmRefGo);
                    yield break;
                }
                
                // Cache defined points
                RFFragment.CacheMeshesMult (tmRefGo.transform, ref meshesList, ref pivotsList, ref subList, scr, batchAmount, i);
                // TODO create fragments for current batch
                // TODO record time and decrease batches amount if less 30 fps
                yield return null;
            }
            
            // Set to main data vars
            scr.meshes = meshesList.ToArray();
            scr.pivots = pivotsList.ToArray();
            scr.subIds = subList;

            // Clear
            scr.DestroyObject (tmRefGo);
            scr.meshDemolition.scrShatter = null;
            
            // Set demolition ready state
            if (runtimeCaching.skipFirstDemolition == false && demolitionShouldLocal == true)
                scr.limitations.demolitionShould = true;
            
            // Reset damage
            if (runtimeCaching.skipFirstDemolition == true && demolitionShouldLocal == true)
                scr.damage.Reset();
            
            // Caching finished
            runtimeCaching.inProgress = false;
            runtimeCaching.wasUsed = true;
        }

        // Stop runtime caching and reset it
        public void StopRuntimeCaching()
        {
            if (runtimeCaching.inProgress == true)
                runtimeCaching.stop = true;
        }
        
        // Reset caching
        void ResetRuntimeCaching (RayfireRigid scr, GameObject tmRefGo)
        {
            scr.DestroyObject (tmRefGo);
            runtimeCaching.stop = false;
            runtimeCaching.inProgress = false;
            scr.meshDemolition.rfShatter = null;
            scr.DeleteCache();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Precache and Prefragment
        /// /////////////////////////////////////////////////////////  
        
        // Precache meshes at awake
        public static void AwakePrecache(RayfireRigid scr)
        {
            if (scr.demolitionType == DemolitionType.AwakePrecache && scr.objectType == ObjectType.Mesh)
                PreCache(scr);
        }
        
        // Predefine fragments
        public static void AwakePrefragment(RayfireRigid scr)
        {
            if (scr.demolitionType == DemolitionType.AwakePrefragment && scr.objectType == ObjectType.Mesh)
            {
                PreCache(scr);
                scr.Prefragment();
            }
        }

        // PreCache meshes
        static void PreCache(RayfireRigid scr)
        {
            // Save and disable bias
            float bias = scr.meshDemolition.contactBias;
            scr.meshDemolition.contactBias = 0;
                
            // Cache frag meshes
            CacheInstant (scr);
                
            // Restore bias
            scr.meshDemolition.contactBias = bias;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Children ops
        /// /////////////////////////////////////////////////////////  
        
        // Set children with mesh as additional fragments
        public static void ChildrenToFragments(RayfireRigid scr)
        {
            // Not for clusters
            if (scr.IsCluster == true)
                return;

            // Disabled
            if (scr.meshDemolition.addChildren == false)
                return;
            
            // No children
            if (scr.transForm.childCount == 0)
                return;
            
            // Iterate children TODO precache in awake and use now. Set init type to by method at awake.
            Transform child;
            int childCount = scr.transForm.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                // Get child
                child = scr.transForm.GetChild (i);

                // Skip if has no mesh
                if (child.GetComponent<MeshFilter>() == false)
                    continue;

                // Set parent to main fragments root
                child.parent = scr.rootChild;
                
                // Get Already applied Rigid
                RayfireRigid childScr = child.GetComponent<RayfireRigid>();

                // Add new if has no. Copy properties
                if (childScr == null)
                {
                    childScr = child.gameObject.AddComponent<RayfireRigid>();
                    childScr.initialization = RayfireRigid.InitType.ByMethod;
                    scr.CopyPropertiesTo (childScr);
                    
                    // Enable use shatter
                    childScr.meshDemolition.scrShatter = child.GetComponent<RayfireShatter>();
                    if (childScr.meshDemolition.scrShatter != null)
                        childScr.meshDemolition.useShatter = true;
                }
                
                // Set custom fragment simulation type if not inherited
                SetFragmentSimulationType (childScr, scr);

                // Init
                childScr.Initialize();
                
                // Update depth level and amount
                childScr.limitations.currentDepth = scr.limitations.currentDepth + 1;
                
                // Collect
                scr.fragments.Add (childScr);
            }
        }
    }
}