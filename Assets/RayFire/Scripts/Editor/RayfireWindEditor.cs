using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

namespace RayFire
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireWind))]
    public class RayfireWindEditor : Editor
    {
        RayfireWind  wind;
        List<string> layerNames;
        
        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////
        
        // Static
        static int   space    = 3;
        static Color wireColor = new Color (0.58f, 0.77f, 1f);
        static Color sphCol    = new Color (1.0f,  0.60f, 0f);
        static Color color     = Color.red;
        
        static GUIContent gui_gizmoShow = new GUIContent ("Show",          "");
        static GUIContent gui_gizmoSize = new GUIContent ("Size",          "");
        static GUIContent gui_nsShow    = new GUIContent ("Show",          "");
        static GUIContent gui_nsGlobal  = new GUIContent ("Global",        "");
        static GUIContent gui_nsLength  = new GUIContent ("Length",        "");
        static GUIContent gui_nsWidth   = new GUIContent ("Width",         "");
        static GUIContent gui_nsSpeed   = new GUIContent ("Speed",         "");
        static GUIContent gui_strMin    = new GUIContent ("Minimum",       "");
        static GUIContent gui_strMax    = new GUIContent ("Maximum",       "");
        static GUIContent gui_strTor    = new GUIContent ("Torque",        "");
        static GUIContent gui_strFrc    = new GUIContent ("Force By Mass", "");
        static GUIContent gui_dirDiv    = new GUIContent ("Divergency",    "");
        static GUIContent gui_dirTur    = new GUIContent ("Turbulence",    "");
        static GUIContent gui_prevDens  = new GUIContent ("Density",       "");
        static GUIContent gui_prevSize  = new GUIContent ("Size",          "");
        
        /// /////////////////////////////////////////////////////////
        /// Inspector
        /// /////////////////////////////////////////////////////////      
        
        public override void OnInspectorGUI()
        {
            wind = (RayfireWind)target;
            if (wind == null)
                return;
            
            GUILayout.Space (8);

            UI_Gizmo();
            
            GUILayout.Space (space);

            UI_Noise();

            GUILayout.Space (space);
            
            UI_Strength();
            
            GUILayout.Space (space);

            UI_Direction();
            
            GUILayout.Space (space);

            UI_Preview();
            
            GUILayout.Space (space);

            UI_Filters();
            
            GUILayout.Space (8);
        }

        /// /////////////////////////////////////////////////////////
        /// Noise
        /// /////////////////////////////////////////////////////////  

        void UI_Gizmo()
        {
            GUILayout.Label ("  Gizmo", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            wind.showGizmo = EditorGUILayout.Toggle (gui_gizmoShow, wind.showGizmo);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireWind scr in targets)
                {
                    scr.showGizmo = wind.showGizmo;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            wind.gizmoSize = EditorGUILayout.Vector3Field (gui_gizmoSize, wind.gizmoSize);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireWind script in targets)
                {
                    script.gizmoSize = wind.gizmoSize;
                    SetDirty (script);
                }
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Noise
        /// /////////////////////////////////////////////////////////  

        void UI_Noise()
        {
            GUILayout.Label ("  Noise Scale", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            wind.showNoise = EditorGUILayout.Toggle (gui_nsShow, wind.showNoise);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireWind scr in targets)
                {
                    scr.showNoise = wind.showNoise;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            wind.globalScale = EditorGUILayout.Slider (gui_nsGlobal, wind.globalScale, 1f, 100f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireWind scr in targets)
                {
                    scr.globalScale = wind.globalScale;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            wind.lengthScale = EditorGUILayout.Slider (gui_nsLength, wind.lengthScale, 1f, 300f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireWind scr in targets)
                {
                    scr.lengthScale = wind.lengthScale;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
           
            EditorGUI.BeginChangeCheck();
            wind.widthScale = EditorGUILayout.Slider (gui_nsWidth, wind.widthScale, 1f, 300f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireWind scr in targets)
                {
                    scr.widthScale = wind.widthScale;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            wind.speed = EditorGUILayout.Slider (gui_nsSpeed, wind.speed, -200f, 200f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireWind scr in targets)
                {
                    scr.speed = wind.speed;
                    SetDirty (scr);
                }
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Strength
        /// /////////////////////////////////////////////////////////  

        void UI_Strength()
        {
            GUILayout.Label ("  Strength", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            wind.minimum = EditorGUILayout.Slider (gui_strMin, wind.minimum, -5f, 5f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireWind scr in targets)
                {
                    scr.minimum = wind.minimum;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            wind.maximum = EditorGUILayout.Slider (gui_strMax, wind.maximum, -5f, 5f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireWind scr in targets)
                {
                    scr.maximum = wind.maximum;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
           
            EditorGUI.BeginChangeCheck();
            wind.torque = EditorGUILayout.Slider (gui_strTor, wind.torque, 0f, 10f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireWind scr in targets)
                {
                    scr.torque = wind.torque;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            wind.forceByMass = EditorGUILayout.Toggle (gui_strFrc, wind.forceByMass);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireWind scr in targets)
                {
                    scr.forceByMass = wind.forceByMass;
                    SetDirty (scr);
                }
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Direction
        /// /////////////////////////////////////////////////////////  

        void UI_Direction()
        {
            GUILayout.Label ("  Direction", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            wind.divergency = EditorGUILayout.Slider (gui_dirDiv, wind.divergency, 0f, 180f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireWind scr in targets)
                {
                    scr.divergency = wind.divergency;
                    SetDirty (scr);
                }
            }

            GUILayout.Space (space);
           
            EditorGUI.BeginChangeCheck();
            wind.turbulence = EditorGUILayout.Slider (gui_dirTur, wind.turbulence, 0.01f, 2f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireWind scr in targets)
                {
                    scr.turbulence = wind.turbulence;
                    SetDirty (scr);
                }
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Preview
        /// /////////////////////////////////////////////////////////  

        void UI_Preview()
        {
            GUILayout.Label ("  Preview", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            wind.previewDensity = EditorGUILayout.Slider (gui_prevDens, wind.previewDensity, 0.5f, 5f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireWind scr in targets)
                {
                    scr.previewDensity = wind.previewDensity;
                    SetDirty (scr);
                }
            }

            GUILayout.Space (space);
           
            EditorGUI.BeginChangeCheck();
            wind.previewSize = EditorGUILayout.Slider (gui_prevSize, wind.previewSize, 0.1f, 5f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireWind scr in targets)
                {
                    scr.previewSize = wind.previewSize;
                    SetDirty (scr);
                }
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Filters
        /// /////////////////////////////////////////////////////////  
        
        void UI_Filters()
        {
            GUILayout.Label ("  Filters", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            wind.tagFilter = EditorGUILayout.TagField ("Tag", wind.tagFilter);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireWind scr in targets)
                {
                    scr.tagFilter = wind.tagFilter;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);

            // Layer mask
            if (layerNames == null)
            {
                layerNames = new List<string>();
                for (int i = 0; i <= 31; i++)
                    layerNames.Add (i + ". " + LayerMask.LayerToName (i));
            }
            
            EditorGUI.BeginChangeCheck();
            wind.mask = EditorGUILayout.MaskField ("Layer", wind.mask, layerNames.ToArray());
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireWind scr in targets)
                {
                    scr.mask = wind.mask;
                    SetDirty (scr);
                }
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////  
        
        // Draw gizmo
        [DrawGizmo (GizmoType.Selected | GizmoType.NonSelected | GizmoType.Pickable)]
        static void DrawGizmosSelected (RayfireWind wind, GizmoType gizmoType)
        {
            // Vars
            int     stepX;
            int     stepZ;
            float   windStr;
            float   x,  y,  z;
            Vector3 p1, p2, p3, p4, p5, p6, p7, p8, p10, p11, to;
            Vector3 vector;
            Vector3 localPos;
            float   perlinVal;
            color = Color.red;
            color.b = 0.0f;

           


            

            // Gizmo preview
            if (wind.showGizmo == true)
            {
                // Offsets
                x = wind.gizmoSize.x / 2f;
                y = wind.gizmoSize.y;
                z = wind.gizmoSize.z / 2f;

                // Get points
                p1 = new Vector3 (-x, 0, -z);
                p2 = new Vector3 (-x, 0, +z);
                p3 = new Vector3 (+x, 0, -z);
                p4 = new Vector3 (+x, 0, +z);
                p5 = new Vector3 (-x, y, -z);
                p6 = new Vector3 (-x, y, +z);
                p7 = new Vector3 (+x, y, -z);
                p8 = new Vector3 (+x, y, +z);

                p10 = new Vector3 (-x, 0, 0);
                p11 = new Vector3 (+x, 0, 0);
                to  = new Vector3 (+0, 0, z);

                // Gizmo properties
                Gizmos.color  = wireColor;
                Gizmos.matrix = wind.transform.localToWorldMatrix;
                
                // Gizmo Lines
                Gizmos.DrawLine (p1, p2);
                Gizmos.DrawLine (p3, p4);
                Gizmos.DrawLine (p5, p6);
                Gizmos.DrawLine (p7, p8);
                Gizmos.DrawLine (p1, p5);
                Gizmos.DrawLine (p2, p6);
                Gizmos.DrawLine (p3, p7);
                Gizmos.DrawLine (p4, p8);
                Gizmos.DrawLine (p1, p3);
                Gizmos.DrawLine (p2, p4);
                Gizmos.DrawLine (p5, p7);
                Gizmos.DrawLine (p6, p8);

                // Arrow
                Gizmos.DrawLine (p1,  Vector3.zero);
                Gizmos.DrawLine (p3,  Vector3.zero);
                Gizmos.DrawLine (p10, to);
                Gizmos.DrawLine (p11, to);

                // Selectable sphere
                float sphereSize = (x + y + z) * 0.02f;
                if (sphereSize < 0.1f)
                    sphereSize = 0.1f;
                float ySph = y / 2f;
                Gizmos.color = sphCol;
                Gizmos.DrawSphere (new Vector3 (x,  ySph, 0f), sphereSize);
                Gizmos.DrawSphere (new Vector3 (-x, ySph, 0f), sphereSize);
                Gizmos.DrawSphere (new Vector3 (0f, ySph, z),  sphereSize);
                Gizmos.DrawSphere (new Vector3 (0f, ySph, -z), sphereSize);
                
                // Force preview
                if (wind.showNoise == true)
                {
                    // Preview rate
                    stepX = (int)(wind.gizmoSize.x / wind.previewDensity);
                    stepZ = (int)(wind.gizmoSize.z / wind.previewDensity);

                    // Create preview helpers
                    for (int xx = -(stepX / 2); xx < stepX / 2 + 1; xx++)
                    {
                        for (int zz = -(stepZ / 2); zz < stepZ / 2 + 1; zz++)
                        {
                            // Local position
                            localPos   = Vector3.zero;
                            localPos.x = xx * wind.previewDensity;
                            localPos.z = zz * wind.previewDensity;
                            localPos.y = 0.2f;

                            // Get perlin value for local position
                            perlinVal = wind.PerlinFixedLocal (localPos);

                            // Get final strength for local position by min and max str
                            windStr = wind.WindStrength (perlinVal);

                            // Get vector for current point
                            vector = wind.GetVectorLocalPreview (localPos) * wind.previewSize;
                            
                            // Set color
                            if (windStr >= 0)
                            {
                                color.r = perlinVal;
                                color.g = 1f - perlinVal;
                                color.b = 0f;
                            }
                            else
                            {
                                color.r = 0f;
                                color.g = perlinVal;
                                color.b = 1f - perlinVal;
                            }

                            Gizmos.color = color;
                            Gizmos.DrawWireSphere (localPos, windStr * 0.1f * wind.previewSize);
                            Gizmos.DrawLine (localPos, localPos + vector * windStr);
                        }
                    }
                }
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////

        void SetDirty (RayfireWind scr)
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


/*

EditorGUILayout.PrefixLabel ("MinMax");
EditorGUILayout.FloatField (wind.minimum, GUILayout.Width (50));

EditorGUI.BeginChangeCheck();
EditorGUILayout.MinMaxSlider (ref wind.minimum, ref wind.maximum, -5f, 5, GUILayout.Width (EditorGUIUtility.currentViewWidth - 400f));
if (EditorGUI.EndChangeCheck() == true)
{
    foreach (RayfireWind scr in targets)
    {
        scr.minimum = wind.minimum;
        scr.maximum = wind.maximum;
        SetDirty (scr);
    }
}

EditorGUILayout.FloatField (wind.maximum, GUILayout.Width (50));
GUILayout.EndHorizontal ();

*/