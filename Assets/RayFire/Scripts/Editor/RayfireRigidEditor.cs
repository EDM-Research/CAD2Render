using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;

namespace RayFire
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireRigid), true)]
    public class RayfireRigidEditor : Editor
    {
        RayfireRigid rigid;
        
        SerializedProperty refsProp;
        ReorderableList    refsList;  
        
        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////
        
        // static int space = 3;
        static string rfRig     = "RayFire Rigid: ";
        static string misShards = " has missing shards. Reset or Setup cluster.";
        static int    space     = 3;
        
        static bool exp_phy;
        static bool exp_act;
        static bool exp_lim;
        static bool exp_msh;
        static bool exp_prp;
        static bool exp_cls;
        static bool exp_clp;
        static bool exp_ref;
        static bool exp_mat;
        static bool exp_dmg;
        static bool exp_fade;
        static bool exp_res;
        
        
        static GUIContent gui_mn_ini      = new GUIContent ("Initialization",     "");
        static GUIContent gui_mn_obj      = new GUIContent ("Object Type",        "");
        static GUIContent gui_mn_sim      = new GUIContent ("Simulation Type",    "Defines behaviour of object during simulation.");
        static GUIContent gui_mn_dml      = new GUIContent ("Demolition Type",    "Defines when and how object will be demolished.");
        static GUIContent gui_phy         = new GUIContent ("Physics",            "Defines all physics properties for simulated object.");
        static GUIContent gui_phy_mtp     = new GUIContent ("Type",               "Material preset with predefined density, friction, elasticity and solidity. Can be edited in Rayfire Man component.");
        static GUIContent gui_phy_mat     = new GUIContent ("Material",           "Allows to define own Physic Material.");
        static GUIContent gui_phy_mby     = new GUIContent ("Mass By",            "");
        static GUIContent gui_phy_mss     = new GUIContent ("Mass",               "Mass which will be applied to object if Mass By set to By Mass Property.");
        static GUIContent gui_phy_ctp     = new GUIContent ("Type",               "");
        static GUIContent gui_phy_pln     = new GUIContent ("Planar Check",       "Do not add Mesh Collider to objects with planar low poly mesh.");
        static GUIContent gui_phy_ign     = new GUIContent ("Ignore Near",        "");
        static GUIContent gui_phy_grv     = new GUIContent ("Use Gravity",        "Enables gravity for simulated object.");
        static GUIContent gui_phy_slv     = new GUIContent ("Solver Iterations",  "");
        static GUIContent gui_phy_dmp     = new GUIContent ("Dampening",          "Multiplier for demolished fragments velocity.");
        static GUIContent gui_act         = new GUIContent ("Activation",         "Allows to activate ( make dynamic ) inactive and kinematic objects.");
        static GUIContent gui_act_loc     = new GUIContent ("Local",              "Activation By Local Offset relative to parent.");
        static GUIContent gui_act_ofs     = new GUIContent ("Offset",             "Inactive object will be activated if will be pushed from it's original position farther than By Offset value.");
        static GUIContent gui_act_vel     = new GUIContent ("Velocity",           "Inactive object will be activated when it's velocity will be higher than By Velocity value when pushed by other dynamic objects.");
        static GUIContent gui_act_dmg     = new GUIContent ("Damage",             "Inactive object will be activated if will get total damage higher than this value.");
        static GUIContent gui_act_act     = new GUIContent ("Activator",          "Inactive object will be activated by overlapping with object with RayFire Activator component.");
        static GUIContent gui_act_imp     = new GUIContent ("Impact",             "Inactive object will be activated when it will be shot by RayFireGun component.");
        static GUIContent gui_act_con     = new GUIContent ("Connectivity",       "Inactive object will be activated by Connectivity component if it will not be connected with Unyielding zone.");
        static GUIContent gui_act_uny     = new GUIContent ("Unyielding",         "Allows to define Inactive/Kinematic object as Unyielding to check for connection with other Inactive/Kinematic objects with enabled By Connectivity activation type.");
        static GUIContent gui_act_acd     = new GUIContent ("Activatable",        "Unyielding object can not be activate by default. When On allows to activate Unyielding objects as well.");
        static GUIContent gui_act_l       = new GUIContent ("Change Layer",       "Change layer for activated objects.");
        static GUIContent gui_act_lay     = new GUIContent ("Layer",              "Custom layer for activated objects.");
        static GUIContent gui_lim         = new GUIContent ("Limitations",        "");
        static GUIContent gui_lim_col     = new GUIContent ("By Collision",       "Enables demolition by collision.");
        static GUIContent gui_lim_sol     = new GUIContent ("Solidity",           "Local Object solidity multiplier for object. Low Solidity makes object more fragile at collision.");
        static GUIContent gui_lim_tag     = new GUIContent ("Tag",                "Object will be demolished only if it will collide with other objects with defined Tag.");
        static GUIContent gui_lim_dep     = new GUIContent ("Depth",              "Defines how deep object can be demolished. Depth is limitless if set to 0.");
        static GUIContent gui_lim_tim     = new GUIContent ("Time",               "Safe time. Measures in seconds and allows to prevent fragments from being demolished right after they were just created.");
        static GUIContent gui_lim_siz     = new GUIContent ("Size",               "Prevent objects with bounding box size less than defined value to be demolished.");
        static GUIContent gui_lim_vis     = new GUIContent ("Visible",            "Object will be demolished only if it is visible to any camera including scene camera.");
        static GUIContent gui_lim_slc     = new GUIContent ("Slice By Blade",     "Allows object to be sliced by object with RayFire Blade component.");
        static GUIContent gui_msh         = new GUIContent ("Mesh Demolition",    "");
        static GUIContent gui_msh_am      = new GUIContent ("Amount",             "Defines amount of points in point cloud for fragments after demolition.");
        static GUIContent gui_msh_vr      = new GUIContent ("Variation",          "Defines additional amount variation for object in percents.");
        static GUIContent gui_msh_dp      = new GUIContent ("Depth Fade",         "Amount multiplier for next Depth level. Allows to decrease fragments amount of every next demolition level.");
        static GUIContent gui_msh_cb      = new GUIContent ("Contact Bias",       "Higher value allows to create more tiny fragments closer to collision contact point and bigger fragments far from it.");
        static GUIContent gui_msh_sd      = new GUIContent ("Seed",               "Defines Seed for fragmentation algorithm. Same Seed will produce same fragments for same object every time.");
        static GUIContent gui_msh_sh      = new GUIContent ("Use Shatter",        "Allows to use RayFire Shatter properties for fragmentation. Works only if object has RayFire Shatter component.");
        static GUIContent gui_msh_ch      = new GUIContent ("Add Children",       "Add children mesh objects to fragments.");
        static GUIContent gui_msh_adv_cls = new GUIContent ("Clusterize",         "Convert demolished fragments into Connected Cluster and demolish it instantly relative to contact point."); 
        static GUIContent gui_msh_adv_sim = new GUIContent ("Sim Type",           "Simulation type for demolished fragments."); 
        static GUIContent gui_msh_adv_inp = new GUIContent ("Mesh Input",         "Defines time for Mesh Input to process it and prepare for demolition. Useful for mid and hi poly objects.");
        static GUIContent gui_msh_rnt     = new GUIContent ("Runtime Caching",    ""); 
        static GUIContent gui_msh_rnt_fr  = new GUIContent ("Frames",             "");
        static GUIContent gui_msh_rnt_fg  = new GUIContent ("Fragments",          "");
        static GUIContent gui_msh_rnt_sk  = new GUIContent ("Skip First",         "Only initiate Runtime Caching on first demolition and demolish at second.");
        static GUIContent gui_msh_adv     = new GUIContent ("Properties",         "");
        static GUIContent gui_msh_adv_col = new GUIContent ("Collider",           "");
        static GUIContent gui_msh_adv_szl = new GUIContent ("Size Filter",        "Fragments with size less than this value will not get collider.");
        static GUIContent gui_msh_adv_rem = new GUIContent ("Remove Collinear",   "Remove collier vertices to decrease amount of triangles.");
        static GUIContent gui_msh_adv_l   = new GUIContent ("Inherit Layer",      "Inherit Layer for fragments.");
        static GUIContent gui_msh_adv_lay = new GUIContent ("  Custom Layer",     "Custom layer for fragments.");
        static GUIContent gui_msh_adv_t   = new GUIContent ("Inherit Tag",        "Inherit Tag for fragments.");
        static GUIContent gui_msh_adv_tag = new GUIContent ("  Custom Tag",       "Custom Tag fr fragments.");
        static GUIContent gui_cls         = new GUIContent ("Cluster Demolition", "");
        static GUIContent gui_cls_conn    = new GUIContent ("Connectivity",       "Defines Connectivity algorithm for clusters.");
        static GUIContent gui_cls_fl_ar   = new GUIContent ("Minimum Area",       "Two shards will have connection if their shared area is bigger than this value.");
        static GUIContent gui_cls_fl_sz   = new GUIContent ("Minimum Size",       "Two shards will have connection if their size is bigger than this value.");
        static GUIContent gui_cls_fl_pr   = new GUIContent ("Percentage",         "Random percentage of connections will be discarded.");
        static GUIContent gui_cls_fl_sd   = new GUIContent ("Seed",               "Seed for random percentage filter and for Random Collapse.");
        static GUIContent gui_cls_ds_tp   = new GUIContent ("Type",               "");
        static GUIContent gui_cls_ds_rt   = new GUIContent ("Ratio",              "Defines demolition distance from contact point in percentage relative to object's size.");
        static GUIContent gui_cls_ds_un   = new GUIContent ("Units",              "Defines demolition distance from contact point in world units.");
        static GUIContent gui_cls_sh_ar   = new GUIContent ("Area",               "");
        static GUIContent gui_cls_sh_dm   = new GUIContent ("Demolition",         "");
        static GUIContent gui_cls_min     = new GUIContent ("Minimum",            "");
        static GUIContent gui_cls_max     = new GUIContent ("Maximum",            "");
        static GUIContent gui_cls_dml     = new GUIContent ("Demolishable",       "");
        static GUIContent gui_clp_type    = new GUIContent ("Type", " By Area: Shard will loose it's connections if it's shared area surface is less then defined value.\n" + 
                                                                    " By Size: Shard will loose it's connections if it's Size is less then defined value.\n" + 
                                                                    " Random: Shard will loose it's connections if it's random value in range from 0 to 100 is less then defined value.");
        static GUIContent gui_clp_str    = new GUIContent ("Start",                "Defines start value in percentage relative to whole range of picked type.");
        static GUIContent gui_clp_end    = new GUIContent ("End",                  "Defines end value in percentage relative to whole range of picked type.");
        static GUIContent gui_clp_step   = new GUIContent ("Steps",                "Amount of times when defined threshold value will be set during Duration period.");
        static GUIContent gui_clp_dur    = new GUIContent ("Duration",             "Time which it will take Start value to be increased to End value.");
        static GUIContent gui_ref        = new GUIContent ("Reference Demolition", "");
        static GUIContent gui_ref_ref    = new GUIContent ("Reference",            "");
        static GUIContent gui_ref_lst    = new GUIContent ("Random List",          "");
        static GUIContent gui_ref_act    = new GUIContent ("Action",               "");
        static GUIContent gui_ref_add    = new GUIContent ("Add Rigid",            "Add RayFire Rigid component to reference with mesh.");
        static GUIContent gui_ref_scl    = new GUIContent ("Inherit Scale",        "");
        static GUIContent gui_ref_mat    = new GUIContent ("Inherit Materials",    "");
        static GUIContent gui_mat        = new GUIContent ("Materials",            "");
        static GUIContent gui_mat_scl    = new GUIContent ("Mapping",              "Mapping scale for inner surface");
        static GUIContent gui_mat_inn    = new GUIContent ("Inner",                "Material for inner fragments surface");
        static GUIContent gui_mat_out    = new GUIContent ("Outer",                "Material for outer fragments surface");
        static GUIContent gui_dmg        = new GUIContent ("Damage",               "Allows to demolish object by it's own floating Damage value.");
        static GUIContent gui_dmg_en     = new GUIContent ("Enable",               "");
        static GUIContent gui_dmg_max    = new GUIContent ("Max Damage",           "Defines maximum allowed damage for object to be demolished.");
        static GUIContent gui_dmg_cur    = new GUIContent ("Current Damage",       "Shows current damage value. Can be increased by public method: \nApplyDamage(float damageValue, Vector3 damagePosition)");
        static GUIContent gui_dmg_col    = new GUIContent ("Collect",              "Allows to accumulate damage value by collisions during dynamic simulation.");
        static GUIContent gui_dmg_mul    = new GUIContent ("Multiplier",           "Multiplier for every collision damage.");
        static GUIContent gui_dmg_sh     = new GUIContent ("To Shards",            "Apply damage to Connected Cluster shards.");
        static GUIContent gui_fade       = new GUIContent ("Fading",               "");
        static GUIContent gui_fade_dml   = new GUIContent ("On Demolition",        "");
        static GUIContent gui_fade_act   = new GUIContent ("On Activation",        "");
        static GUIContent gui_fade_ofs   = new GUIContent ("By Offset",            "");
        static GUIContent gui_fade_tp    = new GUIContent ("Type",                 "");
        static GUIContent gui_fade_tm    = new GUIContent ("Time",                 "Fade duration time.");
        static GUIContent gui_fade_lf_tp = new GUIContent ("Type",                 "");
        static GUIContent gui_fade_lf_tm = new GUIContent ("Time",                 "Time which object will be simulated before start to fade.");
        static GUIContent gui_fade_lf_vr = new GUIContent ("Variation",            "");
        static GUIContent gui_fade_sz    = new GUIContent ("Size",                 "Fade won't affect objects with size bigger than this value. Disabled if set to 0.");
        static GUIContent gui_fade_sh    = new GUIContent ("Shards",               "Fade won't affect Connected clusters with shard amount bigger than this value. Disabled if set to 0.");
        static GUIContent gui_res        = new GUIContent ("Reset",                "");
        static GUIContent gui_res_tm     = new GUIContent ("Transform",            "Reset transform to position and rotation when object was initialized.");
        static GUIContent gui_res_dm     = new GUIContent ("Damage",               "Reset damage value.");
        static GUIContent gui_res_cn     = new GUIContent ("Connectivity",         "Reset Connectivity.");
        static GUIContent gui_res_ac     = new GUIContent ("Action",               "");
        static GUIContent gui_res_dl     = new GUIContent ("Destroy Delay",        "Object will be destroyed after defined delay.");
        static GUIContent gui_res_ms     = new GUIContent ("Mesh",                 "");
        static GUIContent gui_res_fr     = new GUIContent ("Fragments",            "");

        static GUIStyle damageStyle = new GUIStyle();
        
        /// /////////////////////////////////////////////////////////
        /// Enable
        /// /////////////////////////////////////////////////////////

        private void OnEnable()
        {
            refsProp                     = serializedObject.FindProperty("referenceDemolition.randomList");
            refsList                     = new ReorderableList(serializedObject, refsProp, true, true, true, true);
            refsList.drawElementCallback = DrawRefListItems;
            refsList.drawHeaderCallback  = DrawRefHeader;
            refsList.onAddCallback       = AddRed;
            refsList.onRemoveCallback    = RemoveRef;
            
            if (EditorPrefs.HasKey ("rf_rp") == true) exp_phy  = EditorPrefs.GetBool ("rf_rp");
            if (EditorPrefs.HasKey ("rf_ra") == true) exp_act  = EditorPrefs.GetBool ("rf_ra");
            if (EditorPrefs.HasKey ("rf_rl") == true) exp_lim  = EditorPrefs.GetBool ("rf_rl");
            if (EditorPrefs.HasKey ("rf_rm") == true) exp_msh  = EditorPrefs.GetBool ("rf_rm");
            if (EditorPrefs.HasKey ("rf_rc") == true) exp_cls  = EditorPrefs.GetBool ("rf_rc");
            if (EditorPrefs.HasKey ("rf_rp") == true) exp_clp  = EditorPrefs.GetBool ("rf_rp");
            if (EditorPrefs.HasKey ("rf_rr") == true) exp_ref  = EditorPrefs.GetBool ("rf_rr");
            if (EditorPrefs.HasKey ("rf_rm") == true) exp_mat  = EditorPrefs.GetBool ("rf_rm");
            if (EditorPrefs.HasKey ("rf_rd") == true) exp_dmg  = EditorPrefs.GetBool ("rf_rd");
            if (EditorPrefs.HasKey ("rf_rf") == true) exp_fade = EditorPrefs.GetBool ("rf_rf");
            if (EditorPrefs.HasKey ("rf_re") == true) exp_res  = EditorPrefs.GetBool ("rf_re");
        }

        /// /////////////////////////////////////////////////////////
        /// Main
        /// /////////////////////////////////////////////////////////

        void UI_Main()
        {
            GUILayout.Label ("  Main", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            rigid.initialization = (RayfireRigid.InitType)EditorGUILayout.EnumPopup (gui_mn_ini, rigid.initialization);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireRigid scr in targets)
                {
                    scr.initialization = rigid.initialization;
                    SetDirty (scr);
                }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            rigid.objectType = (ObjectType)EditorGUILayout.EnumPopup (gui_mn_obj, rigid.objectType);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireRigid scr in targets)
                {
                    scr.objectType = rigid.objectType;
                    SetDirty (scr);
                }
        }

        /// /////////////////////////////////////////////////////////
        /// Simulation
        /// /////////////////////////////////////////////////////////

        void UI_Simulation()
        {
            GUILayout.Label ("  Simulation", EditorStyles.boldLabel);

            GUILayout.Space (space);

            EditorGUI.BeginChangeCheck();
            rigid.simulationType = (SimType)EditorGUILayout.EnumPopup (gui_mn_sim, rigid.simulationType);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireRigid scr in targets)
                {
                    scr.simulationType = rigid.simulationType;
                    SetDirty (scr);
                }

            GUILayout.Space (space);
            
            UI_Physic();

            if (ActivatableState() == false)
                return;
            
            GUILayout.Space (space);

            UI_Activation();
        }
        
        void UI_Physic()
        {
            SetFoldoutPref (ref exp_phy, "rf_rp", gui_phy, true);
            if (exp_phy == true)
            {
                EditorGUI.indentLevel++;
                
                GUILayout.Space (space);
                
                GUILayout.Label ("  Material", EditorStyles.boldLabel);
                
                EditorGUI.BeginChangeCheck();
                rigid.physics.materialType = (MaterialType)EditorGUILayout.EnumPopup (gui_phy_mtp, rigid.physics.materialType);
                if (EditorGUI.EndChangeCheck())
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.physics.materialType = rigid.physics.materialType;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                rigid.physics.material = (PhysicMaterial)EditorGUILayout.ObjectField (gui_phy_mat, rigid.physics.material, typeof(PhysicMaterial), true);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.physics.material = rigid.physics.material;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);
                
                GUILayout.Label ("  Mass", EditorStyles.boldLabel);
                
                EditorGUI.BeginChangeCheck();
                rigid.physics.massBy = (MassType)EditorGUILayout.EnumPopup (gui_phy_mby, rigid.physics.massBy);
                if (EditorGUI.EndChangeCheck())
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.physics.massBy = rigid.physics.massBy;
                        SetDirty (scr);
                    }

                if (rigid.physics.massBy == MassType.MassProperty)
                {
                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    rigid.physics.mass = EditorGUILayout.Slider (gui_phy_mss, rigid.physics.mass, 0.1f, 500f);
                    if (EditorGUI.EndChangeCheck() == true)
                        foreach (RayfireRigid scr in targets)
                        {
                            scr.physics.mass = rigid.physics.mass;
                            SetDirty (scr);
                        }
                }

                GUILayout.Space (space);
                
                GUILayout.Label ("  Collider", EditorStyles.boldLabel);
                
                EditorGUI.BeginChangeCheck();
                rigid.physics.colliderType = (RFColliderType)EditorGUILayout.EnumPopup (gui_phy_ctp, rigid.physics.colliderType);
                if (EditorGUI.EndChangeCheck())
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.physics.colliderType = rigid.physics.colliderType;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                rigid.physics.planarCheck = EditorGUILayout.Toggle (gui_phy_pln, rigid.physics.planarCheck);
                if (EditorGUI.EndChangeCheck())
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.physics.planarCheck = rigid.physics.planarCheck;
                        SetDirty (scr);
                    }
                    
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                rigid.physics.ignoreNear = EditorGUILayout.Toggle (gui_phy_ign, rigid.physics.ignoreNear);
                if (EditorGUI.EndChangeCheck())
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.physics.ignoreNear = rigid.physics.ignoreNear;
                        SetDirty (scr);
                    }
                    
                GUILayout.Space (space);
                
                GUILayout.Label ("  Other", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                rigid.physics.useGravity = EditorGUILayout.Toggle (gui_phy_grv, rigid.physics.useGravity);
                if (EditorGUI.EndChangeCheck())
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.physics.useGravity = rigid.physics.useGravity;
                        SetDirty (scr);
                    }
                    
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                rigid.physics.solverIterations = EditorGUILayout.IntSlider (gui_phy_slv, rigid.physics.solverIterations, 1, 20);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.physics.solverIterations = rigid.physics.solverIterations;
                        SetDirty (scr);
                    }
                
                GUILayout.Space (space);
                
                GUILayout.Label ("  Fragments", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                rigid.physics.dampening = EditorGUILayout.Slider (gui_phy_dmp, rigid.physics.dampening, 0f, 1f);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.physics.dampening = rigid.physics.dampening;
                        SetDirty (scr);
                    }
                
                EditorGUI.indentLevel--;
            }
        }
        
        void UI_Activation()
        {
            SetFoldoutPref (ref exp_act, "rf_ra", gui_act, true);
            if (exp_act == true)
            {
                EditorGUI.indentLevel++;
                
                GUILayout.Space (space);
                
                GUILayout.Label ("  Activation By", EditorStyles.boldLabel);
                
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                rigid.activation.byOffset = EditorGUILayout.Slider (gui_act_ofs, rigid.activation.byOffset, 0, 10f);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.activation.byOffset = rigid.activation.byOffset;
                        SetDirty (scr);
                    }
                
                GUILayout.Space (space);

                if (rigid.activation.byOffset > 0)
                {
                    EditorGUI.indentLevel++;
                    
                    EditorGUI.BeginChangeCheck();
                    rigid.activation.local = EditorGUILayout.Toggle (gui_act_loc, rigid.activation.local);
                    if (EditorGUI.EndChangeCheck())
                        foreach (RayfireRigid scr in targets)
                        {
                            scr.activation.local = rigid.activation.local;
                            SetDirty (scr);
                        }
                
                    GUILayout.Space (space);
                    
                    EditorGUI.indentLevel--;
                }

                EditorGUI.BeginChangeCheck();
                rigid.activation.byVelocity = EditorGUILayout.Slider (gui_act_vel, rigid.activation.byVelocity, 0, 5f);
                if (EditorGUI.EndChangeCheck())
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.activation.byVelocity = rigid.activation.byVelocity;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                rigid.activation.byDamage = EditorGUILayout.Slider (gui_act_dmg, rigid.activation.byDamage, 0, 100f);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.activation.byDamage = rigid.activation.byDamage;
                        SetDirty (scr);
                    }
                
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                rigid.activation.byActivator = EditorGUILayout.Toggle (gui_act_act, rigid.activation.byActivator);
                if (EditorGUI.EndChangeCheck())
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.activation.byActivator = rigid.activation.byActivator;
                        SetDirty (scr);
                    }
                
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                rigid.activation.byImpact = EditorGUILayout.Toggle (gui_act_imp, rigid.activation.byImpact);
                if (EditorGUI.EndChangeCheck())
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.activation.byImpact = rigid.activation.byImpact;
                        SetDirty (scr);
                    }
                
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                rigid.activation.byConnectivity = EditorGUILayout.Toggle (gui_act_con, rigid.activation.byConnectivity);
                if (EditorGUI.EndChangeCheck())
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.activation.byConnectivity = rigid.activation.byConnectivity;
                        SetDirty (scr);
                    }

                if (rigid.activation.byConnectivity == true)
                {
                    EditorGUI.indentLevel++;
                    
                    GUILayout.Space (space);
                    
                    EditorGUI.BeginChangeCheck();
                    rigid.activation.unyielding = EditorGUILayout.Toggle (gui_act_uny, rigid.activation.unyielding);
                    if (EditorGUI.EndChangeCheck())
                        foreach (RayfireRigid scr in targets)
                        {
                            scr.activation.unyielding = rigid.activation.unyielding;
                            SetDirty (scr);
                        }

                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    rigid.activation.activatable = EditorGUILayout.Toggle (gui_act_acd, rigid.activation.activatable);
                    if (EditorGUI.EndChangeCheck())
                        foreach (RayfireRigid scr in targets)
                        {
                            scr.activation.activatable = rigid.activation.activatable;
                            SetDirty (scr);
                        }

                    EditorGUI.indentLevel--;
                }
                
                GUILayout.Space (space);

                GUILayout.Label ("  Post Activation", EditorStyles.boldLabel);

                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                rigid.activation.l = EditorGUILayout.Toggle (gui_act_l, rigid.activation.l);
                if (EditorGUI.EndChangeCheck())
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.activation.l = rigid.activation.l;
                        SetDirty (scr);
                    }
                
                if (rigid.activation.l == true)
                {
                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    rigid.activation.layer = EditorGUILayout.LayerField (gui_act_lay, rigid.activation.layer);
                    if (EditorGUI.EndChangeCheck())
                        foreach (RayfireRigid scr in targets)
                        {
                            scr.activation.layer = rigid.activation.layer;
                            SetDirty (scr);
                        }
                }
                
                EditorGUI.indentLevel--;
            }
        }
        
        bool ActivatableState()
        {
            foreach (RayfireRigid scr in targets)
                if (ActivatableState(scr) == true)
                    return true;
            return false;
        }
        
        static bool ActivatableState(RayfireRigid scr)
        {
            if (scr.simulationType == SimType.Inactive || scr.simulationType == SimType.Kinematic)
                    return true;
            if (scr.meshDemolition.simType == FragSimType.Inactive || scr.meshDemolition.simType == FragSimType.Kinematic)
                    return true;
            return false;
        }

        /// /////////////////////////////////////////////////////////
        /// Demolition
        /// /////////////////////////////////////////////////////////

        void UI_Demolition()
        {
            GUILayout.Label ("  Demolition", EditorStyles.boldLabel);

            GUILayout.Space (space);

            EditorGUI.BeginChangeCheck();
            rigid.demolitionType = (DemolitionType)EditorGUILayout.EnumPopup (gui_mn_dml, rigid.demolitionType);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireRigid scr in targets)
                {
                    scr.demolitionType = rigid.demolitionType;
                    SetDirty (scr);
                }
            
            GUILayout.Space (space);

            if (rigid.objectType == ObjectType.MeshRoot || rigid.demolitionType != DemolitionType.None)
                UI_Limitations();
            
            if (MeshState() == true && rigid.demolitionType != DemolitionType.None)
            {
                GUILayout.Space (space);
                UI_Mesh();
            }

            if (rigid.IsCluster == true || rigid.meshDemolition.clusterize == true || rigid.objectType == ObjectType.MeshRoot)
            {
                GUILayout.Space (space);
                UI_Cluster();
            }
            
            if (DemolishableState() == false)
                return;

            if (rigid.demolitionType == DemolitionType.ReferenceDemolition)
            {
                GUILayout.Space (space);
                UI_Reference();
            }
            
            if (MeshState() == true)
            {
                GUILayout.Space (space);
                UI_Materials();
            }
            
            GUILayout.Space (space);
            
            UI_Damage();
        }
        
        bool DemolishableState()
        {
            foreach (RayfireRigid scr in targets)
                if (DemolishableState(scr) == true)
                    return true;
            return false;
        }
        
        static bool DemolishableState(RayfireRigid scr)
        {
            if (scr.demolitionType != DemolitionType.None)
                return true;
            return false;
        }
        
        bool MeshState()
        {
            foreach (RayfireRigid scr in targets)
                if (MeshState(scr) == true)
                    return true;
            return false;
        }
        
        static bool MeshState(RayfireRigid scr)
        {
            if (scr.objectType == ObjectType.Mesh ||
                scr.objectType == ObjectType.MeshRoot ||
                scr.objectType == ObjectType.SkinnedMesh)
                return true;
            if (scr.clusterDemolition.shardDemolition == true)
                return true;
            return false;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Limitations
        /// /////////////////////////////////////////////////////////

        void UI_Limitations()
        {
            SetFoldoutPref (ref exp_lim, "rf_rl", gui_lim, true);
            if (exp_lim == true)
            {
                EditorGUI.indentLevel++;
                
                GUILayout.Space (space);
                
                GUILayout.Label ("  Collision", EditorStyles.boldLabel);
                
                EditorGUI.BeginChangeCheck();
                rigid.limitations.byCollision = EditorGUILayout.Toggle (gui_lim_col, rigid.limitations.byCollision);
                if (EditorGUI.EndChangeCheck())
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.limitations.byCollision = rigid.limitations.byCollision;
                        SetDirty (scr);
                    }
                
                if (rigid.limitations.byCollision == true)
                {
                    GUILayout.Space (space);
                    
                    EditorGUI.BeginChangeCheck();
                    rigid.limitations.solidity = EditorGUILayout.Slider (gui_lim_sol, rigid.limitations.solidity, 0, 10f);
                    if (EditorGUI.EndChangeCheck() == true)
                        foreach (RayfireRigid scr in targets)
                        {
                            scr.limitations.solidity = rigid.limitations.solidity;
                            SetDirty (scr);
                        }
                }
                
                GUILayout.Space (space);
                    
                EditorGUI.BeginChangeCheck();
                rigid.limitations.tag = EditorGUILayout.TagField (gui_lim_tag, rigid.limitations.tag);
                if (EditorGUI.EndChangeCheck())
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.limitations.tag = rigid.limitations.tag;
                        SetDirty (scr);
                    }

                GUILayout.Label ("  Other", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                rigid.limitations.depth = EditorGUILayout.IntSlider (gui_lim_dep, rigid.limitations.depth, 0, 7);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.limitations.depth = rigid.limitations.depth;
                        SetDirty (scr);
                    }
                
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                rigid.limitations.time = EditorGUILayout.Slider (gui_lim_tim, rigid.limitations.time, 0.05f, 10f);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.limitations.time = rigid.limitations.time;
                        SetDirty (scr);
                    }
                
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                rigid.limitations.size = EditorGUILayout.Slider (gui_lim_siz, rigid.limitations.size, 0.01f, 5f);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.limitations.size = rigid.limitations.size;
                        SetDirty (scr);
                    }
                
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                rigid.limitations.visible = EditorGUILayout.Toggle (gui_lim_vis, rigid.limitations.visible);
                if (EditorGUI.EndChangeCheck())
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.limitations.visible = rigid.limitations.visible;
                        SetDirty (scr);
                    }
                
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                rigid.limitations.sliceByBlade = EditorGUILayout.Toggle (gui_lim_slc, rigid.limitations.sliceByBlade);
                if (EditorGUI.EndChangeCheck())
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.limitations.sliceByBlade = rigid.limitations.sliceByBlade;
                        SetDirty (scr);
                    }
                
                EditorGUI.indentLevel--;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Mesh
        /// /////////////////////////////////////////////////////////

        void UI_Mesh()
        {
            SetFoldoutPref (ref exp_msh, "rf_rm", gui_msh, true);
            if (exp_msh == true)
            {
                EditorGUI.indentLevel++;
                
                UI_Mesh_Frags();
                
                GUILayout.Space (space);

                GUILayout.Label ("  Advanced", EditorStyles.boldLabel);
                
                GUILayout.Space (space);

                UI_Mesh_Adv();
                
                GUILayout.Space (space);

                UI_Mesh_Runtime();
                
                GUILayout.Space (space);
                
                UI_Mesh_Props();
                
                EditorGUI.indentLevel--;
            }
        }

        void UI_Mesh_Frags ()
        {
            GUILayout.Label ("  Fragments", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            rigid.meshDemolition.amount = EditorGUILayout.IntSlider (gui_msh_am, rigid.meshDemolition.amount, 2, 300);
            if (EditorGUI.EndChangeCheck() == true)
                foreach (RayfireRigid scr in targets)
                {
                    scr.meshDemolition.amount = rigid.meshDemolition.amount;
                    SetDirty (scr);
                }

            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            rigid.meshDemolition.variation = EditorGUILayout.IntSlider (gui_msh_vr, rigid.meshDemolition.variation, 0, 100);
            if (EditorGUI.EndChangeCheck() == true)
                foreach (RayfireRigid scr in targets)
                {
                    scr.meshDemolition.variation = rigid.meshDemolition.variation;
                    SetDirty (scr);
                }

            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            rigid.meshDemolition.depthFade = EditorGUILayout.Slider (gui_msh_dp, rigid.meshDemolition.depthFade, 0.01f, 1f);
            if (EditorGUI.EndChangeCheck() == true)
                foreach (RayfireRigid scr in targets)
                {
                    scr.meshDemolition.depthFade = rigid.meshDemolition.depthFade;
                    SetDirty (scr);
                }

            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            rigid.meshDemolition.contactBias = EditorGUILayout.Slider (gui_msh_cb, rigid.meshDemolition.contactBias, 0, 1f);
            if (EditorGUI.EndChangeCheck() == true)
                foreach (RayfireRigid scr in targets)
                {
                    scr.meshDemolition.contactBias = rigid.meshDemolition.contactBias;
                    SetDirty (scr);
                }

            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            rigid.meshDemolition.seed = EditorGUILayout.IntSlider (gui_msh_sd, rigid.meshDemolition.seed, 0, 50);
            if (EditorGUI.EndChangeCheck() == true)
                foreach (RayfireRigid scr in targets)
                {
                    scr.meshDemolition.seed = rigid.meshDemolition.seed;
                    SetDirty (scr);
                }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            rigid.meshDemolition.useShatter = EditorGUILayout.Toggle (gui_msh_sh, rigid.meshDemolition.useShatter);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireRigid scr in targets)
                {
                    scr.meshDemolition.useShatter = rigid.meshDemolition.useShatter;
                    SetDirty (scr);
                }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            rigid.meshDemolition.addChildren = EditorGUILayout.Toggle (gui_msh_ch, rigid.meshDemolition.addChildren);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireRigid scr in targets)
                {
                    scr.meshDemolition.addChildren = rigid.meshDemolition.addChildren;
                    SetDirty (scr);
                }
        }

        void UI_Mesh_Adv()
        {
            EditorGUI.BeginChangeCheck();
            rigid.meshDemolition.clusterize = EditorGUILayout.Toggle (gui_msh_adv_cls, rigid.meshDemolition.clusterize);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireRigid scr in targets)
                {
                    scr.meshDemolition.clusterize = rigid.meshDemolition.clusterize;
                    SetDirty (scr);
                }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            rigid.meshDemolition.simType = (FragSimType)EditorGUILayout.EnumPopup (gui_msh_adv_sim, rigid.meshDemolition.simType);
            if (EditorGUI.EndChangeCheck() == true)
                foreach (RayfireRigid scr in targets)
                {
                    scr.meshDemolition.simType = rigid.meshDemolition.simType;
                    SetDirty (scr);
                }
        }

        void UI_Mesh_Runtime()
        {
            EditorGUI.BeginChangeCheck();
            rigid.meshDemolition.runtimeCaching.type = (CachingType)EditorGUILayout.EnumPopup (gui_msh_rnt, rigid.meshDemolition.runtimeCaching.type);
            if (EditorGUI.EndChangeCheck() == true)
                foreach (RayfireRigid scr in targets)
                {
                    scr.meshDemolition.runtimeCaching.type = rigid.meshDemolition.runtimeCaching.type;
                    SetDirty (scr);
                }
            
            if (rigid.meshDemolition.runtimeCaching.type == CachingType.Disable)
                return;
            
            EditorGUI.indentLevel++;
            
            if (rigid.meshDemolition.runtimeCaching.type == CachingType.ByFrames)
            {
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                rigid.meshDemolition.runtimeCaching.frames = EditorGUILayout.IntSlider (gui_msh_rnt_fr, rigid.meshDemolition.runtimeCaching.frames, 2, 300);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.meshDemolition.runtimeCaching.frames = rigid.meshDemolition.runtimeCaching.frames;
                        SetDirty (scr);
                    }
            }
            
            if (rigid.meshDemolition.runtimeCaching.type == CachingType.ByFragmentsPerFrame)
            {
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                rigid.meshDemolition.runtimeCaching.fragments = EditorGUILayout.IntSlider (gui_msh_rnt_fg, rigid.meshDemolition.runtimeCaching.fragments, 1, 20);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.meshDemolition.runtimeCaching.fragments = rigid.meshDemolition.runtimeCaching.fragments;
                        SetDirty (scr);
                    }
            }
            
            GUILayout.Space (space);

            EditorGUI.BeginChangeCheck();
            rigid.meshDemolition.runtimeCaching.skipFirstDemolition = EditorGUILayout.Toggle (gui_msh_rnt_sk, rigid.meshDemolition.runtimeCaching.skipFirstDemolition);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireRigid scr in targets)
                {
                    scr.meshDemolition.runtimeCaching.skipFirstDemolition = rigid.meshDemolition.runtimeCaching.skipFirstDemolition;
                    SetDirty (scr);
                }

            EditorGUI.indentLevel--;
        }

        void UI_Mesh_Props()
        {
            exp_prp = EditorGUILayout.Foldout (exp_prp, gui_msh_adv, true);
            if (exp_prp == true)
            {
                GUILayout.Space (space);
                
                EditorGUI.indentLevel++;

                EditorGUI.BeginChangeCheck();
                rigid.meshDemolition.properties.removeCollinear = EditorGUILayout.Toggle (gui_msh_adv_rem, rigid.meshDemolition.properties.removeCollinear);
                if (EditorGUI.EndChangeCheck())
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.meshDemolition.properties.removeCollinear = rigid.meshDemolition.properties.removeCollinear;
                        SetDirty (scr);
                    }
                
                GUILayout.Space (space);
            
                EditorGUI.BeginChangeCheck();
                rigid.meshDemolition.meshInput = (RFDemolitionMesh.MeshInputType)EditorGUILayout.EnumPopup (gui_msh_adv_inp, rigid.meshDemolition.meshInput);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.meshDemolition.meshInput = rigid.meshDemolition.meshInput;
                        SetDirty (scr);
                    }
                
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                rigid.meshDemolition.properties.colliderType = (RFColliderType)EditorGUILayout.EnumPopup (gui_msh_adv_col, rigid.meshDemolition.properties.colliderType);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.meshDemolition.properties.colliderType = rigid.meshDemolition.properties.colliderType;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                rigid.meshDemolition.properties.sizeFilter = EditorGUILayout.Slider (gui_msh_adv_szl, rigid.meshDemolition.properties.sizeFilter, 0, 10);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.meshDemolition.properties.sizeFilter = rigid.meshDemolition.properties.sizeFilter;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                rigid.meshDemolition.properties.l = EditorGUILayout.Toggle (gui_msh_adv_l, rigid.meshDemolition.properties.l);
                if (EditorGUI.EndChangeCheck())
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.meshDemolition.properties.l = rigid.meshDemolition.properties.l;
                        SetDirty (scr);
                    }
                
                if (rigid.meshDemolition.properties.l == false)
                {
                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    rigid.meshDemolition.properties.layer = EditorGUILayout.LayerField (gui_msh_adv_lay, rigid.meshDemolition.properties.layer);
                    if (EditorGUI.EndChangeCheck())
                        foreach (RayfireRigid scr in targets)
                        {
                            scr.meshDemolition.properties.layer = rigid.meshDemolition.properties.layer;
                            SetDirty (scr);
                        }
                }

                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                rigid.meshDemolition.properties.t = EditorGUILayout.Toggle (gui_msh_adv_t, rigid.meshDemolition.properties.t);
                if (EditorGUI.EndChangeCheck())
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.meshDemolition.properties.t = rigid.meshDemolition.properties.t;
                        SetDirty (scr);
                    }

                if (rigid.meshDemolition.properties.t == false)
                {
                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    rigid.meshDemolition.properties.tag = EditorGUILayout.TagField (gui_msh_adv_tag, rigid.meshDemolition.properties.tag);
                    if (EditorGUI.EndChangeCheck())
                        foreach (RayfireRigid scr in targets)
                        {
                            scr.meshDemolition.properties.tag = rigid.meshDemolition.properties.tag;
                            SetDirty (scr);
                        }
                }

                EditorGUI.indentLevel--;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Cluster
        /// /////////////////////////////////////////////////////////

        void UI_Cluster()
        {
            SetFoldoutPref (ref exp_cls, "rf_rc", gui_cls, true);
            if (exp_cls == true)
            {
                EditorGUI.indentLevel++;
                
                UI_Cluster_Props();
                
                GUILayout.Space (space);

                UI_Cluster_Filters();
                
                GUILayout.Space (space);

                UI_Cluster_Dist();
                
                GUILayout.Space (space);

                UI_Cluster_Shard();
                
                GUILayout.Space (space);

                UI_Cluster_Cls();
                
                GUILayout.Space (space);

                UI_Cluster_Collapse();
                
                EditorGUI.indentLevel--;
            }
        }

        void UI_Cluster_Props()
        {
            GUILayout.Label ("  Properties", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            rigid.clusterDemolition.connectivity = (ConnectivityType)EditorGUILayout.EnumPopup (gui_cls_conn, rigid.clusterDemolition.connectivity);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireRigid scr in targets)
                {
                    scr.clusterDemolition.connectivity = rigid.clusterDemolition.connectivity;
                    SetDirty (scr);
                }
        }

        void UI_Cluster_Filters () 
        {
            GUILayout.Label ("  Filters", EditorStyles.boldLabel);
            
            if (rigid.clusterDemolition.connectivity != ConnectivityType.ByBoundingBox)
            {
                EditorGUI.BeginChangeCheck();
                rigid.clusterDemolition.minimumArea = EditorGUILayout.Slider (gui_cls_fl_ar, rigid.clusterDemolition.minimumArea, 0, 1f);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.clusterDemolition.minimumArea = rigid.clusterDemolition.minimumArea;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);
            }

            EditorGUI.BeginChangeCheck();
            rigid.clusterDemolition.minimumSize = EditorGUILayout.Slider (gui_cls_fl_sz, rigid.clusterDemolition.minimumSize, 0, 10f);
            if (EditorGUI.EndChangeCheck() == true)
                foreach (RayfireRigid scr in targets)
                {
                    scr.clusterDemolition.minimumSize = rigid.clusterDemolition.minimumSize;
                    SetDirty (scr);
                }

            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            rigid.clusterDemolition.percentage = EditorGUILayout.IntSlider (gui_cls_fl_pr, rigid.clusterDemolition.percentage, 0, 100);
            if (EditorGUI.EndChangeCheck() == true)
                foreach (RayfireRigid scr in targets)
                {
                    scr.clusterDemolition.percentage = rigid.clusterDemolition.percentage;
                    SetDirty (scr);
                }

            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            rigid.clusterDemolition.seed = EditorGUILayout.IntSlider (gui_cls_fl_sd, rigid.clusterDemolition.seed, 0, 100);
            if (EditorGUI.EndChangeCheck() == true)
                foreach (RayfireRigid scr in targets)
                {
                    scr.clusterDemolition.seed = rigid.clusterDemolition.seed;
                    SetDirty (scr);
                }
        }

        void UI_Cluster_Dist()
        {
            GUILayout.Label ("  Demolition Distance", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            rigid.clusterDemolition.type = (RFDemolitionCluster.RFDetachType)EditorGUILayout.EnumPopup (gui_cls_ds_tp, rigid.clusterDemolition.type);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireRigid scr in targets)
                {
                    scr.clusterDemolition.type = rigid.clusterDemolition.type;
                    SetDirty (scr);
                }
            
            GUILayout.Space (space);

            if (rigid.clusterDemolition.type == RFDemolitionCluster.RFDetachType.RatioToSize)
            {
                EditorGUI.BeginChangeCheck();
                rigid.clusterDemolition.ratio = EditorGUILayout.IntSlider (gui_cls_ds_rt, rigid.clusterDemolition.ratio, 1, 100);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.clusterDemolition.ratio = rigid.clusterDemolition.ratio;
                        SetDirty (scr);
                    }
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                rigid.clusterDemolition.units = EditorGUILayout.Slider (gui_cls_ds_un, rigid.clusterDemolition.units, 0, 10f);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.clusterDemolition.units = rigid.clusterDemolition.units;
                        SetDirty (scr);
                    }
            }
        }

        void UI_Cluster_Shard()
        {
            GUILayout.Label ("  Shards", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            rigid.clusterDemolition.shardArea = EditorGUILayout.IntSlider (gui_cls_sh_ar, rigid.clusterDemolition.shardArea, 0, 100);
            if (EditorGUI.EndChangeCheck() == true)
                foreach (RayfireRigid scr in targets)
                {
                    scr.clusterDemolition.shardArea = rigid.clusterDemolition.shardArea;
                    SetDirty (scr);
                }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            rigid.clusterDemolition.shardDemolition = EditorGUILayout.Toggle (gui_cls_sh_dm, rigid.clusterDemolition.shardDemolition);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireRigid scr in targets)
                {
                    scr.clusterDemolition.shardDemolition = rigid.clusterDemolition.shardDemolition;
                    SetDirty (scr);
                }
        }
        
        void UI_Cluster_Cls()
        {
            GUILayout.Label ("  Clusters", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            rigid.clusterDemolition.minAmount = EditorGUILayout.IntSlider (gui_cls_min, rigid.clusterDemolition.minAmount, 1, 20);
            if (EditorGUI.EndChangeCheck() == true)
                foreach (RayfireRigid scr in targets)
                {
                    scr.clusterDemolition.minAmount = rigid.clusterDemolition.minAmount;
                    SetDirty (scr);
                }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            rigid.clusterDemolition.maxAmount = EditorGUILayout.IntSlider (gui_cls_max, rigid.clusterDemolition.maxAmount, 1, 20);
            if (EditorGUI.EndChangeCheck() == true)
                foreach (RayfireRigid scr in targets)
                {
                    scr.clusterDemolition.maxAmount = rigid.clusterDemolition.maxAmount;
                    SetDirty (scr);
                }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            rigid.clusterDemolition.demolishable = EditorGUILayout.Toggle (gui_cls_dml, rigid.clusterDemolition.demolishable);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireRigid scr in targets)
                {
                    scr.clusterDemolition.demolishable = rigid.clusterDemolition.demolishable;
                    SetDirty (scr);
                }
        }

        void UI_Cluster_Collapse()
        {
            GUILayout.Label ("  Collapse", EditorStyles.boldLabel);

            SetFoldoutPref (ref exp_clp, "rf_rp", gui_msh_adv, true);
            if (exp_clp == true)
            {
                GUILayout.Space (space);

                EditorGUI.indentLevel++;

                EditorGUI.BeginChangeCheck();
                rigid.clusterDemolition.collapse.type = (RFCollapse.RFCollapseType)EditorGUILayout.EnumPopup (gui_clp_type, rigid.clusterDemolition.collapse.type);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.clusterDemolition.collapse.type = rigid.clusterDemolition.collapse.type;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                rigid.clusterDemolition.collapse.start = EditorGUILayout.IntSlider (gui_clp_str, rigid.clusterDemolition.collapse.start, 0, 99);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.clusterDemolition.collapse.start = rigid.clusterDemolition.collapse.start;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                rigid.clusterDemolition.collapse.end = EditorGUILayout.IntSlider (gui_clp_end, rigid.clusterDemolition.collapse.end, 1, 100);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.clusterDemolition.collapse.end = rigid.clusterDemolition.collapse.end;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                rigid.clusterDemolition.collapse.steps = EditorGUILayout.IntSlider (gui_clp_step, rigid.clusterDemolition.collapse.steps, 1, 100);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.clusterDemolition.collapse.steps = rigid.clusterDemolition.collapse.steps;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                rigid.clusterDemolition.collapse.duration = EditorGUILayout.Slider (gui_clp_dur, rigid.clusterDemolition.collapse.duration, 0, 60f);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.clusterDemolition.collapse.duration = rigid.clusterDemolition.collapse.duration;
                        SetDirty (scr);
                    }

                EditorGUI.indentLevel--;
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Reference
        /// /////////////////////////////////////////////////////////

        void UI_Reference()
        {
            SetFoldoutPref (ref exp_ref, "rf_rr", gui_ref, true);
            if (exp_ref == true)
            {
                EditorGUI.indentLevel++;
                
                UI_Reference_Props();
                
                GUILayout.Space (space);
                
                UI_Reference_Source();
                
                EditorGUI.indentLevel--;
            }
        }
        
        void UI_Reference_Props()
        {
            GUILayout.Label ("  Properties", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            rigid.referenceDemolition.action = (RFReferenceDemolition.ActionType)EditorGUILayout.EnumPopup (gui_ref_act, rigid.referenceDemolition.action);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireRigid scr in targets)
                {
                    scr.referenceDemolition.action = rigid.referenceDemolition.action;
                    SetDirty (scr);
                }

            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            rigid.referenceDemolition.addRigid = EditorGUILayout.Toggle (gui_ref_add, rigid.referenceDemolition.addRigid);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireRigid scr in targets)
                {
                    scr.referenceDemolition.addRigid = rigid.referenceDemolition.addRigid;
                    SetDirty (scr);
                }

            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            rigid.referenceDemolition.inheritScale = EditorGUILayout.Toggle (gui_ref_scl, rigid.referenceDemolition.inheritScale);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireRigid scr in targets)
                {
                    scr.referenceDemolition.inheritScale = rigid.referenceDemolition.inheritScale;
                    SetDirty (scr);
                }

            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            rigid.referenceDemolition.inheritMaterials = EditorGUILayout.Toggle (gui_ref_mat, rigid.referenceDemolition.inheritMaterials);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireRigid scr in targets)
                {
                    scr.referenceDemolition.inheritMaterials = rigid.referenceDemolition.inheritMaterials;
                    SetDirty (scr);
                }

            GUILayout.Space (space);
        }
        
        void UI_Reference_Source()
        {
            GUILayout.Label ("  Source", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            rigid.referenceDemolition.reference = (GameObject)EditorGUILayout.ObjectField (gui_ref_ref, rigid.referenceDemolition.reference, typeof(GameObject), true);
            if (EditorGUI.EndChangeCheck() == true)
                foreach (RayfireRigid scr in targets)
                {
                    scr.referenceDemolition.reference = rigid.referenceDemolition.reference;
                    SetDirty (scr);
                }

            GUILayout.Space (space);
            
            serializedObject.Update();
            refsList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }

        void DrawRefListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = refsList.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y+2, EditorGUIUtility.currentViewWidth - 80f, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        }
        
        void DrawRefHeader(Rect rect)
        {
            rect.x += 10;
            EditorGUI.LabelField(rect, gui_ref_lst);
        }

        void AddRed(ReorderableList list)
        {
            if (rigid.referenceDemolition.randomList == null)
                rigid.referenceDemolition.randomList = new List<GameObject>();
            rigid.referenceDemolition.randomList.Add (null);
            list.index = list.count;
        }
        
        void RemoveRef(ReorderableList list)
        {
            if (rigid.referenceDemolition.randomList != null)
            {
                rigid.referenceDemolition.randomList.RemoveAt (list.index);
                list.index = list.index - 1;
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Materials
        /// /////////////////////////////////////////////////////////

        void UI_Materials()
        {
            SetFoldoutPref (ref exp_mat, "rf_rm", gui_mat, true);
            if (exp_mat == true)
            {
                EditorGUI.indentLevel++;

                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                rigid.materials.mappingScale = EditorGUILayout.Slider (gui_mat_scl, rigid.materials.mappingScale, 0.01f, 2f);
                if (EditorGUI.EndChangeCheck())
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.materials.mappingScale = rigid.materials.mappingScale;
                        SetDirty (scr);
                    }
                
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                rigid.materials.innerMaterial = (Material)EditorGUILayout.ObjectField (gui_mat_inn, rigid.materials.innerMaterial, typeof(Material), true);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.materials.innerMaterial = rigid.materials.innerMaterial;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                rigid.materials.outerMaterial = (Material)EditorGUILayout.ObjectField (gui_mat_out, rigid.materials.outerMaterial, typeof(Material), true);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.materials.outerMaterial = rigid.materials.outerMaterial;
                        SetDirty (scr);
                    }
                
                EditorGUI.indentLevel--;
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Damage
        /// /////////////////////////////////////////////////////////

        void UI_Damage()
        {
            SetFoldoutPref (ref exp_dmg, "rf_rd", gui_dmg, true);
            if (exp_dmg == true)
            {
                EditorGUI.indentLevel++;

                UI_Damage_Props();
                
                GUILayout.Space (space);
                
                UI_Damage_Coll();
                
                EditorGUI.indentLevel--;
            }
        }
        
        void UI_Damage_Props()
        {
            GUILayout.Label ("  Properties", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            rigid.damage.enable = EditorGUILayout.Toggle (gui_dmg_en, rigid.damage.enable);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireRigid scr in targets)
                {
                    scr.damage.enable = rigid.damage.enable;
                    SetDirty (scr);
                }

            if (rigid.objectType == ObjectType.ConnectedCluster)
            {
                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                rigid.damage.toShards = EditorGUILayout.Toggle (gui_dmg_sh, rigid.damage.toShards);
                if (EditorGUI.EndChangeCheck())
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.damage.toShards = rigid.damage.toShards;
                        SetDirty (scr);
                    }
            }

            GUILayout.Space (space);

            EditorGUI.BeginChangeCheck();
            rigid.damage.maxDamage = EditorGUILayout.FloatField (gui_dmg_max, rigid.damage.maxDamage);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireRigid scr in targets)
                {
                    scr.damage.maxDamage = rigid.damage.maxDamage;
                    SetDirty (scr);
                }

            if (rigid.objectType == ObjectType.ConnectedCluster && rigid.damage.toShards == true)
            {
                // To Damage preview
            }
            else
            {
                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                rigid.damage.currentDamage = EditorGUILayout.FloatField (gui_dmg_cur, rigid.damage.currentDamage);
                if (EditorGUI.EndChangeCheck())
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.damage.currentDamage = rigid.damage.currentDamage;
                        SetDirty (scr);
                    }
            }
        }
        
        void UI_Damage_Coll()
        {
            GUILayout.Label ("  Collision", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            rigid.damage.collect = EditorGUILayout.Toggle (gui_dmg_col, rigid.damage.collect);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireRigid scr in targets)
                {
                    scr.damage.collect = rigid.damage.collect;
                    SetDirty (scr);
                }

            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            rigid.damage.multiplier = EditorGUILayout.Slider (gui_dmg_mul, rigid.damage.multiplier, 0.01f, 5f);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireRigid scr in targets)
                {
                    scr.damage.multiplier = rigid.damage.multiplier;
                    SetDirty (scr);
                }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Fade
        /// /////////////////////////////////////////////////////////

        void UI_Fade()
        {
            SetFoldoutPref (ref exp_fade, "rf_rf", gui_fade, true);
            if (exp_fade == true)
            {
                EditorGUI.indentLevel++;

                UI_Fade_Init();
                
                GUILayout.Space (space);

                UI_Fade_Type();
                
                GUILayout.Space (space);
                
                UI_Fade_Life();
                
                GUILayout.Space (space);

                UI_Fade_Filt();

                EditorGUI.indentLevel--;
            }
        }

        void UI_Fade_Init()
        {
            GUILayout.Label ("  Initiate", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            rigid.fading.onDemolition = EditorGUILayout.Toggle (gui_fade_dml, rigid.fading.onDemolition);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireRigid scr in targets)
                {
                    scr.fading.onDemolition = rigid.fading.onDemolition;
                    SetDirty (scr);
                }

            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            rigid.fading.onActivation = EditorGUILayout.Toggle (gui_fade_act, rigid.fading.onActivation);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireRigid scr in targets)
                {
                    scr.fading.onActivation = rigid.fading.onActivation;
                    SetDirty (scr);
                }

            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            rigid.fading.byOffset = EditorGUILayout.Slider (gui_fade_ofs, rigid.fading.byOffset, 0f, 20f);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireRigid scr in targets)
                {
                    scr.fading.byOffset = rigid.fading.byOffset;
                    SetDirty (scr);
                }
        }
        
        void UI_Fade_Type()
        {
            GUILayout.Label ("  Type", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            rigid.fading.fadeType = (FadeType)EditorGUILayout.EnumPopup (gui_fade_tp, rigid.fading.fadeType);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireRigid scr in targets)
                {
                    scr.fading.fadeType = rigid.fading.fadeType;
                    SetDirty (scr);
                }

            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            rigid.fading.fadeTime = EditorGUILayout.Slider (gui_fade_tm, rigid.fading.fadeTime, 1f, 20f);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireRigid scr in targets)
                {
                    scr.fading.fadeTime = rigid.fading.fadeTime;
                    SetDirty (scr);
                }
        }
        
        void UI_Fade_Life()
        {
            GUILayout.Label ("  Life", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            rigid.fading.lifeType = (RFFadeLifeType)EditorGUILayout.EnumPopup (gui_fade_lf_tp, rigid.fading.lifeType);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireRigid scr in targets)
                {
                    scr.fading.lifeType = rigid.fading.lifeType;
                    SetDirty (scr);
                }

            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            rigid.fading.lifeTime = EditorGUILayout.Slider (gui_fade_lf_tm, rigid.fading.lifeTime, 0f, 90f);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireRigid scr in targets)
                {
                    scr.fading.lifeTime = rigid.fading.lifeTime;
                    SetDirty (scr);
                }

            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            rigid.fading.lifeVariation = EditorGUILayout.Slider (gui_fade_lf_vr, rigid.fading.lifeVariation, 0f, 20f);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireRigid scr in targets)
                {
                    scr.fading.lifeVariation = rigid.fading.lifeVariation;
                    SetDirty (scr);
                }
        }

        void UI_Fade_Filt()
        {
            GUILayout.Label ("  Filters", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            rigid.fading.sizeFilter = EditorGUILayout.Slider (gui_fade_sz, rigid.fading.sizeFilter, 0f, 20f);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireRigid scr in targets)
                {
                    scr.fading.sizeFilter = rigid.fading.sizeFilter;
                    SetDirty (scr);
                }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            rigid.fading.shardAmount = EditorGUILayout.IntSlider (gui_fade_sh, rigid.fading.shardAmount, 0, 50);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireRigid scr in targets)
                {
                    scr.fading.shardAmount = rigid.fading.shardAmount;
                    SetDirty (scr);
                }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Reset
        /// /////////////////////////////////////////////////////////

        void UI_Reset()
        {
            SetFoldoutPref (ref exp_res, "rf_re", gui_res, true);
            if (exp_res == true )
            {
                EditorGUI.indentLevel++;
                
                UI_Reset_Types();
                
                GUILayout.Space (space);

                if (rigid.demolitionType != DemolitionType.None)
                {
                    UI_Reset_Dml();

                    GUILayout.Space (space);

                    if (ReuseState (rigid) == true)
                        UI_Reset_Reuse();
                }

                EditorGUI.indentLevel--;
            }
        }

        void UI_Reset_Types()
        {
            GUILayout.Label ("  Reset", EditorStyles.boldLabel);
                
            EditorGUI.BeginChangeCheck();
            rigid.reset.transform = EditorGUILayout.Toggle (gui_res_tm, rigid.reset.transform);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireRigid scr in targets)
                {
                    scr.reset.transform = rigid.reset.transform;
                    SetDirty (scr);
                }

            GUILayout.Space (space);
                
            EditorGUI.BeginChangeCheck();
            rigid.reset.damage = EditorGUILayout.Toggle (gui_res_dm, rigid.reset.damage);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireRigid scr in targets)
                {
                    scr.reset.damage = rigid.reset.damage;
                    SetDirty (scr);
                }

            GUILayout.Space (space);
                
            EditorGUI.BeginChangeCheck();
            rigid.reset.connectivity = EditorGUILayout.Toggle (gui_res_cn, rigid.reset.connectivity);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireRigid scr in targets)
                {
                    scr.reset.connectivity = rigid.reset.connectivity;
                    SetDirty (scr);
                }
        }

        void UI_Reset_Dml()
        {
            GUILayout.Label ("  Demolition", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            rigid.reset.action = (RFReset.PostDemolitionType)EditorGUILayout.EnumPopup (gui_res_ac, rigid.reset.action);
            if (EditorGUI.EndChangeCheck() == true)
                foreach (RayfireRigid scr in targets)
                {
                    scr.reset.action = rigid.reset.action;
                    SetDirty (scr);
                }

            if (rigid.reset.action == RFReset.PostDemolitionType.DestroyWithDelay)
            {
                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                rigid.reset.destroyDelay = EditorGUILayout.Slider (gui_res_dl, rigid.reset.destroyDelay, 0, 60);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireRigid scr in targets)
                    {
                        scr.reset.destroyDelay = rigid.reset.destroyDelay;
                        SetDirty (scr);
                    }
            }
        }

        void UI_Reset_Reuse()
        {
            GUILayout.Label ("  Reuse", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            rigid.reset.mesh = (RFReset.MeshResetType)EditorGUILayout.EnumPopup (gui_res_ms, rigid.reset.mesh);
            if (EditorGUI.EndChangeCheck() == true)
                foreach (RayfireRigid scr in targets)
                {
                    scr.reset.mesh = rigid.reset.mesh;
                    SetDirty (scr);
                }

            GUILayout.Space (space);

            EditorGUI.BeginChangeCheck();
            rigid.reset.fragments = (RFReset.FragmentsResetType)EditorGUILayout.EnumPopup (gui_res_fr, rigid.reset.fragments);
            if (EditorGUI.EndChangeCheck() == true)
                foreach (RayfireRigid scr in targets)
                {
                    scr.reset.fragments = rigid.reset.fragments;
                    SetDirty (scr);
                }
        }

        bool ReuseState(RayfireRigid scr)
        {
            if (scr.objectType == ObjectType.Mesh || scr.objectType == ObjectType.MeshRoot)
                return true;

            if (scr.clusterDemolition.shardDemolition == true)
                return true;

            return false;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Inspector
        /// /////////////////////////////////////////////////////////

        public override void OnInspectorGUI()
        {
            rigid = target as RayfireRigid;
            if (rigid == null)
                return;
            
            // Space
            GUILayout.Space (8);
            
            // Initialize
            if (Application.isPlaying == true)
            {
                if (rigid.initialized == false)
                {
                    if (GUILayout.Button ("Initialize", GUILayout.Height (25)))
                        foreach (var targ in targets)
                            if (targ as RayfireRigid != null)
                                if ((targ as RayfireRigid).initialized == false)
                                    (targ as RayfireRigid).Initialize();
                }
                
                // Reuse
                else
                {
                    if (GUILayout.Button ("Reset Rigid", GUILayout.Height (25)))
                            foreach (var targ in targets)
                                if (targ as RayfireRigid != null)
                                    if ((targ as RayfireRigid).initialized == true)
                                        (targ as RayfireRigid).ResetRigid();
                }
            }
            
            RigidManUI();

            // Setup
            if (Application.isPlaying == false)
            {
                // Clusters
                if (rigid.objectType == ObjectType.MeshRoot)
                {
                    GUILayout.Label ("  MeshRoot", EditorStyles.boldLabel);
                    SetupUI();
                }
            }
            
            // Clusters
            if (rigid.IsCluster == true)
            {
                GUILayout.Label ("  Cluster", EditorStyles.boldLabel);

                if (Application.isPlaying == false)
                    SetupUI();

                GUILayout.Space (1);

                ClusterPreviewUI (rigid);

                if (Application.isPlaying == true)
                    ClusterCollapseUI();
            }
            
            InfoUI();
            
            GUILayout.Space (space);

            UI_Main();
            
            GUILayout.Space (space);

            UI_Simulation();
            
            GUILayout.Space (space);
            
            UI_Demolition();     
            
            GUILayout.Space (space);
            
            GUILayout.Label ("  Common", EditorStyles.boldLabel);

            GUILayout.Space (space);
            
            UI_Fade();
            
            GUILayout.Space (space);

            UI_Reset();
            
            GUILayout.Space (8);
        }

        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////
        
        void InfoUI()
        {
            // Cache info
            if (rigid.HasMeshes == true)
                GUILayout.Label ("    Precached Unity Meshes: " + rigid.meshes.Length);
            if (rigid.HasFragments == true)
                GUILayout.Label ("    Fragments: " + rigid.fragments.Count);
            if (rigid.HasRfMeshes == true)
                GUILayout.Label ("    Precached Serialized Meshes: " + rigid.rfMeshes.Length);

            // Demolition info
            if (Application.isPlaying == true && rigid.enabled == true && rigid.initialized == true && rigid.objectType != ObjectType.MeshRoot)
            {
                // Space
                GUILayout.Space (3);

                // Info
                GUILayout.Label ("Info", EditorStyles.boldLabel);

                // Excluded
                if (rigid.physics.exclude == true)
                    GUILayout.Label ("WARNING: Object excluded from simulation.");

                // Size
                GUILayout.Label ("    Size: " + rigid.limitations.bboxSize.ToString());

                // Demolition
                GUILayout.Label ("    Demolition depth: " + rigid.limitations.currentDepth.ToString() + "/" + rigid.limitations.depth.ToString());

                // Damage
                if (rigid.damage.enable == true)
                    GUILayout.Label ("    Damage applied: " + rigid.damage.currentDamage.ToString() + "/" + rigid.damage.maxDamage.ToString());
                
                // Fading
                if (rigid.fading.state == 1)
                    GUILayout.Label ("    Object about to fade...");
                
                // Fading
                if (rigid.fading.state == 2)
                    GUILayout.Label ("    Fading in progress...");

                // Bad mesh
                if (rigid.meshDemolition.badMesh > RayfireMan.inst.advancedDemolitionProperties.badMeshTry)
                    GUILayout.Label ("    Object has bad mesh and will not be demolished anymore");
            }
            
            // Mesh Root info
            if (rigid.objectType == ObjectType.MeshRoot)
            {
                if (rigid.physics.HasIgnore == true)
                    GUILayout.Label ("    Ignore Pairs: " + rigid.physics.ignoreList.Count / 2);
            }
            
            // Cluster info
            if (rigid.objectType == ObjectType.NestedCluster || rigid.objectType == ObjectType.ConnectedCluster)
            {
                if (rigid.physics.clusterColliders != null && rigid.physics.clusterColliders.Count > 0)
                {
                    if (rigid.clusterDemolition != null && rigid.clusterDemolition.cluster != null)
                    {
                        GUILayout.Label ("    Cluster Colliders: " + rigid.physics.clusterColliders.Count);

                        if (rigid.objectType == ObjectType.ConnectedCluster)
                        {
                            GUILayout.Label ("    Cluster Shards: " + rigid.clusterDemolition.cluster.shards.Count + "/" + rigid.clusterDemolition.am);
                            GUILayout.Label ("    Amount Integrity: " + rigid.AmountIntegrity + "%");
                        }

                        if (rigid.physics.HasIgnore == true)
                            GUILayout.Label ("    Ignore Pairs: " + rigid.physics.ignoreList.Count / 2);
                    }
                }
            }
        }

        void RigidManUI()
        {
            if (Application.isPlaying == true)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button ("Demolish", GUILayout.Height (25))) Demolish();
                if (GUILayout.Button ("Activate", GUILayout.Height (25))) Activate();
                if (GUILayout.Button ("Fade",     GUILayout.Height (25))) Fade();
                EditorGUILayout.EndHorizontal();
            }
        }
        
        void Demolish()
        {
            if (Application.isPlaying == true)
                foreach (var targ in targets)
                    if (targ as RayfireRigid != null)
                        (targ as RayfireRigid).Demolish();
        }
        
        void Activate()
        {
            if (Application.isPlaying == true)
                foreach (var targ in targets)
                    if (targ as RayfireRigid != null)
                        if ((targ as RayfireRigid).simulationType == SimType.Inactive || (targ as RayfireRigid).simulationType == SimType.Kinematic)
                            (targ as RayfireRigid).Activate();
        }
        
        void Fade()
        {
            if (Application.isPlaying == true)
                foreach (var targ in targets)
                    if (targ as RayfireRigid != null)
                        (targ as RayfireRigid).Fade();
        }
        
        void SetupUI()
        {
            GUILayout.BeginHorizontal();
             
            if (GUILayout.Button (" Editor Setup ", GUILayout.Height (25)))
                foreach (var targ in targets)
                    if (targ as RayfireRigid != null)
                    {
                        (targ as RayfireRigid).EditorSetup();
                        SetDirty (targ as RayfireRigid); 
                    }
            
            if (GUILayout.Button (  "Reset Setup", GUILayout.Height (25)))
                foreach (var targ in targets)
                    if (targ as RayfireRigid != null)
                    {
                        (targ as RayfireRigid).ResetSetup();
                        SetDirty (targ as RayfireRigid); 
                    }

            EditorGUILayout.EndHorizontal();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Cluster UI
        /// /////////////////////////////////////////////////////////
        
        void ClusterCollapseUI()
        {
            if (rigid.objectType == ObjectType.ConnectedCluster)
            {
                GUILayout.Label ("  Collapse", EditorStyles.boldLabel);

                GUILayout.BeginHorizontal();

                GUILayout.Label ("By Area:", GUILayout.Width (55));

                // Start check for slider change
                EditorGUI.BeginChangeCheck();
                rigid.clusterDemolition.cluster.areaCollapse = EditorGUILayout.Slider (rigid.clusterDemolition.cluster.areaCollapse,
                    rigid.clusterDemolition.cluster.minimumArea, rigid.clusterDemolition.cluster.maximumArea);
                if (EditorGUI.EndChangeCheck() == true)
                    if (Application.isPlaying == true)
                        RFCollapse.AreaCollapse (rigid, rigid.clusterDemolition.cluster.areaCollapse);

                EditorGUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                GUILayout.Label ("By Size:", GUILayout.Width (55));

                // Start check for slider change
                EditorGUI.BeginChangeCheck();
                rigid.clusterDemolition.cluster.sizeCollapse = EditorGUILayout.Slider (rigid.clusterDemolition.cluster.sizeCollapse,
                    rigid.clusterDemolition.cluster.minimumSize, rigid.clusterDemolition.cluster.maximumSize);
                if (EditorGUI.EndChangeCheck() == true)
                    if (Application.isPlaying == true)
                        RFCollapse.SizeCollapse (rigid, rigid.clusterDemolition.cluster.sizeCollapse);

                EditorGUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                GUILayout.Label ("Random:", GUILayout.Width (55));

                // Start check for slider change
                EditorGUI.BeginChangeCheck();
                rigid.clusterDemolition.cluster.randomCollapse = EditorGUILayout.IntSlider (rigid.clusterDemolition.cluster.randomCollapse, 0, 100);
                if (EditorGUI.EndChangeCheck() == true)
                    RFCollapse.RandomCollapse (rigid, rigid.clusterDemolition.cluster.randomCollapse, rigid.clusterDemolition.seed);

                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button ("Start Collapse", GUILayout.Height (25)))
                    if (Application.isPlaying)
                        foreach (var targ in targets)
                            if (targ as RayfireRigid != null)
                                RFCollapse.StartCollapse (targ as RayfireRigid);
            }
        }
        
        void ClusterPreviewUI(RayfireRigid scr)
        {
            if (rigid.objectType == ObjectType.ConnectedCluster)
            {
                GUILayout.BeginHorizontal();

                // Show nodes
                EditorGUI.BeginChangeCheck();
                scr.clusterDemolition.cn = GUILayout.Toggle (scr.clusterDemolition.cn, "Show Connections",   "Button", GUILayout.Height (22));
                scr.clusterDemolition.nd = GUILayout.Toggle (scr.clusterDemolition.nd, "    Show Nodes    ", "Button", GUILayout.Height (22));
                if (EditorGUI.EndChangeCheck())
                {
                    foreach (Object targ in targets)
                        if (targ as RayfireRigid != null)
                        {
                            (targ as RayfireRigid).clusterDemolition.cn = rigid.clusterDemolition.cn;
                            (targ as RayfireRigid).clusterDemolition.nd = rigid.clusterDemolition.nd;
                            SetDirty (targ as RayfireRigid); 
                        }
                    SceneView.RepaintAll();
                }
                
                EditorGUILayout.EndHorizontal();
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Draw
        /// /////////////////////////////////////////////////////////

        [DrawGizmo (GizmoType.Selected | GizmoType.NonSelected | GizmoType.Pickable)]
        static void DrawGizmosSelected (RayfireRigid targ, GizmoType gizmoType)
        {
            // Missing shards
            if (RFCluster.IntegrityCheck (targ.clusterDemolition.cluster) == false)
                Debug.Log (rfRig + targ.name + misShards, targ.gameObject);
            
            ClusterDraw (targ);
        }
        
        // CLuster connection and nodes viewport preview
        static void ClusterDraw(RayfireRigid targ)
        {
            if (targ.objectType == ObjectType.ConnectedCluster)
            {
                // Damage style
                damageStyle.fontSize         = 15;
                damageStyle.normal.textColor = Color.red;
                
                if (targ.clusterDemolition.cluster != null && targ.clusterDemolition.cluster.shards.Count > 0)
                {
                    // Reinit connections
                    if (targ.clusterDemolition.cluster.initialized == false)
                        RFCluster.InitCluster (targ, targ.clusterDemolition.cluster);
                    
                    // Draw
                    for (int i = 0; i < targ.clusterDemolition.cluster.shards.Count; i++)
                    {
                        if (targ.clusterDemolition.cluster.shards[i].tm != null)
                        {
                            // Damage
                            if (targ.damage.toShards == true)
                            {
                                if (targ.clusterDemolition.cluster.shards[i].dm > 0)
                                {
                                    Vector3 pos = targ.clusterDemolition.cluster.shards[i].tm.position;
                                    Handles.Label (pos, targ.clusterDemolition.cluster.shards[i].dm.ToString ("F1"), damageStyle);
                                }
                            }

                            // Set color
                            if (targ.clusterDemolition.cluster.shards[i].uny == false)
                            {
                                Gizmos.color = targ.clusterDemolition.cluster.shards[i].nIds.Count > 0 
                                    ? Color.blue 
                                    : Color.gray;
                            }
                            else
                                Gizmos.color = targ.clusterDemolition.cluster.shards[i].act == true ? Color.magenta : Color.red;

                            // Nodes
                            if (targ.clusterDemolition.nd == true) 
                                Gizmos.DrawWireSphere (targ.clusterDemolition.cluster.shards[i].tm.position, targ.clusterDemolition.cluster.shards[i].sz / 12f);
                            
                            // Connections
                            if (targ.clusterDemolition.cn == true)
                                if (targ.clusterDemolition.cluster.shards[i].neibShards != null)
                                    for (int j = 0; j < targ.clusterDemolition.cluster.shards[i].neibShards.Count; j++)
                                        if (targ.clusterDemolition.cluster.shards[i].neibShards[j].tm != null)
                                            Gizmos.DrawLine (targ.clusterDemolition.cluster.shards[i].tm.position, targ.clusterDemolition.cluster.shards[i].neibShards[j].tm.position);
                        }
                    }
                }
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////

        void SetDirty (RayfireRigid scr)
        {
            if (Application.isPlaying == false)
            {
                EditorUtility.SetDirty (scr);
                EditorSceneManager.MarkSceneDirty (scr.gameObject.scene);
                SceneView.RepaintAll();
            }
        }
        
        void SetFoldoutPref (ref bool val, string pref, GUIContent caption, bool state) 
        {
            EditorGUI.BeginChangeCheck();
            val = EditorGUILayout.Foldout (val, caption, state);
            if (EditorGUI.EndChangeCheck() == true)
                EditorPrefs.SetBool (pref, val);
        }
    }
}