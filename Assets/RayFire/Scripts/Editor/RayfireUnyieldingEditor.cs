using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;

namespace RayFire
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireUnyielding))]
    public class RayfireUnyieldingEditor : Editor
    {
        RayfireUnyielding uny;
        Vector3           centerWorldPos;
        BoxBoundsHandle   m_BoundsHandle = new BoxBoundsHandle();
        
        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////
        
        static int space = 3;
        static Color wireColor = new Color (0.58f, 0.77f, 1f);

        static GUIContent gui_propUny = new GUIContent ("Unyielding",  "Set Unyielding property for children Rigids and Shards.");
        static GUIContent gui_propAct = new GUIContent ("Activatable", "Set Activatable property for children Rigids and Shards.");
        static GUIContent gui_propSim = new GUIContent ("Simulation Type", "Custom simulation type.");
        
        static GUIContent gui_gizmoShow = new GUIContent ("Show", "");
        static GUIContent gui_gizmoSize = new GUIContent ("Size", "Unyielding gizmo size.");
        static GUIContent gui_gizmoCenter = new GUIContent ("Center", "Unyielding gizmo center.");
        
        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////
        
        [DrawGizmo (GizmoType.Selected | GizmoType.NonSelected | GizmoType.Pickable)]
        static void DrawGizmosSelected (RayfireUnyielding targ, GizmoType gizmoType)
        {
            if (targ.enabled && targ.showGizmo == true)
            {
                Gizmos.color  = wireColor;
                Gizmos.matrix = targ.transform.localToWorldMatrix;
                Gizmos.DrawWireCube (targ.centerPosition, targ.size);
            }
        }

        private void OnSceneGUI()
        {
            // Get shatter
            uny = target as RayfireUnyielding;
            if (uny == null)
                return;

            if (uny.enabled && uny.showGizmo == true)
            {
                Transform transform      = uny.transform;
                centerWorldPos  = transform.TransformPoint (uny.centerPosition);
                //centerWorldQuat = transform.rotation * uny.centerDirection;
                
                // Point3 handle
                if (uny.showCenter == true)
                {
                    EditorGUI.BeginChangeCheck();
                    centerWorldPos = Handles.PositionHandle (centerWorldPos, Quaternion.identity);
                    if (EditorGUI.EndChangeCheck() == true)
                        Undo.RecordObject (uny, "Center Move");
                                    
                    uny.centerPosition = transform.InverseTransformPoint (centerWorldPos);
                    
                    //EditorGUI.BeginChangeCheck();
                    //centerWorldQuat = Handles.RotationHandle (centerWorldQuat, centerWorldPos);
                    //if (EditorGUI.EndChangeCheck() == true)
                    //    Undo.RecordObject (uny, "Center Rotate");
                }
                
                //uny.centerDirection = Quaternion.Inverse (transform.rotation) * centerWorldQuat;
                

                Handles.matrix = uny.transform.localToWorldMatrix;
                m_BoundsHandle.wireframeColor = wireColor;
                m_BoundsHandle.center         = uny.centerPosition;
                m_BoundsHandle.size           = uny.size;

                // draw the handle
                EditorGUI.BeginChangeCheck();
                m_BoundsHandle.DrawHandle();
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject (uny, "Change Bounds");
                    uny.size = m_BoundsHandle.size;
                }
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Inspector
        /// /////////////////////////////////////////////////////////

        public override void OnInspectorGUI()
        {
            uny = target as RayfireUnyielding;
            if (uny == null)
                return;

            GUILayout.Space (8);
            
            if (Application.isPlaying == true)
                if (GUILayout.Button ("   Activate   ", GUILayout.Height (25)))
                    foreach (var targ in targets)
                        if (targ as RayfireUnyielding != null)
                            (targ as RayfireUnyielding).Activate();
            
            GUILayout.Space (space);
            
            UI_Properties();
            
            GUILayout.Space (space);
            
            UI_Gizmo();
            
            GUILayout.Space (8);

            // DrawDefaultInspector();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Gizmo
        /// /////////////////////////////////////////////////////////
        
        void UI_Gizmo()
        {
            GUILayout.Label ("  Gizmo", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            uny.showGizmo = EditorGUILayout.Toggle (gui_gizmoShow, uny.showGizmo);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireUnyielding scr in targets)
                {
                    scr.showGizmo = uny.showGizmo;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            uny.size = EditorGUILayout.Vector3Field (gui_gizmoSize, uny.size);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireUnyielding scr in targets)
                {
                    scr.size = uny.size;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
                
            EditorGUI.BeginChangeCheck();
            uny.centerPosition = EditorGUILayout.Vector3Field (gui_gizmoCenter, uny.centerPosition);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireUnyielding scr in targets)
                {
                    scr.centerPosition = uny.centerPosition;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUILayout.BeginHorizontal ();
            EditorGUI.BeginChangeCheck();
            uny.showCenter = GUILayout.Toggle (uny.showCenter, "Show Center", "Button", GUILayout.Height (22));
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireUnyielding scr in targets)
                {
                    scr.showCenter = uny.showCenter;
                    SetDirty (scr);
                }
            }
            
            // Reset center
            if (GUILayout.Button ("   Reset   ", GUILayout.Height (22)))
            {
                foreach (RayfireUnyielding scr in targets)
                {
                    scr.centerPosition = Vector3.zero;
                    SetDirty (scr);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Properties
        /// /////////////////////////////////////////////////////////
        
        void UI_Properties()
        {
            GUILayout.Label ("  Properties", EditorStyles.boldLabel);

            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            uny.unyielding = EditorGUILayout.Toggle (gui_propUny, uny.unyielding);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireUnyielding scr in targets)
                {
                    scr.unyielding = uny.unyielding;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            uny.activatable = EditorGUILayout.Toggle (gui_propAct, uny.activatable);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireUnyielding scr in targets)
                {
                    scr.activatable = uny.activatable;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            uny.simulationType = (RayfireUnyielding.UnySimType)EditorGUILayout.EnumPopup (gui_propSim, uny.simulationType);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireUnyielding scr in targets)
                {
                    scr.simulationType = uny.simulationType;
                    SetDirty (scr);
                }
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////
        
        void SetDirty (RayfireUnyielding scr)
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