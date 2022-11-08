using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RayFire
{
    
    //[Serializable]
    public class RFFrag
    {
        public Mesh mesh;
        public Vector3 pivot;
        //public RFMesh rfMesh;
        public RFDictionary subId;
        public RayfireRigid fragment;
    }
    
    public class RFTri
    {
        public int meshId;
        public int subMeshId = -1;
        public List<int> ids = new List<int>();
        public List<Vector3> vpos = new List<Vector3>();
        public List<Vector3> vnormal = new List<Vector3>();
        public List<Vector2> uvs = new List<Vector2>();
        public List<Vector4> tangents = new List<Vector4>();
        public List<RFTri> neibTris = new List<RFTri>();
    }

    [Serializable]
    public class RFDictionary
    {
        public List<int> keys;
        public List<int> values;

        // Constructor
        public RFDictionary(Dictionary<int, int> dictionary)
        {
            keys = new List<int>();
            values = new List<int>();
            keys = dictionary.Keys.ToList();
            values =  dictionary.Values.ToList();
        }
    }

    /// /////////////////////////////////////////////////////////
    /// Fragments Clustering
    /// /////////////////////////////////////////////////////////
    
    [Serializable]
    public class RFShatterCluster
    {
        public bool  enable;
        public int   count;
        public int   seed;
        public float relax;
        public int   amount;
        public int   layers;
        public float scale;
        public int   min;
        public int   max;

        public RFShatterCluster()
        {
            enable = false;
            count  = 10;
            seed   = 1;
            relax  = 0.5f;
            layers = 0;
            amount = 0;
            scale  = 1f;
            min    = 1;
            max    = 3;
        }
        
        public RFShatterCluster (RFShatterCluster src)
        {
            enable = src.enable;
            count  = src.count;
            seed   = src.seed;
            relax  = src.relax;
            layers = src.layers;
            amount = src.amount;
            scale  = src.scale;
            min    = src.min;
            max    = src.max;
        }
    }

    /// /////////////////////////////////////////////////////////
    /// Shatter
    /// /////////////////////////////////////////////////////////

    [Serializable]
    public class RFVoronoi
    {
        public int amount;
        public float centerBias;
        
        public RFVoronoi()
        {
            amount = 30;
            centerBias = 0f;
        }
        
        public RFVoronoi(RFVoronoi src)
        {
            amount     = src.amount;
            centerBias = src.centerBias;
        }
        
        // Amount
        public int Amount
        {
            get
            {
                if (amount < 1)
                    return 1;
                if (amount > 20000)
                    return 2;
                return amount;
            }
        }
    }

    [Serializable]
    public class RFSplinters
    {
        public AxisType axis;
        public int amount;
        public float strength;
        public float centerBias;
        
        public RFSplinters()
        {
            axis = AxisType.YGreen; 
            amount     = 30;
            strength   = 0.7f;
            centerBias = 0f;
        }
        
        public RFSplinters(RFSplinters src)
        {
            axis       = src.axis; 
            amount     = src.amount;
            strength   = src.strength;
            centerBias = src.centerBias;
        }
        
        // Amount
        public int Amount
        {
            get
            {
                if (amount < 2)
                    return 2;
                if (amount > 20000)
                    return 2;
                return amount;
            }
        }
    }

    [Serializable]
    public class RFRadial
    {
        public AxisType centerAxis;
        public float    radius;
        public float    divergence;
        public bool     restrictToPlane;
        public int rings;
        public int focus;
        public int focusStr;
        public int randomRings;
        public int rays;
        public int randomRays;
        public int twist;
        
        public RFRadial()
        {
            centerAxis  = AxisType.YGreen;
            radius          = 1f;
            divergence      = 1f;
            restrictToPlane = true;
            rings           = 10;
            focus           = 0;
            focusStr        = 50;
            randomRings     = 50;
            rays            = 10;
            randomRays      = 0;
            twist           = 0;
        }
        
        public RFRadial(RFRadial src)
        {
            centerAxis      = src.centerAxis;
            radius          = src.radius;
            divergence      = src.divergence;
            restrictToPlane = src.restrictToPlane;
            rings           = src.rings;
            focus           = src.focus;
            focusStr        = src.focusStr;
            randomRings     = src.randomRings;
            rays            = src.rays;
            randomRays      = src.randomRays;
            twist           = src.twist;
        }
    }

    [Serializable]
    public class RFSlice
    {
        public PlaneType       plane;
        public List<Transform> sliceList;
        
        public RFSlice()
        {
            plane = PlaneType.XZ;
        }
        
        public RFSlice(RFSlice src)
        {
            plane     = src.plane;
            sliceList = src.sliceList;
        }
        
        // Get axis
        public Vector3 Axis (Transform tm)
        {
            if (plane == PlaneType.YZ)
                return tm.right;
            if (plane == PlaneType.XZ)
                return tm.up;
            return tm.forward;
        }
    }
    
    [Serializable]
    public class RFBricks
    {
        public enum RFBrickType
        {
            ByAmount = 0,
            BySize = 1
        }

        public RFBrickType amountType;
        public float       mult = 1f;
        public int         amount_X;
        public int         amount_Y;
        public int         amount_Z;
        public bool        size_Lock;
        public float       size_X = 1f;
        public float       size_Y = 1f;
        public float       size_Z = 1f;
        public int         sizeVar_X;
        public int         sizeVar_Y;
        public int         sizeVar_Z;
        public float       offset_X;
        public float       offset_Y;
        public float       offset_Z;
        public bool        split_X;
        public bool        split_Y;
        public bool        split_Z;
        public int         split_probability;
        public float       split_offset   = 0.5f;
        public int         split_rotation = 30;
        

        // Constructor
        public RFBricks()
        {
            amount_X = 3;
            amount_Y = 6;
            amount_Z = 0;

            offset_X = 0.5f;
            offset_Y = 0.5f;
            offset_Z = 0;
        }
    }

    [Serializable]
    public class RFVoxels
    {
        [Range(0.01f, 10)] public float size;

        public RFVoxels()
        {
            size = 1f;
        }
    }
    
    [Serializable]
    public class RFTets
    {
        public enum TetType
        {
            Uniform = 0,
            Curved  = 1
        }
        
        public TetType lattice;
        public int     density;
        public int     noise;
        
        public RFTets()
        {
            lattice = TetType.Uniform;
            density = 7;
            noise   = 100;
        }
        
        public RFTets(RFTets src)
        {
            lattice = src.lattice;
            density = src.density;
            noise   = src.noise;
        }
    }
}

