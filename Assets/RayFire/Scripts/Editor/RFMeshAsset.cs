using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RayFire
{
	// Class to save mesh to asset file
	public static class RFMeshAsset
	{
		
		// 
		public static string shatterPath = "Assets/";
		
        // Save mesh as asset
        public static void SaveMesh (MeshFilter mf, string name) 
        {
	        if (mf == null)
	        {
		        Debug.Log ("MeshFilter is null");
		        return;
	        }
	        
	        if (mf.sharedMesh == null)
            {
                Debug.Log ("Mesh is null");
                return;
            } 
    
			// Save path
            string savePath = EditorUtility.SaveFilePanel ("Save Mesh Asset", "Assets/", name, "asset");
        	
            // No path
            if (string.IsNullOrEmpty(savePath) == true) 
                return;
            
            // Convert path
        	savePath = FileUtil.GetProjectRelativePath(savePath);
    
            // No path
            if (string.IsNullOrEmpty(savePath) == true) 
                return;
            
            // Create asset
        	AssetDatabase.CreateAsset(mf.sharedMesh, savePath);
            AssetDatabase.SaveAssets();
        }

        // Save mesh as asset
        public static void SaveFragments (RayfireShatter shatter, string path)
        {
	        // Get asset name
	        string saveName = shatter.gameObject.name + shatter.export.suffix;
	        
            // Save path
            string savePath = EditorUtility.SaveFilePanel ("Save Fragments To Asset", shatterPath, saveName, "asset");
            
            //string saveFolder = EditorUtility.SaveFolderPanel ("Save Fragments To Asset", "Assets/", saveName);
           // Debug.Log (saveFolder);
            
            // Convert path
            savePath = FileUtil.GetProjectRelativePath(savePath);

            // No path
            if (string.IsNullOrEmpty(savePath) == true) 
                return;

            // Save path for next save
            shatterPath = Path.GetDirectoryName (savePath);
            
            // Collect all meshes to save
            bool hasMesh = false;
            List<Mesh> meshes = new List<Mesh>();
            List<MeshFilter> meshFilters = new List<MeshFilter>();
			List<GameObject> gameObjects = new List<GameObject>();
            
			// Collect fragments meshes
			if (shatter.export.source == RFMeshExport.MeshExportType.LastFragments)
			{
				// No fragments
                if (shatter.fragmentsLast.Count == 0)
                    return;
				
				gameObjects = shatter.fragmentsLast;
			}
			else if (shatter.export.source == RFMeshExport.MeshExportType.Children)
			{
				// No children
                if (shatter.transform.childCount == 0)
                    return;
				
				gameObjects.AddRange (shatter.gameObject.GetComponentsInChildren<MeshFilter>().Select (mf => mf.gameObject));
			}
			
            // Collect meshes
            foreach (var frag in gameObjects)
            {
	            // Get mf
	            MeshFilter mf = frag.GetComponent<MeshFilter>();
	            meshFilters.Add (mf);
	            
	            // No mf
	            if (mf == null)
		            meshes.Add (null);

	            // No mesh
	            if (mf != null && mf.sharedMesh == null)
		            meshes.Add (null);
	            
	            // New mesh
	            Mesh tempMesh = Object.Instantiate(mf.sharedMesh);
	            tempMesh.name = mf.sharedMesh.name;
	            
	            // Collect
	            meshes.Add (tempMesh);

	            // List has mesh
	            hasMesh = true;
            }

            // List has no meshes to save
            if (hasMesh == false)
	            return;

            // Empty mesh
            Mesh emptyMesh = new Mesh();
            emptyMesh.name = saveName;
            
            // Create asset
	        AssetDatabase.CreateAsset(emptyMesh, savePath);
            
            // Save each fragment mesh
            for (int i = 0; i < meshFilters.Count; i++)
            {
	            // Skip if no mesh
	            if (meshFilters[i] == null)
		            continue;
	            
	            // Apply to meshfilter to avoid save of already referenced mesh
	            meshFilters[i].sharedMesh = meshes[i];
	            
	            // Add all meshes
	            AssetDatabase.AddObjectToAsset (meshFilters[i].sharedMesh, savePath);
            }
            
            // Save
            AssetDatabase.SaveAssets();
        }
	}
}