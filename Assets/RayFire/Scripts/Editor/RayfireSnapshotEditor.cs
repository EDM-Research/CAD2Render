using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace RayFire
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireSnapshot))]
    public class RayfireSnapshotEditor : Editor
    {
        // Target
        RayfireSnapshot snap;

        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////

        static int space = 3;
        
        static GUIContent gui_saveName = new GUIContent ("Asset Name", "");
        static GUIContent gui_saveComp = new GUIContent ("Compress",   "");
        static GUIContent gui_loadSnap = new GUIContent ("Snapshot Asset", "");
        static GUIContent gui_loadSize = new GUIContent ("Size Filter",   "");
        
        /// /////////////////////////////////////////////////////////
        /// Inspector
        /// /////////////////////////////////////////////////////////

        public override void OnInspectorGUI()
        {
            // Get target
            snap = target as RayfireSnapshot;
            if (snap == null)
                return;
            
            GUILayout.Space (8);
           
            UI_Save();
            
            GUILayout.Space (space);

            UI_Load();
            
            GUILayout.Space (8);
        }

        /// /////////////////////////////////////////////////////////
        /// Save
        /// /////////////////////////////////////////////////////////

        void UI_Save()
        {
            GUILayout.Label ("  Save", EditorStyles.boldLabel);
            
            if (snap.transform.childCount > 0)
                if (GUILayout.Button ("Snapshot", GUILayout.Height (25)))
                    snap.Snapshot();
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            snap.assetName = EditorGUILayout.TextField (gui_saveName, snap.assetName);
            
            GUILayout.Space (space);
            
            snap.compress = EditorGUILayout.Toggle (gui_saveComp, snap.compress);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireSnapshot scr in targets)
                    SetDirty (scr);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Load
        /// /////////////////////////////////////////////////////////

        void UI_Load()
        {
            GUILayout.Label ("  Load", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            
            snap.snapshotAsset = (Object)EditorGUILayout.ObjectField (gui_loadSnap, snap.snapshotAsset, typeof(Object), true);
            if (EditorGUI.EndChangeCheck() == true)
                SetDirty (snap);
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            snap.sizeFilter = EditorGUILayout.Slider (gui_loadSize, snap.sizeFilter, 0f, 1f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireSnapshot scr in targets)
                {
                    scr.sizeFilter = snap.sizeFilter;
                    SetDirty (scr);
                }
            }
            
            // Load
            // if (snap.snapshotAsset != null)
            {
                GUILayout.Space (space);
                
                if (GUILayout.Button ("Load", GUILayout.Height (25)))
                    snap.Load();
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////

        void SetDirty (RayfireSnapshot scr)
        {
            if (Application.isPlaying == false)
            {
                EditorUtility.SetDirty (scr);
                EditorSceneManager.MarkSceneDirty (scr.gameObject.scene);
                SceneView.RepaintAll();
            }
        }
    }
}