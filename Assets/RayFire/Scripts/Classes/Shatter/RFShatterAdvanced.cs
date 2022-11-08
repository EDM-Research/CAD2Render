using System;
using System.Collections.Generic;
using UnityEngine;

namespace RayFire
{
	[Serializable]
    public class RFMeshExport
    {
	    public enum MeshExportType
        {
            LastFragments         = 0,
            Children              = 3
        }
        
        public MeshExportType source;
        public string         suffix = "_frags";
    	
    	// by path, by window
    	// public string path = "RayFireFragments";
    	
    	// all, last
        // generate colliders
    }
    
	[Serializable]
	public class RFShatterAdvanced
	{
		public int  seed;
		public bool decompose;
		public bool removeCollinear;
		public bool copyComponents;
		
		// Not used
		public bool postWeld;
		
        public bool  smooth;
        public bool  inputPrecap;
        public bool  outputPrecap;
        public bool  removeDoubleFaces;
        public int   elementSizeThreshold;
        public bool  combineChildren;
        public bool  inner;
        public bool  planar;
        public int   relativeSize;
        public float absoluteSize;
        public bool  sizeLimitation;
        public float sizeAmount;
        public bool  vertexLimitation;
        public int   vertexAmount;
        public bool  triangleLimitation;
        public int   triangleAmount;
        
        // Planar mesh vert offset threshold
        public static float planarThreshold = 0.01f;
        
        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////
		
		// Constructor
		public RFShatterAdvanced()
		{
			seed                 = 0;
			decompose            = true;
			removeCollinear      = false;
			copyComponents       = false;
			inputPrecap          = true;
			outputPrecap         = false;
			removeDoubleFaces    = true;
			elementSizeThreshold = 5;
			postWeld             = false;
			inner                = false;
			planar               = false;
			absoluteSize         = 0.1f;
			relativeSize         = 4;
			sizeLimitation       = false;
			sizeAmount           = 5f;
			vertexLimitation     = false;
			vertexAmount         = 300;
			triangleLimitation   = false;
			triangleAmount       = 300;
		}
        
        // Constructor
        public RFShatterAdvanced (RFShatterAdvanced src)
        {
	        seed                 = src.seed;
	        decompose            = src.decompose;
	        removeCollinear      = src.removeCollinear;
	        copyComponents       = src.copyComponents;
	        inputPrecap          = src.inputPrecap;
	        outputPrecap         = src.outputPrecap;
	        removeDoubleFaces    = src.removeDoubleFaces;
	        inner                = src.inner;
	        elementSizeThreshold = src.elementSizeThreshold;
	        sizeLimitation       = src.sizeLimitation;
	        sizeAmount           = src.sizeAmount;
	        vertexLimitation     = src.vertexLimitation;
	        vertexAmount         = src.vertexAmount;
	        triangleLimitation   = src.triangleLimitation;
	        triangleAmount       = src.triangleAmount;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////
        
        // Check if mesh is coplanar. All verts on a plane
        public static bool IsCoplanar(Mesh mesh, float threshold)
        {
            // Coplanar 3 verts
            if (mesh.vertices.Length <= 3)
                return true;
            
            // Get second vert for plane
            int ind = 1;
            List<int> ids = new List<int>() {0};
            for (int i = ind; i < mesh.vertices.Length; i++)
            {
	            if (Vector3.Distance (mesh.vertices[0], mesh.vertices[i]) > threshold)
	            {
		            
		            ids.Add (i);
		            ind = i;
		            break;
	            }
            }

            // No second vert
            if (ids.Count == 1)
                return true;

            // Second vert is the last ver
            if (ind == mesh.vertices.Length - 1)
                return true;
            
            // Get third vert
            ind++;
            Vector3 vector1 = (mesh.vertices[ids[1]] - mesh.vertices[ids[0]]).normalized;
            for (int i = ind; i < mesh.vertices.Length; i++)
            {
                if (Vector3.Distance (mesh.vertices[1], mesh.vertices[i]) > threshold)
                {
                    Vector3 vector2  = (mesh.vertices[i] - mesh.vertices[ids[0]]).normalized;
                    float   distance = Vector3.Cross (vector1, vector2).magnitude;
                    if (distance > threshold)
                    {
                        ids.Add (i);
                        break;
                    }
                }
            }
            
            // No third vert
            if (ids.Count == 2)
                return true;
            
            // Create plane and check other verts for coplanar
            Plane plane = new Plane(mesh.vertices[ids[0]], mesh.vertices[ids[1]], mesh.vertices[ids[2]]);
            for (int i = 0; i < mesh.vertices.Length; i++)
            {
                if (i != ids[0] && i != ids[1] && i != ids[2])
                {
                    float dist = plane.GetDistanceToPoint (mesh.vertices[i]);
                    if (Math.Abs (dist) > threshold)
                        return false;
                }
            }
            
            return true;
        }
	}
}