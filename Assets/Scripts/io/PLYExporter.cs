using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine.Assertions;



// http://wiki.unity3d.com/index.php/ObjExporter#ObjExporter.cs
// https://stackoverflow.com/questions/46733430/convert-mesh-to-stl-obj-fbx-in-runtime
public class PLYExporter
{

    public static string MeshToString(Mesh m)
    {
        //Mesh m = mf.mesh;

        StringBuilder sb = new StringBuilder();


        sb.Append("ply").Append("\n");
        sb.Append("format ascii 1.0").Append("\n");
        sb.Append("comment Generated with HDRPSyntheticDataGenerator").Append("\n");


        sb.Append(string.Format("element vertex {0}\n", m.vertices.Length));
        sb.Append("property float x").Append("\n");
        sb.Append("property float y").Append("\n");
        sb.Append("property float z").Append("\n");

        Assert.AreEqual(m.normals.Length, m.vertices.Length);
        sb.Append("property float nx").Append("\n");
        sb.Append("property float ny").Append("\n");
        sb.Append("property float nz").Append("\n");


        //Assert.AreEqual(m.uv.Length, m.vertices.Length);
        sb.Append("property float texture_u").Append("\n");
        sb.Append("property float texture_v").Append("\n");


        int triangleCount = 0;
        for (int i = 0; i < m.subMeshCount; ++i)
            triangleCount += m.GetTriangles(i).Length/3;
        sb.Append(string.Format("element face {0}\n", triangleCount));
        sb.Append("property list uchar int vertex_index").Append("\n"); ;

        sb.Append("end_header").Append("\n");



        for (int i=0; i < m.vertices.Length; ++i)
        {
            sb.Append(string.Format("{0} {1} {2} {3} {4} {5} {6} {7}\n", m.vertices[i].x, m.vertices[i].y, m.vertices[i].z, m.normals[i].x, m.normals[i].y, m.normals[i].z, m.uv[i].x, m.uv[i].y));
        }
        

        for (int material = 0; material < m.subMeshCount; material++)
        {
            int[] triangles = m.GetTriangles(material);

            for (int i = 0; i < triangles.Length; i += 3)
            {
                sb.Append(string.Format("3 {0} {1} {2}\n", triangles[i], triangles[i + 1], triangles[i + 2]));
            }
        }
        return sb.ToString();
    }

    public static void MeshToFile(Mesh m, string filename)
    {
        using (StreamWriter sw = new StreamWriter(filename))
        {
            sw.Write(MeshToString(m));
        }
    }
}