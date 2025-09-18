//Copyright (c) 2020 Nick Michiels <nick.michiels@uhasselt.be>, Hasselt University, Belgium, All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Numerics;

public static class MitsubaXML
{
    public class XMLBSDF
    {
        [XmlAttribute("type")]
        public string type { get; set; }



    }



    public class XMLTexture
    {
        public XMLTexture() { }

        public XMLTexture(string filename, string type, string name, string ch = "all")
        {
            this.xmltype = type;
            this.xmlname = name;
            this.filename.xmlvalue = filename;
            //if (ch.CompareTo("all") != 0)
            //{
            //    this.channel = new XMLString("channel", ch);
            //}
        }

        [XmlAttribute("name")]
        public string xmlname { get; set; } = "reflectance";

        [XmlAttribute("type")]
        public string xmltype { get; set; } = "bitmap";


        [XmlElement("boolean", Order = 0)]
        public XMLBoolean raw;

        [XmlElement("string", Order = 1)]
        public XMLString filename = new XMLString("filename", "");



        [XmlElement("transform", Order = 2)]
        public XMLTransform transform { get; set; }

        public void scale(float x, float y)
        {
            if (transform == null)
                transform = new XMLTransform("toUV");
            transform.scale = new XMLScale(x, y);
        }

        public void translate(float x, float y)
        {
            if (transform == null)
                transform = new XMLTransform("toUV");
            transform.translate = new XMLTranslate(x, y);
        }


        //[XmlElement("string")]
        //public XMLString channel;
    }



    public class XMLBSDF_NormalMap : XMLBSDF
    {
        public XMLBSDF_NormalMap()
        {
            this.type = "normalmap";
        }

        public XMLBSDF_NormalMap(string filename, XMLBSDF nestedBSDF, UnityEngine.Vector4 tiling, UnityEngine.Vector4 offset)
        {
            this.type = "normalmap";
            this.bsdf = nestedBSDF;

            this.normalmap = new XMLTexture(filename, "bitmap", "normalmap");
            this.normalmap.scale(tiling.x, tiling.y);
            this.normalmap.translate(offset.x, offset.y);
            this.normalmap.raw = new XMLBoolean("raw", true);

        }


        [XmlElement("texture", Order = 0)]
        public XMLTexture normalmap;

        [XmlElement("bsdf", Order = 1)]
        public XMLBSDF bsdf;



    }

    public class XMLBSDF_Plastic : XMLBSDF
    {
        public XMLBSDF_Plastic()
        {
            this.type = "plastic";
        }

        [XmlElement("rgb", Order = 0)]
        public XMLRGB diffuse_reflectance = new XMLRGB("diffuseReflectance", new UnityEngine.Color(1.0f, 1.0f, 1.0f));

        [XmlElement("texture", Order = 1)]
        public XMLTexture diffuse_reflectance_tex;

        [XmlElement("float", Order = 2)]
        public XMLFloat int_ior = new XMLFloat("intIor", 1.0f);

        public void setDiffuseReflectanceColor(UnityEngine.Color color)
        {
            diffuse_reflectance = new XMLRGB("diffuseReflectance", color);
            diffuse_reflectance_tex = null;
        }

        public void setDiffuseReflectanceTexture(string texPath)
        {
            diffuse_reflectance_tex = new XMLTexture(texPath, "bitmap", "diffuseReflectance");
            diffuse_reflectance = null;
        }

    }




    public class XMLBSDF_RoughPlastic : XMLBSDF
    {
        public XMLBSDF_RoughPlastic()
        {
            this.type = "roughplastic";
        }

        [XmlElement("rgb", Order = 0)]
        public XMLRGB diffuse_reflectance = new XMLRGB("diffuseReflectance", new UnityEngine.Color(1.0f, 1.0f, 1.0f));

        [XmlElement("texture", Order = 1)]
        public XMLTexture diffuse_reflectance_tex;

        [XmlElement("float", Order = 2)]
        public XMLFloat int_ior = new XMLFloat("intIor", 1.49f);


        [XmlElement("float", Order = 3, Namespace = "dummy")]
        public XMLFloat alpha = new XMLFloat("alpha", 0.1f);

        [XmlElement("boolean", Order = 4)]
        public XMLBoolean nonlinear;


        public void setDiffuseReflectanceColor(UnityEngine.Color color)
        {
            diffuse_reflectance = new XMLRGB("diffuseReflectance", color);
            diffuse_reflectance_tex = null;
        }

        public void setDiffuseReflectanceTexture(string texPath)
        {
            diffuse_reflectance_tex = new XMLTexture(texPath, "bitmap", "diffuseReflectance");
            diffuse_reflectance = null;
        }

    }




    public class XMLFloat
    {
        public XMLFloat() { }

        public XMLFloat(string n, float val)
        {
            name = n;
            value = val;
        }

        [XmlAttribute("name")]
        public string name;

        [XmlAttribute("value")]
        public float value;
    }


    public class XMLBoolean
    {
        public XMLBoolean() { }

        public XMLBoolean(string n, Boolean val)
        {
            name = n;
            if (val)
                value = "true";
            else
                value = "false";
        }

        [XmlAttribute("name")]
        public string name;

        [XmlAttribute("value")]
        public string value;
    }



    public class XMLIntegrator
    {
        [XmlAttribute("type")]
        public string type = "path";

        [XmlElement("integer")]
        public XMLInteger max_depth;// = new XMLInteger("max_depth", 2);
    }





    public class XMLEmitter
    {
        public void setEnvironmentMap(string env)
        {
            type = "envmap";
            filename = new XMLString("filename", env);
        }

        [XmlAttribute("type")]
        public string type = "envmap";

        [XmlAttribute("id")]
        public string id;

        [XmlElement("float")]
        public XMLFloat scale = new XMLFloat("scale", 1.0f);

        [XmlElement("string")]
        public XMLString filename;

        [XmlElement("transform")]
        public XMLTransform transform;

    }


    [XmlRoot("sensor")]
    public class Sensor
    {
        [XmlAttribute("type")]
        public string type = "perspective";

        public void setFOV(float fov)
        {
            xmlFOV = new XMLFloat("fov", fov);
        }

        public void lookat(UnityEngine.Vector3 orgin, UnityEngine.Vector3 target, UnityEngine.Vector3 up)
        {
            if (transform == null)
                transform = new XMLTransform();
            transform.lookat = new XMLLookAt(orgin, target, up);
        }

        public void toWorld(UnityEngine.Matrix4x4 toWorld)
        {
            if (transform == null)
                transform = new XMLTransform();
            transform.matrix = new XMLMatrix(toWorld);
        }

        [XmlElement("float", Order = 1)]
        public XMLFloat xmlFOV { get; set; }

        [XmlElement("transform", Order = 0)]
        public XMLTransform transform = null;

        [XmlElement("film", Order = 2)]
        public XMLFilm film = new XMLFilm();

        [XmlElement("sampler", Order = 3)]
        public XMLSampler sampler = new XMLSampler();
    }



    public class XMLSampler
    {
        [XmlAttribute("type")]
        public string type = "independent";

        [XmlElement("integer")]
        public XMLInteger sample_count = new XMLInteger("sample_count", 16);
    }


    public class XMLFilm
    {
        [XmlAttribute("type")]
        public string type = "hdrfilm";

        [XmlElement("string")]
        public XMLString pixel_format = new XMLString("pixel_format", "rgb");


        [XmlElement("integer")]
        public XMLInteger width = new XMLInteger("width", 512);

        [XmlElement("integer", Namespace = "dummy")]
        public XMLInteger height = new XMLInteger("height", 512);
    }

    [XmlRoot("integer")]
    public class XMLInteger
    {
        public XMLInteger() { }

        public XMLInteger(string n, int v)
        {
            this.name = n;
            this.value = v;
        }

        [XmlAttribute("name")]
        public string name;

        [XmlAttribute("value")]
        public int value;
    }


    [XmlRoot("include")]
    public class XMLInclude
    {
        public XMLInclude() { }

        public XMLInclude(string file)
        {
            filename = file;
        }

        [XmlAttribute("filename")]
        public string filename;
    }


    [XmlRoot("string")]
    public class XMLString
    {
        public XMLString() { }

        public XMLString(string name, string value)
        {
            xmlname = name;
            xmlvalue = value;
        }

        [XmlAttribute("name")]
        public string xmlname;

        [XmlAttribute("value")]
        public string xmlvalue;
    }

    [XmlRoot("string")]
    public class XMLFilename
    {
        public XMLFilename() { }

        public XMLFilename(string filename)
        {
            value = filename;
        }

        [XmlAttribute("name")]
        public string name = "filename";

        [XmlAttribute("value")]
        public string value;
    }


    [XmlRoot("ref")]
    public class XMLRef
    {
        public XMLRef() { }

        public XMLRef(string reference_id)
        {
            id = reference_id;
        }

        [XmlAttribute("id")]
        public string id;
    }



    [XmlRoot("matrix")]
    public class XMLMatrix
    {
        public XMLMatrix() { }
        public XMLMatrix(UnityEngine.Matrix4x4 mat, bool changeHandiness = false)
        {
            //if (changeHandiness)
            //{
            //    // change handiness: left handed (unity) to right handed (mitsuba)
            //    UnityEngine.Matrix4x4 inversionMatrix = UnityEngine.Matrix4x4.zero;
            //    inversionMatrix.m00 = -1;
            //    inversionMatrix.m11 = 1;
            //    inversionMatrix.m22 = 1;
            //    inversionMatrix.m33 = 1;
            //    matrix = inversionMatrix * mat * inversionMatrix;
            //}
            //else
            //{
            matrix = mat;
            //}


        }

        [XmlAttribute("value")]
        public string serializablMatrix
        {
            get
            {

                return string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14} {15}", matrix.m00, matrix.m01, matrix.m02, matrix.m03, matrix.m10, matrix.m11, matrix.m12, matrix.m13, matrix.m20, matrix.m21, matrix.m22, matrix.m23, matrix.m30, matrix.m31, matrix.m32, matrix.m33);
            }

            set
            {
                ;// origin = new Vector3();//value.Split(',');
            }
        }


        [XmlIgnore]
        public UnityEngine.Matrix4x4 matrix;
    }

    [XmlInclude(typeof(XMLRotateX))]
    [XmlInclude(typeof(XMLRotateY))]
    [XmlInclude(typeof(XMLRotateZ))]
    [XmlRoot("transform")]
    public class XMLTransform
    {
        public XMLTransform() { }
        public XMLTransform(string n) { name = n; }

        [XmlAttribute("name")]
        public string name = "toWorld";

        [XmlElement("lookat")]
        public XMLLookAt lookat;

        [XmlElement("matrix")]
        public XMLMatrix matrix;

        [XmlElement("scale")]
        public XMLScale scale;

        [XmlElement("translate")]
        public XMLTranslate translate;

        [XmlElement("rotate")]
        public List<XMLRotate> rotates { get; set; }

    }


    [XmlRoot("scale")]
    public class XMLScale
    {
        public XMLScale() { }

        public XMLScale(float x, float y)
        {
            xml_x = x;
            xml_y = y;
        }


        [XmlAttribute("x")]
        public float xml_x;

        [XmlAttribute("y")]
        public float xml_y;
    }


    [XmlRoot("translate")]
    public class XMLTranslate
    {
        public XMLTranslate() { }

        public XMLTranslate(float x, float y)
        {
            xml_x = x;
            xml_y = y;
        }


        [XmlAttribute("x")]
        public float xml_x;

        [XmlAttribute("y")]
        public float xml_y;
    }

    [XmlRoot("rotate")]
    public class XMLRotate
    {
        [XmlAttribute("angle")]
        public float angle;
    }


    [XmlRoot("rotate")]
    public class XMLRotateX : XMLRotate
    {
        public XMLRotateX() { }

        public XMLRotateX(float x) { xml_x = 1; angle = x; }

        [XmlAttribute("x")]
        public float xml_x;

    }

    [XmlRoot("rotate")]
    public class XMLRotateY : XMLRotate
    {
        public XMLRotateY() { }

        public XMLRotateY(float y) { xml_y = 1; angle = y; }

        [XmlAttribute("y")]
        public float xml_y;

    }

    [XmlRoot("rotate")]
    public class XMLRotateZ : XMLRotate
    {
        public XMLRotateZ() { }

        public XMLRotateZ(float z) { xml_z = 1; angle = z; }

        [XmlAttribute("z")]
        public float xml_z;

    }





    [XmlRoot("lookat")]
    public class XMLLookAt
    {
        public XMLLookAt() { }

        public XMLLookAt(UnityEngine.Vector3 o, UnityEngine.Vector3 t, UnityEngine.Vector3 u)
        {
            this.origin = o;
            this.target = t;
            this.up = u;
            Debug.Log(serializablOrigin);

        }

        [XmlAttribute("origin")]
        public string serializablOrigin
        {
            get
            {
                return string.Format("{0}, {1}, {2}", origin.x, origin.y, origin.z);
            }

            set
            {
                ;// origin = new Vector3();//value.Split(',');
            }
        }

        [XmlAttribute("target")]
        public string serializablTarget
        {
            get
            {
                return string.Format("{0}, {1}, {2}", target.x, target.y, target.z);
            }

            set
            {
                ;// target = new Vector3();//value.Split(',');
            }
        }

        [XmlAttribute("up")]
        public string serializablUp
        {
            get
            {
                return string.Format("{0}, {1}, {2}", up.x, up.y, up.z);
            }

            set
            {
                ;// origin = new Vector3();//value.Split(',');
            }
        }


        [XmlIgnore]
        public UnityEngine.Vector3 origin;
        [XmlIgnore]
        public UnityEngine.Vector3 target;
        [XmlIgnore]
        public UnityEngine.Vector3 up;




    }



    [XmlRoot("rgb")]
    public class XMLRGB
    {
        public XMLRGB() { }


        public XMLRGB(string n, UnityEngine.Color c)
        {
            this.color = c;
            this.name = n;
        }


        [XmlAttribute("name")]
        public string name;

        [XmlAttribute("value")]
        public string serializableColor
        {
            get
            {
                return string.Format("{0}, {1}, {2}", color.r, color.g, color.b);
            }

            set
            {
                ;// color = new Vector3();//value.Split(',');
            }
        }



        [XmlIgnore]
        public UnityEngine.Color color;





    }


    [XmlRoot("scene")]
    public class Scene
    {
        [XmlAttribute("version")]
        public string version = "0.6.0";


        [XmlElement("include", Order = 0)]
        public List<XMLInclude> includes { get; set; }


        [XmlElement("integrator", Order = 1)]
        public XMLIntegrator integrator;// = new XMLIntegrator();


        [XmlElement("shape", Order = 2)]
        public List<Shape> shapes { get; set; }



        [XmlElement("emitter", Order = 3)]
        public XMLEmitter emitter;// = new XMLEmitter();


        [XmlElement("sensor", Order = 4)]
        public Sensor sensor { get; set; }

        public void AddInclude(string filename)
        {
            if (includes == null)
                includes = new List<XMLInclude>();

            includes.Add(new XMLInclude(filename));
        }

    }

    [XmlRoot("shape", Namespace = "")]
    public class Shape_OBJ : Shape
    {
        public Shape_OBJ() { this.type = "obj"; }
        [XmlElement("string")]
        public XMLFilename filename { get; set; }

    }



    [XmlRoot("shape", Namespace = "")]
    public class Shape_Instance : Shape
    {
        public Shape_Instance() { this.type = "instance"; }

        public Shape_Instance(string id)
        {
            this.type = "instance";
            this.reference = new XMLRef(id);
        }

        [XmlElement("ref")]
        public XMLRef reference { get; set; }

    }


    [XmlInclude(typeof(Shape_OBJ))]
    [XmlInclude(typeof(Shape_Instance))]
    [XmlInclude(typeof(XMLBSDF))]
    [XmlInclude(typeof(XMLBSDF_Plastic))]
    [XmlInclude(typeof(XMLBSDF_RoughPlastic))]
    [XmlInclude(typeof(XMLBSDF_NormalMap))]
    [XmlRoot("shape")]
    public class Shape
    {
        [XmlAttribute("type")]
        public string type { get; set; }

        [XmlAttribute("id")]
        public string id { get; set; }

        [XmlElement("shape")]
        public List<Shape> shapes { get; set; }


        [XmlElement("transform")]
        public XMLTransform transform { get; set; }

        public void toWorld(UnityEngine.Matrix4x4 toWorld)
        {
            if (transform == null)
                transform = new XMLTransform();
            transform.matrix = new XMLMatrix(toWorld);
        }

        [XmlElement("bsdf")]
        public XMLBSDF bsdf { get; set; }
    }

    public static void WriteXMLToFile(ref Scene scene, string filename)
    {
        //System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(Scene));
        //System.Xml.XmlWriterSettings settings = new System.Xml.XmlWriterSettings
        //{
        //    Indent = true,
        //    OmitXmlDeclaration = true
        //};

        //// write xml without xml tag and namespaces
        //System.IO.FileStream file = System.IO.File.Create(Path.Combine(path, model.name + ".xml"));
        //XmlSerializerNamespaces emptyNamespaces = new XmlSerializerNamespaces(new[] { System.Xml.XmlQualifiedName.Empty });
        //using (System.Xml.XmlWriter xmlWriter = System.Xml.XmlWriter.Create(file, settings))
        //{
        //    writer.Serialize(xmlWriter, scene, emptyNamespaces);
        //}

        XmlSerializerNamespaces emptyNamespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });

        XmlSerializer serializer = new XmlSerializer(typeof(Scene));


        StringWriter sw = new StringWriter();
        XmlWriterSettings settings = new XmlWriterSettings
        {
            Indent = true,
            OmitXmlDeclaration = true
        };
        using (XmlWriter xmlWriter = XmlWriter.Create(sw, settings))
        {
            serializer.Serialize(xmlWriter, scene, emptyNamespaces);
        }

        string xml = sw.GetStringBuilder().ToString();

        // clean out types and namespaces
        xml = xml.Replace(" p3:type=\"Shape_OBJ\"", "");
        xml = xml.Replace(" xmlns:p3=\"http://www.w3.org/2001/XMLSchema-instance\"", "");
        xml = xml.Replace(" p2:type=\"Shape_Instance\"", "");
        xml = xml.Replace(" xmlns:p2=\"http://www.w3.org/2001/XMLSchema-instance\"", "");
        xml = xml.Replace(" xmlns:p4=\"http://www.w3.org/2001/XMLSchema-instance\"", "");
        xml = xml.Replace(" xmlns=\"dummy\"", "");
        xml = xml.Replace(" p3:type=\"XMLBSDF_Plastic\"", "");
        xml = xml.Replace(" p3:type=\"XMLBSDF_RoughPlastic\"", "");
        xml = xml.Replace(" p3:type=\"XMLBSDF_NormalMap\"", "");
        xml = xml.Replace(" p4:type=\"XMLRotateX\"", "");
        xml = xml.Replace(" p4:type=\"XMLRotateY\"", "");
        xml = xml.Replace(" p4:type=\"XMLRotateZ\"", "");


        StreamWriter stringwriter = new StreamWriter(filename, false);
        stringwriter.WriteLine(xml);
        stringwriter.Close();

    }
}