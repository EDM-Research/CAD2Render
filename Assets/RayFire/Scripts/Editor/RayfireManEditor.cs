using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using UnityEditor.SceneManagement;

namespace RayFire
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireMan))]
    public class RayfireManEditor : Editor
    {
        Texture2D  logo;
        Texture2D  icon;
        RayfireMan man;
        
        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////
        
        static int space = 3;
        
        // Expand
        static bool expandDemolition;
        static bool expandMat;
        static bool expandMatPresets;
        static bool expandMatHeavyMetal;
        static bool expandMatLightMetal;
        static bool expandMatDenseRock;
        static bool expandMatPorousRock;
        static bool expandMatConcrete;
        static bool expandMatBrick;
        static bool expandMatGlass;
        static bool expandMatRubber;
        static bool expandMatIce;
        static bool expandMatWood;
        
        
        static GUIContent gui_ph_set = new GUIContent ("Set Gravity",   "Sets custom gravity for simulated objects.");
        static GUIContent gui_ph_mul = new GUIContent ("    Multiplier",    "Custom gravity multiplier.");
        static GUIContent gui_ph_int = new GUIContent ("Interpolation", "");
        static GUIContent gui_ph_col = new GUIContent ("Collider Size",  "");
        
        static GUIContent gui_col_mesh = new GUIContent ("Mesh", "Collision detection which will be used for simulated mesh objects.");
        static GUIContent gui_col_cls = new GUIContent ("Cluster", "Collision detection which will be used for Connected and Nested clusters.");
                
        static GUIContent gui_mat_min = new GUIContent ("Minimum Mass", "Minimum mass value which will be assigned to simulated object" + 
                                   " if it's mass calculated by it's volume and density will be less than this value.");
        static GUIContent gui_mat_max = new GUIContent ("Maximum Mass", "Maximum mass value which will be assigned to simulated object" + 
                                                                        " if it's mass calculated by it's volume and density will be higher than this value.");
        static GUIContent gui_mat_pres = new GUIContent ("Material Presets", "List of hardcoded materials with predefined simulation and demolition properties.");
        static GUIContent gui_mat_dest = new GUIContent ("Demolishable",     "Makes object with this material demolishable in runtime.");
        static GUIContent gui_mat_sol  = new GUIContent ("Solidity",         "Global material solidity multiplier which used at collision to calculate if object should be demolished or not.");
        static GUIContent gui_mat_dens = new GUIContent ("Density",          "Object mass depends on picked material density and collider volume.");
        static GUIContent gui_mat_drag = new GUIContent ("Drag",             "Allows to decrease position velocity over time.");
        static GUIContent gui_mat_ang  = new GUIContent ("Angular Drag",     "Allows to decrease rotation velocity over time.");
        static GUIContent gui_mat_mat  = new GUIContent ("Material",         "Physic material which will be used for all objects with this material." + 
                                                                             "If Material is not define then it will be created and defined here at Start using following Frictions and Bounciness properties.");
        static GUIContent gui_mat_dyn  = new GUIContent ("Dynamic Friction", "");
        static GUIContent gui_mat_stat = new GUIContent ("Static Friction",  "");
        static GUIContent gui_mat_bnc  = new GUIContent ("Bounciness",       "");
        
        static GUIContent gui_act_par = new GUIContent ("Parent", "Object which will become parent of activated object");
        
        static GUIContent gui_dml_sol = new GUIContent ("Global Solidity", "Global Solidity multiplier. Affect solidity of all simulated objects.");
        static GUIContent gui_dml_time = new GUIContent ("Time Quota", "Demolition time quota in milliseconds. Allows to prevent demolition at " +
                                                                       "the same frame if there was already another demolition " + 
                                                                       "at the same frame and it took more time than Time Quota value.");
        
        static GUIContent gui_adv_expand = new GUIContent ("Advanced Properties", "");
        static GUIContent gui_adv_parent = new GUIContent ("Parent",              "Defines parent for all new fragments.");
        static GUIContent gui_adv_current = new GUIContent ("Current Amount",              "Amount of created fragments.");
        static GUIContent gui_adv_amount = new GUIContent ("Maximum Amount", "Maximum amount of allowed fragments. Object won't be demolished if existing amount of fragments "+
                                                                             "in scene higher that this value. Fading allows to decrease amount of fragments in scene.");
        static GUIContent gui_adv_bad = new GUIContent ("Bad Mesh Try", "Defines parent for all new fragments.");
        static GUIContent gui_adv_size = new GUIContent ("Size Threshold", "Disable Shadow Casting for all objects with size less than this value.");
        
        static GUIContent gui_pl_frg = new GUIContent ("Fragments", "");
        static GUIContent gui_pl_prt = new GUIContent ("Particles", "");
        static GUIContent gui_pl_cap = new GUIContent ("    Capacity", "");
        
        /// /////////////////////////////////////////////////////////
        /// Inspector
        /// /////////////////////////////////////////////////////////
        
        public override void OnInspectorGUI()
        {
            man = target as RayfireMan;
            if (man == null)
                return;
            
            // Set new static instance
            if (RayfireMan.inst == null)
                RayfireMan.inst = man;

            GUILayout.Space (8);

            if (Application.isPlaying == true)
            {
                if (GUILayout.Button ("Destroy", GUILayout.Height (20)))
                    RayfireMan.inst.storage.DestroyAll();
                
                GUILayout.Space (space);
            }
            
            UI_Physics();
            
            GUILayout.Space (space);
            
            UI_Collision();
            
            GUILayout.Space (space);

            UI_Materials();

            GUILayout.Space (space);
            
            UI_Activation();

            GUILayout.Space (space);
            
            UI_Demolition();

            GUILayout.Space (space);

            UI_Pooling();
            
            GUILayout.Space (space);

            UI_Info();

            GUILayout.Space (space);
            
            UI_About();

            GUILayout.Space (8);
        }

        void UI_About()
        {
            GUILayout.Label ("  About", EditorStyles.boldLabel);
            
            GUILayout.Label ("Plugin build: " + RayfireMan.buildMajor + '.' + RayfireMan.buildMinor.ToString ("D2"));

            // Logo TODO remove if component removed
            if (logo == null)
                logo = (Texture2D)AssetDatabase.LoadAssetAtPath ("Assets/RayFire/Info/Logo/logo_small.png", typeof(Texture2D));
            if (logo != null)
                GUILayout.Box (logo, GUILayout.Width ((int)EditorGUIUtility.currentViewWidth - 19f), GUILayout.Height (64));
            
            if (GUILayout.Button ("     Changelog     ", GUILayout.Height (20)))
                Application.OpenURL ("https://assetstore.unity.com/packages/tools/game-toolkits/rayfire-for-unity-148690#releases");
        }

        /// /////////////////////////////////////////////////////////
        /// Physics
        /// /////////////////////////////////////////////////////////

        void UI_Physics()
        {
            GUILayout.Label ("  Physics", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            man.setGravity = EditorGUILayout.Toggle (gui_ph_set, man.setGravity);
            if (EditorGUI.EndChangeCheck() == true)
                SetDirty (man);

            GUILayout.Space (space);
            
            if (man.setGravity == true)
            {
                EditorGUI.BeginChangeCheck();
                man.multiplier = EditorGUILayout.Slider (gui_ph_mul, man.multiplier, 0f, 1f);
                if (EditorGUI.EndChangeCheck() == true)
                    SetDirty (man);
                
                GUILayout.Space (space);
            }
            
            EditorGUI.BeginChangeCheck();
            man.colliderSize = EditorGUILayout.Slider (gui_ph_col, man.colliderSize, 0f, 1f);
            if (EditorGUI.EndChangeCheck() == true)
                SetDirty (man);
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            man.interpolation = (RigidbodyInterpolation)EditorGUILayout.EnumPopup (gui_ph_int, man.interpolation);
            if (EditorGUI.EndChangeCheck() == true)
                SetDirty (man);
        }

        /// /////////////////////////////////////////////////////////
        /// Collision
        /// /////////////////////////////////////////////////////////
        
        void UI_Collision()
        {
            GUILayout.Label ("  Collision Detection", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            man.meshCollision = (CollisionDetectionMode)EditorGUILayout.EnumPopup (gui_col_mesh, man.meshCollision);
            if (EditorGUI.EndChangeCheck() == true)
                SetDirty (man);
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            man.clusterCollision = (CollisionDetectionMode)EditorGUILayout.EnumPopup (gui_col_cls, man.clusterCollision);
            if (EditorGUI.EndChangeCheck() == true)
                SetDirty (man);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Materials
        /// /////////////////////////////////////////////////////////

        void UI_Materials()
        {
            GUILayout.Label ("  Materials", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            man.minimumMass = EditorGUILayout.Slider (gui_mat_min, man.minimumMass, 0f, 1f);
            if (EditorGUI.EndChangeCheck() == true)
                SetDirty (man);
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            man.maximumMass = EditorGUILayout.Slider (gui_mat_max, man.maximumMass, 0f, 4000f);
            if (EditorGUI.EndChangeCheck() == true)
                SetDirty (man);

            GUILayout.Space (space);
            
            UI_Materials_Presets();
        }

        void UI_Materials_Presets()
        {
            expandMatPresets = EditorGUILayout.Foldout (expandMatPresets, gui_mat_pres, true);
            if (expandMatPresets == true)
            {
                EditorGUI.indentLevel++;
                
                GUILayout.Space (space);
                
                UI_Material (man.materialPresets.heavyMetal, ref expandMatHeavyMetal, "Heavy Metal");
                
                GUILayout.Space (space);
                
                UI_Material (man.materialPresets.lightMetal, ref expandMatLightMetal, "Light Metal");
                
                GUILayout.Space (space);
                
                UI_Material (man.materialPresets.denseRock, ref expandMatDenseRock, "Dense Rock");
                
                GUILayout.Space (space);
                
                UI_Material (man.materialPresets.porousRock, ref expandMatPorousRock, "Porous Rock");
                
                GUILayout.Space (space);
                
                UI_Material (man.materialPresets.concrete, ref expandMatConcrete, "Concrete");
                
                GUILayout.Space (space);
                
                UI_Material (man.materialPresets.brick, ref expandMatBrick, "Brick");
                
                GUILayout.Space (space);
                
                UI_Material (man.materialPresets.glass, ref expandMatGlass, "Glass");
                
                GUILayout.Space (space);
                
                UI_Material (man.materialPresets.rubber, ref expandMatRubber, "Rubber");
                
                GUILayout.Space (space);
                
                UI_Material (man.materialPresets.ice, ref expandMatIce, "Ice");
                
                GUILayout.Space (space);
                
                UI_Material (man.materialPresets.wood, ref expandMatWood, "Wood");
                
                EditorGUI.indentLevel--;
            }
        }

        void UI_Material(RFMaterial mat, ref bool state, string cap)
        {
            state = EditorGUILayout.Foldout (state, cap, true);
            if (state == true)
            {
                GUILayout.Space (space);
                
                EditorGUI.indentLevel++;
                
                GUILayout.Label ("          Demolition", EditorStyles.boldLabel);
                
                EditorGUI.BeginChangeCheck();
                mat.destructible = EditorGUILayout.Toggle (gui_mat_dest, mat.destructible);
                if (EditorGUI.EndChangeCheck() == true)
                    SetDirty (man);
                
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                mat.solidity = EditorGUILayout.IntSlider (gui_mat_sol, mat.solidity, 0, 100);
                if (EditorGUI.EndChangeCheck() == true)
                    SetDirty (man);
                
                GUILayout.Label ("          Rigid Body", EditorStyles.boldLabel);
                
                EditorGUI.BeginChangeCheck();
                mat.density = EditorGUILayout.Slider (gui_mat_dens, mat.density, 0.01f, 100f);
                if (EditorGUI.EndChangeCheck() == true)
                    SetDirty (man);
                
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                mat.drag = EditorGUILayout.Slider (gui_mat_drag, mat.drag, 0f, 1f);
                if (EditorGUI.EndChangeCheck() == true)
                    SetDirty (man);
                
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                mat.angularDrag = EditorGUILayout.Slider (gui_mat_ang, mat.angularDrag, 0f, 1f);
                if (EditorGUI.EndChangeCheck() == true)
                    SetDirty (man);
                
                GUILayout.Label ("          Physic Material", EditorStyles.boldLabel);
                
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                mat.material = (PhysicMaterial)EditorGUILayout.ObjectField (gui_mat_mat, mat.material, typeof(PhysicMaterial), true);
                if (EditorGUI.EndChangeCheck() == true)
                    SetDirty (man);
                
                EditorGUI.BeginChangeCheck();
                mat.dynamicFriction = EditorGUILayout.Slider (gui_mat_dyn, mat.dynamicFriction, 0.01f, 1f);
                if (EditorGUI.EndChangeCheck() == true)
                    SetDirty (man);
                
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                mat.staticFriction = EditorGUILayout.Slider (gui_mat_stat, mat.staticFriction, 0.01f, 1f);
                if (EditorGUI.EndChangeCheck() == true)
                    SetDirty (man);
                
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                mat.bounciness = EditorGUILayout.Slider (gui_mat_bnc, mat.bounciness, 0.01f, 1f);
                if (EditorGUI.EndChangeCheck() == true)
                    SetDirty (man);
                
                EditorGUI.indentLevel--;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Activation
        /// /////////////////////////////////////////////////////////

        void UI_Activation()
        {
            GUILayout.Label ("  Activation", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            man.parent = (GameObject)EditorGUILayout.ObjectField (gui_act_par, man.parent, typeof(GameObject), true);
            if (EditorGUI.EndChangeCheck() == true)
                SetDirty (man);
        }

        /// /////////////////////////////////////////////////////////
        /// Demolition
        /// /////////////////////////////////////////////////////////

        void UI_Demolition()
        {
            GUILayout.Label ("  Demolition", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            man.globalSolidity = EditorGUILayout.Slider (gui_dml_sol, man.globalSolidity, 0f, 5f);
            if (EditorGUI.EndChangeCheck() == true)
                SetDirty (man);
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            man.timeQuota = EditorGUILayout.Slider (gui_dml_time, man.timeQuota, 0f, 0.1f);
            if (EditorGUI.EndChangeCheck() == true)
                SetDirty (man);
            
            GUILayout.Space (space);

            UI_Demolition_Adv();
        }

        void UI_Demolition_Adv()
        {
            expandDemolition = EditorGUILayout.Foldout (expandDemolition, gui_adv_expand, true);
            if (expandDemolition == true)
            {
                EditorGUI.indentLevel++;

                GUILayout.Label ("  Fragments", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                man.advancedDemolitionProperties.parent = (RFManDemolition.FragmentParentType)EditorGUILayout.EnumPopup
                    (gui_adv_parent, man.advancedDemolitionProperties.parent);
                if (EditorGUI.EndChangeCheck() == true)
                    SetDirty (man);

                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                man.advancedDemolitionProperties.currentAmount = EditorGUILayout.IntField
                    (gui_adv_current, man.advancedDemolitionProperties.currentAmount);
                if (EditorGUI.EndChangeCheck() == true)
                    SetDirty (man);
                
                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                man.advancedDemolitionProperties.maximumAmount = EditorGUILayout.IntField
                    (gui_adv_amount, man.advancedDemolitionProperties.maximumAmount);
                if (EditorGUI.EndChangeCheck() == true)
                    SetDirty (man);

                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                man.advancedDemolitionProperties.badMeshTry = EditorGUILayout.IntSlider
                    (gui_adv_bad, man.advancedDemolitionProperties.badMeshTry, 1, 10);
                if (EditorGUI.EndChangeCheck() == true)
                    SetDirty (man);

                GUILayout.Label ("  Shadows", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                man.advancedDemolitionProperties.sizeThreshold = EditorGUILayout.Slider
                    (gui_adv_size, man.advancedDemolitionProperties.sizeThreshold, 0, 1f);
                if (EditorGUI.EndChangeCheck() == true)
                    SetDirty (man);
                
                EditorGUI.indentLevel--;
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Pooling
        /// /////////////////////////////////////////////////////////

        void UI_Pooling()
        {
            GUILayout.Label ("  Pooling", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            man.fragments.enable = EditorGUILayout.Toggle (gui_pl_frg, man.fragments.enable);
            if (EditorGUI.EndChangeCheck() == true)
                SetDirty (man);
            
            GUILayout.Space (space);

            if (man.fragments.enable == true)
            {
                EditorGUI.BeginChangeCheck();
                man.fragments.capacity = EditorGUILayout.IntSlider (gui_pl_cap, man.fragments.capacity, 0, 500);
                if (EditorGUI.EndChangeCheck() == true)
                    SetDirty (man);

                GUILayout.Space (space);
            }
            
            EditorGUI.BeginChangeCheck();
            man.particles.enable = EditorGUILayout.Toggle (gui_pl_prt, man.particles.enable);
            if (EditorGUI.EndChangeCheck() == true)
                SetDirty (man);
            
            GUILayout.Space (space);
            
            if (man.particles.enable == true)
            {
                EditorGUI.BeginChangeCheck();
                man.particles.capacity = EditorGUILayout.IntSlider (gui_pl_cap, man.particles.capacity, 0, 500);
                if (EditorGUI.EndChangeCheck() == true)
                    SetDirty (man);
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Info
        /// /////////////////////////////////////////////////////////

        void UI_Info()
        {
            if (Application.isPlaying == true)
            {
                GUILayout.Label ("  Info:", EditorStyles.boldLabel);

                if (man.fragments.poolList.Count > 0)
                    GUILayout.Label ("Pool amount: " + man.fragments.poolList.Count);

                if (man.advancedDemolitionProperties.currentAmount > 0)
                    GUILayout.Label ("Fragments: " + man.advancedDemolitionProperties.currentAmount + "/" + man.advancedDemolitionProperties.maximumAmount);
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////

        void SetDirty (RayfireMan scr)
        {
            if (Application.isPlaying == false)
            {
                EditorUtility.SetDirty (scr);
                EditorSceneManager.MarkSceneDirty (scr.gameObject.scene);
            }
        }
    }
}