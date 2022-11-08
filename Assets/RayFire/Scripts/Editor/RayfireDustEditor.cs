using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine.Rendering;

namespace RayFire
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireDust))]
    public class RayfireDustEditor : Editor
    {
        RayfireDust        dust;
        List<string>       layerNames;
        SerializedProperty matListProp;
        ReorderableList    matList; 
        
        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////
        
        static int space = 3;
        static bool exp_mat;
        static bool exp_emit;
        static bool exp_dyn;
        static bool exp_noise;
        static bool exp_coll;
        static bool exp_lim;
        static bool exp_rend;
        
        static GUIContent gui_emit_dml     = new GUIContent ("Demolition",     "");
        static GUIContent gui_emit_act     = new GUIContent ("Activation",     "");
        static GUIContent gui_emit_imp     = new GUIContent ("Impact",         "");
        static GUIContent gui_main_op      = new GUIContent ("Opacity",        "");
        static GUIContent gui_main_mat     = new GUIContent ("Material",       "");
        static GUIContent gui_ems_tp       = new GUIContent ("Type",           "");
        static GUIContent gui_ems_am       = new GUIContent ("Amount",         "");
        static GUIContent gui_ems_rate     = new GUIContent ("Rate",           "");
        static GUIContent gui_ems_dur      = new GUIContent ("Duration",       "");
        static GUIContent gui_ems_life_min = new GUIContent ("Life Min",       "");
        static GUIContent gui_ems_life_max = new GUIContent ("Life Max",       "");
        static GUIContent gui_ems_size_min = new GUIContent ("Size Min",       "");
        static GUIContent gui_ems_size_max = new GUIContent ("Size Max",       "");
        static GUIContent gui_ems_mat      = new GUIContent ("Material",       "");
        static GUIContent gui_dn_speed_min = new GUIContent ("Speed Min",      "");
        static GUIContent gui_dn_speed_max = new GUIContent ("Speed Max",      "");
        static GUIContent gui_dn_grav_min  = new GUIContent ("Gravity Min",    "");
        static GUIContent gui_dn_grav_max  = new GUIContent ("Gravity Max",    "");
        static GUIContent gui_dn_rot       = new GUIContent ("Rotation",       "");
        static GUIContent gui_ns_en        = new GUIContent ("Enable",         "");
        static GUIContent gui_ns_qual      = new GUIContent ("Quality",        "");
        static GUIContent gui_ns_str_min   = new GUIContent ("Strength Min",   "");
        static GUIContent gui_ns_str_max   = new GUIContent ("Strength Max",   "");
        static GUIContent gui_ns_freq      = new GUIContent ("Frequency",      "");
        static GUIContent gui_ns_scroll    = new GUIContent ("Scroll Speed",   "");
        static GUIContent gui_ns_damp      = new GUIContent ("Damping",        "");
        static GUIContent gui_col_mask     = new GUIContent ("Collides With",  "");
        static GUIContent gui_col_qual     = new GUIContent ("Quality",        "");
        static GUIContent gui_col_rad      = new GUIContent ("Radius Scale",   "");
        static GUIContent gui_lim_min      = new GUIContent ("Min Particles",  "");
        static GUIContent gui_lim_max      = new GUIContent ("Max Particles",  "");
        static GUIContent gui_lim_perc     = new GUIContent ("Percentage",     "");
        static GUIContent gui_lim_size     = new GUIContent ("Size Threshold", "");
        static GUIContent gui_ren_cast     = new GUIContent ("Cast",           "");
        static GUIContent gui_ren_rec      = new GUIContent ("Receive",        "");
        static GUIContent gui_ren_prob     = new GUIContent ("Light Probes",   "");
        
        /// /////////////////////////////////////////////////////////
        /// Enable
        /// /////////////////////////////////////////////////////////
        
        private void OnEnable()
        {
            matListProp                 = serializedObject.FindProperty("dustMaterials");
            matList                     = new ReorderableList(serializedObject, matListProp, true, true, true, true);
            matList.drawElementCallback = DrawInitListItems;
            matList.drawHeaderCallback  = DrawInitHeader;
            matList.onAddCallback       = AddInit;
            matList.onRemoveCallback    = RemoveInit;
            
            if (EditorPrefs.HasKey ("rf_um") == true) exp_mat   = EditorPrefs.GetBool ("rf_um");
            if (EditorPrefs.HasKey ("rf_ue") == true) exp_emit  = EditorPrefs.GetBool ("rf_ue");
            if (EditorPrefs.HasKey ("rf_ud") == true) exp_dyn   = EditorPrefs.GetBool ("rf_ud");
            if (EditorPrefs.HasKey ("rf_un") == true) exp_noise = EditorPrefs.GetBool ("rf_un");
            if (EditorPrefs.HasKey ("rf_uc") == true) exp_coll  = EditorPrefs.GetBool ("rf_uc");
            if (EditorPrefs.HasKey ("rf_ul") == true) exp_lim   = EditorPrefs.GetBool ("rf_ul");
            if (EditorPrefs.HasKey ("rf_ur") == true) exp_rend  = EditorPrefs.GetBool ("rf_ur");
        }
        
        /// /////////////////////////////////////////////////////////
        /// Inspector
        /// /////////////////////////////////////////////////////////
        
        public override void OnInspectorGUI()
        {
            dust = target as RayfireDust;
            if (dust == null)
                return;
            
            GUILayout.Space (8);

            UI_Buttons();
            
            GUILayout.Space (space);
            
            UI_Emit();

            GUILayout.Space (space);

            UI_Main();

            GUILayout.Space (space);

            UI_Properties();
            
            GUILayout.Space (8);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Buttons
        /// /////////////////////////////////////////////////////////

        void UI_Buttons()
        {
            GUILayout.BeginHorizontal();

            if (Application.isPlaying == true)
            {
                if (GUILayout.Button ("Emit", GUILayout.Height (25)))
                    foreach (var targ in targets)
                        if (targ as RayfireDust != null)
                            (targ as RayfireDust).Emit();

                if (GUILayout.Button ("Clean", GUILayout.Height (25)))
                    foreach (var targ in targets)
                        if (targ as RayfireDust != null)
                            (targ as RayfireDust).Clean();
            }

            EditorGUILayout.EndHorizontal();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Emit
        /// /////////////////////////////////////////////////////////
        
        void UI_Emit()
        {
            GUILayout.Label ("  Emit Event", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            dust.onDemolition = EditorGUILayout.Toggle (gui_emit_dml, dust.onDemolition);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireDust scr in targets)
                {
                    scr.onDemolition = dust.onDemolition;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            dust.onActivation = EditorGUILayout.Toggle (gui_emit_act, dust.onActivation);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireDust scr in targets)
                {
                    scr.onActivation = dust.onActivation;
                    SetDirty (scr);
                }
            }

            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            dust.onImpact = EditorGUILayout.Toggle (gui_emit_imp, dust.onImpact);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireDust scr in targets)
                {
                    scr.onImpact = dust.onImpact;
                    SetDirty (scr);
                }
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Main
        /// /////////////////////////////////////////////////////////

        void UI_Main()
        {
            GUILayout.Label ("  Dust", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            dust.opacity = EditorGUILayout.Slider (gui_main_op, dust.opacity, 0.01f, 1f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireDust scr in targets)
                {
                    scr.opacity = dust.opacity;
                    SetDirty (scr);
                }
            }

            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            dust.dustMaterial = (Material)EditorGUILayout.ObjectField (gui_main_mat, dust.dustMaterial, typeof(Material), true);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireDust scr in targets)
                {
                    scr.dustMaterial = dust.dustMaterial;
                    SetDirty (scr);
                }
            }

            GUILayout.Space (space);

            SetFoldoutPref (ref exp_mat, "rf_um", "Random Materials", true);
            if (exp_mat == true)
            {
                GUILayout.Space (space);

                serializedObject.Update();
                matList.DoLayoutList();
                serializedObject.ApplyModifiedProperties();
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Properties
        /// /////////////////////////////////////////////////////////
        
        void UI_Properties()
        {
            GUILayout.Label ("  Properties", EditorStyles.boldLabel);

            UI_Emission();

            GUILayout.Space (space);

            UI_Dynamic();

            GUILayout.Space (space);

            UI_Noise();

            GUILayout.Space (space);

            UI_Collision();

            GUILayout.Space (space);

            UI_Limitations();
            
            GUILayout.Space (space);

            UI_Rendering();
        }

        /// /////////////////////////////////////////////////////////
        /// Emission
        /// /////////////////////////////////////////////////////////
        
        void UI_Emission()
        {
            SetFoldoutPref (ref exp_emit, "rf_ue", "Emission", true);
            if (exp_emit == true)
            {
                GUILayout.Space (space);

                EditorGUI.indentLevel++;

                GUILayout.Label ("      Burst", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                dust.emission.burstType = (RFParticles.BurstType)EditorGUILayout.EnumPopup (gui_ems_tp, dust.emission.burstType);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDust scr in targets)
                    {
                        scr.emission.burstType = dust.emission.burstType;
                        SetDirty (scr);
                    }
                }

                if (dust.emission.burstType != RFParticles.BurstType.None)
                {
                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    dust.emission.burstAmount = EditorGUILayout.IntSlider (gui_ems_am, dust.emission.burstAmount, 0, 500);
                    if (EditorGUI.EndChangeCheck() == true)
                    {
                        foreach (RayfireDust scr in targets)
                        {
                            scr.emission.burstAmount = dust.emission.burstAmount;
                            SetDirty (scr);
                        }
                    }
                }

                GUILayout.Label ("      Distance", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                dust.emission.distanceRate = EditorGUILayout.Slider (gui_ems_rate, dust.emission.distanceRate, 0f, 5f);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDust scr in targets)
                    {
                        scr.emission.distanceRate = dust.emission.distanceRate;
                        SetDirty (scr);
                    }
                }

                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                dust.emission.duration = EditorGUILayout.Slider (gui_ems_dur, dust.emission.duration, 0.5f, 10);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDust scr in targets)
                    {
                        scr.emission.duration = dust.emission.duration;
                        SetDirty (scr);
                    }
                }

                GUILayout.Label ("      Lifetime", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                dust.emission.lifeMin = EditorGUILayout.Slider (gui_ems_life_min, dust.emission.lifeMin, 1f, 60f);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDust scr in targets)
                    {
                        scr.emission.lifeMin = dust.emission.lifeMin;
                        SetDirty (scr);
                    }
                }

                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                dust.emission.lifeMax = EditorGUILayout.Slider (gui_ems_life_max, dust.emission.lifeMax, 1f, 60f);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDust scr in targets)
                    {
                        scr.emission.lifeMax = dust.emission.lifeMax;
                        SetDirty (scr);
                    }
                }

                GUILayout.Label ("      Size", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                dust.emission.sizeMin = EditorGUILayout.Slider (gui_ems_size_min, dust.emission.sizeMin, 0.1f, 10f);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDust scr in targets)
                    {
                        scr.emission.sizeMin = dust.emission.sizeMin;
                        SetDirty (scr);
                    }
                }

                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                dust.emission.sizeMax = EditorGUILayout.Slider (gui_ems_size_max, dust.emission.sizeMax, 0.1f, 10f);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDust scr in targets)
                    {
                        scr.emission.sizeMax = dust.emission.sizeMax;
                        SetDirty (scr);
                    }
                }
                
                GUILayout.Label ("      Material", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                dust.emissionMaterial = (Material)EditorGUILayout.ObjectField (gui_ems_mat, dust.emissionMaterial, typeof(Material), true);
                if (EditorGUI.EndChangeCheck())
                {
                    foreach (RayfireDust scr in targets)
                    {
                        scr.emissionMaterial = dust.emissionMaterial;
                        SetDirty (scr);
                    }
                }

                EditorGUI.indentLevel--;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Dynamic
        /// /////////////////////////////////////////////////////////
        
        void UI_Dynamic()
        {
            SetFoldoutPref (ref exp_dyn, "rf_ud", "Dynamic", true);
            if (exp_dyn == true)
            {
                GUILayout.Space (space);

                EditorGUI.indentLevel++;

                GUILayout.Label ("      Speed", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                dust.dynamic.speedMin = EditorGUILayout.Slider (gui_dn_speed_min, dust.dynamic.speedMin, 0f, 10f);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDust scr in targets)
                    {
                        scr.dynamic.speedMin = dust.dynamic.speedMin;
                        SetDirty (scr);
                    }
                }

                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                dust.dynamic.speedMax = EditorGUILayout.Slider (gui_dn_speed_max, dust.dynamic.speedMax, 0f, 10f);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDust scr in targets)
                    {
                        scr.dynamic.speedMax = dust.dynamic.speedMax;
                        SetDirty (scr);
                    }
                }
                
                GUILayout.Label ("      Gravity", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                dust.dynamic.gravityMin = EditorGUILayout.Slider (gui_dn_grav_min, dust.dynamic.gravityMin, -2f, 2f);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDust scr in targets)
                    {
                        scr.dynamic.gravityMin = dust.dynamic.gravityMin;
                        SetDirty (scr);
                    }
                }

                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                dust.dynamic.gravityMax = EditorGUILayout.Slider (gui_dn_grav_max, dust.dynamic.gravityMax, -2f, 2f);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDust scr in targets)
                    {
                        scr.dynamic.gravityMax = dust.dynamic.gravityMax;
                        SetDirty (scr);
                    }
                }

                GUILayout.Label ("      Rotation", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                dust.dynamic.rotation = EditorGUILayout.Slider (gui_dn_rot, dust.dynamic.rotation, 0f, 1f);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDust scr in targets)
                    {
                        scr.dynamic.rotation = dust.dynamic.rotation;
                        SetDirty (scr);
                    }
                }

                EditorGUI.indentLevel--;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Noise
        /// /////////////////////////////////////////////////////////
        
        void UI_Noise()
        {
            SetFoldoutPref (ref exp_noise, "rf_un", "Noise", true);
            if (exp_noise == true)
            {
                GUILayout.Space (space);

                EditorGUI.indentLevel++;

                GUILayout.Label ("      Main", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                dust.noise.enabled = EditorGUILayout.Toggle (gui_ns_en, dust.noise.enabled);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDust scr in targets)
                    {
                        scr.noise.enabled = dust.noise.enabled;
                        SetDirty (scr);
                    }
                }

                if (dust.noise.enabled == true)
                {
                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    dust.noise.quality = (ParticleSystemNoiseQuality)EditorGUILayout.EnumPopup (gui_ns_qual, dust.noise.quality);
                    if (EditorGUI.EndChangeCheck() == true)
                    {
                        foreach (RayfireDust scr in targets)
                        {
                            scr.noise.quality = dust.noise.quality;
                            SetDirty (scr);
                        }
                    }

                    GUILayout.Label ("      Strength", EditorStyles.boldLabel);

                    EditorGUI.BeginChangeCheck();
                    dust.noise.strengthMin = EditorGUILayout.Slider (gui_ns_str_min, dust.noise.strengthMin, 0f, 3f);
                    if (EditorGUI.EndChangeCheck() == true)
                    {
                        foreach (RayfireDust scr in targets)
                        {
                            scr.noise.strengthMin = dust.noise.strengthMin;
                            SetDirty (scr);
                        }
                    }

                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    dust.noise.strengthMax = EditorGUILayout.Slider (gui_ns_str_max, dust.noise.strengthMax, 0f, 3f);
                    if (EditorGUI.EndChangeCheck() == true)
                    {
                        foreach (RayfireDust scr in targets)
                        {
                            scr.noise.strengthMax = dust.noise.strengthMax;
                            SetDirty (scr);
                        }
                    }

                    GUILayout.Label ("      Other", EditorStyles.boldLabel);

                    EditorGUI.BeginChangeCheck();
                    dust.noise.frequency = EditorGUILayout.Slider (gui_ns_freq, dust.noise.frequency, 0.001f, 3f);
                    if (EditorGUI.EndChangeCheck() == true)
                    {
                        foreach (RayfireDust scr in targets)
                        {
                            scr.noise.frequency = dust.noise.frequency;
                            SetDirty (scr);
                        }
                    }

                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    dust.noise.scrollSpeed = EditorGUILayout.Slider (gui_ns_scroll, dust.noise.scrollSpeed, 0f, 2f);
                    if (EditorGUI.EndChangeCheck() == true)
                    {
                        foreach (RayfireDust scr in targets)
                        {
                            scr.noise.scrollSpeed = dust.noise.scrollSpeed;
                            SetDirty (scr);
                        }
                    }

                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    dust.noise.damping = EditorGUILayout.Toggle (gui_ns_damp, dust.noise.damping);
                    if (EditorGUI.EndChangeCheck() == true)
                    {
                        foreach (RayfireDust scr in targets)
                        {
                            scr.noise.damping = dust.noise.damping;
                            SetDirty (scr);
                        }
                    }
                }

                EditorGUI.indentLevel--;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Collision
        /// /////////////////////////////////////////////////////////
        
        void UI_Collision()
        {
            SetFoldoutPref (ref exp_coll, "rf_uc", "Collision", true);
            if (exp_coll == true)
            {
                GUILayout.Space (space);

                EditorGUI.indentLevel++;

                GUILayout.Label ("      Common", EditorStyles.boldLabel);
                
                // Layer mask
                if (layerNames == null)
                {
                    layerNames = new List<string>();
                    for (int i = 0; i <= 31; i++)
                        layerNames.Add (i + ". " + LayerMask.LayerToName (i));
                }

                EditorGUI.BeginChangeCheck();
                dust.collision.collidesWith = EditorGUILayout.MaskField (gui_col_mask, dust.collision.collidesWith, layerNames.ToArray());
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDust scr in targets)
                    {
                        scr.collision.collidesWith = dust.collision.collidesWith;
                        SetDirty (scr);
                    }
                }
                
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                dust.collision.quality = (ParticleSystemCollisionQuality)EditorGUILayout.EnumPopup (gui_col_qual, dust.collision.quality);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDust scr in targets)
                    {
                        scr.collision.quality = dust.collision.quality;
                        SetDirty (scr);
                    }
                }

                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                dust.collision.radiusScale = EditorGUILayout.Slider (gui_col_rad, dust.collision.radiusScale, 0.1f, 2f);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDust scr in targets)
                    {
                        scr.collision.radiusScale = dust.collision.radiusScale;
                        SetDirty (scr);
                    }
                }
        
                EditorGUI.indentLevel--;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Limitations
        /// /////////////////////////////////////////////////////////

        void UI_Limitations()
        {
            SetFoldoutPref (ref exp_lim, "rf_ul", "Limitations", true);
            if (exp_lim == true)
            {
                GUILayout.Space (space);

                EditorGUI.indentLevel++;

                GUILayout.Label ("      Particle System", EditorStyles.boldLabel);
                
                EditorGUI.BeginChangeCheck();
                dust.limitations.minParticles = EditorGUILayout.IntSlider (gui_lim_min, dust.limitations.minParticles, 3, 100);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDust scr in targets)
                    {
                        scr.limitations.minParticles = dust.limitations.minParticles;
                        SetDirty (scr);
                    }
                }
                
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                dust.limitations.maxParticles = EditorGUILayout.IntSlider (gui_lim_max, dust.limitations.maxParticles, 5, 100);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDust scr in targets)
                    {
                        scr.limitations.maxParticles = dust.limitations.maxParticles;
                        SetDirty (scr);
                    }
                }
                
                GUILayout.Label ("      Fragments", EditorStyles.boldLabel);
                
                EditorGUI.BeginChangeCheck();
                dust.limitations.percentage = EditorGUILayout.IntSlider (gui_lim_perc, dust.limitations.percentage, 10, 100);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDust scr in targets)
                    {
                        scr.limitations.percentage = dust.limitations.percentage;
                        SetDirty (scr);
                    }
                }
                
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                dust.limitations.sizeThreshold = EditorGUILayout.Slider (gui_lim_size, dust.limitations.sizeThreshold, 0.05f, 5);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDust scr in targets)
                    {
                        scr.limitations.sizeThreshold = dust.limitations.sizeThreshold;
                        SetDirty (scr);
                    }
                }
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Rendering
        /// /////////////////////////////////////////////////////////

        void UI_Rendering()
        {
            SetFoldoutPref (ref exp_rend, "rf_ur", "Rendering", true);
            if (exp_rend == true)
            {
                GUILayout.Space (space);

                EditorGUI.indentLevel++;

                GUILayout.Label ("      Shadows", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                dust.rendering.castShadows = EditorGUILayout.Toggle (gui_ren_cast, dust.rendering.castShadows);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDust scr in targets)
                    {
                        scr.rendering.castShadows = dust.rendering.castShadows;
                        SetDirty (scr);
                    }
                }
                
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                dust.rendering.receiveShadows = EditorGUILayout.Toggle (gui_ren_rec, dust.rendering.receiveShadows);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDust scr in targets)
                    {
                        scr.rendering.receiveShadows = dust.rendering.receiveShadows;
                        SetDirty (scr);
                    }
                }
                
                GUILayout.Label ("      Light", EditorStyles.boldLabel);
                
                EditorGUI.BeginChangeCheck();
                dust.rendering.lightProbes = (LightProbeUsage)EditorGUILayout.EnumPopup (gui_ren_prob, dust.rendering.lightProbes);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDust scr in targets)
                    {
                        scr.rendering.lightProbes = dust.rendering.lightProbes;
                        SetDirty (scr);
                    }
                }
            }
        }

        /// /////////////////////////////////////////////////////////
        /// ReorderableList draw
        /// /////////////////////////////////////////////////////////
        
        void DrawInitListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = matList.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y+2, EditorGUIUtility.currentViewWidth - 80f, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        }
        
        void DrawInitHeader(Rect rect)
        {
            rect.x += 10;
            EditorGUI.LabelField(rect, "Random Materials");
        }

        void AddInit(ReorderableList list)
        {
            if (dust.dustMaterials == null)
                dust.dustMaterials = new List<Material>();
            dust.dustMaterials.Add (null);
            list.index = list.count;
        }
        
        void RemoveInit(ReorderableList list)
        {
            if (dust.dustMaterials != null)
            {
                dust.dustMaterials.RemoveAt (list.index);
                list.index = list.index - 1;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////

        void SetDirty (RayfireDust scr)
        {
            if (Application.isPlaying == false)
            {
                EditorUtility.SetDirty (scr);
                EditorSceneManager.MarkSceneDirty (scr.gameObject.scene);
            }
        }
        
        void SetFoldoutPref (ref bool val, string pref, string caption, bool state) 
        {
            EditorGUI.BeginChangeCheck();
            val = EditorGUILayout.Foldout (val, caption, state);
            if (EditorGUI.EndChangeCheck() == true)
                EditorPrefs.SetBool (pref, val);
        }
    }
}