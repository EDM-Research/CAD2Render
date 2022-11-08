using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;


namespace RayFire
{
    [Serializable]
    public class RFPhysic
    {
        public MaterialType   materialType;
        public PhysicMaterial material;
        public MassType       massBy;
        public float          mass;
        public RFColliderType colliderType;
        public bool           planarCheck;
        public bool           ignoreNear;
        public bool           useGravity;
        public int            solverIterations;
        public float          dampening;
        
        public Rigidbody      rigidBody;
        public Collider       meshCollider;
        public List<Collider> clusterColliders;
        public List<int>      ignoreList;
        public bool           rec;
        public bool           exclude;
        public int            solidity;
        public bool           destructible;
        public bool           physicsDataCorState;
        
        [NonSerialized] public Vector3     velocity;
        [NonSerialized] public Vector3     initScale;
        [NonSerialized] public Vector3     initPosition;
        [NonSerialized] public Quaternion  initRotation;
        [NonSerialized] public Vector3     localPosition;
        [NonSerialized] public IEnumerator physicsEnum;
        
        public static          int       coplanarVertLimit = 30;
        
        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////

        // Constructor
        public RFPhysic()
        {
            materialType     = MaterialType.Concrete;
            material         = null;
            massBy           = MassType.MaterialDensity;
            mass             = 1f;
            colliderType     = RFColliderType.Mesh;
            
            planarCheck      = true;
            ignoreNear       = false;
            
            useGravity       = true;
            solverIterations = 6;
            dampening        = 0.7f;
            
            solidity         = 1;
            
            Reset();
            
            ignoreList = null;

            initScale     = Vector3.one;
            initPosition  = Vector3.zero;
            initRotation  = Quaternion.identity;
            localPosition = Vector3.zero;

            physicsDataCorState = false;
        }

        // Copy from
        public void CopyFrom(RFPhysic physics)
        {
            materialType     = physics.materialType;
            material         = physics.material;
            massBy           = physics.massBy;
            mass             = physics.mass;
            colliderType     = physics.colliderType;
            
            planarCheck      = physics.planarCheck;
            ignoreNear       = false;
            
            useGravity       = physics.useGravity;
            solverIterations = physics.solverIterations;
            dampening        = physics.dampening;
            
            ignoreList       = null;
            
            Reset();
        }
        
        // Reset
        public void Reset()
        {
            rec                 = false;
            exclude             = false;
            physicsDataCorState = false;
            
            velocity    = Vector3.zero;
            physicsEnum = null;
        }

        // Save init transform. Birth tm for activation check and reset
        public void SaveInitTransform(Transform tm)
        {
            initScale     = tm.localScale;
            initPosition  = tm.position;
            initRotation  = tm.rotation;
            localPosition = tm.localPosition;
        }
        
        // Save init transform. Birth tm for activation check and reset
        public void LoadInitTransform(Transform tm)
        {
            tm.localScale = initScale;
            tm.position   = initPosition;
            tm.rotation   = initRotation;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Simulation Type
        /// /////////////////////////////////////////////////////////
        
        // Set simulation type properties
        public static void SetSimulationType(Rigidbody rb, SimType simulationType, ObjectType objectType, bool useGravity, int solver)
        {
            if (simulationType == SimType.Static)
                return;    
            
            // Interpolation
            rb.interpolation = RayfireMan.inst.interpolation;
            
            // Solver iterations
            rb.solverIterations         = solver;
            rb.solverVelocityIterations = solver;

            // Dynamic
            if (simulationType == SimType.Dynamic)
            {
                SetDynamic (rb, useGravity);
                SetCollisionDetection (rb, objectType);
            }

            // Sleeping 
            else if (simulationType == SimType.Sleeping)
            {
                SetSleeping (rb, useGravity);
                SetCollisionDetection (rb, objectType);
            }

            // Inactive
            else if (simulationType == SimType.Inactive)
            {
                SetInactive (rb);
                SetCollisionDetection (rb, objectType);
            }

            // Kinematic
            else if (simulationType == SimType.Kinematic)
                SetKinematic(rb, useGravity);
        }

        // Set as dynamic
        static void SetDynamic(Rigidbody rb, bool useGravity)
        {
            rb.isKinematic = false;
            rb.useGravity  = useGravity;
        }

        // Set as sleeping
        static void SetSleeping(Rigidbody rb, bool useGravity)
        {
            rb.isKinematic = false;
            rb.useGravity  = useGravity;
            rb.Sleep();
        }

        // Set as inactive
        static void SetInactive(Rigidbody rb)
        {
            rb.isKinematic = false;
            rb.useGravity  = false;
            rb.Sleep();
        }

        // Set as Kinematic
        static void SetKinematic(Rigidbody rb, bool useGravity)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            rb.isKinematic            = true;
            rb.useGravity             = useGravity;
        }

        // Collision detection
        static void SetCollisionDetection(Rigidbody rb, ObjectType objectType)
        {
            if (objectType == ObjectType.NestedCluster || objectType == ObjectType.ConnectedCluster)
                rb.collisionDetectionMode = RayfireMan.inst.clusterCollision;
            else
                rb.collisionDetectionMode = RayfireMan.inst.meshCollision;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Density
        /// /////////////////////////////////////////////////////////
        
        // Set density. After collider defined.
        public static void SetDensity(RayfireRigid scr)
        {
            // Default mass from inspector
            float m = scr.physics.mass;

            // Mass by rigid body
            if (scr.physics.massBy == MassType.RigidBodyComponent)
            {
                // Return if has rigidbody component with defined mass
                if (scr.physics.rigidBody != null)
                    return;

                // Set to by density if has no rigid component
                scr.physics.massBy = MassType.MaterialDensity;
            } 
            
            // Get mass by density
            if (scr.physics.massBy == MassType.MaterialDensity)
            {
                scr.physics.rigidBody.SetDensity(RayfireMan.inst.materialPresets.Density(scr.physics.materialType));
                m = scr.physics.rigidBody.mass;
            }
            
            // Check for min/max mass
            m = MassLimit (m);
            
            // Update mass in inspector
            scr.physics.rigidBody.mass = m;
        }
        
        // Set density. After collider defined.
        public static void SetDensity(RFShard shard, RFPhysic physics, float density)
        {
            // Set mass if it was already defined before
            if (shard.m > 0)
            {
                shard.rb.mass = shard.m;
                // TODO STOP??? Check if mass need to be updated and reset it to 0
            }
            
            // Default mass from inspector
            float m = physics.mass;

            // Set mass by density
            if (physics.massBy == MassType.MaterialDensity)
            {
                shard.rb.SetDensity (density);
                m = shard.rb.mass;
            }
            
            // Set mass by rb component. Stop
            else if (physics.massBy == MassType.RigidBodyComponent)
                return;

            // Check for min/max mass
            m = MassLimit (m);
            
            // set mass in shard properties
            shard.m = m;
            
            // Update mass in rigidbody
            shard.rb.mass = m;
        }

        // Limit mass with min max range
        static float MassLimit(float m)
        {
            if (RayfireMan.inst.minimumMass > 0)
                if (m < RayfireMan.inst.minimumMass)
                    return RayfireMan.inst.minimumMass;
            if (RayfireMan.inst.maximumMass > 0)
                if (m > RayfireMan.inst.maximumMass)
                    return RayfireMan.inst.maximumMass;
            return m;
        }

        // Set mass by mass value accordingly to parent
        public static void SetMassByParent(RFPhysic target, float targetSize, float parentMass, float parentSize)
        {
            target.mass           = parentMass * (targetSize / parentSize) * 0.7f;
            target.rigidBody.mass = target.mass;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Drag
        /// /////////////////////////////////////////////////////////
        
        // Set drag properties
        public static void SetDrag(RayfireRigid scr)
        {
            scr.physics.rigidBody.drag        = RayfireMan.inst.materialPresets.Drag(scr.physics.materialType);
            scr.physics.rigidBody.angularDrag = RayfireMan.inst.materialPresets.AngularDrag(scr.physics.materialType);
        }

        // Set drag properties
        public static void SetDrag(RFShard shard, float drag, float dragAngular)
        {
            shard.rb.drag        = drag;
            shard.rb.angularDrag = dragAngular;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Rigid body
        /// /////////////////////////////////////////////////////////
        
        // Set velocity
        public static void SetFragmentsVelocity (RayfireRigid scr)
        {
            // TODO different for clusters, get rigid body center of mass
            
            // Current velocity
            if (scr.meshDemolition.runtimeCaching.wasUsed == true && scr.meshDemolition.runtimeCaching.skipFirstDemolition == false)
            {
                for (int i = 0; i < scr.fragments.Count; i++)
                    if (scr.fragments[i] != null)
                        scr.fragments[i].physics.rigidBody.velocity = scr.physics.rigidBody.GetPointVelocity (scr.fragments[i].transForm.position) * scr.physics.dampening;
            }

            // Previous frame velocity
            else
            {
                Vector3 baseVelocity = scr.physics.velocity * scr.physics.dampening;
                for (int i = 0; i < scr.fragments.Count; i++)
                    if (IsNull(scr.fragments[i].physics.rigidBody) == false)
                        scr.fragments[i].physics.rigidBody.velocity = baseVelocity;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Mesh Collider
        /// /////////////////////////////////////////////////////////
        
        // Set fragments collider
        public static void SetFragmentCollider(RayfireRigid scr, Mesh mesh)
        {
            // Custom collider
            scr.physics.colliderType = scr.meshDemolition.properties.colliderType;
            
            // Size filter check
            if (scr.meshDemolition.properties.sizeFilter > 0)
                if (mesh.bounds.size.magnitude < scr.meshDemolition.properties.sizeFilter)
                    scr.physics.colliderType = RFColliderType.None;
            
            // Skip collider
            SetRigidCollider (scr, mesh);
        }
        
        // Set fragments collider
        public static void SetRigidCollider (RayfireRigid scr, Mesh mesh = null)
        {
            // Skip collider
            if (scr.physics.colliderType == RFColliderType.None)
                return;
            
            // Discard collider if just trigger
            if (scr.physics.meshCollider != null && scr.physics.meshCollider.isTrigger == true)
                scr.physics.meshCollider = null;

            // Size check
            if (RayfireMan.inst != null && RayfireMan.inst.colliderSize > 0)
                if (scr.meshRenderer.bounds.size.magnitude < RayfireMan.inst.colliderSize)
                    return;
            
            // No collider. Add own
            if (scr.physics.meshCollider == null)
            {
                // Mesh collider
                if (scr.physics.colliderType == RFColliderType.Mesh)
                {
                    // Low vert check
                    if (scr.meshFilter.sharedMesh.vertexCount <= 3)
                        return;
                    
                    // Optional coplanar check
                    if (scr.physics.planarCheck == true && scr.meshFilter.sharedMesh.vertexCount < coplanarVertLimit)
                        if (RFShatterAdvanced.IsCoplanar (scr.meshFilter.sharedMesh, RFShatterAdvanced.planarThreshold) == true)
                        {
                            Debug.Log ("RayFire Rigid: " + scr.name + " had planar low poly mesh. Object can't get Mesh Collider.", scr.gameObject);
                            scr.physics.colliderType = RFColliderType.None;
                            return;
                        }
                    
                    // Add Mesh collider
                    MeshCollider mCol = scr.gameObject.AddComponent<MeshCollider>();
                    
                    // Set mesh
                    if (mesh != null)
                        mCol.sharedMesh = mesh;

                    // Set convex
                    if (scr.simulationType != SimType.Static)
                        mCol.convex = true;
                    scr.physics.meshCollider = mCol;
                }
                    
                // Box.Sphere collider
                else if (scr.physics.colliderType == RFColliderType.Box)
                    scr.physics.meshCollider = scr.gameObject.AddComponent<BoxCollider>();
                else if (scr.physics.colliderType == RFColliderType.Sphere)
                    scr.physics.meshCollider = scr.gameObject.AddComponent<SphereCollider>();
            }
        }
        
        // Set fragments collider
        public static void SetRigidRootCollider (RayfireRigidRoot root, RFPhysic physics, RFShard shard)
        {
            // Get collider
            shard.col = shard.tm.GetComponent<Collider>(); 
            
            // Skip collider
            if (physics.colliderType == RFColliderType.None)
                return;
            
            // No collider. Add own
            if (shard.col == null)
            {
                // Mesh collider
                if (physics.colliderType == RFColliderType.Mesh)
                {
                    // Add Mesh collider
                    MeshCollider col = shard.tm.gameObject.AddComponent<MeshCollider>();
                    col.sharedMesh = shard.mf.sharedMesh;
                    col.convex     = true;
                    shard.col      = col;
                }
                    
                // Box / Sphere collider
                else if (physics.colliderType == RFColliderType.Box)
                    shard.col = shard.tm.gameObject.AddComponent<BoxCollider>();
                else if (physics.colliderType == RFColliderType.Sphere)
                    shard.col = shard.tm.gameObject.AddComponent<SphereCollider>();
                
                // Collect applied collider to destroy at setup reset
                root.collidersList.Add (shard.col);
            }
        }

        // Set collider for mesh root fragments in editor setup
        public static void SetupMeshRootColliders(RayfireRigid scr)
        {
            scr.physics.clusterColliders = new List<Collider>();
            for (int i = 0; i < scr.fragments.Count; i++)
            {
                // Collect own colliders
                Collider col = scr.fragments[i].GetComponent<Collider>();
                if (col != null)
                    scr.physics.clusterColliders.Add (col);

                // Add Collider
                SetRigidCollider (scr.fragments[i]);
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Add Connected/Nested Cluster Colliders
        /// /////////////////////////////////////////////////////////
        
        // Create mesh colliders for every input mesh TODO input cluster to control all nest roots for correct colliders
        public static void SetClusterCollidersByShards (RayfireRigid scr)
        {
            // Check colliders list
            CollidersRemoveNull (scr);

            // Already clusterized
            if (scr.physics.HasClusterColliders == true)
                return;
            
            // Colliders list
            if (scr.physics.clusterColliders == null)
                scr.physics.clusterColliders = new List<Collider>();
            
            // Connected/Nested colliders
            if (scr.objectType == ObjectType.ConnectedCluster)
                SetShardColliders (scr, scr.clusterDemolition.cluster);
            else if (scr.objectType == ObjectType.NestedCluster)
                SetDeepShardColliders (scr, scr.clusterDemolition.cluster);
        }

        // Null check and remove
        static void CollidersRemoveNull(RayfireRigid scr)
        {
            if (scr.physics.HasClusterColliders == true)
                for (int i = scr.physics.clusterColliders.Count - 1; i >= 0; i--)
                    if (scr.physics.clusterColliders[i] == null)
                        scr.physics.clusterColliders.RemoveAt (i);
        }
        
        // Check children for mesh or cluster root until all children will not be checked
        static void SetShardColliders (RayfireRigid scr, RFCluster cluster)
        {
            // Mesh collider
            if (scr.physics.colliderType == RFColliderType.Mesh)
            {
                for (int i = 0; i < cluster.shards.Count; i++)
                {
                    // Get mesh filter and collider TODO set collider by type
                    MeshCollider meshCol = cluster.shards[i].tm.GetComponent<MeshCollider>();
                    if (meshCol == null)
                    {
                        meshCol            = cluster.shards[i].mf.gameObject.AddComponent<MeshCollider>();
                        meshCol.sharedMesh = cluster.shards[i].mf.sharedMesh;
                    }
                    meshCol.convex = true;
   
                    // Set shard collider and collect
                    cluster.shards[i].col = meshCol;
                    scr.physics.clusterColliders.Add (meshCol);
                }
            }
                    
            // Box.Sphere collider
            else if (scr.physics.colliderType == RFColliderType.Box)
            {
                for (int i = 0; i < cluster.shards.Count; i++)
                {
                    // Set shard collider and collect
                    cluster.shards[i].col = cluster.shards[i].mf.gameObject.AddComponent<BoxCollider>();
                    scr.physics.clusterColliders.Add (cluster.shards[i].col);
                }
            }
            else if (scr.physics.colliderType == RFColliderType.Sphere)
            {
                for (int i = 0; i < cluster.shards.Count; i++)
                {
                    cluster.shards[i].col = cluster.shards[i].mf.gameObject.AddComponent<SphereCollider>();
                    scr.physics.clusterColliders.Add (cluster.shards[i].col);
                }
            }
        }
        
        // Check children for mesh or cluster root until all children will not be checked
        static void SetDeepShardColliders (RayfireRigid scr, RFCluster cluster)
        {
            // Set shard colliders
            SetShardColliders (scr, cluster);

            // Set child cluster colliders
            if (cluster.HasChildClusters == true)
                for (int i = 0; i < cluster.childClusters.Count; i++)
                    SetDeepShardColliders (scr, cluster.childClusters[i]);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Cluster Colliders
        /// /////////////////////////////////////////////////////////   
        
        // Set cluster colliders by shards
        public static void CollectClusterColliders (RayfireRigid scr, RFCluster cluster)
        {
            // Reset original cluster colliders list
            if (scr.physics.clusterColliders == null)
                scr.physics.clusterColliders = new List<Collider>();
            else
                scr.physics.clusterColliders.Clear();
            
            // Collect all shards colliders
            CollectDeepColliders (scr, cluster);
        }
        
        // Check children for mesh or cluster root until all children will not be checked
        static void CollectDeepColliders (RayfireRigid scr, RFCluster cluster)
        {
            // Collect shards colliders
            for (int i = 0; i < cluster.shards.Count; i++)
                scr.physics.clusterColliders.Add (cluster.shards[i].col);

            // Set child cluster colliders
            if (scr.objectType == ObjectType.NestedCluster)
                if (cluster.HasChildClusters == true)
                    for (int i = 0; i < cluster.childClusters.Count; i++)
                        CollectDeepColliders (scr, cluster.childClusters[i]);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Collider material
        /// /////////////////////////////////////////////////////////       
         
        // Set collider material
        public static void SetColliderMaterial(RayfireRigid scr)
        {
            // Set physics material if not defined by user
            if (scr.physics.material == null)
                scr.physics.material = scr.physics.PhysMaterial;
            
            // Set mesh collider material and stop
            if (scr.physics.meshCollider != null)
            {
                scr.physics.meshCollider.sharedMaterial = scr.physics.material;
                return;
            }
            
            // Set cluster colliders material
            if (scr.physics.HasClusterColliders == true)
                for (int i = 0; i < scr.physics.clusterColliders.Count; i++)
                    scr.physics.clusterColliders[i].sharedMaterial = scr.physics.material;
        }
        
        // Set shard collider material
        public static void SetColliderMaterial(RFPhysic physics, RFShard shard)
        {
            if (shard.col != null)
                shard.col.sharedMaterial = physics.material;
        }
        
        // Set debris collider material
        public static void SetParticleColliderMaterial (List<RayfireDebris> debris)
        {
            if (debris != null && debris.Count > 0)
                for (int i = 0; i < debris.Count; i++)
                    if (debris[i] != null)
                        debris[i].collision.SetMaterialProps (debris[i]);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Collider properties
        /// /////////////////////////////////////////////////////////   
        
        // Set collider convex state
        public static void SetColliderConvex(RayfireRigid scr)
        {
            if (scr.physics.meshCollider != null)
            {
                // Not Mesh collider
                if (scr.physics.meshCollider is MeshCollider == false)
                    return;
                
                // Turn on convex for non kinematic
                MeshCollider mCol = (MeshCollider)scr.physics.meshCollider;
                if (scr.physics.rigidBody.isKinematic == false)
                    mCol.convex = true;
            }
        }
        
        // EDITOR clear colliders
        public static void DestroyColliders(RayfireRigid scr)
        {
            if (scr.physics.HasClusterColliders == true)
                for (int i = scr.physics.clusterColliders.Count - 1; i >= 0; i--)
                    if (scr.physics.clusterColliders[i] != null)
                        Object.DestroyImmediate (scr.physics.clusterColliders[i], true);
        }
        
        /// /////////////////////////////////////////////////////////
        /// RigidRoot
        /// /////////////////////////////////////////////////////////
        
        // Set rigidbody simType, mass, drag, solver iterations
        public static void SetPhysics(RayfireRigidRoot root)
        {
            // Set physics properties for rigidRoot shards
            SetPhysics (root.rigidRootShards, root.physics);
            
            // Set physics properties for meshRoot shards
            for (int i = 0; i < root.meshRootShards.Count; i++)
                SetPhysics (root.meshRootShards[i], root.meshRootShards[i].rigid.physics);
        }
        
        // Set shard Rigidbody and set physics properties. Uses for RigidRoot shards
        public static void SetPhysics(List<RFShard> shards, RFPhysic physic)
        {
            // Set phys props
            float density     = RayfireMan.inst.materialPresets.Density (physic.materialType);
            float drag        = RayfireMan.inst.materialPresets.Drag (physic.materialType);
            float dragAngular = RayfireMan.inst.materialPresets.AngularDrag (physic.materialType);
            
            // Add Collider and Rigid body if has no Rigid component
            for (int i = 0; i < shards.Count; i++)
            {
                // Get rigidbody
                shards[i].rb = shards[i].tm.gameObject.GetComponent<Rigidbody>();
                
                // Set Rigid body
                if (shards[i].rb == null)
                    shards[i].rb = shards[i].tm.gameObject.AddComponent<Rigidbody>();
                
                // Set simulation
                SetSimulationType (shards[i].rb, shards[i].sm, ObjectType.Mesh, physic.useGravity, physic.solverIterations);
                
                // Set density. After collider defined
                SetDensity (shards[i], physic, density);

                // Set drag properties
                SetDrag (shards[i], drag, dragAngular);
            }
        }
        
        // Set shard Rigidbody and set physics properties. Uses for RigidRoot -> MeshRoot shards
        public static void SetPhysics(RFShard shard, RFPhysic physic)
        {
            // Get rigidbody
            shard.rb = shard.tm.gameObject.GetComponent<Rigidbody>();
            
            // Set Rigid body
            if (shard.rb == null)
                shard.rb = shard.tm.gameObject.AddComponent<Rigidbody>();
            
            // Set simulation
            SetSimulationType (shard.rb, shard.sm, ObjectType.Mesh, physic.useGravity, physic.solverIterations);
            
            // Set density. After collider defined
            SetDensity (shard, physic, RayfireMan.inst.materialPresets.Density (physic.materialType));

            // Set drag properties
            SetDrag (shard, RayfireMan.inst.materialPresets.Drag (physic.materialType), RayfireMan.inst.materialPresets.AngularDrag (physic.materialType));
        }

        /// /////////////////////////////////////////////////////////
        /// Ignore colliders
        /// /////////////////////////////////////////////////////////
        
        // Pair structure
        struct RFIgnorePair
        {
            int a;
            int b;
            public RFIgnorePair(int A, int B)
            {
                a = A;
                b = B;
            }
        }
        
        // Set ignore list
        public static void SetIgnoreColliders(RFPhysic physics, List<RayfireRigid> rigids)
        {
            //float f1 = Time.realtimeSinceStartup;
         
            // Ignore colliders enabled
            if (physics.ignoreNear == true)
            {
                // Get ignore list if has no
                if (physics.HasIgnore == false)
                {
                    // Set bounds for Editor Setup
                    if (Application.isPlaying == false)
                    {
                        for (int i = 0; i < rigids.Count; i++)
                        {
                            if (rigids[i].meshRenderer == null)
                                rigids[i].meshRenderer = rigids[i].gameObject.GetComponent<MeshRenderer>();
                            if (rigids[i].meshRenderer != null)
                                rigids[i].limitations.bound = rigids[i].meshRenderer.bounds;
                        }
                    }

                    // Collect bounds to check overlap
                    List<Bounds> bounds = new List<Bounds>();
                    for (int i = 0; i < rigids.Count; i++)
                        bounds.Add (rigids[i].limitations.bound);

                    // Get ignore list
                    physics.ignoreList = Application.isPlaying == true 
                        ? GetIgnoreListFast (bounds) 
                        : GetIgnoreListShort (bounds);
                }
                                
                // Set physics ignore pairs. Runtime only
                if (Application.isPlaying == true)
                    IgnoreNeibCollision (rigids, physics.ignoreList);
            }
            
            // Nullify if runtime
            if (Application.isPlaying == true)
                physics.ignoreList = null;

            //Debug.Log (Time.realtimeSinceStartup - f1);
        }
        
        // Set ignore list
        public static void SetIgnoreColliders(RFPhysic physics, List<RFShard> shards)
        {
            // Ignore colliders enabled
            if (physics.ignoreNear == true)
            {
                // Get ignore list if has no
                if (physics.HasIgnore == false)
                    SetIgnoreListShards (physics, shards);
                
                // Set physics ignore pairs
                if (Application.isPlaying == true)
                    IgnoreNeibCollision (shards, physics.ignoreList);
            }
            
            // Nullify if runtime
            if (Application.isPlaying == true)
                physics.ignoreList = null;
        }
        
        // Ignore collision for overlapped shards
        public static void SetIgnoreListShards(RFPhysic physics, List<RFShard> shards)
        {
            // Collect bounds to check overlap
            List<Bounds> bounds = new List<Bounds>();
            for (int i = 0; i < shards.Count; i++)
                bounds.Add (shards[i].bnd);
            
            // Get ignore list
            physics.ignoreList = Application.isPlaying == true
                ? GetIgnoreListFast (bounds)
                : GetIgnoreListShort (bounds);
        }
        
        // Ignore collision for overlapped shards
        public static List<int> GetIgnoreListFast(List<Bounds> bounds)
        {
            // Get prune list
            List<int> pruneList = new List<int>();
            for (int s = 0; s < bounds.Count; s++)
            {
                for (int n = 0; n < bounds.Count; n++)
                {
                    if (s != n)
                    {
                        // Check bound intersection
                        if (bounds[s].Intersects (bounds[n]) == true)
                        {
                            pruneList.Add (s);
                            pruneList.Add (n);
                        }
                    }
                }
            }
            return pruneList;
        }
        
        // Ignore collision for overlapped shards
        public static List<int> GetIgnoreListShort(List<Bounds> bounds)
        {
            RFIgnorePair          pair;
            HashSet<RFIgnorePair> ignorePairsHash = new HashSet<RFIgnorePair>();

            // Get prune list
            List<int> pruneList = new List<int>();
            for (int s = 0; s < bounds.Count; s++)
            {
                for (int n = 0; n < bounds.Count; n++)
                {
                    if (s != n)
                    {
                        // Check bound intersection
                        if (bounds[s].Intersects (bounds[n]) == true)
                        {
                            // Create pair
                            pair = new RFIgnorePair (s, n);

                            // Has no such pair yet
                            if (ignorePairsHash.Contains (pair) == false)
                            {
                                pruneList.Add (s);
                                pruneList.Add (n);

                                ignorePairsHash.Add (pair);
                                ignorePairsHash.Add (new RFIgnorePair (n, s));
                            }
                        }
                    }
                }
            }
            return pruneList;
        }
        
        // Ignore collision for overlapped shards
        public static void IgnoreNeibCollision(List<RayfireRigid> rigids, List<int> pr)
        {
            for (int s = 0; s < pr.Count / 2; s++)
                if (rigids[pr[s * 2 + 0]].physics.meshCollider != null && rigids[pr[s * 2 + 1]].physics.meshCollider != null)
                    Physics.IgnoreCollision (rigids[pr[s * 2 + 0]].physics.meshCollider, rigids[pr[s * 2 + 1]].physics.meshCollider, true);
        }
        
        // Ignore collision for overlapped shards
        public static void IgnoreNeibCollision(List<RFShard> shards, List<int> pr)
        {
            for (int s = 0; s < pr.Count / 2; s++)
                if (shards[pr[s * 2 + 0]].col != null && shards[pr[s * 2 + 1]].col != null)
                    Physics.IgnoreCollision (shards[pr[s * 2 + 0]].col, shards[pr[s * 2 + 1]].col, true);
        }

        /// /////////////////////////////////////////////////////////
        /// Coroutines
        /// /////////////////////////////////////////////////////////
        
        // Null check. x5 faster than == null
        public static bool IsNull(Rigidbody rb)
        {
            return ReferenceEquals(rb, null);
        }
        
        // Cache physics data for fragments 
        public IEnumerator PhysicsDataCor (RayfireRigid scr)
        {
            // Stop if running 
            if (physicsDataCorState == true)
                yield break;
            
            // Set running state
            physicsDataCorState = true;
            
            // Set velocity
            if (IsNull(scr.physics.rigidBody) == false)
                velocity = scr.physics.rigidBody.velocity;

            while (exclude == false)
            {
                if (IsNull(scr.physics.rigidBody) == false)
                    velocity = scr.physics.rigidBody.velocity;
                yield return null;
            }
   
            // Set state
            physicsDataCorState = false;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Getters
        /// /////////////////////////////////////////////////////////
        
        // Ignore list state
        public bool HasIgnore { get { return ignoreList != null && ignoreList.Count > 0; } }
        
        // Get Destructible state
        public bool HasClusterColliders
        {
            get
            {
                if (clusterColliders != null && clusterColliders.Count > 0)
                    return true;
                return false;
            }
        }
        
        // Get Destructible state
        public bool Destructible { get { return RayfireMan.inst.materialPresets.Destructible(materialType); } }

        // Get physic material
        public int Solidity { get { return RayfireMan.inst.materialPresets.Solidity(materialType); } }

        // Get physic material
        public PhysicMaterial PhysMaterial
        {
            get
            {
                // Return predefine material
                if (material != null)
                    return material;

                // Crete new material
                return RFMaterialPresets.Material(materialType);
            }
        }
    }
}
