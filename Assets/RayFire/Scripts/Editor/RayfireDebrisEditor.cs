using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using UnityEngine.Rendering;

namespace RayFire
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireDebris))]
    public class RayfireDebrisEditor : Editor
    {
        RayfireDebris debris;
        List<string>  layerNames;
        
        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////
        
        static int space = 3;
        static bool exp_emit;
        static bool exp_dyn;
        static bool exp_noise;
        static bool exp_coll;
        static bool exp_lim;
        static bool exp_rend;
        
        static GUIContent gui_emit_dml     = new GUIContent ("Demolition",     "");
        static GUIContent gui_emit_act     = new GUIContent ("Activation",     "");
        static GUIContent gui_emit_imp     = new GUIContent ("Impact",         "");
        static GUIContent gui_main_ref     = new GUIContent ("Reference",      "");
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
        static GUIContent gui_dn_vel_min   = new GUIContent ("Velocity Min",   "");
        static GUIContent gui_dn_vel_max   = new GUIContent ("Velocity Max",   "");
        static GUIContent gui_dn_grav_min  = new GUIContent ("Gravity Min",    "");
        static GUIContent gui_dn_grav_max  = new GUIContent ("Gravity Max",    "");
        static GUIContent gui_dn_rot       = new GUIContent ("Rotation Speed", "");
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
        static GUIContent gui_col_dmp_tp   = new GUIContent ("Type",           "");
        static GUIContent gui_col_dmp_min  = new GUIContent ("Minimum",        "");
        static GUIContent gui_col_dmp_max  = new GUIContent ("Maximum",        "");
        static GUIContent gui_col_bnc_tp   = new GUIContent ("Type",           "");
        static GUIContent gui_col_bnc_min  = new GUIContent ("Minimum",        "");
        static GUIContent gui_col_bnc_max  = new GUIContent ("Maximum",        "");
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
            if (EditorPrefs.HasKey ("rf_de") == true) exp_emit  = EditorPrefs.GetBool ("rf_de");
            if (EditorPrefs.HasKey ("rf_dd") == true) exp_dyn   = EditorPrefs.GetBool ("rf_dd");
            if (EditorPrefs.HasKey ("rf_dn") == true) exp_noise = EditorPrefs.GetBool ("rf_dn");
            if (EditorPrefs.HasKey ("rf_dc") == true) exp_coll  = EditorPrefs.GetBool ("rf_dc");
            if (EditorPrefs.HasKey ("rf_dl") == true) exp_lim   = EditorPrefs.GetBool ("rf_dl");
            if (EditorPrefs.HasKey ("rf_dr") == true) exp_rend  = EditorPrefs.GetBool ("rf_dr");
        }
        
        /// /////////////////////////////////////////////////////////
        /// Inspector
        /// /////////////////////////////////////////////////////////
        
        public override void OnInspectorGUI()
        {
            debris = target as RayfireDebris;
            if (debris == null)
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
                        if (targ as RayfireDebris != null)
                            (targ as RayfireDebris).Emit();

                if (GUILayout.Button ("Clean", GUILayout.Height (25)))
                    foreach (var targ in targets)
                        if (targ as RayfireDebris != null)
                            (targ as RayfireDebris).Clean();
            }

            EditorGUILayout.EndHorizontal();
        }

        /// /////////////////////////////////////////////////////////
        /// Emit
        /// /////////////////////////////////////////////////////////
        
        void UI_Emit()
        {
            GUILayout.Label ("  Emit Event", EditorStyles.boldLabel);

            // EditorGUILayout.EnumFlagsField(gui_emit_dml, debris.emission.burstType);

            EditorGUI.BeginChangeCheck();
            debris.onDemolition = EditorGUILayout.Toggle (gui_emit_dml, debris.onDemolition);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireDebris scr in targets)
                {
                    scr.onDemolition = debris.onDemolition;
                    SetDirty (scr);
                }
            }

            GUILayout.Space (space);

            EditorGUI.BeginChangeCheck();
            debris.onActivation = EditorGUILayout.Toggle (gui_emit_act, debris.onActivation);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireDebris scr in targets)
                {
                    scr.onActivation = debris.onActivation;
                    SetDirty (scr);
                }
            }

            GUILayout.Space (space);

            EditorGUI.BeginChangeCheck();
            debris.onImpact = EditorGUILayout.Toggle (gui_emit_imp, debris.onImpact);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireDebris scr in targets)
                {
                    scr.onImpact = debris.onImpact;
                    SetDirty (scr);
                }
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Main
        /// /////////////////////////////////////////////////////////
       
        void UI_Main()
        {
            GUILayout.Label ("  Debris", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            debris.debrisReference = (GameObject)EditorGUILayout.ObjectField (gui_main_ref, debris.debrisReference, typeof(GameObject), true);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireDebris scr in targets)
                {
                    scr.debrisReference = debris.debrisReference;
                    SetDirty (scr);
                }
            }

            GUILayout.Space (space);

            EditorGUI.BeginChangeCheck();
            debris.debrisMaterial = (Material)EditorGUILayout.ObjectField (gui_main_mat, debris.debrisMaterial, typeof(Material), true);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireDebris scr in targets)
                {
                    scr.debrisMaterial = debris.debrisMaterial;
                    SetDirty (scr);
                }
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
            SetFoldoutPref (ref exp_emit, "rf_de", "Emission", true);
            if (exp_emit == true)
            {
                GUILayout.Space (space);

                EditorGUI.indentLevel++;

                GUILayout.Label ("      Burst", EditorStyles.boldLabel);
                
                EditorGUI.BeginChangeCheck();
                debris.emission.burstType = (RFParticles.BurstType)EditorGUILayout.EnumPopup (gui_ems_tp, debris.emission.burstType);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.emission.burstType = debris.emission.burstType;
                        SetDirty (scr);
                    }
                }

                if (debris.emission.burstType != RFParticles.BurstType.None)
                {
                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    debris.emission.burstAmount = EditorGUILayout.IntSlider (gui_ems_am, debris.emission.burstAmount, 0, 500);
                    if (EditorGUI.EndChangeCheck() == true)
                    {
                        foreach (RayfireDebris scr in targets)
                        {
                            scr.emission.burstAmount = debris.emission.burstAmount;
                            SetDirty (scr);
                        }
                    }
                }

                GUILayout.Label ("      Distance", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                debris.emission.distanceRate = EditorGUILayout.Slider (gui_ems_rate, debris.emission.distanceRate, 0f, 5f);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.emission.distanceRate = debris.emission.distanceRate;
                        SetDirty (scr);
                    }
                }

                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                debris.emission.duration = EditorGUILayout.Slider (gui_ems_dur, debris.emission.duration, 0.5f, 10);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.emission.duration = debris.emission.duration;
                        SetDirty (scr);
                    }
                }

                GUILayout.Label ("      Lifetime", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                debris.emission.lifeMin = EditorGUILayout.Slider (gui_ems_life_min, debris.emission.lifeMin, 1f, 60f);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.emission.lifeMin = debris.emission.lifeMin;
                        SetDirty (scr);
                    }
                }

                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                debris.emission.lifeMax = EditorGUILayout.Slider (gui_ems_life_max, debris.emission.lifeMax, 1f, 60f);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.emission.lifeMax = debris.emission.lifeMax;
                        SetDirty (scr);
                    }
                }

                GUILayout.Label ("      Size", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                debris.emission.sizeMin = EditorGUILayout.Slider (gui_ems_size_min, debris.emission.sizeMin, 0.001f, 10f);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.emission.sizeMin = debris.emission.sizeMin;
                        SetDirty (scr);
                    }
                }

                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                debris.emission.sizeMax = EditorGUILayout.Slider (gui_ems_size_max, debris.emission.sizeMax, 0.1f, 10f);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.emission.sizeMax = debris.emission.sizeMax;
                        SetDirty (scr);
                    }
                }
                
                GUILayout.Label ("      Material", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                debris.emissionMaterial = (Material)EditorGUILayout.ObjectField (gui_ems_mat, debris.emissionMaterial, typeof(Material), true);
                if (EditorGUI.EndChangeCheck())
                {
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.emissionMaterial = debris.emissionMaterial;
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
            SetFoldoutPref (ref exp_dyn, "rf_dd", "Dynamic", true);
            if (exp_dyn == true)
            {
                GUILayout.Space (space);

                EditorGUI.indentLevel++;

                GUILayout.Label ("      Speed", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                debris.dynamic.speedMin = EditorGUILayout.Slider (gui_dn_speed_min, debris.dynamic.speedMin, 0f, 10f);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.dynamic.speedMin = debris.dynamic.speedMin;
                        SetDirty (scr);
                    }
                }

                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                debris.dynamic.speedMax = EditorGUILayout.Slider (gui_dn_speed_max, debris.dynamic.speedMax, 0f, 10f);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.dynamic.speedMax = debris.dynamic.speedMax;
                        SetDirty (scr);
                    }
                }

                GUILayout.Label ("      Inherit Velocity", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                debris.dynamic.velocityMin = EditorGUILayout.Slider (gui_dn_vel_min, debris.dynamic.velocityMin, 0f, 3f);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.dynamic.velocityMin = debris.dynamic.velocityMin;
                        SetDirty (scr);
                    }
                }

                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                debris.dynamic.velocityMax = EditorGUILayout.Slider (gui_dn_vel_max, debris.dynamic.velocityMax, 0f, 3f);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.dynamic.velocityMax = debris.dynamic.velocityMax;
                        SetDirty (scr);
                    }
                }

                GUILayout.Label ("      Gravity", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                debris.dynamic.gravityMin = EditorGUILayout.Slider (gui_dn_grav_min, debris.dynamic.gravityMin, -2f, 2f);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.dynamic.gravityMin = debris.dynamic.gravityMin;
                        SetDirty (scr);
                    }
                }

                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                debris.dynamic.gravityMax = EditorGUILayout.Slider (gui_dn_grav_max, debris.dynamic.gravityMax, -2f, 2f);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.dynamic.gravityMax = debris.dynamic.gravityMax;
                        SetDirty (scr);
                    }
                }

                GUILayout.Label ("      Rotation", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                debris.dynamic.rotationSpeed = EditorGUILayout.Slider (gui_dn_rot, debris.dynamic.rotationSpeed, 0f, 1f);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.dynamic.rotationSpeed = debris.dynamic.rotationSpeed;
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
            SetFoldoutPref (ref exp_noise, "rf_dn", "Noise", true);
            if (exp_noise == true)
            {
                GUILayout.Space (space);

                EditorGUI.indentLevel++;

                GUILayout.Label ("      Main", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                debris.noise.enabled = EditorGUILayout.Toggle (gui_ns_en, debris.noise.enabled);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.noise.enabled = debris.noise.enabled;
                        SetDirty (scr);
                    }
                }

                if (debris.noise.enabled == true)
                {
                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    debris.noise.quality = (ParticleSystemNoiseQuality)EditorGUILayout.EnumPopup (gui_ns_qual, debris.noise.quality);
                    if (EditorGUI.EndChangeCheck() == true)
                    {
                        foreach (RayfireDebris scr in targets)
                        {
                            scr.noise.quality = debris.noise.quality;
                            SetDirty (scr);
                        }
                    }

                    GUILayout.Label ("      Strength", EditorStyles.boldLabel);

                    EditorGUI.BeginChangeCheck();
                    debris.noise.strengthMin = EditorGUILayout.Slider (gui_ns_str_min, debris.noise.strengthMin, 0f, 3f);
                    if (EditorGUI.EndChangeCheck() == true)
                    {
                        foreach (RayfireDebris scr in targets)
                        {
                            scr.noise.strengthMin = debris.noise.strengthMin;
                            SetDirty (scr);
                        }
                    }

                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    debris.noise.strengthMax = EditorGUILayout.Slider (gui_ns_str_max, debris.noise.strengthMax, 0f, 3f);
                    if (EditorGUI.EndChangeCheck() == true)
                    {
                        foreach (RayfireDebris scr in targets)
                        {
                            scr.noise.strengthMax = debris.noise.strengthMax;
                            SetDirty (scr);
                        }
                    }

                    GUILayout.Label ("      Other", EditorStyles.boldLabel);

                    EditorGUI.BeginChangeCheck();
                    debris.noise.frequency = EditorGUILayout.Slider (gui_ns_freq, debris.noise.frequency, 0.001f, 3f);
                    if (EditorGUI.EndChangeCheck() == true)
                    {
                        foreach (RayfireDebris scr in targets)
                        {
                            scr.noise.frequency = debris.noise.frequency;
                            SetDirty (scr);
                        }
                    }

                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    debris.noise.scrollSpeed = EditorGUILayout.Slider (gui_ns_scroll, debris.noise.scrollSpeed, 0f, 2f);
                    if (EditorGUI.EndChangeCheck() == true)
                    {
                        foreach (RayfireDebris scr in targets)
                        {
                            scr.noise.scrollSpeed = debris.noise.scrollSpeed;
                            SetDirty (scr);
                        }
                    }

                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    debris.noise.damping = EditorGUILayout.Toggle (gui_ns_damp, debris.noise.damping);
                    if (EditorGUI.EndChangeCheck() == true)
                    {
                        foreach (RayfireDebris scr in targets)
                        {
                            scr.noise.damping = debris.noise.damping;
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
            SetFoldoutPref (ref exp_coll, "rf_dc", "Collision", true);
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
                debris.collision.collidesWith = EditorGUILayout.MaskField (gui_col_mask, debris.collision.collidesWith, layerNames.ToArray());
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.collision.collidesWith = debris.collision.collidesWith;
                        SetDirty (scr);
                    }
                }
                
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                debris.collision.quality = (ParticleSystemCollisionQuality)EditorGUILayout.EnumPopup (gui_col_qual, debris.collision.quality);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.collision.quality = debris.collision.quality;
                        SetDirty (scr);
                    }
                }

                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                debris.collision.radiusScale = EditorGUILayout.Slider (gui_col_rad, debris.collision.radiusScale, 0.1f, 2f);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.collision.radiusScale = debris.collision.radiusScale;
                        SetDirty (scr);
                    }
                }
                
                GUILayout.Label ("      Dampen", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                debris.collision.dampenType = (RFParticleCollisionDebris.RFParticleCollisionMatType)EditorGUILayout.EnumPopup 
                    (gui_col_dmp_tp, debris.collision.dampenType);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.collision.dampenType = debris.collision.dampenType;
                        SetDirty (scr);
                    }
                }

                if (debris.collision.dampenType == RFParticleCollisionDebris.RFParticleCollisionMatType.ByProperties)
                {
                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    debris.collision.dampenMin = EditorGUILayout.Slider (gui_col_dmp_min, debris.collision.dampenMin, 0f, 1f);
                    if (EditorGUI.EndChangeCheck() == true)
                    {
                        foreach (RayfireDebris scr in targets)
                        {
                            scr.collision.dampenMin = debris.collision.dampenMin;
                            SetDirty (scr);
                        }
                    }

                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    debris.collision.dampenMax = EditorGUILayout.Slider (gui_col_dmp_max, debris.collision.dampenMax, 0f, 1f);
                    if (EditorGUI.EndChangeCheck() == true)
                    {
                        foreach (RayfireDebris scr in targets)
                        {
                            scr.collision.dampenMax = debris.collision.dampenMax;
                            SetDirty (scr);
                        }
                    }
                }

                GUILayout.Label ("      Bounce", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                debris.collision.bounceType = (RFParticleCollisionDebris.RFParticleCollisionMatType)EditorGUILayout.EnumPopup 
                    (gui_col_bnc_tp, debris.collision.bounceType);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.collision.bounceType = debris.collision.bounceType;
                        SetDirty (scr);
                    }
                }

                if (debris.collision.bounceType == RFParticleCollisionDebris.RFParticleCollisionMatType.ByProperties)
                {
                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    debris.collision.bounceMin = EditorGUILayout.Slider (gui_col_bnc_min, debris.collision.bounceMin, 0f, 1f);
                    if (EditorGUI.EndChangeCheck() == true)
                    {
                        foreach (RayfireDebris scr in targets)
                        {
                            scr.collision.bounceMin = debris.collision.bounceMin;
                            SetDirty (scr);
                        }
                    }

                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    debris.collision.bounceMax = EditorGUILayout.Slider (gui_col_bnc_max, debris.collision.bounceMax, 0f, 1f);
                    if (EditorGUI.EndChangeCheck() == true)
                    {
                        foreach (RayfireDebris scr in targets)
                        {
                            scr.collision.bounceMax = debris.collision.bounceMax;
                            SetDirty (scr);
                        }
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
            SetFoldoutPref (ref exp_lim, "rf_dl", "Limitations", true);
            if (exp_lim == true)
            {
                GUILayout.Space (space);

                EditorGUI.indentLevel++;

                GUILayout.Label ("      Particle System", EditorStyles.boldLabel);
                
                EditorGUI.BeginChangeCheck();
                debris.limitations.minParticles = EditorGUILayout.IntSlider (gui_lim_min, debris.limitations.minParticles, 3, 100);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.limitations.minParticles = debris.limitations.minParticles;
                        SetDirty (scr);
                    }
                }
                
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                debris.limitations.maxParticles = EditorGUILayout.IntSlider (gui_lim_max, debris.limitations.maxParticles, 5, 100);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.limitations.maxParticles = debris.limitations.maxParticles;
                        SetDirty (scr);
                    }
                }
                
                GUILayout.Label ("      Fragments", EditorStyles.boldLabel);
                
                EditorGUI.BeginChangeCheck();
                debris.limitations.percentage = EditorGUILayout.IntSlider (gui_lim_perc, debris.limitations.percentage, 10, 100);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.limitations.percentage = debris.limitations.percentage;
                        SetDirty (scr);
                    }
                }
                
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                debris.limitations.sizeThreshold = EditorGUILayout.Slider (gui_lim_size, debris.limitations.sizeThreshold, 0.05f, 5);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.limitations.sizeThreshold = debris.limitations.sizeThreshold;
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
            SetFoldoutPref (ref exp_rend, "rf_dr", "Rendering", true);
            if (exp_rend == true)
            {
                GUILayout.Space (space);

                EditorGUI.indentLevel++;

                GUILayout.Label ("      Shadows", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                debris.rendering.castShadows = EditorGUILayout.Toggle (gui_ren_cast, debris.rendering.castShadows);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.rendering.castShadows = debris.rendering.castShadows;
                        SetDirty (scr);
                    }
                }
                
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                debris.rendering.receiveShadows = EditorGUILayout.Toggle (gui_ren_rec, debris.rendering.receiveShadows);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.rendering.receiveShadows = debris.rendering.receiveShadows;
                        SetDirty (scr);
                    }
                }
                
                GUILayout.Label ("      Light", EditorStyles.boldLabel);
                
                EditorGUI.BeginChangeCheck();
                debris.rendering.lightProbes = (LightProbeUsage)EditorGUILayout.EnumPopup (gui_ren_prob, debris.rendering.lightProbes);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.rendering.lightProbes = debris.rendering.lightProbes;
                        SetDirty (scr);
                    }
                }
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////
        
        void SetDirty (RayfireDebris scr)
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