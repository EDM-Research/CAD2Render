using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using Random = UnityEngine.Random;

// Max shard checks per. Spreading over frames
// TODO use tri normals instead of tm vectors
// Stress for supported fragments as well if heavy weight on low size support

// Recache at start if rotation is different. Check warning.
// A* with shortest path from every shard to every shard, count amount of paths for every connection, TODO sum with weight,
//     consider at tress increase as multiplier. At activation start runtime recache over few seconds.

// max path search steps. property


namespace RayFire
{
    [Serializable]
    public class RFStress
    {
        public bool  enable;
        public int   threshold;
        public float erosion;
        public float interval;
        public int   support;
        public bool  exposed;
        public bool  bySize;
        
        public List<RFShard> strShards; // TODO Make nonserialized because recollected at initialize
        public List<RFShard> supShards; // TODO Make nonserialized because recollected at initialize
        public Vector3       rotCache;
        public Vector3       grvCache;
        public int           supCache;
        public float         sizeCache;
        public bool          initialized;

        List<RFShard> checkShards;
        
        // Non serialized
        [NonSerialized] public bool inProgress;
        [NonSerialized] public int  strAmount;
        [NonSerialized] public int  supAmount;

        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////

        // Constructor
        public RFStress()
        {
            threshold = 100;
            erosion   = 1f;
            interval  = 1f;
            support   = 45;
        }

        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////

        // Stop stress check
        public static void StopStress (RayfireConnectivity scr)
        {
            scr.stress.inProgress = false;
        }

        // Initiate stress check
        public static void StartStress (RayfireConnectivity scr)
        {
            // Already running
            if (scr.stress.inProgress == true)
                return;

            // Not enough shards
            if (scr.cluster.shards.Count <= 1)
                return;

            scr.StartCoroutine (scr.stress.StressCor (scr));
        }

        // Connectivity check cor
        IEnumerator StressCor (RayfireConnectivity scr)
        {
            // Set state
            scr.stress.inProgress = true;

            // Delay
            WaitForSeconds wait = new WaitForSeconds (scr.stress.interval);

            // Random offset delay
            yield return new WaitForSeconds (Random.Range (0.01f, 0.5f));

            // Repeat every second
            while (scr.stress.inProgress == true)
            {
                if (scr.stress.erosion > 0)
                {
                    // Increase erosion stress for stressed shards
                    StressErosion (scr);

                    // Init stress collapse check
                    StressCollapse (scr, scr.stress.threshold);

                    // Recalculate supporting and stressed shards using support data
                    if (CheckSupport (scr) == true)
                        SetStressSupport (scr.cluster.shards, scr.stress);
                }

                yield return wait;
            }

            // Set state
            scr.stress.inProgress = false;
        }

        // Check if supported shards should be recached
        bool CheckSupport (RayfireConnectivity scr)
        {
            if (scr.rigidRootHost != null)
                foreach (var sup in scr.stress.supShards)
                    if (sup.sm == SimType.Dynamic)
                        return true;

            if (scr.meshRootHost != null)
                foreach (var sup in scr.stress.supShards)
                    if (sup.rigid.simulationType == SimType.Dynamic)
                        return true;

            return false;
        }

        // Increase connection stress
        static void StressErosion (RayfireConnectivity scr)
        {
            // Remove stressed shard without connections
            for (int s = scr.stress.strShards.Count - 1; s >= 0; s--)
                if (scr.stress.strShards[s].cluster == null)
                    scr.stress.strShards.RemoveAt (s);

            // Increase connection stress
            if (scr.stress.exposed == false)
                for (int i = 0; i < scr.stress.strShards.Count; i++)
                    for (int n = 0; n < scr.stress.strShards[i].nSt.Count / 3; n++)
                        scr.stress.strShards[i].nSt[n] += scr.stress.strShards[i].nSt[n * 3 + 2] * scr.stress.strShards[i].nSt[n * 3 + 1] * scr.stress.erosion * scr.stress.strShards[i].sSt;
            else
                for (int i = 0; i < scr.stress.strShards.Count; i++)
                    if (scr.stress.strShards[i].nIds.Count < scr.stress.strShards[i].nAm)
                        for (int n = 0; n < scr.stress.strShards[i].nSt.Count / 3; n++)
                            scr.stress.strShards[i].nSt[n] += scr.stress.strShards[i].nSt[n * 3 + 2] * scr.stress.strShards[i].nSt[n * 3 + 1] * scr.stress.erosion * scr.stress.strShards[i].sSt;
        }

        // Break neib connection by connection stress
        static void StressCollapse (RayfireConnectivity connectivity, int maxStressValue)
        {
            // TODO optimize like in AreaCollapse
            // TODO Check only stressed shards, keep public list, add on uny activation

            // Main cluster
            int removed = RemNeibByStress (connectivity.stress, maxStressValue);
            if (removed > 0)
                connectivity.CheckConnectivity();
        }

        // Remove neibs by stress
        static int RemNeibByStress (RFStress stress, int stressVal)
        {
            int removed = 0;
            for (int s = stress.strShards.Count - 1; s >= 0; s--)
            {
                // Check all shards connection stress values
                for (int n = (stress.strShards[s].nSt.Count / 3 - 1); n >= 0; n--)
                {
                    // Maximum stress reached
                    if (stress.strShards[s].nSt[n * 3] > stressVal)
                    {
                        // Remove self in neib's neib list
                        for (int i = stress.strShards[s].neibShards[n].neibShards.Count - 1; i >= 0; i--)
                        {
                            if (stress.strShards[s].neibShards[n].neibShards[i] == stress.strShards[s])
                            {
                                stress.strShards[s].neibShards[n].RemoveNeibAt (i);
                                break;
                            }
                        }

                        // Remove in self
                        stress.strShards[s].RemoveNeibAt (n);
                        removed++;
                    }
                }
            }

            return removed;
        }

        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////

        // Set Stress
        public static void Initialize (RayfireConnectivity conn)
        {
            // Initialize if never was initialized, rotation or gravity different
            if (InitializeNeed (conn) == true)
            {
                // Set Stress data
                SetStressConnections (conn);

                // Set stress path multiplier
                if (conn.stress.bySize == true)
                    SetSizeMultiplier (conn);

                // Initialized
                conn.stress.initialized = true;
            }

            // Set supporting shards
            SetStressSupport (conn.cluster.shards, conn.stress);
        }

        // Check if stress need to be reinitialized
        static bool InitializeNeed (RayfireConnectivity conn)
        {
            // Was not initialized before
            if (conn.stress.initialized == false)
                return true;

            // Gravity direction changed
            if (conn.stress.grvCache != Physics.gravity.normalized)
                return true;

            // World rotation changed
            if (conn.stress.rotCache != conn.gameObject.transform.rotation.eulerAngles)
                return true;

            // TODO check if nSt list is declared
            
            return false;
        }

        // Set Stress data 
        static void SetStressConnections (RayfireConnectivity conn)
        {
            // Set list
            for (int s = 0; s < conn.cluster.shards.Count; s++)
            {
                conn.cluster.shards[s].sSt = 1f;
                conn.cluster.shards[s].nSt = new List<float>();
            }

            // Cache world rotation 
            conn.stress.grvCache = Physics.gravity.normalized;
            conn.stress.rotCache = conn.gameObject.transform.rotation.eulerAngles;

            // Set neib direction, size ration and stress value
            Vector3 dir;
            for (int s = 0; s < conn.cluster.shards.Count; s++)
            {
                for (int n = 0; n < conn.cluster.shards[s].neibShards.Count; n++)
                {
                    // Set stress
                    conn.cluster.shards[s].nSt.Add (0f);

                    // Set direction 
                    dir = conn.cluster.shards[s].neibShards[n].pos - conn.cluster.shards[s].pos;
                    conn.cluster.shards[s].nSt.Add (Vector3.Angle (-conn.stress.grvCache, dir) / 180f);

                    // Set size ration
                    conn.cluster.shards[s].nSt.Add (conn.cluster.shards[s].neibShards[n].sz / conn.cluster.shards[s].sz);
                }
            }
        }

        // Set supporting shards
        static void SetStressSupport (List<RFShard> shards, RFStress stress)
        {
            // Save support value to check at start
            stress.supCache = stress.support;

            // Support angle as direction
            float dirThreshold = stress.support / 180f;

            // Set list
            for (int s = 0; s < shards.Count; s++)
                shards[s].sIds = new List<int>();

            // Lists
            if (stress.supShards == null) stress.supShards = new List<RFShard>();
            else stress.supShards.Clear();
            if (stress.strShards == null) stress.strShards = new List<RFShard>();
            else stress.strShards.Clear();

            // Set unchecked state
            RFShard.SetUnchecked (shards);

            // Collect uny shards
            PrepareCheckShards (stress);

            for (int i = 0; i < shards.Count; i++)
            {
                if (shards[i].neibShards.Count > 0 && shards[i].uny == true)
                {
                    // Mark as checked
                    shards[i].check = true;

                    // Collect
                    stress.supShards.Add (shards[i]);
                    stress.checkShards.Add (shards[i]);
                }
            }

            // Check all shards for supporting neibs
            while (stress.checkShards.Count > 0)
            {
                // Check all neibs
                for (int n = 0; n < stress.checkShards[0].neibShards.Count; n++)
                {
                    // Skip checked shards
                    if (stress.checkShards[0].neibShards[n].check == true)
                        continue;

                    // Neib shard is supported by this shard
                    if (stress.checkShards[0].nSt[n * 3 + 1] < dirThreshold)
                    {
                        // Mark as checked
                        stress.checkShards[0].neibShards[n].check = true;

                        // Add new shard to check it's neibs
                        stress.checkShards.Add (stress.checkShards[0].neibShards[n]);

                        // Set supporting shards Id
                        stress.checkShards[0].neibShards[n].sIds.Add (stress.checkShards[0].id);

                        // Collect supported shard
                        stress.supShards.Add (stress.checkShards[0].neibShards[n]);
                    }
                }

                // Remove checked
                stress.checkShards.RemoveAt (0);
            }

            // Set unchecked state
            RFShard.SetUnchecked (shards);

            // Get stressed shards
            for (int i = 0; i < shards.Count; i++)
                if (shards[i].uny == false && shards[i].sIds.Count == 0)
                    stress.strShards.Add (shards[i]);
        }

        /// /////////////////////////////////////////////////////////
        /// Prepare vars
        /// /////////////////////////////////////////////////////////

        // Prepare check shards list
        static void PrepareCheckShards (RFStress stress)
        {
            if (stress.checkShards == null) stress.checkShards = new List<RFShard>();
            else stress.checkShards.Clear();
        }

        /// /////////////////////////////////////////////////////////
        /// Astar
        /// /////////////////////////////////////////////////////////
        
        // Set size multiplier TODO remove foreach
        static void SetSizeMultiplier (RayfireConnectivity conn)
        {
            // 63162 / 6.45

            // Timestamp
            // float t1 = Time.realtimeSinceStartup;


            // Set shards ids to skip path checks: self, neibs
            foreach (var shard in conn.cluster.shards)
            {
                shard.sCheck           = new bool [conn.cluster.shards.Count];
                shard.sCheck[shard.id] = true;
                for (int i = 0; i < shard.nIds.Count; i++)
                    shard.sCheck[shard.nIds[i]] = true;
            }

            // int iterations = 0;
            foreach (var shard in conn.cluster.shards)
            {
                // float t2 = Time.realtimeSinceStartup;

                if (shard.nIds.Count > 1) // was 0
                {
                    for (int i = 0; i < conn.cluster.shards.Count; i++)
                    {
                        // 
                        if (shard.sCheck[i] == false)
                        {
                            if (conn.cluster.shards[i].uny == false)
                            {
                                // Mark path to avoid reversed path calculation
                                conn.cluster.shards[i].sCheck[shard.id] = true;

                                GetPathAstar (conn.stress, conn.cluster.shards, shard.id, conn.cluster.shards[i].id);
                               // iterations++;
                            }
                        }
                    }
                }

                // Debug.Log (Time.realtimeSinceStartup - t2);
            }
            
            // Get range
            float pathMin = conn.cluster.shards[0].sSt;
            float pathMax = conn.cluster.shards[0].sSt;
            foreach (var shard in conn.cluster.shards)
            {
                if (shard.sSt > pathMax)
                    pathMax = shard.sSt;
                if (shard.sSt < pathMin)
                    pathMin = shard.sSt;
            }


            // Set max difference
            conn.stress.sizeCache = pathMax - pathMin;

            // Set ratio
            foreach (var shard in conn.cluster.shards)
            {
                shard.sSt    -= pathMin;
                shard.sSt    /= conn.stress.sizeCache;
                shard.sCheck =  null;
            }
        }

        // Set path stress multiplier
        static void SetSizeMultiplierOLD (RayfireConnectivity conn)
        {
            // 63162 / 6.45

            // Timestamp
            // float t1 = Time.realtimeSinceStartup;

            int iterations = 0;
            foreach (var shard in conn.cluster.shards)
            {
                // float t2 = Time.realtimeSinceStartup;

                if (shard.nIds.Count > 1) // was 0
                {
                    for (int i = 0; i < conn.cluster.shards.Count; i++)

                        // Skip self and neibs
                        if (shard.id != conn.cluster.shards[i].id && shard.nIds.Contains (conn.cluster.shards[i].id) == false)
                            if (conn.cluster.shards[i].uny == false)
                            {
                                GetPathAstar (conn.stress, conn.cluster.shards, shard.id, conn.cluster.shards[i].id);
                                iterations++;
                            }
                }

                // Debug.Log (Time.realtimeSinceStartup - t2);
            }

            // Debug.Log (iterations);
            // Debug.Log (Time.realtimeSinceStartup - t1);

            // Get range
            float pathMin = conn.cluster.shards[0].sSt;
            float pathMax = conn.cluster.shards[0].sSt;
            foreach (var shard in conn.cluster.shards)
            {
                if (shard.sSt > pathMax)
                    pathMax = shard.sSt;
                if (shard.sSt < pathMin)
                    pathMin = shard.sSt;
            }

            conn.stress.sizeCache = pathMax - pathMin;

            foreach (var shard in conn.cluster.shards)
                shard.sSt -= pathMin;

            foreach (var shard in conn.cluster.shards)
                shard.sSt /= conn.stress.sizeCache;
        }

        private List<int> path       = new List<int>();
        private List<int> frontier   = new List<int>();
        private List<int> frontPrior = new List<int>();
        //private int[]     cameFrom;
        //private int[]     costSoFar;

        // Get shortest path ASTAR
        static List<int> GetPathAstar (RFStress str, List<RFShard> shards, int startId, int lastId)
        {
            // Dest Found Approve
            bool destinationFound = false;

            // Create frontier and visited hexes
            str.frontier.Clear();
            str.frontier.Add (shards[startId].id);

            // Create frontier priority list
            str.frontPrior.Clear();
            str.frontPrior.Add (0);

            // Fill Came from array with none values
            int[] cameFrom = new int[shards.Count];
            for (int i = 0; i < shards.Count; i++)
                cameFrom[i] = -1;

            // Create array to store all movement costs for every hex
            int[] costSoFar = new int[shards.Count];
            for (int i = 0; i < shards.Count; i++)
                costSoFar[i] = -1;
            costSoFar[startId] = 0;

            // While there are frontier
            while (str.frontier.Count > 0)
            {
                // Init Current hex
                int current = str.frontier[0];

                // And remove it from frontier
                str.frontier.RemoveAt (0);
                str.frontPrior.RemoveAt (0);

                // For every neighbour of current frontier
                foreach (int next in shards[current].nIds)
                {
                    // Get hex passing cost. Not passable if 0
                    int cost = 1; // PathHexCheck (pathGraph[next], type); TODO consider size
                    if (cost > 0)
                    {
                        // Get new cost
                        int new_cost = costSoFar[current] + cost;

                        // Reached destination
                        if (next == lastId)
                        {
                            cameFrom[next] = current;
                            str.frontier.Clear();
                            str.frontPrior.Clear();
                            destinationFound = true;
                            break;
                        }

                        // Compare movement cost
                        if (costSoFar[next] < 0 || new_cost < costSoFar[next]) // Expand frontier and mark next as came from
                        {
                            if (next != startId)
                            {
                                // Save cost to cost so far
                                costSoFar[next] = new_cost;

                                //  Get priority
                                int priority = new_cost + (int)Math.Abs (Vector3.Distance (shards[next].pos, shards[lastId].pos));

                                // If new cost is less than 
                                if (str.frontier.Count > 0)
                                {
                                    for (int i = 0; i < str.frontPrior.Count; i++)
                                    {
                                        if (priority <= str.frontPrior[i])
                                        {
                                            str.frontier.Insert (i, next);
                                            str.frontPrior.Insert (i, priority);
                                            break;
                                        }

                                        // Check if new cost is the highest value, add as last
                                        if (i == str.frontPrior.Count - 1 && priority > str.frontPrior[i])
                                        {
                                            str.frontier.Insert (str.frontPrior.Count, next);
                                            str.frontPrior.Insert (str.frontPrior.Count, priority);
                                        }
                                    }
                                }
                                else
                                {
                                    str.frontier.Add (next);
                                    str.frontPrior.Add (priority);
                                }

                                cameFrom[next] = current;
                            }
                        }
                    }
                }
            }

            // If destination reached then reverse to start and return otherwise return empty list
            str.path.Clear();
            if (destinationFound == true)
            {
                // Reverse path from destination to start
                int currentRev = lastId;
                str.path.Add (currentRev);
                while (currentRev != startId)
                {
                    currentRev = cameFrom[currentRev];
                    str.path.Add (currentRev);
                }

                // Get size path
                float sz = 0;
                for (int p = 0; p < str.path.Count; p++)
                    sz += shards[str.path[p]].sz;

                // Add average size by path
                for (int p = 0; p < str.path.Count; p++)
                    shards[str.path[p]].sSt += sz / str.path.Count;
            }

            return str.path;
        }
    }
}