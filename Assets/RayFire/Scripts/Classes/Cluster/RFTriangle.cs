using System.Collections.Generic;
using UnityEngine;

namespace RayFire
{
    //[Serializable]
    public class RFTriangle
    {
        public int       id;
        public float     area;
        public Vector3   normal;
        public Vector3   pos;
        public List<int> neibs;

        static int[]     triangles;
        static Vector3[] vertices;
        static Vector3[] normals;

        // Constructor
        RFTriangle (float Area, Vector3 Normal, Vector3 Pos)
        {
            area   = Area;
            normal = Normal;
            pos    = Pos;
        }

        // Set mesh triangles
        public static void SetTriangles (RFShard shard)
        {
            // Check if triangles already calculated
            if (shard.tris != null)
                return;
            
            // Cached Vars
            triangles = shard.mf.sharedMesh.triangles;
            vertices  = shard.mf.sharedMesh.vertices;
            normals   = shard.mf.sharedMesh.normals;
            
            // Collect tris
            int v1, v2, v3;
            Vector3 p1, p2, p3, cross, pos;
            shard.tris = new List<RFTriangle>();
            for (int i = 0; i < triangles.Length; i += 3)
            {
                // Vertex indexes
                v1 = triangles[i];
                v2 = triangles[i + 1];
                v3 = triangles[i + 2];

                // Get vertices position and area
                p1    = shard.tm.TransformPoint (vertices[v1]);
                p2    = shard.tm.TransformPoint (vertices[v2]);
                p3    = shard.tm.TransformPoint (vertices[v3]);
                cross = Vector3.Cross (p1 - p2, p1 - p3);

                // Set position
                pos = (p1 + p2 + p3) / 3f;

                // Create triangle and collect it
                shard.tris.Add (new RFTriangle ((cross.magnitude * 0.5f), normals[v1], pos));
            }

            triangles = null;
            vertices  = null;
            normals   = null;
        }
        
        // Clear
        public static void Clear(RFShard shard)
        {
            if (shard.tris != null)
                shard.tris.Clear();
            shard.tris = null;
        }
    }
}