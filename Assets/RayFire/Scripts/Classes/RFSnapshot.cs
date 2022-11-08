#if UNITY_EDITOR

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Namespace
namespace RayFire
{
    // Single object snapshot data
    [Serializable]
    public class RFSnapshot
    {
        // Vars
        public string nm;
        
        // Ids
        public int oldId;
        public int parentOldId;
        
        public int newId;
        [NonSerialized]public Transform newTm;
         
        public Vector3 pos;
        public Quaternion rot;
        public Vector3 scale;
        public List<string> mats;
        public RFMesh mesh;
        
        // Constructor
        public RFSnapshot(GameObject go, bool compress)
        {
            nm = go.name;
            oldId = go.GetInstanceID();
            if (go.transform.parent != null)
                parentOldId = go.transform.parent.gameObject.GetInstanceID();
            pos = go.transform.position;
            rot = go.transform.rotation;
            scale = go.transform.localScale;
            
            // Mesh
            MeshFilter mf = go.GetComponent<MeshFilter>();
            if (mf != null)
                mesh = new RFMesh (mf.sharedMesh, compress);

            // Materials
            mats = new List<string>();
            MeshRenderer mr = go.GetComponent<MeshRenderer>();
            if (mr != null)
                foreach (var mat in mr.sharedMaterials)
                    mats.Add (AssetDatabase.GetAssetPath (mat));
        }

        // Create object
        public static GameObject Create (RFSnapshot cap, float sizeFilter)
        {
            // Mesh and size filtering
            Mesh mesh = null;
            if (cap.mesh.subMeshCount > 0)
            { 
                // Size filtering
                if (cap.mesh.bounds.size.magnitude < sizeFilter)
                    return null;
                
                // Get mesh
                mesh = cap.mesh.GetMesh();
            }

            // Object
            GameObject go = new GameObject();
            go.name = cap.nm;
            go.transform.position = cap.pos;
            go.transform.rotation = cap.rot;
            go.transform.localScale = cap.scale;

            // Mesh
            if (mesh != null)
            {
                MeshFilter mf = go.AddComponent<MeshFilter>();
                mf.sharedMesh = mesh;
            }
            
            // Materials
            if (cap.mats.Count > 0)
            {
                MeshRenderer  mr = go.AddComponent<MeshRenderer>();
                List<Material> materials = new List<Material>();
                foreach (var matPath in cap.mats)
                    materials.Add ((Material)AssetDatabase.LoadAssetAtPath (matPath, typeof(Material)));
                mr.sharedMaterials = materials.ToArray();
            }

            cap.newId = go.GetInstanceID();
            cap.newTm = go.transform;
                
            return go;
        }

        // Get parent new id by old id
        public static void SetParent (List<RFSnapshot> list, int parentId, Transform tm, Transform parentTm)
        {
            foreach (var snap in list)
            {
                if (snap.oldId == parentId)
                {
                    tm.parent = snap.newTm;
                    break;
                }
                tm.parent = parentTm;
            }
        }
    }
    
    // Snapshot asset
    [Serializable]
    public class RFSnapshotAsset
    {
        // Vars
        public List<RFSnapshot> assets;

        // Constructor
        public RFSnapshotAsset()
        {
            assets = new List<RFSnapshot>();
        }
        
        // Constructor
        public RFSnapshotAsset(List<GameObject> list, bool compress)
        {
            assets = new List<RFSnapshot>();
            foreach (var go in list)
                assets.Add (new RFSnapshot (go, compress));
        }
        
        // Save asset
        public static void Snapshot(GameObject gameObject, bool compress, string assetName)
        {
            // Get all nested game objects
            List<Transform> tms = gameObject.GetComponentsInChildren<Transform>().ToList();
            tms.Remove (gameObject.transform);
            
            // No asset
            if (tms.Count == 0)
            {
                Debug.Log ("RayFire Snapshot: " + gameObject.name + " has no children", gameObject);
                return;
            }
            
            // Create asset data
            List<GameObject> list = tms.Select (t => t.gameObject).ToList();
            RFSnapshotAsset  data = new RFSnapshotAsset (list, compress);
            
            // Set Folder
            string fld = Application.dataPath + "/RayFireSnapshots/";
            if (Directory.Exists (fld) == false) 
                Directory.CreateDirectory(fld);
            string nm         = assetName + "_snapshot.json";
            string stringData = JsonUtility.ToJson (data, true);
    
            // Save data
            File.WriteAllText (fld + nm, stringData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        // Load asset
        public static void Load(UnityEngine.Object snapshotAsset, GameObject gameObject, float sizeFilter)
        {
            // No asset
            if (snapshotAsset == null)
            {
                Debug.Log ("RayFire Snapshot: " + gameObject.name + " Snapshot asset is not defined", gameObject);
                return;
            }
            
            // Get path
            string path1 = Application.dataPath;
            string path2 = AssetDatabase.GetAssetPath(snapshotAsset);
            path2 = path2.Remove (0, 6);
            
            // Read
            string dataString = File.ReadAllText (path1 + path2);
            RFSnapshotAsset assetData = JsonUtility.FromJson<RFSnapshotAsset> (dataString);
            
            // No asset
            if (assetData == null)
            {
                Debug.Log ("RayFire Snapshot: " + gameObject.name + " Snapshot asset is not defined", gameObject);
                return;
            }
            
            // Create objects from asset
            if (assetData.assets.Count > 0)
                foreach (var ast in assetData.assets)
                    RFSnapshot.Create (ast, sizeFilter);
            
            // Set parents
            foreach (var snap in assetData.assets)
            {
                RFSnapshot.SetParent (assetData.assets, snap.parentOldId, snap.newTm, gameObject.transform);
            }
        }
        
    }
        
}

#endif 
