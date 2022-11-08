using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RayFire
{
    [AddComponentMenu ("RayFire/Rayfire Unyielding")]
    [HelpURL ("https://rayfirestudios.com/unity-online-help/components/unity-unyielding-component/")]
    public class RayfireUnyielding : MonoBehaviour
    {
        public enum UnySimType
        {
            Original  = 1,
            Inactive  = 2, 
            Kinematic = 3
        }
        
        public bool       unyielding     = true;
        public bool       activatable    = false;
        public UnySimType simulationType = UnySimType.Original;
        public Vector3    centerPosition;
        public Vector3    size = new Vector3 (1f, 1f, 1f);
        
        // Hidden
        public RayfireRigid        rigidHost;
        public List<RayfireRigid>  rigidList;
        public List<RFShard>       shardList;
        public bool                showGizmo = true;
        public bool                showCenter;
        
        /// /////////////////////////////////////////////////////////
        /// Connected Cluster setup
        /// /////////////////////////////////////////////////////////
        
        // Set clusterized rigids uny state and mesh root rigids
        public static void ClusterSetup (RayfireRigid rigid)
        {
            if (rigid.simulationType == SimType.Inactive || rigid.simulationType == SimType.Kinematic)
            {
                RayfireUnyielding[] unyArray =  rigid.GetComponents<RayfireUnyielding>();
                for (int i = 0; i < unyArray.Length; i++)
                    if (unyArray[i].enabled == true)
                    {
                        unyArray[i].rigidHost = rigid;
                        ClusterOverlap (unyArray[i]);
                    }
            }
        }
        
        // Set uny state for mesh root rigids. Used by Mesh Root. Can be used for cluster shards
        static void ClusterOverlap (RayfireUnyielding uny)
        {
            // Get target mask and overlap colliders
            int               finalMask     = ClusterLayerMask(uny.rigidHost);
            Collider[]        colliders     = Physics.OverlapBox (uny.transform.TransformPoint (uny.centerPosition), uny.Extents, uny.transform.rotation, finalMask);
            HashSet<Collider> collidersHash = new HashSet<Collider> (colliders);
            
            // Check with connected cluster
            uny.shardList = new List<RFShard>();
            if (uny.rigidHost.objectType == ObjectType.ConnectedCluster)
                for (int i = 0; i < uny.rigidHost.physics.clusterColliders.Count; i++)
                    if (uny.rigidHost.physics.clusterColliders[i] != null)
                        if (collidersHash.Contains (uny.rigidHost.physics.clusterColliders[i]) == true)
                        {
                            SetShardUnyState (uny.rigidHost.clusterDemolition.cluster.shards[i], uny.unyielding, uny.activatable);
                            uny.shardList.Add (uny.rigidHost.clusterDemolition.cluster.shards[i]);
                        }
        }
        
        // Get combined layer mask
        static int ClusterLayerMask(RayfireRigid rigid)
        {
            int mask = 0;
            if (rigid.objectType == ObjectType.ConnectedCluster)
                for (int i = 0; i < rigid.physics.clusterColliders.Count; i++)
                    if (rigid.physics.clusterColliders[i] != null)
                        mask = mask | 1 << rigid.clusterDemolition.cluster.shards[i].tm.gameObject.layer;
            return mask;
        }
        
        // Set unyielding state
        static void SetShardUnyState (RFShard shard, bool unyielding, bool activatable)
        {
            shard.uny = unyielding;
            shard.act = activatable;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Mesh Root setup
        /// /////////////////////////////////////////////////////////
        
        // Set clusterized rigids uny state and mesh root rigids
        public static void MeshRootSetup (RayfireRigid mRoot)
        {
            // Get uny list
            List<RayfireUnyielding> unyList = GetUnyList (mRoot.transform);
            
            // Iterate every unyielding component
            for (int i = 0; i < unyList.Count; i++)
                SetMeshRootUnyRigidList (mRoot, unyList[i]);
            
            // Set rigid list uny and sim states 
            SetMeshRootUny (mRoot.transform, unyList);
        }
        
        // Get uny list
        static List<RayfireUnyielding> GetUnyList (Transform tm)
        {
            List<RayfireUnyielding> unyList = tm.GetComponents<RayfireUnyielding>().ToList();
            for (int i = unyList.Count - 1; i >= 0; i--)
                if (unyList[i].enabled == false)
                    unyList.RemoveAt (i);
            return unyList;
        }
        
        // Set uny state for mesh root rigids. Used by Mesh Root. Can be used for cluster shards
        static void SetMeshRootUnyRigidList (RayfireRigid mRoot, RayfireUnyielding uny)
        {
            // Get target mask
            int               finalMask     = MeshRootLayerMask(mRoot);
            Collider[]        colliders     = Physics.OverlapBox (uny.transform.TransformPoint (uny.centerPosition), uny.Extents, uny.transform.rotation, finalMask);
            HashSet<Collider> collidersHash = new HashSet<Collider> (colliders);
            
            // Check with connectivity rigids
            uny.rigidList = new List<RayfireRigid>();
            for (int i = 0; i < mRoot.fragments.Count; i++)
                if (mRoot.fragments[i].physics.meshCollider != null)
                    if (collidersHash.Contains (mRoot.fragments[i].physics.meshCollider) == true)
                        uny.rigidList.Add (mRoot.fragments[i]);
        }
        
        // Get combined layer mask
        static int MeshRootLayerMask(RayfireRigid mRoot)
        {
            int mask = 0;
            for (int i = 0; i < mRoot.fragments.Count; i++)
                if (mRoot.fragments[i].physics.meshCollider != null)
                    mask = mask | 1 << mRoot.fragments[i].gameObject.layer;
            return mask;
        }
        
        // Set rigid list uny and sim states 
        public static void SetMeshRootUny (Transform tm, List<RayfireUnyielding> unyList)
        {
            // Get uny list
            if (unyList == null)
                unyList = GetUnyList (tm);
            
            // Iterate uny components list
            for (int c = 0; c < unyList.Count; c++)
            {
                // No rigids
                if (unyList[c].rigidList.Count == 0)
                    continue;

                // Set uny and act states for Rigids
                SetRigidUnyState (unyList[c]);
                
                // Set simulation type by
                SetRigidUnySim (unyList[c]);
            }
        }
        
        // Set unyielding state
        static void SetRigidUnyState (RayfireUnyielding uny)
        {
            // Common ops> Editor and Runtime
            for (int i = 0; i < uny.rigidList.Count; i++)
            {
                uny.rigidList[i].activation.unyielding  = uny.unyielding;
                uny.rigidList[i].activation.activatable = uny.activatable;
            }

            // Runtime ops.
            if (Application.isPlaying == true)
            {
                for (int i = 0; i < uny.rigidList.Count; i++)
                {
                    // Stop velocity and offset activation coroutines for not activatable uny objects 
                    if (uny.unyielding == true && uny.activatable == false)
                    {
                        if (uny.rigidList[i].activation.velocityEnum != null )
                            uny.rigidList[i].StopCoroutine (uny.rigidList[i].activation.velocityEnum);
                        if (uny.rigidList[i].activation.offsetEnum != null )
                            uny.rigidList[i].StopCoroutine (uny.rigidList[i].activation.offsetEnum);
                    }
                }
            }
        }
        
        
        
        // Set unyielding rigids sim type by
        static void SetRigidUnySim (RayfireUnyielding uny)
        {
            if (Application.isPlaying == true && uny.simulationType != UnySimType.Original)
                for (int i = 0; i < uny.rigidList.Count; i++)
                {
                    uny.rigidList[i].simulationType = (SimType)uny.simulationType;
                    RFPhysic.SetSimulationType (uny.rigidList[i].physics.rigidBody, uny.rigidList[i].simulationType,
                        ObjectType.Mesh, uny.rigidList[i].physics.useGravity, uny.rigidList[i].physics.solverIterations);
                }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Runtime Fragments
        /// /////////////////////////////////////////////////////////
        
        // Check for overlap with mesh Rigid
        public static void  SetUnyieldingFragments (RayfireRigid scr)
        {
            // Only inactive and kinematic
            if (scr.simulationType != SimType.Inactive && scr.simulationType != SimType.Kinematic)
                return;

            // No fragments
            if (scr.HasFragments == false)
                return;

            // TODO collect layer mask by all layers -> int finalMask = RayfireUnyielding.ClusterLayerMask(scr);
            int layerMask = 1 << scr.fragments[0].gameObject.layer;
            
            // Overlapped objects: Copy uny, stay kinematic
            List<RayfireUnyielding> unyList   = scr.GetComponents<RayfireUnyielding>().ToList();
            
            // Get all overlapped fragments
            foreach (RayfireUnyielding uny in unyList)
            {
                // Get box overlap colliders
                Collider[]        colliders     = Physics.OverlapBox (uny.transform.TransformPoint (uny.centerPosition), uny.Extents, uny.transform.rotation, layerMask);
                HashSet<Collider> collidersHash = new HashSet<Collider> (colliders);
                
                // Activate if do not overlap
                for (int i = 0; i < scr.fragments.Count; i++)
                {
                    // Activate not overlapped and copy to overlapped
                    if (collidersHash.Contains (scr.fragments[i].physics.meshCollider) == true)
                    {
                        // Copy overlap uny to overlapped object
                        SetRigidUnyState (scr.fragments[i], scr.activation.unyielding, scr.activation.activatable);
                        
                        // Set simulation type
                        if (uny.simulationType == UnySimType.Original)
                            scr.fragments[i].simulationType = scr.simulationType;
                        else
                            scr.fragments[i].simulationType = (SimType)uny.simulationType;
                        
                        // Set simulation type
                        RFPhysic.SetSimulationType (scr.fragments[i].physics.rigidBody, scr.fragments[i].simulationType, scr.fragments[i].objectType, scr.fragments[i].physics.useGravity, scr.fragments[i].physics.solverIterations);
                        
                        // Copy rigid to overlapped fragments for further slices.
                        CopyUny (uny, scr.fragments[i].gameObject);
                        
                        // Start inactive coroutines
                        scr.fragments[i].InactiveCors();
                    } 
                }
            }
        }
        
        // Set unyielding state
        static void SetRigidUnyState (RayfireRigid rigid, bool uny, bool act)
        {
            rigid.activation.unyielding  = uny;
            rigid.activation.activatable = act;
            
            // Stop velocity and offset activation coroutines for not activatable uny objects 
            if (uny == true && act == false)
            {
                if (rigid.activation.velocityEnum != null)
                    rigid.StopCoroutine (rigid.activation.velocityEnum);
                if (rigid.activation.offsetEnum != null)
                    rigid.StopCoroutine (rigid.activation.offsetEnum);
            }
        }
        
        // Copy unyielding component
        static void CopyUny (RayfireUnyielding source, GameObject target)
        {
            RayfireUnyielding newUny = target.AddComponent<RayfireUnyielding>();

            // Copy position
            Vector3 globalCenter = source.transform.TransformPoint (source.centerPosition);
            newUny.centerPosition = newUny.transform.InverseTransformPoint (globalCenter);

            // Copy size
            newUny.size   =  source.size;
            newUny.size.x *= source.transform.localScale.x;
            newUny.size.y *= source.transform.localScale.y;
            newUny.size.z *= source.transform.localScale.z;

            // Copy properties
            newUny.simulationType = source.simulationType;
            newUny.unyielding     = source.unyielding;
            newUny.activatable    = source.activatable;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Rigid Root Setup
        /// /////////////////////////////////////////////////////////
        
        // Set uny state for mesh root rigids. Used by Mesh Root. Can be used for cluster shards
        public void GetRigidRootUnyShardList(RayfireRigidRoot rigidRoot)
        {
            // Uny disabled
            if (enabled == false)
                return;

            // Get target mask TODO check fragments layer
            int mask = 0;
            
            // Check with rigid root shards colliders
            for (int i = 0; i < rigidRoot.cluster.shards.Count; i++)
                if (rigidRoot.cluster.shards[i].col != null)
                    mask = mask | 1 << rigidRoot.cluster.shards[i].tm.gameObject.layer;
                            
            // Get box overlap colliders
            Collider[]        colliders     = Physics.OverlapBox (transform.TransformPoint (centerPosition), Extents, transform.rotation, mask);
            HashSet<Collider> collidersHash = new HashSet<Collider> (colliders);

            // Check with rigid root shards colliders
            shardList = new List<RFShard>();
            for (int i = 0; i < rigidRoot.cluster.shards.Count; i++)
                if (rigidRoot.cluster.shards[i].col != null)
                    if (collidersHash.Contains (rigidRoot.cluster.shards[i].col) == true)
                        shardList.Add (rigidRoot.cluster.shards[i]);
        }
        
        // Set sim amd uny states for cached shards
        public void SetRigidRootUnyShardList()
        {
            // No shards
            if (shardList.Count == 0)
                return;
            
            // Iterate cached shards
            for (int i = 0; i < shardList.Count; i++)
            {
                // Set uny states
                shardList[i].uny = unyielding;
                shardList[i].act = activatable;
                
                // Set sim states
                if (simulationType != UnySimType.Original)
                    shardList[i].sm = (SimType)simulationType;
            }

            // TODO Stop velocity and offset activation coroutines for not activatable uny objects (copy from above)
        }
        
        /// /////////////////////////////////////////////////////////
        /// Activate
        /// /////////////////////////////////////////////////////////
        
        // Activate inactive\kinematic shards/fragments
        public void Activate()
        {
            // Activate all rigids, init connectivity check after last activation, nullify connectivity for every
            if (HasRigids == true)
            {
                for (int i = 0; i < rigidList.Count; i++)
                {
                    // Activate if activatable
                    if (rigidList[i].activation.activatable == true)
                    {
                        rigidList[i].Activate (i == rigidList.Count - 1);
                        rigidList[i].activation.connect = null;
                    }
                }
            }

            // Activate connected clusters shards
            if (HasShards == true)
            {
                // Collect shards colliders
                List<Collider> colliders = new List<Collider>();
                for (int i = 0; i < shardList.Count; i++)
                    if (shardList[i].col != null)
                        colliders.Add (shardList[i].col);

                // No colliders
                if (colliders.Count == 0)
                    return;
                
                // Get Unyielding shards
                List<RFShard> shards = RFDemolitionCluster.DemolishConnectedCluster (rigidHost, colliders.ToArray());

                // Activate
                if (shards != null && shards.Count > 0)
                    for (int i = 0; i < shards.Count; i++)
                        RFActivation.ActivateShard (shards[i], null);
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Getters
        /// /////////////////////////////////////////////////////////
        
        // Had child cluster
        bool HasRigids { get { return rigidList != null && rigidList.Count > 0; } }
        bool HasShards { get { return shardList != null && shardList.Count > 0; } }
        
        // Get final extents
        public Vector3 Extents
        {
            get
            {
                Vector3 ext = size / 2f;
                ext.x *= transform.lossyScale.x;
                ext.y *= transform.lossyScale.y;
                ext.z *= transform.lossyScale.z;
                return ext;
            }
        }
    }
}