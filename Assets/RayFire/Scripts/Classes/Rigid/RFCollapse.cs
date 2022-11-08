using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RayFire
{
    [Serializable]
    public class RFCollapse
    {
        public enum RFCollapseType
        {
            ByArea = 1,
            BySize = 3,
            Random = 5
        }

        [Tooltip (" By Area: Shard will loose it's connections if it's shared area surface is less then defined value.\n" + 
                  " By Size: Shard will loose it's connections if it's Size is less then defined value.\n" + 
                  " Random: Shard will loose it's connections if it's random value in range from 0 to 100 is less then defined value.")]
        public RFCollapseType type;
        public int start;
        public int end;
        public int steps;
        public float duration;

        [NonSerialized] public bool inProgress;

        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////
        
        // Constructor
        public RFCollapse()
        {
            type     = RFCollapseType.ByArea;
            start    = 0;
            end      = 75;
            steps    = 10;
            duration = 15f;
        }

        /// /////////////////////////////////////////////////////////
        /// Rigid Collapse
        /// /////////////////////////////////////////////////////////

        // Start collapse
        public static void StartCollapse(RayfireRigid scr)
        {
            // Not initialized
            if (scr.initialized == false)
                return;
            
            // Already running
            if (scr.clusterDemolition.collapse.inProgress == true)
                return;
            
            // Not enough shards
            if (scr.clusterDemolition.cluster.shards.Count <= 1)
                return;
            
            scr.StartCoroutine(scr.clusterDemolition.collapse.CollapseCor (scr));
        }

        // Start collapse coroutine
        IEnumerator CollapseCor (RayfireRigid scr)
        {
            // Wait time
            WaitForSeconds wait = new WaitForSeconds (duration/steps);

            // Set state
            inProgress = true;
            
            // Iterate collapse
            float step = (end - start) / (float)steps;
            for (int i = 0; i < steps; i++)
            {
                // Stop
                if (inProgress == false)
                    break;
                
                float percentage = start + step * i;
                if (type == RFCollapseType.ByArea)
                    AreaCollapse (scr, (int)percentage);
                else if (type == RFCollapseType.BySize)
                    SizeCollapse (scr, (int)percentage);
                else if (type == RFCollapseType.Random)
                    RandomCollapse (scr, (int)percentage, scr.clusterDemolition.seed);
                yield return wait;
            }
            
            // Set state
            inProgress = false;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Connectivity Collapse
        /// /////////////////////////////////////////////////////////
        
        // Start collapse
        public static void StartCollapse(RayfireConnectivity scr)
        {
            // Already running
            if (scr.collapse.inProgress == true)
                return;

            // Not enough shards
            if (scr.cluster.shards.Count <= 1)
                return;
            
            scr.StartCoroutine(scr.collapse.CollapseCor (scr));
        }
        
        // Stop collapse
        public static void StopCollapse (RayfireConnectivity scr)
        {
            scr.collapse.inProgress = false;
        }

        // Start collapse coroutine
        IEnumerator CollapseCor (RayfireConnectivity scr)
        {
            // Wait time
            WaitForSeconds wait = new WaitForSeconds (duration/steps);
            
            // Set running state
            inProgress = true;
            
            // Iterate collapse
            float step = (end - start) / (float)steps;
            for (int i = 0; i < steps; i++)
            {
                // Stop
                if (inProgress == false)
                    break;

                // Init collapse
                float percentage = start + step * i;
                if (type == RFCollapseType.ByArea)
                    AreaCollapse (scr, (int)percentage);
                else if (type == RFCollapseType.BySize)
                    SizeCollapse (scr, (int)percentage);
                else if (type == RFCollapseType.Random)
                    RandomCollapse (scr, (int)percentage, scr.seed);
                
                yield return wait;
            }
            
            // Set state
            inProgress = false;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Connected Cluster Collapse
        /// /////////////////////////////////////////////////////////

        // Collapse in percents
        public static void AreaCollapse (RayfireRigid scr, int areaPercentage)
        {
            areaPercentage = Mathf.Clamp (areaPercentage, 0, 100);
            AreaCollapse (scr, Mathf.Lerp (scr.clusterDemolition.cluster.minimumArea, scr.clusterDemolition.cluster.maximumArea, areaPercentage / 100f));
        }
        
        // Collapse in percents
        public static void SizeCollapse (RayfireRigid scr, int sizePercentage)
        {
            sizePercentage = Mathf.Clamp (sizePercentage, 0, 100);
            SizeCollapse (scr, Mathf.Lerp (scr.clusterDemolition.cluster.minimumSize, scr.clusterDemolition.cluster.maximumSize, sizePercentage / 100f));
        }
        
        // Break neib connection by shared are value and demolish cluster
        public static void AreaCollapse (RayfireRigid scr, float minAreaValue)
        {
            // Not initialized
            if (scr.initialized == false)
                return;
            
            // Value lower than last
            if (minAreaValue < scr.clusterDemolition.cluster.areaCollapse)
                return;

            // Set value
            scr.clusterDemolition.cluster.areaCollapse = minAreaValue;

            // Main cluster.
            int removed = RemNeibByArea (scr.clusterDemolition.cluster, minAreaValue);
            if (removed > 0)
                CollapseCluster (scr);
        }

        // Break neib connection by size
        public static void SizeCollapse (RayfireRigid scr, float minSizeValue)
        {
             // Not initialized
             if (scr.initialized == false)
                 return;

             // Value lower than last
             if (minSizeValue < scr.clusterDemolition.cluster.sizeCollapse)
                 return;

             // Set value
             scr.clusterDemolition.cluster.sizeCollapse = minSizeValue;

             // Main cluster.
             int removed = RemNeibBySize (scr.clusterDemolition.cluster, minSizeValue);
             if (removed > 0)
                 CollapseCluster (scr);
        }

        // Break neib connection randomly
        public static void RandomCollapse (RayfireRigid scr, int randomValue, int randomSeed)
        {
            // Not initialized
            if (scr.initialized == false)
                return;
            
            // Value lower than last
            if (randomValue < scr.clusterDemolition.cluster.randomCollapse)
                return;

            // Set value
            scr.clusterDemolition.cluster.randomCollapse = randomValue;
            scr.clusterDemolition.cluster.randomSeed     = randomSeed;

            // Main cluster.
            int removed = RemNeibRandom (scr.clusterDemolition.cluster, randomValue, randomSeed);
            if (removed > 0)
                CollapseCluster (scr);
        }

        // Init collapse after connection loss
        static void CollapseCluster (RayfireRigid scr)
        {
            // Collect solo shards, remove from cluster, no need to reinit
            List<RFShard> detachShards = new List<RFShard>();
            RFCluster.GetSoloShards (scr.clusterDemolition.cluster, detachShards);

            // Clear fragments in case of previous demolition
            if (scr.HasFragments == true)
                scr.fragments.Clear();

            // Dynamic cluster connectivity check, all clusters are equal, pick biggest to keep as original
            if (scr.simulationType == SimType.Dynamic || scr.simulationType == SimType.Sleeping)
            {
                // Check left cluster shards for connectivity and collect not connected child clusters. Should be before ClusterizeDetachShards
                RFCluster.ConnectivityCheck (scr.clusterDemolition.cluster);

                // Cluster is not connected. Set biggest child cluster shards to original cluster. Cant be 1 child cluster here
                RFCluster.ReduceChildClusters (scr.clusterDemolition.cluster);
            }

            // Kinematic/ Inactive cluster, Connectivity check if cluster has uny shards. Main cluster keeps all not activated
            else if (scr.simulationType == SimType.Kinematic || scr.simulationType == SimType.Inactive)
            {
                RFCluster.ConnectivityUnyCheck (scr.clusterDemolition.cluster);
            }
            
            // Init final cluster ops
            RFDemolitionCluster.PostDemolitionCluster (scr, detachShards);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Connectivity Collapse
        /// /////////////////////////////////////////////////////////

        // Collapse in percents
        public static void AreaCollapse (RayfireConnectivity connectivity, int areaPercentage)
        {
            areaPercentage = Mathf.Clamp (areaPercentage, 0, 100);
            AreaCollapse (connectivity, Mathf.Lerp (connectivity.cluster.minimumArea, connectivity.cluster.maximumArea, areaPercentage / 100f));
        }
        
        // Collapse in percents
        public static void SizeCollapse (RayfireConnectivity connectivity, int sizePercentage)
        {
            sizePercentage = Mathf.Clamp (sizePercentage, 0, 100);
            SizeCollapse (connectivity, Mathf.Lerp (connectivity.cluster.minimumSize, connectivity.cluster.maximumSize, sizePercentage / 100f));
        }
        
        // Crumbling
        public static void AreaCollapse (RayfireConnectivity connectivity, float areaValue)
        {
            // Value lower than last
            if (areaValue < connectivity.cluster.areaCollapse)
                return;

            // Set value
            connectivity.cluster.areaCollapse = areaValue;
            
            // Main cluster
            int removed = RemNeibByArea (connectivity.cluster, areaValue);
            if (removed > 0)
                connectivity.CheckConnectivity();
        }
        
        // Crumbling
        public static void SizeCollapse (RayfireConnectivity connectivity, float sizeValue)
        {
            // Value lower than last
            if (sizeValue < connectivity.cluster.sizeCollapse)
                return;
            
            // Set value
            connectivity.cluster.sizeCollapse = sizeValue;
            
            // Main cluster.
            int removed = RemNeibBySize (connectivity.cluster, sizeValue);
            if (removed > 0)
                connectivity.CheckConnectivity();
        }

        // Crumbling
        public static void RandomCollapse (RayfireConnectivity connectivity, int randomPercentage, int seedValue)
        {
            // Clamp
            randomPercentage = Mathf.Clamp (randomPercentage, 0, 100);
            
            // Value lower than last
            if (randomPercentage < connectivity.cluster.randomCollapse)
                return;

            // Set value
            connectivity.cluster.randomCollapse = randomPercentage;
            connectivity.cluster.randomSeed     = seedValue;
            
            // Main cluster.
            int removed = RemNeibRandom(connectivity.cluster, randomPercentage, seedValue);
            if (removed > 0)
                connectivity.CheckConnectivity();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Neib removing
        /// /////////////////////////////////////////////////////////
        
        // Remove neibs by area
        static int RemNeibByArea (RFCluster cluster, float minArea)
        {
            int removed = 0;
            for (int s = 0; s < cluster.shards.Count; s++)
            {
                // Skip unyielding
                if (cluster.shards[s].uny == true)
                    continue;
                
                for (int n = cluster.shards[s].neibShards.Count - 1; n >= 0; n--)
                {
                    if (cluster.shards[s].nArea[n] < minArea)
                    {
                        // Remove self in neib's neib list
                        for (int i = cluster.shards[s].neibShards[n].neibShards.Count - 1; i >= 0; i--)
                        {
                            if (cluster.shards[s].neibShards[n].neibShards[i] == cluster.shards[s])
                            {
                                cluster.shards[s].neibShards[n].RemoveNeibAt (i);
                                break;
                            }
                        }
                       
                        // Remove in self
                        cluster.shards[s].RemoveNeibAt (n);
                        removed++;
                    }
                }
            }
            return removed;
        }
        
        // Remove neibs by size
        static int RemNeibBySize (RFCluster cluster, float minSize)
        {
            int removed = 0;
            for (int s = 0; s < cluster.shards.Count; s++)
            {
                // Skip unyielding
                if (cluster.shards[s].uny == true)
                    continue;
                
                if (cluster.shards[s].sz < minSize)
                {
                    for (int n = cluster.shards[s].neibShards.Count - 1; n >= 0; n--)
                    {
                        // Remove self in neib's neib list
                        for (int i = cluster.shards[s].neibShards[n].neibShards.Count - 1; i >= 0; i--)
                        {
                            if (cluster.shards[s].neibShards[n].neibShards[i] == cluster.shards[s])
                            {
                                cluster.shards[s].neibShards[n].RemoveNeibAt (i);
                                break;
                            }
                        }
                    }
                        
                    // Remove in self
                    cluster.shards[s].ClearNeib();
                    removed++;
                }
            }
            return removed;
        }
        
        // Remove neibs by area
        static int RemNeibRandom (RFCluster cluster, int percent, int seed)
        {
            int removed = 0;
            cluster.randomSeed = seed;
            for (int s = 0; s < cluster.shards.Count; s++)
            {
                // Skip unyielding
                if (cluster.shards[s].uny == true)
                    continue;
                
                for (int n = cluster.shards[s].neibShards.Count - 1; n >= 0; n--)
                {
                    // Set random state for same pair
                    Random.InitState (cluster.shards[s].id + cluster.shards[s].neibShards[n].id + seed);
                    if (Random.Range (0, 100) < percent)
                    {
                        // Remove self in neib's neib list
                        for (int i = cluster.shards[s].neibShards[n].neibShards.Count - 1; i >= 0; i--)
                        {
                            if (cluster.shards[s].neibShards[n].neibShards[i] == cluster.shards[s])
                            {
                                cluster.shards[s].neibShards[n].RemoveNeibAt (i);
                                break;
                            }
                        }
                       
                        // Remove in self
                        cluster.shards[s].RemoveNeibAt (n);
                        removed++;
                    }
                }
            }
            return removed;
        }

        // Remove connection in cluster in s shard and for its n neib 
        static void RemoveConnection(RFCluster cluster, int s, int n)
        {
            // Remove self in neib's neib list
            for (int i = cluster.shards[s].neibShards[n].neibShards.Count - 1; i >= 0; i--)
            {
                if (cluster.shards[s].neibShards[n].neibShards[i] == cluster.shards[s])
                {
                    cluster.shards[s].neibShards[n].RemoveNeibAt (i);
                    break;
                }
            }
                       
            // Remove in self
            cluster.shards[s].RemoveNeibAt (n);
        }
        
        // Set range for area and size
        public static void SetRangeData (RFCluster cluster, int perc = 0, int seed = 0)
        {
            if (cluster.shards.Count == 0)
                return;

            // Start values
            cluster.maximumSize = cluster.shards[0].sz;
            cluster.minimumSize = cluster.shards[0].sz;
            cluster.maximumArea = 0f;
            cluster.minimumArea = 10000f;
            cluster.randomCollapse = perc;
            cluster.randomSeed = seed;
            
            // Loop shards
            for (int i = 0; i < cluster.shards.Count; i++)
            {
                if (cluster.shards[i].sz > cluster.maximumSize)
                    cluster.maximumSize = cluster.shards[i].sz;
                if (cluster.shards[i].sz < cluster.minimumSize)
                    cluster.minimumSize = cluster.shards[i].sz;

                for (int j = 0; j < cluster.shards[i].nArea.Count; j++)
                {
                    if (cluster.shards[i].nArea[j] > cluster.maximumArea)
                        cluster.maximumArea = cluster.shards[i].nArea[j];
                    
                    if (cluster.shards[i].nArea[j] < cluster.minimumArea)
                        cluster.minimumArea = cluster.shards[i].nArea[j];
                }
            }

            // Fix
            if (cluster.minimumArea < 0.001f)
                cluster.minimumArea = 0f;
            
            cluster.areaCollapse = cluster.minimumArea;
            cluster.sizeCollapse = cluster.minimumSize;
        }
    }
}