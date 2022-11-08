using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

namespace RayFire
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireGun))]
    public class RayfireGunEditor : Editor
    {
        RayfireGun   gun;
        List<string> layerNames;
        
        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////
        
        static int space = 3;
        
        static GUIContent gui_dir_show = new GUIContent ("Show",   "Show shooting ray");
        static GUIContent gui_dir_axis = new GUIContent ("Axis",     "Shooting direction if Target is not defined.");
        static GUIContent gui_dir_dist = new GUIContent ("Distance", "Maximum shooting distance.");
        static GUIContent gui_dir_targ = new GUIContent ("Target",   "");
        
        static GUIContent gui_bur_rnd  = new GUIContent ("Rounds", "");
        static GUIContent gui_bur_rate = new GUIContent ("Rate", "");
        
        static GUIContent gui_imp_show = new GUIContent ("Show",             "Show impact position and radius. Visible when shooting ray intersects with collider.");
        static GUIContent gui_imp_tp   = new GUIContent ("Type",             "");
        static GUIContent gui_imp_str  = new GUIContent ("Strength",         "");
        static GUIContent gui_imp_rad  = new GUIContent ("Radius",           "");
        static GUIContent gui_imp_cls  = new GUIContent ("Demolish Cluster", "");
        static GUIContent gui_imp_ina  = new GUIContent ("Affect Inactive",  "");
        
        static GUIContent gui_comp_rg = new GUIContent ("Rigid",    "");
        static GUIContent gui_comp_rt = new GUIContent ("RigidRoot", "");
        static GUIContent gui_comp_rb = new GUIContent ("RigidBody",   "");
        
        static GUIContent gui_dmg_val = new GUIContent ("Damage", "");
        
        static GUIContent gui_vfx_debris = new GUIContent ("Debris", "");
        static GUIContent gui_vfx_dust = new GUIContent ("Dust", "");
        static GUIContent gui_vfx_flash = new GUIContent ("Flash", "");
        
        static GUIContent gui_fl_int_min  = new GUIContent ("Minimum",  "");
        static GUIContent gui_fl_int_max  = new GUIContent ("Maximum",  "");
        static GUIContent gui_fl_rng_min  = new GUIContent ("Minimum",  "");
        static GUIContent gui_fl_rng_max  = new GUIContent ("Maximum",  "");
        static GUIContent gui_fl_distance = new GUIContent ("Distance", "");
        static GUIContent gui_fl_color    = new GUIContent ("Color",    "");
        
        /// /////////////////////////////////////////////////////////
        /// Inspector
        /// /////////////////////////////////////////////////////////
        
        public override void OnInspectorGUI()
        {
            gun = target as RayfireGun;
            if (gun == null)
                return;

            GUILayout.Space (8);

            UI_Buttons();
            
            GUILayout.Space (space);

            UI_Direction();
            
            GUILayout.Space (space);

            UI_Burst();
            
            GUILayout.Space (space);

            UI_Impact();
            
            GUILayout.Space (space);

            UI_Components();
            
            GUILayout.Space (space);

            UI_Damage();

            GUILayout.Space (space);

            UI_VFX();
            
            GUILayout.Space (space);

            UI_Filters();
            
            GUILayout.Space (8);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Buttons
        /// /////////////////////////////////////////////////////////

        void UI_Buttons()
        {
            if (Application.isPlaying == true)
            {
                if (GUILayout.Toggle (gun.shooting, "Start Shooting", "Button", GUILayout.Height (25)) == true)
                    gun.StartShooting();
                else
                    gun.StopShooting();

                GUILayout.BeginHorizontal();

                if (GUILayout.Button ("Single Shot", GUILayout.Height (22)))
                    foreach (var targ in targets)
                        (targ as RayfireGun).Shoot();

                if (GUILayout.Button ("    Burst   ", GUILayout.Height (22)))
                    foreach (var targ in targets)
                        (targ as RayfireGun).Burst();

                EditorGUILayout.EndHorizontal();
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Direction
        /// /////////////////////////////////////////////////////////
        
        void UI_Direction()
        {
            GUILayout.Label ("  Direction", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            gun.showRay = EditorGUILayout.Toggle (gui_dir_show, gun.showRay);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireGun scr in targets)
                {
                    scr.showRay = gun.showRay;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);

            EditorGUI.BeginChangeCheck();
            gun.axis = (AxisType)EditorGUILayout.EnumPopup (gui_dir_axis, gun.axis);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireGun scr in targets)
                {
                    scr.axis = gun.axis;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            gun.target = (Transform)EditorGUILayout.ObjectField (gui_dir_targ, gun.target, typeof(Transform), true);
            if (EditorGUI.EndChangeCheck() == true)
                SetDirty (gun);
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            gun.maxDistance = EditorGUILayout.Slider (gui_dir_dist, gun.maxDistance, 0f, 100f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireGun scr in targets)
                {
                    scr.maxDistance = gun.maxDistance;
                    SetDirty (scr);
                }
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Burst
        /// /////////////////////////////////////////////////////////
        
        void UI_Burst()
        {
            GUILayout.Label ("  Burst", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            gun.rounds = EditorGUILayout.IntSlider (gui_bur_rnd, gun.rounds, 2, 20);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireGun scr in targets)
                {
                    scr.rounds = gun.rounds;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            gun.rate = EditorGUILayout.Slider (gui_bur_rate, gun.rate, 0.01f, 5f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireGun scr in targets)
                {
                    scr.rate = gun.rate;
                    SetDirty (scr);
                }
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Impact
        /// /////////////////////////////////////////////////////////

        void UI_Impact()
        {
            GUILayout.Label ("  Impact", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            gun.showHit = EditorGUILayout.Toggle (gui_imp_show, gun.showHit);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireGun scr in targets)
                {
                    scr.showHit = gun.showHit;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            gun.type = (RayfireGun.ImpactType)EditorGUILayout.EnumPopup (gui_imp_tp, gun.type);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireGun scr in targets)
                {
                    scr.type = gun.type;
                    SetDirty (scr);
                }
            }

            GUILayout.Space (space);

            EditorGUI.BeginChangeCheck();
            gun.strength = EditorGUILayout.Slider (gui_imp_str, gun.strength, 0f, 20f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireGun scr in targets)
                {
                    scr.strength = gun.strength;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);

            EditorGUI.BeginChangeCheck();
            gun.radius = EditorGUILayout.Slider (gui_imp_rad, gun.radius, 0f, 10f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireGun scr in targets)
                {
                    scr.radius = gun.radius;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);

            EditorGUI.BeginChangeCheck();
            gun.affectInactive = EditorGUILayout.Toggle (gui_imp_ina, gun.affectInactive);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireGun scr in targets)
                {
                    scr.affectInactive = gun.affectInactive;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);

            EditorGUI.BeginChangeCheck();
            gun.demolishCluster = EditorGUILayout.Toggle (gui_imp_cls, gun.demolishCluster);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireGun scr in targets)
                {
                    scr.demolishCluster = gun.demolishCluster;
                    SetDirty (scr);
                }
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Components
        /// /////////////////////////////////////////////////////////

        void UI_Components()
        {
            GUILayout.Label ("  Components", EditorStyles.boldLabel);
        
            EditorGUI.BeginChangeCheck();
            gun.rigid = EditorGUILayout.Toggle (gui_comp_rg, gun.rigid);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireGun scr in targets)
                {
                    scr.rigid = gun.rigid;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);

            EditorGUI.BeginChangeCheck();
            gun.rigidRoot = EditorGUILayout.Toggle (gui_comp_rt, gun.rigidRoot);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireGun scr in targets)
                {
                    scr.rigidRoot = gun.rigidRoot;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);

            EditorGUI.BeginChangeCheck();
            gun.rigidBody = EditorGUILayout.Toggle (gui_comp_rb, gun.rigidBody);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireGun scr in targets)
                {
                    scr.rigidBody = gun.rigidBody;
                    SetDirty (scr);
                }
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Damage
        /// /////////////////////////////////////////////////////////

        void UI_Damage()
        {
            GUILayout.Label ("  Damage", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            gun.damage = EditorGUILayout.Slider (gui_dmg_val, gun.damage, 0, 100f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireGun scr in targets)
                {
                    scr.damage = gun.damage;
                    SetDirty (scr);
                }
            }
        }

        /// /////////////////////////////////////////////////////////
        /// VFX
        /// /////////////////////////////////////////////////////////

        void UI_VFX()
        {
            GUILayout.Label ("  VFX", EditorStyles.boldLabel);
        
            EditorGUI.BeginChangeCheck();
            gun.debris = EditorGUILayout.Toggle (gui_vfx_debris, gun.debris);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireGun scr in targets)
                {
                    scr.debris = gun.debris;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);

            EditorGUI.BeginChangeCheck();
            gun.dust = EditorGUILayout.Toggle (gui_vfx_dust, gun.dust);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireGun scr in targets)
                {
                    scr.dust = gun.dust;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);

            EditorGUI.BeginChangeCheck();
            gun.flash = EditorGUILayout.Toggle (gui_vfx_flash, gun.flash);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireGun scr in targets)
                {
                    scr.flash = gun.flash;
                    SetDirty (scr);
                }
            }

            if (gun.flash == true)
                UI_Flash();
        }

        void UI_Flash()
        {
            EditorGUI.indentLevel++;
                
            GUILayout.Label ("      Intensity", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            gun.Flash.intensityMin = EditorGUILayout.Slider (gui_fl_int_min, gun.Flash.intensityMin, 0.1f, 5f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireGun scr in targets)
                {
                    scr.Flash.intensityMin = gun.Flash.intensityMin;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            gun.Flash.intensityMax = EditorGUILayout.Slider (gui_fl_int_max, gun.Flash.intensityMax, 0.1f, 5f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireGun scr in targets)
                {
                    scr.Flash.intensityMax = gun.Flash.intensityMax;
                    SetDirty (scr);
                }
            }
                
            GUILayout.Label ("      Range", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            gun.Flash.rangeMin = EditorGUILayout.Slider (gui_fl_rng_min, gun.Flash.rangeMin, 0.01f, 10f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireGun scr in targets)
                {
                    scr.Flash.rangeMin = gun.Flash.rangeMin;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            gun.Flash.rangeMax = EditorGUILayout.Slider (gui_fl_rng_max, gun.Flash.rangeMax, 0.01f, 10f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireGun scr in targets)
                {
                    scr.Flash.rangeMax = gun.Flash.rangeMax;
                    SetDirty (scr);
                }
            }
                
            GUILayout.Label ("      Other", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            gun.Flash.distance = EditorGUILayout.Slider (gui_fl_distance, gun.Flash.distance, 0.01f, 2f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireGun scr in targets)
                {
                    scr.Flash.distance = gun.Flash.distance;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
                
            EditorGUI.BeginChangeCheck();
            gun.Flash.color = EditorGUILayout.ColorField (gui_fl_color, gun.Flash.color);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireGun scr in targets)
                {
                    scr.Flash.color = gun.Flash.color;
                    SetDirty (scr);
                }
            }
                
            EditorGUI.indentLevel--;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Filters
        /// /////////////////////////////////////////////////////////
        
        void UI_Filters()
        {
            GUILayout.Label ("  Filters", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            gun.tagFilter = EditorGUILayout.TagField ("Tag", gun.tagFilter);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireGun scr in targets)
                {
                    scr.tagFilter = gun.tagFilter;
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
            gun.mask = EditorGUILayout.MaskField ("Layer", gun.mask, layerNames.ToArray());
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireGun scr in targets)
                {
                    scr.mask = gun.mask;
                    SetDirty (scr);
                }
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Draw
        /// /////////////////////////////////////////////////////////
        
        [DrawGizmo (GizmoType.Selected | GizmoType.NonSelected | GizmoType.Pickable)]
        static void DrawGizmosSelected (RayfireGun gun, GizmoType gizmoType)
        {
            // Ray
            if (gun.showRay == true)
            {
                Gizmos.DrawRay (gun.transform.position, gun.ShootVector * gun.maxDistance);
            }

            // Hit
            if (gun.showHit == true)
            {
                RaycastHit hit;
                bool       hitState = Physics.Raycast (gun.transform.position, gun.ShootVector, out hit, gun.maxDistance, gun.mask);
                if (hitState == true)
                {
                    // TODO COLOR BY IMPACT STR

                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere (hit.point, gun.radius);
                }
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////
        
        void SetDirty (RayfireGun scr)
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