using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Face remove by angle

namespace RayFire
{
    public class RFCombineMesh
    {
        
        // Combined mesh data
        List<int>       trianglesSubId;
        List<List<int>> triangles;
        List<Vector3>   vertices;
        List<Vector3>   normals;
        List<Vector2>   uvs;
        List<Vector4>   tangents;
        
        // Constructor
        public RFCombineMesh()
        {
            trianglesSubId = new List<int>();
            triangles      = new List<List<int>>();
            vertices       = new List<Vector3>();
            normals        = new List<Vector3>();
            uvs            = new List<Vector2>();
            tangents       = new List<Vector4>();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Combine
        /// /////////////////////////////////////////////////////////
        
        // Set combined mesh data
        public static RFCombineMesh GetCombinedMesh(Transform transForm, List<Mesh> meshList, List<Transform> transList, List<List<int>> matIdList, List<bool> invertNormals)
        {
            // Check all meshes and convert to tris
            int meshVertIdOffset = 0;
            RFCombineMesh cMesh  = new RFCombineMesh();
            
            Mesh mesh;
            for (int m = 0; m < meshList.Count; m++)
            {
                // Get local mesh
                mesh = meshList[m];

                // Collect combined vertices list
                cMesh.vertices.AddRange(mesh.vertices.Select(t => transForm.InverseTransformPoint(transList[m].TransformPoint(t))));

                // Collect combined normals list
                cMesh.normals.AddRange(invertNormals[m] == true
                    ? mesh.normals.Select(o => -o).ToList()
                    : mesh.normals.ToList());

                // Collect combined uvs list
                cMesh.uvs.AddRange(mesh.uv.ToList());

                // Collect combined tangents list TODO FLIP NORMAL FOR INVERTED
                cMesh.tangents.AddRange(mesh.tangents.ToList());
                
                // Iterate every submesh
                for (int s = 0; s < mesh.subMeshCount; s++)
                {
                    // Get all triangles verts ids
                    int[] tris = mesh.GetTriangles(s);

                    // Invert normals
                    if (invertNormals[m] == true)
                        tris = tris.Reverse().ToArray();

                    // Increment by mesh vertices id offset
                    for (int i = 0; i < tris.Length; i++)
                        tris[i] = tris[i] + meshVertIdOffset;
                    
                    // Collect triangles with material which already has other triangles. >> add to existing list
                    if (cMesh.trianglesSubId.Contains(matIdList[m][s]) == true) // TODO change to hashset
                    {
                        int ind = cMesh.trianglesSubId.IndexOf(matIdList[m][s]);
                        cMesh.triangles[ind].AddRange(tris.ToList());
                    }
                    else
                    {
                        // Collect sub mesh triangles >> Create new list
                        cMesh.triangles.Add(tris.ToList());
                                            
                        // Check every triangle and collect tris material id
                        cMesh.trianglesSubId.Add(matIdList[m][s]);
                    }
                }

                // Offset verts ids per mesh
                meshVertIdOffset += mesh.vertices.Length;
            }

            return cMesh;
        }
        
        // Create combined mesh
        public static Mesh CreateMesh (RFCombineMesh cMesh, string name)
        {
            // Create combined mesh
            Mesh newMesh = new Mesh();
            newMesh.name = name + "_Comb";
            newMesh.SetVertices(cMesh.vertices);
            
            newMesh.SetNormals(cMesh.normals);
            newMesh.SetUVs(0, cMesh.uvs);
            newMesh.SetTangents(cMesh.tangents);
            
            // Set triangles by submeshes
            newMesh.subMeshCount = cMesh.trianglesSubId.Count;
            for (int i = 0; i < cMesh.trianglesSubId.Count; i++)
                newMesh.SetTriangles(cMesh.triangles[i], cMesh.trianglesSubId[i]);
        
            // Recalculate
            newMesh.RecalculateNormals();
            newMesh.RecalculateBounds();
            newMesh.RecalculateTangents();

            return newMesh;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Shatter
        /// /////////////////////////////////////////////////////////
                      
        // Combine list of meshfilters
        public static Mesh CombineShatter (RayfireShatter shatter, Transform root, List<MeshFilter> filters)
        {
            // Get meshes and tms
            List<Mesh>           meshList  = new List<Mesh>();
            List<Transform>      transList = new List<Transform>();
            List<List<Material>> matList   = new List<List<Material>>();
            
            //  Verts amount // TODO max vert amount check
            // int totalVerts = 0;

            // Collect meshes, transforms and materials by meshfilter list
            GetMeshTransMatLists (filters, ref meshList, ref transList, ref matList, 4, 0.05f);
            
            // Get all materials list
            List<Material> allMaterials = GetAllMaterials(transList, matList);

            // Set materials list to shatter
            shatter.materials = allMaterials.ToArray();
            
            // Collect material ids per submesh
            List<List<int>> matIdList = GetMatIdList(transList, matList, allMaterials);
            
            // Get invert list
            List<bool> invertNormals = GetInvertList(transList);
            
            // Create combined mesh data
            RFCombineMesh cMesh = GetCombinedMesh(root, meshList, transList, matIdList, invertNormals);
            
            // Create combined mesh and return
            return CreateMesh (cMesh, root.name);
        }

        // Collect meshes, transforms and materials by meshfilter list
        static void GetMeshTransMatLists (List<MeshFilter> filters, ref List<Mesh> meshList, ref List<Transform> transList, ref List<List<Material>> matList, int verts, float size)
        {
            // Collect mesh, tm and mats for meshfilter
            foreach (var mf in filters)
            {
                // Filters
                if (mf.sharedMesh.vertexCount < verts)
                    continue;
                MeshRenderer mr = mf.GetComponent<MeshRenderer>();
                if (mr != null && mr.bounds.size.magnitude < size)
                    continue;
                
                // Collect mats
                List<Material> mats = new List<Material>();
                if (mr != null)
                    mats = mr.sharedMaterials.ToList();
                
                // Collect
                meshList.Add(mf.sharedMesh);
                transList.Add(mf.transform);
                matList.Add(mats);
            }
        }
        
        
        // Get all materials list
        public static List<Material> GetAllMaterials(List<Transform> transList, List<List<Material>> matList)
        {
            List<Material> allMaterials = new List<Material>();
            for (int f = 0; f < transList.Count; f++)
                for (int m = 0; m < matList[f].Count; m++)
                    if (allMaterials.Contains(matList[f][m]) == false)
                        allMaterials.Add(matList[f][m]);
            return allMaterials;
        }
        
        // Collect material ids per submesh
        public static List<List<int>> GetMatIdList(List<Transform> transList, List<List<Material>> matList, List<Material> allMaterials)
        {
            List<List<int>> matIdList = new List<List<int>>();
            for (int f = 0; f < transList.Count; f++)
                matIdList.Add(matList[f].Select(t => allMaterials.IndexOf(t)).ToList());
            return matIdList;
        }

        // Get invert list by transforms
        public static List<bool> GetInvertList(List<Transform> transList)
        {
            List<bool> invertNormals = new List<bool>();
            for (int f = 0; f < transList.Count; f++)
            {
                // Get invert normals because of negative scale
                bool invert = false;
                if (transList[f].localScale.x < 0) 
                    invert = !invert;
                if (transList[f].localScale.y < 0) 
                    invert = !invert;
                if (transList[f].localScale.z < 0) 
                    invert = !invert;
                invertNormals.Add(invert);
            }
            return invertNormals;
        }
    }
    
    [AddComponentMenu("RayFire/Rayfire Combine")]
    [HelpURL("https://rayfirestudios.com/unity-online-help/components/unity-combine-component/")]
    public class RayfireCombine : MonoBehaviour
    {
        public enum CombType
        {
            Children = 0,
            ObjectsList = 1,
        }
        
        public CombType         type; 
        public List<GameObject> objects;
        public bool             meshFilters     = true;
        public bool             skinnedMeshes   = true;
        public bool             particleSystems = true;
        public float            sizeThreshold   = 0.1f;
        public int              vertexThreshold = 5;
        
        private Transform                    transForm;
        private MeshFilter                   mFilter;
        private MeshRenderer                 mRenderer;
        private List<bool>                   invertNormals;
        private List<Transform>              transList;
        private List<MeshFilter>             mFilters;
        private List<SkinnedMeshRenderer>    skinnedMeshRends;
        private List<ParticleSystemRenderer> particleRends;
        private List<Mesh>                   meshList;
        private List<List<int>>              matIdList;
        private List<List<Material>>         matList;

        // Combined mesh data
        private List<Material> allMaterials;

        /// /////////////////////////////////////////////////////////
        /// Combine
        /// /////////////////////////////////////////////////////////
        
        // Combine meshes
        public void Combine()
        {
            // Set combine data
            if (SetData() == false)
                return;
            
            // Get combine mesh data
            RFCombineMesh cMesh = RFCombineMesh.GetCombinedMesh(transForm, meshList, transList, matIdList, invertNormals);
            
            // Set mesh to object
            mFilter.sharedMesh = RFCombineMesh.CreateMesh (cMesh, name);
            
            // Set mesh renderer and materials
            mRenderer.sharedMaterials = allMaterials.ToArray();
        }

        // Set data
        bool SetData()
        {
            transForm = GetComponent<Transform>();
            
            // Reset mesh
            mFilter = GetComponent<MeshFilter>();
            if (mFilter == null)
                mFilter = gameObject.AddComponent<MeshFilter>();
            mFilter.sharedMesh = null;
            
            // Reset mesh renderer
            mRenderer = GetComponent<MeshRenderer>();
            if (mRenderer == null)
                mRenderer = gameObject.AddComponent<MeshRenderer>();
            mRenderer.sharedMaterials = new Material[]{};

            // Get targets
            if (type == CombType.Children)
            {
                if (meshFilters == true)
                    mFilters = GetComponentsInChildren<MeshFilter>().ToList();
                if (skinnedMeshes == true)
                    skinnedMeshRends = GetComponentsInChildren<SkinnedMeshRenderer>().ToList();
                if (particleSystems == true)
                    particleRends = GetComponentsInChildren<ParticleSystemRenderer>().ToList();
            }
            if (type == CombType.ObjectsList)
            {
                mFilters = new List<MeshFilter>();
                if (meshFilters == true)
                {
                    foreach (var obj in objects)
                    {
                        MeshFilter mf = obj.GetComponent<MeshFilter>();
                        if (mf != null)
                            if (mf.sharedMesh != null)
                                mFilters.Add (mf);
                    }
                }

                skinnedMeshRends = new List<SkinnedMeshRenderer>();
                if (skinnedMeshes == true)
                {
                    foreach (var obj in objects)
                    {
                        SkinnedMeshRenderer sk = obj.GetComponent<SkinnedMeshRenderer>();
                        if (sk != null)
                            if (sk.sharedMesh != null)
                                skinnedMeshRends.Add (sk);
                    }
                }

                particleRends = new List<ParticleSystemRenderer>();
                if (particleSystems == true)
                {
                    foreach (var obj in objects)
                    {
                        ParticleSystemRenderer pr = obj.GetComponent<ParticleSystemRenderer>();
                        if (pr != null)
                            particleRends.Add (pr);
                    }
                }
            }

            // Clear mesh filters without mesh
            for (int i = mFilters.Count - 1; i >= 0; i--)
                if (mFilters[i].sharedMesh == null)
                    mFilters.RemoveAt (i);

            // Clear skinned meshes without mesh
            if (skinnedMeshRends != null && skinnedMeshRends.Count > 0)
                for (int i = skinnedMeshRends.Count - 1; i >= 0; i--)
                    if (skinnedMeshRends[i].sharedMesh == null)
                        skinnedMeshRends.RemoveAt(i);
            
            // Get meshes and tms
            meshList   = new List<Mesh>();
            transList  = new List<Transform>();
            matList    = new List<List<Material>>();
            
            //  Verts amount
            int totalVerts = 0;
            
            // Collect mesh, tm and mats for meshfilter
            if (mFilters != null && mFilters.Count > 0)
                foreach (var mf in mFilters)
                {
                    // Filters
                    if (mf.sharedMesh.vertexCount < vertexThreshold)
                        continue;
                    MeshRenderer mr = mf.GetComponent<MeshRenderer>();
                    if (mr != null && mr.bounds.size.magnitude < sizeThreshold)
                        continue;
                    
                    // Collect mats
                    List<Material> mats = new List<Material>();
                    if (mr != null)
                        mats = mr.sharedMaterials.ToList();
                    
                    // Collect
                    meshList.Add(mf.sharedMesh);
                    transList.Add(mf.transform);
                    matList.Add(mats);
                    
                    // Collect verts
                    totalVerts += mf.sharedMesh.vertexCount;
                }
            
            // Collect mesh, tm and mats for skinned mesh
            if (skinnedMeshRends != null && skinnedMeshRends.Count > 0)
                foreach (var sk in skinnedMeshRends)
                {
                    // SKip by vertex amount
                    if (sk.sharedMesh.vertexCount < vertexThreshold)
                        continue;
                    if (sk.bounds.size.magnitude < sizeThreshold)
                        continue;
                    
                    // Collect
                    meshList.Add(RFMesh.BakeMesh(sk));
                    transList.Add(sk.transform);
                    matList.Add(sk.sharedMaterials.ToList());
                    
                    // Collect verts
                    totalVerts += sk.sharedMesh.vertexCount;
                }
            
            // Particle system
            if (particleRends != null && particleRends.Count > 0)
            {
                GameObject g = new GameObject();
                
                foreach (var pr in particleRends)
                {
                    Mesh m = new Mesh();
                    pr.BakeMesh (m, true);
                    
                    // SKip by vertex amount
                    if (m.vertexCount < vertexThreshold)
                        continue;
                    if (m.bounds.size.magnitude < sizeThreshold)
                        continue;
                    
                    // Collect
                    meshList.Add (m);
                    transList.Add (g.transform);
                    matList.Add (pr.sharedMaterials.ToList());
                    
                    // Collect verts
                    totalVerts += m.vertexCount;
                }

                DestroyImmediate (g);
            }

            // No meshes
            if (meshList.Count == 0)
            {
                Debug.Log("No meshes to combine");
                return false;
            }

            // Vert limit reached
            if (totalVerts >= 65535)
            {
                Debug.Log ("RayFire Combine: " + name + " Too much vertices: " + totalVerts + ".  Unity doesn't support meshes with more than 65535 vertices.", gameObject);
                return false;
            }
            
            // Get invert list by transforms
            invertNormals = RFCombineMesh.GetInvertList(transList);
            
            // Get all materials list
            allMaterials = RFCombineMesh.GetAllMaterials(transList, matList);
            
            // Collect material ids per submesh
            matIdList = RFCombineMesh.GetMatIdList(transList, matList, allMaterials);
            
            return true;
        }
        
        // /////////////////////////////////////////////////////////
        // Other
        // /////////////////////////////////////////////////////////

        
        
/*      public void Detach()
        {
            meshFilter = GetComponent<MeshFilter>();
            transForm = GetComponent<Transform>();
            
            // Get all triangles with verts data
            List<Tri> tris = GetTris(meshFilter.sharedMesh);
            
            // Set neib tris
            for (int i = 0; i < tris.Count; i++)
                foreach (var tri in tris)
                    //if (tri.neibTris.Count < 3)
                        if (CompareTri(tris[i], tri) == true)
                        {
                            tris[i].neibTris.Add(tri);
                            //tri.neibTris.Add(tris[i]);
                        }
            
            elements = new List<Element>();
            int subMeshId = 0;

            while (tris.Count > 0)
            {
                List<Tri> subTris = new List<Tri>();
                List<Tri> checkTris = new List<Tri>();
                checkTris.Add(tris[0]);
                
                while (checkTris.Count > 0)
                {
                    
                    if (subTris.Contains(checkTris[0]) == false)
                    {
                        checkTris[0].subMeshId = subMeshId;
                        subTris.Add(checkTris[0]);
                        
                        int ind = tris.IndexOf(checkTris[0]);
                        if (ind >= 0)
                            tris.RemoveAt(ind);
                    }
                    
                    foreach (var neibTri in checkTris[0].neibTris)
                        if (subTris.Contains(neibTri) == false)
                            checkTris.Add(neibTri);
                    checkTris.RemoveAt(0);
                }
                
                Element elem = new Element();
                elem.tris.AddRange(subTris);
                elements.Add(elem);
                subMeshId++;
            }
        }

        // Match tris by shared verts
        private bool CompareTri(Tri tri1, Tri tri2)
        {
            if (tri1 == tri2)
                return false;
            foreach (int id in tri1.ids)
                if (tri2.ids.Contains(id) == true)
                    return true;
            return false;
        }
        
       //[ContextMenu("MeshData")]
        public void GetMeshData()
        {
            meshFilter = GetComponent<MeshFilter>();

            // Check for same position
            List<WeldGroup> weldGroups = GetWeldGroups(meshFilter.sharedMesh.vertices,  0.001f);
           
            // Get all triangles with verts data
            List<Tri> tris = GetTris(meshFilter.sharedMesh);
            
            // Create new tri list with modified tri. Excluded welded vertices
            List<int> remapVertIds = new List<int>();
            List<int> excludeVertIds = new List<int>();
            foreach (WeldGroup weld in weldGroups)
                for (int i = 1; i < weld.verts.Count; i++)
                {
                    remapVertIds.Add(weld.verts[0]);
                    excludeVertIds.Add(weld.verts[i]);
                }
   
            // Remap vertices for tris
            foreach (Tri tri in tris)
            {
                for (int i = 0; i < tri.ids.Count; i++)
                {
                    for (int j = 0; j < excludeVertIds.Count; j++)
                    {
                        if (tri.ids[i] == excludeVertIds[j])
                        {
                            tri.ids[i] = remapVertIds[j];
                            tri.vpos[i] = meshFilter.sharedMesh.vertices[tri.ids[i]];
                            tri.vnormal[i] = meshFilter.sharedMesh.normals[tri.ids[i]];
                        } 
                    }
                }
            }
            
            // Set new triangles array
            List<int> newTriangles = new List<int>();
            foreach (Tri tri in tris)
                newTriangles.AddRange(tri.ids);
            GameObject go = new GameObject();
            go.transform.position = transform.position + new Vector3(0, 0, 1.5f);
            go.transform.rotation = transform.rotation;
            MeshFilter mf = go.AddComponent<MeshFilter>();
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterials = GetComponent<MeshRenderer>().sharedMaterials;
            
            Mesh mesh = new Mesh();
            mesh.name = meshFilter.sharedMesh.name + "_welded";
            mesh.vertices = meshFilter.sharedMesh.vertices;
            mesh.triangles = newTriangles.ToArray();
            mesh.normals = meshFilter.sharedMesh.normals;
            mesh.uv = meshFilter.sharedMesh.uv;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mf.sharedMesh = mesh;
        }

        // Get tris
        List<Tri> GetTris(Mesh mesh)
        {
            List<Tri> tris = new List<Tri>();
            for (int i = 0; i < mesh.triangles.Length; i++)
            {
                Tri tri = new Tri();
                
                // Gt vert ids
                int id0 = mesh.triangles[i + 0];
                int id1 = mesh.triangles[i + 1];
                int id2 = mesh.triangles[i + 2];
                
                // Save vert id
                tri.ids.Add(id0); 
                tri.ids.Add(id1); 
                tri.ids.Add(id2);
                
                // Save vert position
                tri.vpos.Add(mesh.vertices[id0]);
                tri.vpos.Add(mesh.vertices[id1]);
                tri.vpos.Add(mesh.vertices[id2]);
                
                // Save normal
                tri.vnormal.Add(mesh.normals[id0]);
                tri.vnormal.Add(mesh.normals[id1]);
                tri.vnormal.Add(mesh.normals[id2]);
                
                i += 2;
                
                tris.Add(tri);
            }
            return tris;
        }
        
        // Get index of vertex which share same/close position by threshold
        List<WeldGroup> GetWeldGroups(Vector3[] vertices, float threshold)
        {
            List<int> list = new List<int>();
            List<WeldGroup> weldGroups = new List<WeldGroup>();
            for (int i = 0; i < vertices.Length; i++)
            {
                // Already checked
                if (list.Contains(i) == true)
                    continue;
                
                WeldGroup weld = new WeldGroup();
                for (int v = 0; v < vertices.Length; v++)
                {
                    // Comparing with self
                    if (i == v)
                        continue;
                  
                    // Already checked
                    if (list.Contains(v) == true)
                        continue;
                        
                    // Save if close
                    if (Vector3.Distance(vertices[i], vertices[v]) < threshold)
                    {
                        list.Add(v);

                        if (weld.verts.Contains(i) == false)
                            weld.verts.Add(i);
                        
                        if (weld.verts.Contains(v) == false)
                            weld.verts.Add(v);
                    }
                }
                
                if (weld.verts.Count > 0)
                    weldGroups.Add(weld);
            }
            
            return weldGroups;
        }*/

    }
}


