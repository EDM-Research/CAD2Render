using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RayFire
{
    [Serializable]
    public class RFMesh
    {
        [Serializable]
        public class RFSubMeshTris
        {
            public List<int> triangles;

            public RFSubMeshTris()
            {
                triangles = new List<int>();
            }
            
            public RFSubMeshTris(List<int> tris)
            {
                triangles = tris;
            }
        }
        
        // Variables
        public bool compress;
        public int subMeshCount;
        public Bounds bounds;
        public int[] triangles;
        public List<RFSubMeshTris> subTriangles;

        // Uncompressed floats
        public Vector2[] uv;
        public Vector3[] vertices;
        public Vector4[] tangents;
        
        // Compressed ints
        public List<int> uvComp;
        public List<int> verticesComp;
        public List<int> tangentsComp;
        
        // Constructor
        public RFMesh (Mesh mesh, bool comp = false)
        {
            // Common
            compress = comp;
            subMeshCount = mesh.subMeshCount;
            bounds = mesh.bounds;
            
            // Save triangles
            subTriangles = new List<RFSubMeshTris>();
            if (subMeshCount <= 1)
                triangles = mesh.triangles;
            else if (subMeshCount > 1)
                for (int i = 0; i < subMeshCount; i++)
                    subTriangles.Add (new RFSubMeshTris (mesh.GetTriangles(i).ToList()));
            
            // UnCompressed data
            if (comp == false)
            {
                uv = mesh.uv;
                vertices = mesh.vertices;
                tangents = mesh.tangents;
            }
            
            // Compressed data
            else
            {
                uvComp = new List<int>();
                verticesComp = new List<int>();
                tangentsComp = new List<int>();
                
                foreach (var v in mesh.uv)
                {
                    uvComp.Add (Mathf.RoundToInt (v.x * 1000));
                    uvComp.Add (Mathf.RoundToInt (v.y * 1000));
                }
                foreach (var v in mesh.vertices)
                {
                    verticesComp.Add (Mathf.RoundToInt (v.x * 1000));
                    verticesComp.Add (Mathf.RoundToInt (v.y * 1000));
                    verticesComp.Add (Mathf.RoundToInt (v.z * 1000));
                }
                foreach (var v in mesh.tangents)
                {
                    tangentsComp.Add (Mathf.RoundToInt (v.x * 1000));
                    tangentsComp.Add (Mathf.RoundToInt (v.y * 1000));
                    tangentsComp.Add (Mathf.RoundToInt (v.z * 1000));
                }
            }
        }

        // Convert RFmesh to Mesh
        public Mesh GetMesh()
        {
            // Common
            Mesh mesh = new Mesh();
            mesh.subMeshCount = subMeshCount;
            
            // Uncompressed & Compressed
            if (compress == false)
            {
                mesh.vertices = vertices;
                LoadTriangles(mesh);
                mesh.uv = uv;
                mesh.tangents = tangents; 
            }
            else
            {
                mesh.vertices = SetCompressedVertices (verticesComp);
                LoadTriangles(mesh);
                mesh.uv = SetCompressedUv (uvComp);
                mesh.tangents = SetCompressedTangents (tangentsComp);
            }
            
            // Prepare 
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }

        // Load triangles by submesh count
        void LoadTriangles(Mesh mesh)
        {
            if (subMeshCount == 1)
                mesh.triangles = triangles;
            else if (subMeshCount > 1)
                for (int i = 0; i < subMeshCount; i++)
                    mesh.SetTriangles (subTriangles[i].triangles, i);
        }
        
        // Get Baked skinned mesh into static mesh
        public static Mesh BakeMesh (SkinnedMeshRenderer skin)
        {
            Mesh mesh = new Mesh();
            skin.BakeMesh(mesh);
            mesh.name = skin.name + "_bake";

            //mesh.MarkDynamic();
            //mesh.RecalculateTangents();
            //mesh.RecalculateNormals();
            //mesh.RecalculateBounds();
            
            return mesh;
        }
        
        // Convert RF mesh to meshes
        public static void ConvertRfMeshes(RayfireRigid rigid)
        {
            rigid.meshes = new Mesh[rigid.rfMeshes.Length];
            for (int i = 0; i < rigid.rfMeshes.Length; i++)
                rigid.meshes[i] = rigid.rfMeshes[i].GetMesh();
            rigid.rfMeshes = null;
        }

        // Get uv from compressed uv
        static Vector2[] SetCompressedUv (List<int> uvComp)
        {
            Vector2[] uvNew = new Vector2[uvComp.Count / 2];
            for (int i = 0; i < uvNew.Length; i++)
                uvNew[i] = new Vector2 (
                    uvComp[i * 2 + 0] / 1000.0f,
                    uvComp[i * 2 + 1] / 1000.0f);
            return uvNew;
        }
        
        // Get Vertices from compressed Vertices
        static Vector3[] SetCompressedVertices (List<int> verticesComp)
        {
            Vector3[] verticesNew = new Vector3[verticesComp.Count / 3];
            for (int i = 0; i < verticesNew.Length; i++)
                verticesNew[i] = new Vector3 (
                    verticesComp[i * 3 + 0] / 1000.0f,
                    verticesComp[i * 3 + 1] / 1000.0f,
                    verticesComp[i * 3 + 2] / 1000.0f);
            return verticesNew;
        }
        
        // Get tangents from compressed tangents
        static Vector4[] SetCompressedTangents (List<int> tangentsComp)
        {
            Vector4[] tangentsNew = new Vector4[tangentsComp.Count / 3];
            for (int i = 0; i < tangentsNew.Length; i++)
                tangentsNew[i] = new Vector3 (
                    tangentsComp[i * 3 + 0] / 1000.0f,
                    tangentsComp[i * 3 + 1] / 1000.0f,
                    tangentsComp[i * 3 + 2] / 1000.0f);
            return tangentsNew;
        }
    }
}

