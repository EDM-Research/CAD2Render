using System.Collections.Generic;
using UnityEngine;

namespace RayFire
{
    public class RFFace
    {
        public float   area;
        public Vector3 normal;

        // Constructor
        RFFace (float Area, Vector3 Normal)
        {
            area   = Area;
            normal = Normal;
        }

        // Set poly data
        public static void SetPolys (RFShard shard)
        {
            // Check if faces already calculated
            if (shard.poly != null)
                return;
            
            // Check if triangles already calculated
            if (shard.tris == null)
                return;
            
            // Create first poly
            RFFace face = new RFFace (shard.tris[0].area, shard.tris[0].normal);
            
            // Set faces list with first face
            shard.poly = new List<RFFace>();
            shard.poly.Add (face);
            for (int t = 1; t < shard.tris.Count; t++)
            {
                // Check if tri belong to any face
                bool alreadyHasPoly = false;
                for (int f = 0; f < shard.poly.Count; f++)
                {
                    if (shard.poly[f].normal == shard.tris[t].normal)
                    {
                        shard.poly[f].area += shard.tris[t].area;
                        alreadyHasPoly     =  true;
                        break;
                    }
                }

                // New face
                if (alreadyHasPoly == false)
                    shard.poly.Add (new RFFace (shard.tris[t].area, shard.tris[t].normal));
            }
        }
        
        // Clear
        public static void Clear(RFShard shard)
        {
            if (shard.poly != null)
                shard.poly.Clear();
            shard.poly = null;
        }
    }
}