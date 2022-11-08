using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RayFire
{
    [SelectionBase]
    [AddComponentMenu("RayFire/Rayfire Cluster")]
    [HelpURL("http://rayfirestudios.com/unity-online-help/components/unity-cluster-component/")]
    public class RayfireCluster : MonoBehaviour
    {
        // Cluster Type
        public enum ClusterType
        {
            ByPointCloud = 0,
            BySharedArea = 1
        }
        
        [Space(2)]
        [Header("  Properties")]
        [Space (2)]
        
        public ClusterType type = ClusterType.ByPointCloud;
        [Range(1, 7)] public int depth = 1;
        [Range(0, 100)] public int seed = 1;
        [Range(0, 4)] public int smoothPass = 1;

        [Header("  By Point Cloud")]
        [Space (2)]
        
        [Range(2, 100)] public int baseAmount = 5;
        [Range(2, 50)] public int depthAmount = 2;
        public ConnectivityType connectivity = ConnectivityType.ByBoundingBox;

        [Header("  By Shared Area")]
        [Space (2)]
        
        [Range(2, 8)] public int minimumAmount = 2;
        [Range(2, 8)] public int maximumAmount = 5;

        // Preview
        [HideInInspector] public bool showGizmo = true;
        [HideInInspector] public bool colorPreview = false;
        [HideInInspector] public bool scalePreview = false;
        [HideInInspector] public Color wireColor = new Color(0.58f, 0.77f, 1f);
        [HideInInspector] public float previewScale = 0f;
        [HideInInspector] public List<RFCluster> allClusters = new List<RFCluster>();
        [HideInInspector] public List<RFShard> allShards = new List<RFShard>();
        int clusterId = 0;
        
        /// /////////////////////////////////////////////////////////
        /// Clustering
        /// /////////////////////////////////////////////////////////

        // Extract all children under root
        public void Extract()
        {
            previewScale = 0f;
            allShards.Clear();
            allClusters.Clear();

            // Get all child nodes 
            List<Transform> allTm = GetComponentsInChildren<Transform>().ToList();

            // Set root as parent
            for (int i = allTm.Count - 1; i >= 0; i--)
            {
                // Get all components
                Component[] allComponents = allTm[i].GetComponents(typeof(Component));

                // Destroy if empty object with transform only
                if (allComponents.Length == 1)
                {
                    DestroyImmediate(allTm[i].gameObject);
                    allTm.RemoveAt(i);
                    continue;
                }

                // Set as parent
                allTm[i].parent = transform;
            }
        }

        // Clusterize all children
        public void Clusterize()
        {
            // Reset vars
            // soloNow = 0;

            // Extract all children first
            Extract();

            // Clear lists
            allShards.Clear();
            allClusters.Clear();

            // Clusterize by type
            ClusterizeVoronoi();

            // Clusterize by size and range
            ClusterizeRange();
        }

        /// /////////////////////////////////////////////////////////
        /// Voronoi
        /// /////////////////////////////////////////////////////////

        // Clusterize by Voronoi pc
        void ClusterizeVoronoi()
        {
            if (type == ClusterType.ByPointCloud)
            {
                // Create Base cluster
                RFCluster mainCluster = SetupMainCluster(connectivity);

                // Base amount of clusters is more than shards amount
                if (baseAmount >= mainCluster.shards.Count)
                    return;

                // Set shard neibs
                RFShard.SetShardNeibs(mainCluster.shards, connectivity);

                // List with all clusters
                List<RFCluster> clusters = new List<RFCluster> {mainCluster};

                // Collect base cluster
                allClusters.Add(mainCluster);

                // Clusterize
                while (clusters.Count > 0)
                {
                    // Get local cluster
                    RFCluster cls = clusters[0];

                    // Remove current cluster from clustering list
                    clusters.RemoveAt(0);

                    // Low amount of shards
                    if (cls.shards.Count < 4)
                        continue;

                    // Get amount
                    int amount = baseAmount;
                    if (cls.depth > 0)
                        amount = depthAmount;

                    // Get local depth roots
                    cls.childClusters = ClusterizeClusterByAmount(cls, amount);

                    // Collect new clusters
                    allClusters.AddRange(cls.childClusters);

                    // Check if local cluster should be clusterized further and add to list
                    if (cls.childClusters.Count > 0 && depth > cls.depth + 1)
                        clusters.AddRange(cls.childClusters);
                }

                // Set name to roots
                SetClusterNames();
            }
        }

        // Clusterize shards by amount
        List<RFCluster> ClusterizeClusterByAmount(RFCluster parentCluster, int amount)
        {
            // Empty list of all new cluster roots
            List<RFCluster> childClusters = new List<RFCluster>();

            // Check if root has children more than at least 2
            if (parentCluster.tm.childCount <= 2)
                return childClusters;

            // Shards are more than cluster amount
            if (amount >= parentCluster.shards.Count)
                return childClusters;

            // Get bounds for random point cloud generation
            Bounds bound = RFCluster.GetChildrenBound(parentCluster.tm);

            // Collect cluster points
            List<Vector3> voronoiPoints = VoronoiPointCloud(bound, amount);

            // Create cluster for each point
            foreach (Vector3 point in voronoiPoints)
            {
                RFCluster childCluster = new RFCluster();
                childCluster.pos = point;
                childCluster.depth = parentCluster.depth + 1;

                // Set id
                clusterId++;
                childCluster.id = clusterId;

                childClusters.Add(childCluster);
            }

            // Separate shards by closest to cluster distance
            foreach (RFShard shard in parentCluster.shards)
            {
                // Puck first cluster
                float rootDist = Vector3.Distance(shard.tm.position, childClusters[0].pos);
                float minDist = rootDist;
                shard.cluster = childClusters[0];

                // Set closest cluster
                if (childClusters.Count > 1)
                {
                    for (int i = 1; i < childClusters.Count; i++)
                    {
                        rootDist = Vector3.Distance(shard.tm.position, childClusters[i].pos);
                        if (rootDist < minDist)
                        {
                            minDist = rootDist;
                            shard.cluster = childClusters[i];
                        }
                    }
                }

                // Apply shard to closest cluster and reset
                shard.cluster.shards.Add(shard);
                shard.cluster = null;
            }

            // Check child clusters and remove empty or solo shard clusters
            List<RFShard> soloShards = new List<RFShard>();
            for (int i = childClusters.Count - 1; i >= 0; i--)
            {
                if (childClusters[i].shards.Count < 2)
                {
                    soloShards.AddRange(childClusters[i].shards);
                    childClusters.RemoveAt(i);
                }
            }

            // First pass Find neib cluster for solo shards
            SetSoloShardToCluster(soloShards, childClusters);

            //// Second pass Find neib cluster for solo shards
            SetSoloShardToCluster(soloShards, childClusters);

            // Roughness pass. Remove shards from cluster and add to another.
            if (smoothPass > 0 && connectivity == ConnectivityType.ByTriangles)
                for (int i = 0; i < smoothPass; i++)
                    RoughnessPassShards(childClusters);

            // Create clusters by connectivity check
            if (connectivity == ConnectivityType.ByTriangles)
                ConnectivityCheck(childClusters);

            // Check if only one cluster left
            if (childClusters.Count == 1)
            {
                childClusters.Clear();
                return childClusters;
            }

            // Create root for cluster at shards center and set shards as children
            foreach (RFCluster childCluster in childClusters)
                CreateRoot(childCluster, parentCluster.tm);

            return childClusters;
        }

        // Check cluster for connectivity and create new connected clusters
        void ConnectivityCheck(List<RFCluster> childClusters)
        {
            // New list for solo shards
            List<RFShard> soloShards = new List<RFShard>();
            List<RFCluster> newChildClusters = new List<RFCluster>();

            // Check every cluster for connectivity
            foreach (RFCluster childCluster in childClusters)
            {
                // Collect solo shards with no neibs
                for (int i = childCluster.shards.Count - 1; i >= 0; i--)
                    if (childCluster.shards[i].neibShards.Count == 0)
                        soloShards.Add(childCluster.shards[i]);

                // Get list of all shards to check
                List<RFShard> allShardsLoc = new List<RFShard>();
                foreach (RFShard shard in childCluster.shards)
                    allShardsLoc.Add(shard);

                // Check all shards and collect new clusters
                int shardsAmount = allShardsLoc.Count;
                List<RFCluster> newClusters = new List<RFCluster>();
                while (allShardsLoc.Count > 0)
                {
                    // List of connected shards
                    List<RFShard> newClusterShards = new List<RFShard>();

                    // List of check shards
                    List<RFShard> checkShards = new List<RFShard>();

                    // Start from first shard
                    checkShards.Add(allShardsLoc[0]);
                    newClusterShards.Add(allShardsLoc[0]);

                    // Collect by neibs
                    while (checkShards.Count > 0)
                    {
                        // Add neibs to check
                        foreach (RFShard neibShard in checkShards[0].neibShards)
                        {
                            // If neib among current cluster shards
                            if (allShardsLoc.Contains(neibShard) == true)
                            {
                                // And not already collected
                                if (newClusterShards.Contains(neibShard) == false)
                                {
                                    checkShards.Add(neibShard);
                                    newClusterShards.Add(neibShard);
                                }
                            }
                        }

                        // Remove checked
                        checkShards.RemoveAt(0);
                    }

                    // Child cluster connected
                    if (shardsAmount == newClusterShards.Count)
                        allShardsLoc.Clear();

                    // Child cluster not connected
                    else
                    {
                        // Create new cluster and add to parent
                        RFCluster newCluster = new RFCluster();
                        newCluster.pos = childCluster.pos;
                        newCluster.depth = childCluster.depth;
                        newCluster.shards = newClusterShards;

                        // Set id
                        clusterId++;
                        newCluster.id = clusterId;
                        newClusters.Add(newCluster);

                        // Remove from all shards list
                        for (int i = allShardsLoc.Count - 1; i >= 0; i--)
                            if (newClusterShards.Contains(allShardsLoc[i]) == true)
                                allShardsLoc.RemoveAt(i);
                    }
                }

                // Non connectivity. Remove original cluster
                if (newClusters.Count > 0)
                {
                    childCluster.shards.Clear();
                    newChildClusters.AddRange(newClusters);
                }
            }

            // Clear empty clusters
            for (int i = childClusters.Count - 1; i >= 0; i--)
                if (childClusters[i].shards.Count == 0)
                    childClusters.RemoveAt(i);

            // Collect new clusters
            childClusters.AddRange(newChildClusters);

            // Set clusters neib info
            RFCluster.SetClusterNeib(childClusters, true);

            // Second pass Find neib cluster for solo shards
            SetSoloShardToCluster(soloShards, childClusters);

            // Roughness pass. Remove shards from cluster and add to another.
            if (smoothPass > 0)
                RoughnessPassShards(childClusters);
        }

        /// /////////////////////////////////////////////////////////
        /// By range
        /// /////////////////////////////////////////////////////////

        // Second clustering type
        void ClusterizeRange()
        {
            if (type == ClusterType.BySharedArea)
            {
                Random.InitState(seed);

                // Create Base cluster and collect
                RFCluster mainCluster = SetupMainCluster(ConnectivityType.ByTriangles);
                allClusters.Add(mainCluster);

                // Set shard neibs
                RFShard.SetShardNeibs(mainCluster.shards, ConnectivityType.ByTriangles);

                // Clusterize base shards to clusters
                List<RFCluster> childClusters = ClusterizeRangeShards(mainCluster);

                // Create root and set shards and children
                foreach (RFCluster childCluster in childClusters)
                    CreateRoot(childCluster, transform);

                // Add to all clusters
                allClusters.AddRange(childClusters);

                // Clusterize clusters in depth
                if (depth > 1)
                {
                    for (int i = 1; i < depth; i++)
                    {
                        // Set clusters neib info
                        RFCluster.SetClusterNeib(mainCluster.childClusters, true);

                        // Get new depth clusters
                        List<RFCluster> newClusters = ClusterizeRangeClusters(mainCluster);

                        if (newClusters.Count > 1)
                        {
                            // Create root for all new clusters and set as parent for them
                            foreach (RFCluster cls in newClusters)
                            {
                                CreateRoot(cls, mainCluster.tm);
                                foreach (RFCluster childCLuster in cls.childClusters)
                                    childCLuster.tm.parent = cls.tm;
                            }

                            // Set as child cluster for main cluster to be clusterized at next pass
                            mainCluster.childClusters = newClusters;

                            // Add to all clusters
                            allClusters.AddRange(newClusters);

                            // Get all nested clusters and increment depth
                            foreach (RFCluster cls in allClusters)
                                if (cls.id != 0)
                                    cls.depth += 1;
                        }
                    }
                }

                // Set name to roots
                SetClusterNames();
            }
        }

        // Base clustering pass for shards
        List<RFCluster> ClusterizeRangeShards(RFCluster mainCluster)
        {
            // Empty list of all new cluster roots
            List<RFShard> soloShards = new List<RFShard>();

            // List with all clusters
            List<RFCluster> childClusters = new List<RFCluster>();

            // Sort from smallest to biggest
            mainCluster.shards.Sort();

            // Clusterize starting from biggest
            while (mainCluster.shards.Count > 0)
            {
                // Local amount of shards in cluster
                int shardsAmount = Random.Range(minimumAmount, maximumAmount);

                // Start from biggest shard
                RFShard startShard = mainCluster.shards[0];

                // Remove from lists
                mainCluster.shards.RemoveAt(0);

                // Starting shard list
                List<RFShard> shardGroup = new List<RFShard>();
                shardGroup.Add(startShard);

                // Find neibs
                for (int s = 0; s < shardsAmount - 1; s++)
                {
                    // Get neib shard among cluster.shards with biggest shared area
                    RFShard biggestShard = GetNeibShardArea(shardGroup, mainCluster.shards);

                    // No neib with shared area
                    if (biggestShard == null)
                        break;

                    // TODO check if area is much smaller than with another neibs. Set as solo

                    // Add in group
                    shardGroup.Add(biggestShard);

                    // Remove from cluster.shards
                    mainCluster.shards.RemoveAll(t => t.id == biggestShard.id);
                }

                // Solo shard
                if (shardGroup.Count == 1)
                    soloShards.Add(startShard);

                // Group of shards for cluster
                else if (shardGroup.Count > 1)
                {
                    // Clusterize with picked shard
                    RFCluster childCluster = new RFCluster();
                    childCluster.shards.AddRange(shardGroup);
                    childCluster.depth = 1;

                    // Set id
                    clusterId++;
                    childCluster.id = clusterId;

                    // Collect luster
                    childClusters.Add(childCluster);
                    mainCluster.childClusters.Add(childCluster);
                }
            }

            // First pass Find neib cluster for solo shards
            SetSoloShardToCluster(soloShards, childClusters);

            // Second pass Find neib cluster for solo shards
            SetSoloShardToCluster(soloShards, childClusters);

            // Roughness pass. Remove shards from cluster and add to another.
            if (smoothPass > 0)
                for (int i = 0; i < smoothPass; i++)
                    RoughnessPassShards(childClusters);

            // TODO consider solo amount

            // Set id 
            int startId = 1;
            for (int i = 0; i < childClusters.Count; i++)
                childClusters[i].id = startId + i;

            // Set main cluster solo shards back to main cluster
            mainCluster.shards.Clear();
            mainCluster.shards.AddRange(soloShards);

            return childClusters;
        }

        // Clustering pass for clusters
        List<RFCluster> ClusterizeRangeClusters(RFCluster parentCluster)
        {
            // Empty list of all new solo clusters
            List<RFCluster> soloClusters = new List<RFCluster>();

            // List with all new clusters
            List<RFCluster> newClusters = new List<RFCluster>();

            // Sort from smallest to biggest
            parentCluster.childClusters.Sort();

            // Clusterize starting from biggest
            while (parentCluster.childClusters.Count > 0)
            {
                // Local amount of shards in cluster
                int clustersAmount = Random.Range(minimumAmount, maximumAmount);

                // Start from biggest cluster
                RFCluster startCluster = parentCluster.childClusters[0];

                // Remove from lists
                parentCluster.childClusters.RemoveAt(0);

                // Starting list
                List<RFCluster> clusterGroup = new List<RFCluster>();
                clusterGroup.Add(startCluster);
                for (int s = 0; s < clustersAmount - 1; s++)
                {
                    // Get neib cluster among cluster with biggest shared area
                    RFCluster biggestCluster = RFCluster.GetNeibClusterArea(clusterGroup, parentCluster.childClusters);

                    // No neib with shared area
                    if (biggestCluster == null)
                        break;

                    // Add in group
                    clusterGroup.Add(biggestCluster);

                    // Remove from mainCluster.childClusters
                    parentCluster.childClusters.RemoveAll(t => t.id == biggestCluster.id);
                }

                // Solo
                if (clusterGroup.Count == 1)
                    soloClusters.Add(startCluster);

                // Group of clusters. Creat parent clusters for them
                else
                {
                    // Clusterize with picked clusters
                    RFCluster newCluster = new RFCluster();
                    newCluster.childClusters.AddRange(clusterGroup);

                    // Set depth
                    newCluster.depth = 0;

                    // Set id
                    clusterId++;
                    newCluster.id = clusterId;

                    // Collect luster
                    newClusters.Add(newCluster);
                }
            }

            // Attach solo clusters to neib clusters
            SetSoloClusterToCluster(soloClusters, newClusters);

            // Attach solo clusters to neib clusters
            SetSoloClusterToCluster(soloClusters, newClusters);

            // Roughness pass. Remove shards from cluster and add to another.
            if (smoothPass > 0)
                for (int i = 0; i < smoothPass; i++)
                    RoughnessPassClusters(newClusters);

            return newClusters;
        }

        // Roughness pass. Remove shards from cluster and add to another.
        static void RoughnessPassShards(List<RFCluster> clusters)
        {
            // Set clusters neib info
            RFCluster.SetClusterNeib(clusters, true);

            // Check cluster for shard with one neib among cluster shards
            for (int s = clusters.Count - 1; s >= 0; s--)
            {
                RFCluster cluster = clusters[s];

                // Skip clusters with 2 shards
                if (cluster.shards.Count == 2)
                    continue;

                // Skip clusters without neib clusters
                if (cluster.neibClusters.Count == 0)
                    continue;

                // Collect shards to exclude from cluster
                List<RFShard> excludeShards = new List<RFShard>();
                List<RFCluster> attachToClusters = new List<RFCluster>();

                // Check all shards and compare area with own cluster and neib clusters
                foreach (RFShard shard in cluster.shards)
                {
                    // Get amount of neibs among cluster shards
                    float areaInCluster = 0f;
                    for (int i = 0; i < shard.neibShards.Count; i++)
                        if (cluster.shards.Contains(shard.neibShards[i]) == true)
                            areaInCluster += shard.nArea[i];

                    // Compare with amount of shards from neib clusters
                    List<float> neibAreaList = new List<float>();
                    foreach (RFCluster neibCluster in cluster.neibClusters)
                    {
                        float areaInNeibCluster = 0f;
                        for (int i = 0; i < shard.neibShards.Count; i++)
                            if (neibCluster.shards.Contains(shard.neibShards[i]) == true)
                                areaInNeibCluster += shard.nArea[i];
                        neibAreaList.Add(areaInNeibCluster);
                    }

                    // Get maximum neibs in neib cluster
                    float maxArea = neibAreaList.Max();

                    // Skip shard because neib clusters has less neib shards
                    if (areaInCluster >= maxArea)
                        continue;

                    // Collect cluster which has more neibs for shard than own cluster
                    for (int i = 0; i < neibAreaList.Count; i++)
                    {
                        if (maxArea == neibAreaList[i])
                        {
                            excludeShards.Add(shard);
                            attachToClusters.Add(cluster.neibClusters[i]);
                        }
                    }
                }

                // Reorder shards
                if (excludeShards.Count > 0)
                {
                    for (int i = 0; i < excludeShards.Count; i++)
                    {
                        // Exclude from own cluster
                        for (int c = cluster.shards.Count - 1; c >= 0; c--)
                            if (cluster.shards[c] == excludeShards[i])
                                cluster.shards.RemoveAt(c);

                        // Add to neib cluster
                        attachToClusters[i].shards.Add(excludeShards[i]);
                    }
                }
            }

            // Remove empty and solo clusters
            for (int i = clusters.Count - 1; i >= 0; i--)
            {
                // Remove solo shard
                if (clusters[i].shards.Count == 1)
                {
                    clusters[i].shards.Clear();
                }

                // Remove empty cluster
                if (clusters[i].shards.Count == 0)
                    clusters.RemoveAt(i);
            }
        }

        // Roughness pass. Remove shards from cluster and add to another.
        void RoughnessPassClusters(List<RFCluster> clusters)
        {
            // Set clusters neib info
            RFCluster.SetClusterNeib(clusters, true);

            // Check cluster for shard with one neib among cluster shards
            foreach (RFCluster bigCluster in clusters)
            {
                // Skip clusters with 2 child clusters
                if (bigCluster.childClusters.Count <= 2)
                    continue;

                // Skip clusters without neib clusters
                if (bigCluster.neibClusters.Count == 0)
                    continue;

                // Collect shards to exclude from cluster
                List<RFCluster> excludeClusters = new List<RFCluster>();
                List<RFCluster> attachToClusters = new List<RFCluster>();

                foreach (RFCluster childCluster in bigCluster.childClusters)
                {
                    // Get amount of neibs among cluster child clusters
                    float areaInCluster = 0f;
                    for (int i = 0; i < childCluster.neibClusters.Count; i++)
                        if (bigCluster.childClusters.Contains(childCluster.neibClusters[i]) == true)
                            areaInCluster += childCluster.neibArea[i];

                    // Compare with amount of shards from neib clusters
                    List<float> neibAreaList = new List<float>();
                    foreach (RFCluster bigNeibCluster in bigCluster.neibClusters)
                    {
                        float areaInNeibCluster = 0f;
                        for (int i = 0; i < childCluster.neibClusters.Count; i++)
                            if (bigNeibCluster.childClusters.Contains(childCluster.neibClusters[i]) == true)
                                areaInNeibCluster += childCluster.neibArea[i];
                        neibAreaList.Add(areaInNeibCluster);
                    }

                    // Get maximum neibs in neib cluster
                    float maxArea = neibAreaList.Max();

                    // Skip shard because neib clusters has less neib shards
                    if (areaInCluster >= maxArea)
                        continue;

                    // Collect cluster which has more neibs for shard than own cluster
                    for (int i = 0; i < neibAreaList.Count; i++)
                    {
                        if (maxArea == neibAreaList[i])
                        {
                            excludeClusters.Add(childCluster);
                            attachToClusters.Add(bigCluster.neibClusters[i]);
                        }
                    }
                }

                // Skip if cluster may loose all shards
                if (excludeClusters.Count + 1 >= bigCluster.childClusters.Count)
                    continue;

                // Reorder shards
                if (excludeClusters.Count > 0)
                {
                    for (int i = 0; i < excludeClusters.Count; i++)
                    {
                        // Exclude from own cluster
                        for (int s = bigCluster.shards.Count - 1; s >= 0; s--)
                            if (bigCluster.childClusters[s] == excludeClusters[i])
                                bigCluster.childClusters.RemoveAt(s);

                        // Add to neib cluster
                        attachToClusters[i].childClusters.Add(excludeClusters[i]);
                    }
                }
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////
        
        // Add solo shards to closest cluster
        void SetSoloShardToCluster(List<RFShard> soloShards, List<RFCluster> childClusters)
        {
            // No solo shards
            if (soloShards.Count == 0)
                return;

            // Find neib cluster for solo shards
            for (int i = soloShards.Count - 1; i >= 0; i--)
            {
                int ind = GetNeibIndArea(soloShards[i]);
                if (ind >= 0)
                {
                    RFShard neibShard = soloShards[i].neibShards[ind];
                    for (int c = 0; c < childClusters.Count; c++)
                    {
                        if (childClusters[c].shards.Contains(neibShard) == true)
                        {
                            childClusters[c].shards.Add(soloShards[i]);
                            soloShards.RemoveAt(i);
                            continue;
                        }
                    }
                }
            }
        }
        
        // Get neib index with biggest shared area
        int GetNeibIndArea(RFShard shard, List<RFShard> shardList = null)
        {
            // Get neib index with biggest shared area
            float biggestArea = 0f;
            int   neibInd     = 0;
            for (int i = 0; i < shard.neibShards.Count; i++)
            {
                // Skip if check neib shard not in filter list
                if (shardList != null)
                    if (shardList.Contains(shard.neibShards[i]) == false)
                        continue;

                // Remember if bigger
                if (shard.nArea[i] > biggestArea)
                {
                    biggestArea = shard.nArea[i];
                    neibInd     = i;
                }
            }

            // Return index of neib with biggest shared area
            if (biggestArea > 0)
                return neibInd;

            // No neib
            return -1;
        }

        // Add solo shards to closest cluster
        void SetSoloClusterToCluster(List<RFCluster> soloClusters, List<RFCluster> childClusters)
        {
            // No solo clusters
            if (soloClusters.Count == 0)
                return;

            // Find neib cluster for solo cluster
            for (int i = soloClusters.Count - 1; i >= 0; i--)
            {
                int ind = soloClusters[i].GetNeibIndArea();
                if (ind >= 0)
                {
                    RFCluster neibCluster = soloClusters[i].neibClusters[ind];
                    for (int c = 0; c < childClusters.Count; c++)
                    {
                        if (childClusters[c].childClusters.Contains(neibCluster) == true)
                        {
                            childClusters[c].childClusters.Add(soloClusters[i]);
                            soloClusters.RemoveAt(i);
                            continue;
                        }
                    }
                }
            }
        }
        
        // Set up main cluster and set shards
        RFCluster SetupMainCluster (ConnectivityType connect)
        {
            // Create Base cluster
            RFCluster cluster = new RFCluster();

            cluster.tm = transform;
            cluster.depth = 0;
            cluster.pos = transform.position;

            // Set cluster id
            cluster.id = 0;

            // Set shards for main cluster
            RFShard.SetShards(cluster, connectivity);
            
            clusterId = 0;

            // Collect all shards
            allShards.Clear();
            allShards.AddRange(cluster.shards);

            // TODO set bound

            return cluster;
        }

        // Set name to roots
        void SetClusterNames()
        {
            foreach (RFCluster cls in allClusters)
                if (cls.id > 0)
                    if (cls.tm != null)
                        cls.tm.name = gameObject.name + "_cls_" + cls.id;
        }

        // Create root for cluster at shards center and set shards as children
        void CreateRoot(RFCluster childCluster, Transform parentTm)
        {
            // Get cluster bound
            Bounds childBound = GetShardsBound(childCluster.shards, childCluster.childClusters);

            // Set cluster bound
            childCluster.bound = childBound;

            // Create root for cluster
            GameObject childRoot = new GameObject();

            // Set cluster root position
            childCluster.tm = childRoot.transform;
            childCluster.pos = childBound.center;
            childCluster.tm.position = childBound.center;

            // Set cluster parent
            childCluster.tm.parent = parentTm;

            // Set cluster root as shards parent
            foreach (RFShard shard in childCluster.shards)
                shard.tm.parent = childCluster.tm;
        }

        /// /////////////////////////////////////////////////////////
        /// Bounds
        /// /////////////////////////////////////////////////////////

        // Get bound for list of shards
        Bounds GetShardsBound(List<RFShard> shards, List<RFCluster> clusters = null)
        {
            // Get list of bounds
            List<Bounds> bounds = new List<Bounds>();

            // Consider shards bounds
            foreach (RFShard shard in shards)
                bounds.Add(shard.bnd);

            // Consider clusters bounds
            if (clusters != null)
                foreach (RFCluster cluster in clusters)
                    bounds.Add(cluster.bound);

            return RFCluster.GetBoundsBound(bounds);
        }

        // Get neib shard from shardList which is neib to one of the shards
        static RFShard GetNeibShardArea(List<RFShard> shardGroup, List<RFShard> shardList)
        {
            // No shards to pick
            if (shardList.Count == 0)
                return null;

            // Get all neibs for shards, exclude neibs not from shardList
            List<RFShard> allNeibs = new List<RFShard>();

            // Biggest area
            float   biggestArea  = 0f;
            RFShard biggestShard = null;

            // Check shard
            foreach (RFShard shard in shardGroup)
            {
                // Check neibs
                for (int i = 0; i < shard.neibShards.Count; i++)
                {
                    // Neib shard has shared area lower than already founded 
                    if (biggestArea >= shard.nArea[i])
                        continue;

                    // Neib already in neib list
                    if (allNeibs.Contains(shard.neibShards[i]) == true)
                        continue;

                    // Neib not among allowed shards
                    if (shardList.Contains(shard.neibShards[i]) == false)
                        continue;

                    // Remember neib
                    allNeibs.Add(shard.neibShards[i]);
                    biggestArea  = shard.nArea[i];
                    biggestShard = shard.neibShards[i];
                }
            }
            allNeibs = null;

            // Pick shard with biggest area
            return biggestShard;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Point cloud
        /// /////////////////////////////////////////////////////////

        // Generate random point3 cloud by bound and amount
        List<Vector3> VoronoiPointCloud(Bounds bound, int am)
        {
            Random.InitState(seed + clusterId);
            List<Vector3> points = new List<Vector3>();
            for (int i = 0; i < am; i++)
            {
                float randomX = Random.Range(bound.min.x, bound.max.x);
                float randomY = Random.Range(bound.min.y, bound.max.y);
                float randomZ = Random.Range(bound.min.z, bound.max.z);
                Vector3 randomPoint = new Vector3(randomX, randomY, randomZ);
                points.Add(randomPoint);
            }

            return points;
        }
    }
}