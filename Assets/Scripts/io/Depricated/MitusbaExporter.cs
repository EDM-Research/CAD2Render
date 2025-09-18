//Copyright (c) 2020 Nick Michiels <nick.michiels@uhasselt.be>, Hasselt University, Belgium, All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Numerics;


using static MitsubaXML;
public class MitusbaExporter
{
    
    private static void AddShapeToScene(ref Scene scene, UnityEngine.GameObject model, string path, bool withTexture = false)
    {
        UnityEngine.Matrix4x4 inversionMatrix = UnityEngine.Matrix4x4.zero;
        inversionMatrix.m00 = -1;
        inversionMatrix.m11 = 1;
        inversionMatrix.m22 = 1;
        inversionMatrix.m33 = 1;

        Shape shapegroup = new Shape();
        shapegroup.type = "shapegroup";
        shapegroup.id = model.name;
        shapegroup.shapes = new List<Shape>();
        scene.shapes.Add(shapegroup);


        UnityEngine.Matrix4x4 inverseParent = model.transform.localToWorldMatrix.inverse;


        // iterate over parent and all children
        foreach (Transform child in model.transform.GetComponentsInChildren<Transform>())
        {
            string objName = model.name + "_" + child.gameObject.name + ".obj";
            string objPath = Path.Combine(path, objName);

            MeshFilter meshFilter;
            if (meshFilter = child.gameObject.GetComponent<MeshFilter>())
            {

                ObjExporter.MeshToFile(meshFilter, objPath);

    
                Shape_OBJ shape = new Shape_OBJ();
                // TODO: ENABLE THIS IF CHILD OBJECTS ARE TRANSFORMED COMPARED TO PARENT
                shape.toWorld(inversionMatrix * (inverseParent * child.transform.localToWorldMatrix) * inversionMatrix);
                shape.filename = new XMLFilename(objName);

                //Renderer rend = model.GetComponent<>();
                Renderer rend = model.GetComponentInChildren<Renderer>();

                Color color = new Color(1.0f, 1.0f, 1.0f);
                if (rend != null)
                {
                    if (rend.material.HasProperty("_Color"))
                        color = rend.material.GetColor("_Color");
                    if (rend.material.HasProperty("_BaseColor"))
                        color = rend.material.GetColor("_BaseColor");
                    //Debug.Log(color);
                }
                else
                {
                    ;// Debug.Log(model.name);
                }




                XMLBSDF bsdf_plastic;
                bsdf_plastic = BuildRoughPlasticShader(rend.material);
                shape.bsdf = bsdf_plastic;

                ////Debug.Log("Shader: " + rend.material.shader.name);

                //if (rend.material.shader.name.Equals("Shader Graphs/PlasticNormal") || rend.material.shader.name.Equals("Shader Graphs/PlasticDirt"))
                //{
                //    bsdf_plastic = BuildPlasticShader(rend.material);
                //    shape.bsdf = bsdf_plastic;
                //}
                //else if (rend.material.shader.name.Equals("Shader Graphs/WoodAnisotropy") || rend.material.shader.name.Equals("Shader Graphs/WoodCoatAnisotropy") || rend.material.shader.name.Equals("Shader Graphs/Wood") || rend.material.shader.name.Equals("Shader Graphs/WoodCoat"))
                //{
                //    bsdf_plastic = BuildRoughPlasticShader(rend.material);
                //    shape.bsdf = bsdf_plastic;
                //}
                //else
                //{

                //    XMLBSDF_Plastic bsdf = new XMLBSDF_Plastic();
                //    bsdf.int_ior.value = 1.9f;
                //    bsdf.setDiffuseReflectanceColor(color);


   


                //    shape.bsdf = bsdf;
                //}
                shapegroup.shapes.Add(shape);
            }
        }
    }

    public static XMLBSDF BuildRoughPlasticShader(Material mat)
    {
        string[] diffuseColorProperties = new string[] { "_Color", "_ColorTint", "_BaseColor", "_PaintColor"};
        string[] diffuseColorTexProperties = new string[] { "Texture2D_D9FF89B8", "Texture2D_ADF9B91C", "Texture2D_4D3C9E50", "Texture2D_B9CEC4F9" };
        string[] normalMapProperties = new string[] { "Texture2D_6F897852", "Texture2D_F5401D3B", "Texture2D_DDB108FE", "Texture2D_524261BE" };
        string[] intIORProperties = new string[] { "Vector1_A98DF435" , "Vector1_8844C7FB", "Vector1_85B7D83E" };
        string[] smoothnessProperties = new string[] { "Vector1_B649E178" , "Vector1_D25D46D6", "Vector1_2BC3876C" , "Vector1_B8A2FB56", "Vector1_E8712278", "Vector1_745B54BF"};
        string[] anisotropyProperties = new string[] { "Vector1_DFEF8972", "Vector1_A9269C49", "Vector1_695D2DEB", "Vector1_1D63BEA4"};
        string[] metalicIntensityProperties = new string[] { "Vector1_856C34E2", "Vector1_D751A8F5" , "Vector1_BD53BAF8", "Vector1_96F85197"};
        string[] tilingProperties = new string[] { "Vector2_AB87CEC4", "Vector2_793BAC4B" , "Vector2_8DD26402", "Vector2_793BAC4B", "Vector2_3344450D", "Vector2_D3BC5680" };
        string[] offsetProperties = new string[] { };
        //Debug.Log("BuildRoughPlasticShader");
        XMLBSDF_RoughPlastic bsdf_plastic = new XMLBSDF_RoughPlastic();

        // see if tiling is required
        UnityEngine.Vector4 tiling = new UnityEngine.Vector4(1,1,1,1);
        foreach (string tilingProperty in tilingProperties)
        {
            if (mat.HasProperty(tilingProperty))
            {
                tiling = mat.GetVector(tilingProperty);
                break;
            }   
        }

        // see if tiling offset is required
        UnityEngine.Vector4 offset = new UnityEngine.Vector4(0, 0, 0, 0);
        foreach (string offsetProperty in offsetProperties)
        {
            if (mat.HasProperty(offsetProperty))
            {
                offset = mat.GetVector(offsetProperty);
                break;
            }
        }


        bool foundDiffuseTex = false;
        foreach( string diffuseTexProperty in diffuseColorTexProperties)
        {
            if (mat.HasProperty(diffuseTexProperty))
            {
                bsdf_plastic.setDiffuseReflectanceTexture("../" + UnityEditor.AssetDatabase.GetAssetPath(mat.GetTexture(diffuseTexProperty)));
                bsdf_plastic.diffuse_reflectance_tex.scale(tiling.x, tiling.y);
                bsdf_plastic.diffuse_reflectance_tex.translate(offset.x, offset.y);
                foundDiffuseTex = true;
                break;
            }
        }
        if (!foundDiffuseTex)
        {
            Color color = new Color(1.0f, 1.0f, 1.0f);
            foreach (string diffuseColorProperty in diffuseColorProperties)
            {
                if (mat.HasProperty(diffuseColorProperty))
                {
                    color = mat.GetColor(diffuseColorProperty);
                    break;
                }
            }
            bsdf_plastic.setDiffuseReflectanceColor(color);
        }


        float int_ior = -1.0f;
        foreach (string intIORProperty in intIORProperties)
        {
            if (mat.HasProperty(intIORProperty))
            {
                int_ior = mat.GetFloat(intIORProperty);
                bsdf_plastic.int_ior.value = int_ior;
                break;
            }
        }
      



        float smoothness = 0.1f;
        foreach (string smoothnessProperty in smoothnessProperties)
        {
            if (mat.HasProperty(smoothnessProperty))
            {
                smoothness = 1.0f - mat.GetFloat(smoothnessProperty);
                bsdf_plastic.alpha.value = smoothness;
                break;
            }
        }
        bsdf_plastic.alpha.value = smoothness;



        string normalMap;

        foreach (string normalMapProperty in normalMapProperties)
        {
            if (mat.HasProperty(normalMapProperty))
            {
                normalMap = "../" + UnityEditor.AssetDatabase.GetAssetPath(mat.GetTexture(normalMapProperty));
                XMLBSDF_NormalMap normalMapBSDF = new XMLBSDF_NormalMap(normalMap, bsdf_plastic, tiling, offset);
                return normalMapBSDF;
            }
        }

        return bsdf_plastic;


    }



    public static XMLBSDF BuildPlasticShader(Material mat)
    {


        Debug.Log("BuildPlasticShader");

        XMLBSDF_Plastic bsdf_plastic = new XMLBSDF_Plastic();

        Color color = new Color(1.0f, 1.0f, 1.0f);
        if (mat.HasProperty("_Color"))
            color = mat.GetColor("_Color");
        bsdf_plastic.setDiffuseReflectanceColor(color);

        float ior = 1.9f;
        if (mat.HasProperty("Vector1_85B7D83E"))
            ior = mat.GetFloat("Vector1_85B7D83E");
        bsdf_plastic.int_ior.value = ior;



        string normalMap;
        if (mat.HasProperty("Texture2D_520ACDA6"))
        {
            normalMap = "../" + UnityEditor.AssetDatabase.GetAssetPath(mat.GetTexture("Texture2D_520ACDA6"));
            UnityEngine.Vector4 tiling = new UnityEngine.Vector4(1, 1, 1, 1);
            UnityEngine.Vector4 offset = new UnityEngine.Vector4(0, 0, 0, 0);
            XMLBSDF_NormalMap normalMapBSDF = new XMLBSDF_NormalMap(normalMap, bsdf_plastic, tiling, offset);
            return normalMapBSDF;
        }
        else
        {
            return bsdf_plastic;
        }


        
    }


    //public static XMLBSDF BuildHDRLITShader(Material mat)
    //{
    //    XMLBSDF bsdf = new XMLBSDF();

    //    string texPath = "../" + UnityEditor.AssetDatabase.GetAssetPath(mat.GetTexture("Texture2D_D9FF89B8"));
    //    bsdf.setDiffuseReflectanceTexture(texPath);


    //    return bsdf;
    //}

    //public static void SaveMitsuba(GameObject model, Camera camera)
    public static void SaveMitsuba(List<UnityEngine.GameObject> models, int fileID, string outputPath, Camera camera, int width = 512, int height = 512, int sampleCount = 128, string environmentMap = "museum.exr", float env_exposure = 1.0f, float env_angle = 0.0f, UnityEngine.GameObject table = null)
    {
        string path = Path.Combine(outputPath, "mitsuba/");
        //Create Directory if it does not exist
        if (!Directory.Exists(Path.GetDirectoryName(path)))
        {
            Debug.Log("create dir " + path);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
        }



        UnityEngine.Matrix4x4 inversionMatrix = UnityEngine.Matrix4x4.zero;
        inversionMatrix.m00 = -1;
        inversionMatrix.m11 = 1;
        inversionMatrix.m22 = 1;
        inversionMatrix.m33 = 1;



        // First generate all different prefabs in shapegroups
        Scene scene_shapegroups = new Scene();
        scene_shapegroups.shapes = new List<Shape>();

        



        Scene scene = new Scene();
        scene.AddInclude(fileID + "_shapegroups.xml");
        scene.integrator = new XMLIntegrator();
        scene.shapes = new List<Shape>();

        foreach (UnityEngine.GameObject model in models)
        {
            AddShapeToScene(ref scene_shapegroups, model, path);
            Shape_Instance instance = new Shape_Instance(model.name);
            instance.toWorld(inversionMatrix * model.transform.localToWorldMatrix * inversionMatrix);
            
            scene.shapes.Add(instance);
        }
        if (table)
        {
            AddShapeToScene(ref scene_shapegroups, table, path, true);

            Shape_Instance instance = new Shape_Instance(table.name);
            instance.toWorld(inversionMatrix * table.transform.localToWorldMatrix * inversionMatrix);

            scene.shapes.Add(instance);
        }

        scene.sensor = new Sensor();
        scene.sensor.setFOV(60);
        //scene.sensor.lookat(new UnityEngine.Vector3(-200.0f, 0.0f, -200.0f), new UnityEngine.Vector3(-50f, 0f, 0f), new UnityEngine.Vector3(0.0f, 1.0f, 0.0f));

        //UnityEngine.Matrix4x4 inversionMatrixZ = UnityEngine.Matrix4x4.zero;
        //inversionMatrixZ.m00 = 1;
        //inversionMatrixZ.m11 = 1;
        //inversionMatrixZ.m22 = -1;
        //inversionMatrixZ.m33 = 1;
        //UnityEngine.Matrix4x4 view = camera.cameraToWorldMatrix * inversionMatrixZ; // cameraToWorldMatrix is in opengl standards (z flipped), otherwise use transform.localToWorld (https://forum.unity.com/threads/reproducing-cameras-worldtocameramatrix.365645/)
        UnityEngine.Matrix4x4 view = camera.transform.localToWorldMatrix;

        scene.sensor.toWorld(inversionMatrix* view * inversionMatrix);
        //Debug.Log(camera.cameraToWorldMatrix);
        //Debug.Log(camera.transform.localToWorldMatrix);

        //Debug.Log(camera.worldToCameraMatrix);
        //Debug.Log(camera.transform.worldToLocalMatrix);
        scene.emitter = new XMLEmitter();
        scene.emitter.setEnvironmentMap("../" + environmentMap);
        scene.emitter.scale.value = env_exposure;
        scene.emitter.transform = new XMLTransform();
        scene.emitter.transform.rotates = new List<XMLRotate>();
        scene.emitter.transform.rotates.Add(new XMLRotateY(env_angle));



        scene.sensor.film.width.value = width;
        scene.sensor.film.height.value = height;
        scene.sensor.sampler.sample_count.value = sampleCount;
        


        WriteXMLToFile(ref scene_shapegroups, Path.Combine(path, fileID + "_shapegroups.xml"));
        WriteXMLToFile(ref scene, Path.Combine(path, fileID + ".xml"));
    }








        
}
