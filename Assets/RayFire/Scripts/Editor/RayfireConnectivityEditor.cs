using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace RayFire
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireConnectivity))]
    public class RayfireConnectivityEditor : Editor
    {
        RayfireConnectivity conn;
        
        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////
        
        static Color wireColor   = new Color (0.58f, 0.77f, 1f);
        static Color stressColor = Color.green;
        static int   space       = 3;
        static bool  exp_collapse;
        static bool  exp_stress;
        static bool  exp_filters;
        static bool  exp_joints;
        
        static GUIContent gui_con_type  = new GUIContent ("Type",                   "Define the the way connections among Shards will be calculated.");
        static GUIContent gui_con_exp   = new GUIContent ("Expand",                 "Increase size of bounding box for By Bounding Box types.");
        static GUIContent gui_filt_area = new GUIContent ("Minimum Area",           "Two shards will have connection if their shared area is bigger than this value.");
        static GUIContent gui_filt_size = new GUIContent ("Minimum Size",           "Two shards will have connection if their size is bigger than this value.");
        static GUIContent gui_filt_perc = new GUIContent ("Percentage",             "Random percentage of connections will be discarded.");
        static GUIContent gui_filt_seed = new GUIContent ("Seed",                   "Seed for random percentage filter and for Random Collapse.");
        static GUIContent gui_cls_cst   = new GUIContent ("Clusterize",             "Create Connected Cluster for group of Shards connected with each other but not connected with any Unyielding Shard.");
        static GUIContent gui_cls_dml   = new GUIContent ("Demolishable",           "Set Demolition type to Runtime for Connected Clusters created during activation.");
        
        static GUIContent gui_trg_col  = new GUIContent ("Trigger",  "Trigger Collider which will activate all overlapped shards. Only Box and Sphere colliders supported for now.");
        static GUIContent gui_trg_deb  = new GUIContent ("Debris",   "Percentage of solo shards at the edges of fractured cluster.");
        static GUIContent gui_trg_init = new GUIContent ("Fracture", "Evaluate Connectivity public method Fracture (Collider collider, int debris)");
        
        static GUIContent gui_clp_init = new GUIContent ("Initiate",  "Collapse allows you start break connections among shards and activate single Shards or " +
                                                                      "Group of Shards if they are not connected with any of Unyielding Shard. ");
        static GUIContent gui_clp_intg = new GUIContent ("Integrity", "");
        static GUIContent gui_clp_type = new GUIContent ("Type", " By Area: Shard will loose it's connections if it's shared area surface is less then defined value.\n" + 
                                                                 " By Size: Shard will loose it's connections if it's Size is less then defined value.\n" + 
                                                                 " Random: Shard will loose it's connections if it's random value in range from 0 to 100 is less then defined value.");
        static GUIContent gui_clp_str  = new GUIContent ("Start",    "Defines start value in percentage relative to whole range of picked type.");
        static GUIContent gui_clp_end  = new GUIContent ("End",      "Defines end value in percentage relative to whole range of picked type.");
        static GUIContent gui_clp_step = new GUIContent ("Steps",    "Amount of times when defined threshold value will be set during Duration period.");
        static GUIContent gui_clp_dur  = new GUIContent ("Duration", "Time which it will take Start value to be increased to End value.");

        static GUIContent gui_str_enab = new GUIContent ("Enable",    "");
        static GUIContent gui_str_prev = new GUIContent ("Preview",  "");
        static GUIContent gui_str_init = new GUIContent ("Initiate",  "");
        static GUIContent gui_str_thr  = new GUIContent ("Threshold", "Amount of stress every connection can take to break.");
        static GUIContent gui_str_ero  = new GUIContent ("Erosion",   "Multiplier for stress which get connection every Interval.");
        static GUIContent gui_str_int  = new GUIContent ("Interval",  "Connection stress will be increased every Interval. Measures in Seconds.");
        static GUIContent gui_str_sup  = new GUIContent ("Support",   "Angle to define which shards above Shard should be considered as supported shards.");
        static GUIContent gui_str_exp  = new GUIContent ("Exposed",   "Erode connections only for shards which lost their neighbor.");
        static GUIContent gui_str_siz  = new GUIContent ("By Size",   "");
        
        /// /////////////////////////////////////////////////////////
        /// Enable
        /// /////////////////////////////////////////////////////////
        
        void OnEnable()
        {
            if (EditorPrefs.HasKey ("rf_cc") == true) exp_collapse = EditorPrefs.GetBool ("rf_cc");
            if (EditorPrefs.HasKey ("rf_cs") == true) exp_stress   = EditorPrefs.GetBool ("rf_cs");
        }
        
        /// /////////////////////////////////////////////////////////
        /// Inspector
        /// /////////////////////////////////////////////////////////
        
        public override void OnInspectorGUI()
        {
            conn = target as RayfireConnectivity;
            if (conn == null)
                return;
            
            
            //if (GUILayout.Button ("Reset Rigid", GUILayout.Height (25)))
            //    conn.testRigid.ResetRigid();

            
            
            GUILayout.Space (8);

            UI_Preview();
            
            GUILayout.Space (space);
            
            UI_Info();
            
            GUILayout.Space (space);
            
            UI_Connectivity();
            
            if (conn.joints.enable == false)
            {
                GUILayout.Space (space);

                UI_Cluster();

                GUILayout.Space (space);

                UI_Collapse();

                GUILayout.Space (space);

                UI_Stress();

                GUILayout.Space (space);

                UI_Trigger();
            }
            
            GUILayout.Space (space);
            
            UI_Joints();

            GUILayout.Space (8);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Preview
        /// /////////////////////////////////////////////////////////
        
        void UI_Preview()
        {
            EditorGUI.BeginChangeCheck();
            conn.showGizmo = GUILayout.Toggle (conn.showGizmo, " Show Gizmo ", "Button", GUILayout.Height (25));
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireConnectivity scr in targets)
                {
                    scr.showGizmo = conn.showGizmo;
                    SetDirty (scr);
                }
            }

            GUILayout.Space (space);
            
            GUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            conn.showConnections = GUILayout.Toggle (conn.showConnections, "Show Connections",    "Button", GUILayout.Height (25));
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireConnectivity scr in targets)
                {
                    scr.showConnections = conn.showConnections;
                    SetDirty (scr);
                    SceneView.RepaintAll();
                }
            }
            
            EditorGUI.BeginChangeCheck();
            conn.showNodes       = GUILayout.Toggle (conn.showNodes,       "Show Nodes", "Button", GUILayout.Height (25));
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireConnectivity scr in targets)
                {
                    scr.showNodes = conn.showNodes;
                    SetDirty (scr);
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Info
        /// /////////////////////////////////////////////////////////
        
        void UI_Info()
        {
            if (conn.cluster.shards.Count > 0)
            {
                GUILayout.Label ("  Setup Info", EditorStyles.boldLabel);
                GUILayout.Label ("Cluster Shards: " + conn.cluster.shards.Count + "/" + conn.initShardAmount);
                GUILayout.Space (space);
                GUILayout.Label ("Amount Integrity: " + conn.AmountIntegrity + "%");
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Connectivity
        /// /////////////////////////////////////////////////////////
        
        void UI_Connectivity () 
        {
            GUILayout.Label ("  Connectivity", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            conn.type = (ConnectivityType)EditorGUILayout.EnumPopup (gui_con_type, conn.type);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireConnectivity scr in targets)
                {
                    scr.type = conn.type;
                    SetDirty (scr);
                }
            }

            if (conn.type != ConnectivityType.ByTriangles && conn.type != ConnectivityType.ByPolygons)
            {
                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                conn.expand = EditorGUILayout.Slider (gui_con_exp, conn.expand, 0, 1f);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireConnectivity scr in targets)
                    {
                        scr.expand = conn.expand;
                        SetDirty (scr);
                    }
                }
            }
            
            GUILayout.Space (space);
            
            exp_filters = EditorGUILayout.Foldout (exp_filters, "Filters", true);
            if (exp_filters == true)
            {
                GUILayout.Space (space);

                EditorGUI.indentLevel++;
                
                UI_Filters();
                
                EditorGUI.indentLevel--;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Filters
        /// /////////////////////////////////////////////////////////
        
        void UI_Filters () 
        {
            if (conn.type != ConnectivityType.ByBoundingBox)
            {
                EditorGUI.BeginChangeCheck();
                conn.minimumArea = EditorGUILayout.Slider (gui_filt_area, conn.minimumArea, 0, 1f);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireConnectivity scr in targets)
                    {
                        scr.minimumArea = conn.minimumArea;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);
            }

            EditorGUI.BeginChangeCheck();
            conn.minimumSize = EditorGUILayout.Slider (gui_filt_size, conn.minimumSize, 0, 10f);
            if (EditorGUI.EndChangeCheck() == true)
                foreach (RayfireConnectivity scr in targets)
                {
                    scr.minimumSize = conn.minimumSize;
                    SetDirty (scr);
                }

            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            conn.percentage = EditorGUILayout.IntSlider (gui_filt_perc, conn.percentage, 0, 100);
            if (EditorGUI.EndChangeCheck() == true)
                foreach (RayfireConnectivity scr in targets)
                {
                    scr.percentage = conn.percentage;
                    SetDirty (scr);
                }

            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            conn.seed = EditorGUILayout.IntSlider (gui_filt_seed, conn.seed, 0, 100);
            if (EditorGUI.EndChangeCheck() == true)
                foreach (RayfireConnectivity scr in targets)
                {
                    scr.seed = conn.seed;
                    SetDirty (scr);
                }
        }

        /// /////////////////////////////////////////////////////////
        /// Filters
        /// /////////////////////////////////////////////////////////

        void UI_Cluster()
        {
            GUILayout.Label ("  Cluster", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            conn.clusterize = EditorGUILayout.Toggle (gui_cls_cst, conn.clusterize);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireConnectivity scr in targets)
                {
                    scr.clusterize = conn.clusterize;
                    SetDirty (scr);
                }

            if (conn.clusterize == true)
            {
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                conn.demolishable = EditorGUILayout.Toggle (gui_cls_dml, conn.demolishable);
                if (EditorGUI.EndChangeCheck())
                    foreach (RayfireConnectivity scr in targets)
                    {
                        scr.demolishable = conn.demolishable;
                        SetDirty (scr);
                    }
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Collapse
        /// /////////////////////////////////////////////////////////

        void UI_Collapse()
        {
            GUILayout.Label ("  Collapse", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            conn.startCollapse = (RayfireConnectivity.RFConnInitType)EditorGUILayout.EnumPopup (gui_clp_init, conn.startCollapse);
            if (EditorGUI.EndChangeCheck() == true)
                foreach (RayfireConnectivity scr in targets)
                {
                    scr.startCollapse = conn.startCollapse;
                    SetDirty (scr);
                }

            if (conn.startCollapse == RayfireConnectivity.RFConnInitType.ByIntegrity)
            {
                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                conn.collapseByIntegrity = EditorGUILayout.IntSlider (gui_clp_intg, conn.collapseByIntegrity, 1, 99);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireConnectivity scr in targets)
                    {
                        scr.collapseByIntegrity = conn.collapseByIntegrity;
                        SetDirty (scr);
                    }
            }

            GUILayout.Space (space);
            
            SetFoldoutPref (ref exp_collapse, "rf_cc", "Properties");
            if (exp_collapse == true)
            {
                GUILayout.Space (space);
                
                EditorGUI.indentLevel++;
                
                EditorGUI.BeginChangeCheck();
                conn.collapse.type = (RFCollapse.RFCollapseType)EditorGUILayout.EnumPopup (gui_clp_type, conn.collapse.type);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireConnectivity scr in targets)
                    {
                        scr.collapse.type = conn.collapse.type;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                conn.collapse.start = EditorGUILayout.IntSlider (gui_clp_str, conn.collapse.start, 0, 99);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireConnectivity scr in targets)
                    {
                        scr.collapse.start = conn.collapse.start;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                conn.collapse.end = EditorGUILayout.IntSlider (gui_clp_end, conn.collapse.end, 1, 100);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireConnectivity scr in targets)
                    {
                        scr.collapse.end = conn.collapse.end;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                conn.collapse.steps = EditorGUILayout.IntSlider (gui_clp_step, conn.collapse.steps, 1, 100);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireConnectivity scr in targets)
                    {
                        scr.collapse.steps = conn.collapse.steps;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                conn.collapse.duration = EditorGUILayout.Slider (gui_clp_dur, conn.collapse.duration, 0, 60f);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireConnectivity scr in targets)
                    {
                        scr.collapse.duration = conn.collapse.duration;
                        SetDirty (scr);
                    }

                EditorGUI.indentLevel--;
            }
            
            UI_Collapse_Buttons();

            UI_Collapse_Sliders();
        }

        void UI_Collapse_Buttons()
        {
            // Only runtime
            if (Application.isPlaying == false)
                return;

            GUILayout.Space (space);
                
            // Show start collapse if not Start by default
            if (conn.collapse.inProgress == false)
            {
                if (GUILayout.Button ("Start Collapse", GUILayout.Height (25)))
                    foreach (var targ in targets)
                        if (targ as RayfireConnectivity != null)
                            RFCollapse.StartCollapse (targ as RayfireConnectivity);
            }

            // Show stop collapse if not Start by default
            if (conn.collapse.inProgress == true)
            {
                if (GUILayout.Button ("Stop Collapse", GUILayout.Height (25)))
                    foreach (var targ in targets)
                        if (targ as RayfireConnectivity != null)
                            RFCollapse.StopCollapse (targ as RayfireConnectivity);
            }
        }

        void UI_Collapse_Sliders()
        {
            // Only runtime
            if (Application.isPlaying == false)
                return;
            
            GUILayout.Space (space);
            
            GUILayout.BeginHorizontal();

            GUILayout.Label ("By Area:", GUILayout.Width (55));

            // Start check for slider change
            EditorGUI.BeginChangeCheck();
            conn.cluster.areaCollapse = EditorGUILayout.Slider(conn.cluster.areaCollapse, conn.cluster.minimumArea, conn.cluster.maximumArea);
            if (EditorGUI.EndChangeCheck() == true)
                if (Application.isPlaying)
                    RFCollapse.AreaCollapse (conn, conn.cluster.areaCollapse);

            EditorGUILayout.EndHorizontal();

            GUILayout.Space (space);
            
            GUILayout.BeginHorizontal();

            GUILayout.Label ("By Size:", GUILayout.Width (55));

            // Start check for slider change
            EditorGUI.BeginChangeCheck();
            conn.cluster.sizeCollapse = EditorGUILayout.Slider(conn.cluster.sizeCollapse, conn.cluster.minimumSize, conn.cluster.maximumSize);
            if (EditorGUI.EndChangeCheck() == true)
                if (Application.isPlaying)
                    RFCollapse.SizeCollapse (conn, conn.cluster.sizeCollapse);

            EditorGUILayout.EndHorizontal();

            GUILayout.Space (space);
            
            GUILayout.BeginHorizontal();

            GUILayout.Label ("Random:", GUILayout.Width (55));

            // Start check for slider change
            EditorGUI.BeginChangeCheck();
            conn.cluster.randomCollapse = EditorGUILayout.IntSlider(conn.cluster.randomCollapse, 0, 100);
            if (EditorGUI.EndChangeCheck() == true)
                if (Application.isPlaying)
                    RFCollapse.RandomCollapse (conn, conn.cluster.randomCollapse, conn.seed);
            
            EditorGUILayout.EndHorizontal();
        }

        /// /////////////////////////////////////////////////////////
        /// Stress
        /// /////////////////////////////////////////////////////////

        void UI_Stress()
        {
            GUILayout.Label ("  Stress", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            conn.stress.enable = EditorGUILayout.Toggle (gui_str_enab, conn.stress.enable);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireConnectivity scr in targets)
                {
                    scr.stress.enable = conn.stress.enable;
                    SetDirty (scr);
                }

            if (conn.stress.enable == false)
                return;
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            conn.showStress = EditorGUILayout.Toggle (gui_str_prev, conn.showStress);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireConnectivity scr in targets)
                {
                    scr.showStress = conn.showStress;
                    SetDirty (scr);
                }

            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            conn.startStress = (RayfireConnectivity.RFConnInitType)EditorGUILayout.EnumPopup (gui_str_init, conn.startStress);
            if (EditorGUI.EndChangeCheck() == true)
                foreach (RayfireConnectivity scr in targets)
                {
                    scr.startStress = conn.startStress;
                    SetDirty (scr);
                }

            if (conn.startStress == RayfireConnectivity.RFConnInitType.ByIntegrity)
            {
                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                conn.stressByIntegrity = EditorGUILayout.IntSlider (gui_clp_intg, conn.stressByIntegrity, 1, 99);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireConnectivity scr in targets)
                    {
                        scr.stressByIntegrity = conn.stressByIntegrity;
                        SetDirty (scr);
                    }
            }

            GUILayout.Space (space);
            
            SetFoldoutPref (ref exp_stress, "rf_cs", "Properties");
            
            if (exp_stress == true)
                UI_Stress_Properties();
            
            // Start/Stop
            if (Application.isPlaying == true)
            {
                GUILayout.Space (space);
                
                // Show start stress if not Start by default
                if (conn.stress.inProgress == false)
                {
                    if (GUILayout.Button ("Start Stress ", GUILayout.Height (25)))
                        foreach (var targ in targets)
                            if (targ as RayfireConnectivity != null)
                                RFStress.StartStress (targ as RayfireConnectivity);
                }

                // Show stop collapse if not Start by default
                if (conn.stress.inProgress == true)
                {
                    if (GUILayout.Button ("Stop Stress", GUILayout.Height (25)))
                        foreach (var targ in targets)
                            if (targ as RayfireConnectivity != null)
                                RFStress.StopStress (targ as RayfireConnectivity);
                }
            }
        }
        
        void UI_Stress_Properties()
        {
            EditorGUI.indentLevel++;
            
            GUILayout.Label ("      Connections", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            conn.stress.threshold = EditorGUILayout.IntSlider (gui_str_thr, conn.stress.threshold, 1, 1000);
            if (EditorGUI.EndChangeCheck() == true)
                foreach (RayfireConnectivity scr in targets)
                {
                    scr.stress.threshold = conn.stress.threshold;
                    SetDirty (scr);
                }

            GUILayout.Space (space);
                
            EditorGUI.BeginChangeCheck();
            conn.stress.erosion = EditorGUILayout.Slider (gui_str_ero, conn.stress.erosion, 0, 10f);
            if (EditorGUI.EndChangeCheck() == true)
                foreach (RayfireConnectivity scr in targets)
                {
                    scr.stress.erosion = conn.stress.erosion;
                    SetDirty (scr);
                }

            GUILayout.Space (space);
                
            EditorGUI.BeginChangeCheck();
            conn.stress.interval = EditorGUILayout.Slider (gui_str_int, conn.stress.interval, 0.1f, 10f);
            if (EditorGUI.EndChangeCheck() == true)
                foreach (RayfireConnectivity scr in targets)
                {
                    scr.stress.interval = conn.stress.interval;
                    SetDirty (scr);
                }

            GUILayout.Label ("      Shards", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            conn.stress.support = EditorGUILayout.IntSlider (gui_str_sup, conn.stress.support, 0, 90);
            if (EditorGUI.EndChangeCheck() == true)
                foreach (RayfireConnectivity scr in targets)
                {
                    scr.stress.support = conn.stress.support;
                    SetDirty (scr);
                }

            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            conn.stress.exposed = EditorGUILayout.Toggle (gui_str_exp, conn.stress.exposed);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireConnectivity scr in targets)
                {
                    scr.stress.exposed = conn.stress.exposed;
                    SetDirty (scr);
                }

            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            conn.stress.bySize = EditorGUILayout.Toggle (gui_str_siz, conn.stress.bySize);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireConnectivity scr in targets)
                {
                    scr.stress.bySize = conn.stress.bySize;
                    SetDirty (scr);
                }

            EditorGUI.indentLevel--;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Trigger
        /// /////////////////////////////////////////////////////////

        void UI_Trigger()
        {
            GUILayout.Label ("  Fracture", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            conn.triggerCollider = (Collider)EditorGUILayout.ObjectField (gui_trg_col, conn.triggerCollider, typeof(Collider), true);
            if (EditorGUI.EndChangeCheck() == true)
                SetDirty (conn);
            
            if (conn.clusterize == true)
            {
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                conn.triggerDebris = EditorGUILayout.IntSlider (gui_trg_deb, conn.triggerDebris, 0, 50);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireConnectivity scr in targets)
                    {
                        scr.triggerDebris = conn.triggerDebris;
                        SetDirty (scr);
                    }
            }

            if (Application.isPlaying == true && conn.triggerCollider != null)
            {
                GUILayout.Space (space);

                if (GUILayout.Button (gui_trg_init, GUILayout.Height (25)))
                    if (Application.isPlaying == true)
                        conn.Fracture (conn.triggerCollider, conn.triggerDebris);
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Joints
        /// /////////////////////////////////////////////////////////

        void UI_Joints()
        {
            GUILayout.Label ("  Joints WIP", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            conn.joints.enable = EditorGUILayout.Toggle ("Enable", conn.joints.enable);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireConnectivity scr in targets)
                {
                    scr.joints.enable = conn.joints.enable;
                    SetDirty (scr);
                }

            if (conn.joints.enable == true)
            {
                GUILayout.Space (space);

                //exp_joints = EditorGUILayout.Foldout (exp_joints, "Properties", true);
                //if (exp_joints == true)
                {
                    EditorGUI.indentLevel++;

                    GUILayout.Label ("  Break", EditorStyles.boldLabel);
                    
                    EditorGUI.BeginChangeCheck();
                    conn.joints.breakType = (RFJointProperties.RFJointBreakType)EditorGUILayout.EnumPopup ("Type", conn.joints.breakType);
                    if (EditorGUI.EndChangeCheck() == true)
                        foreach (RayfireConnectivity scr in targets)
                        {
                            scr.joints.breakType = conn.joints.breakType;
                            SetDirty (scr);
                        }
                    
                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    conn.joints.breakForce = EditorGUILayout.IntSlider ("Force", conn.joints.breakForce, 0, 5000);
                    if (EditorGUI.EndChangeCheck() == true)
                        foreach (RayfireConnectivity scr in targets)
                        {
                            scr.joints.breakForce = conn.joints.breakForce;
                            SetDirty (scr);

                            if (Application.isPlaying == true)
                                RFJointProperties.SetBreakForce (scr.joints.breakForce, scr.joints.breakForceVar, scr.joints.jointList, scr.joints.forceByMass);
                        }

                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    conn.joints.breakForceVar = EditorGUILayout.IntSlider ("Variation", conn.joints.breakForceVar, 0, 100);
                    if (EditorGUI.EndChangeCheck() == true)
                        foreach (RayfireConnectivity scr in targets)
                        {
                            scr.joints.breakForceVar = conn.joints.breakForceVar;
                            SetDirty (scr);

                            if (Application.isPlaying == true)
                                RFJointProperties.SetBreakForce (scr.joints.breakForce, scr.joints.breakForceVar, scr.joints.jointList, scr.joints.forceByMass);
                        }
                    
                    GUILayout.Space (space);
                    
                    EditorGUI.BeginChangeCheck();
                    conn.joints.forceByMass = EditorGUILayout.Toggle ("Force By Mass", conn.joints.forceByMass);
                    if (EditorGUI.EndChangeCheck())
                        foreach (RayfireConnectivity scr in targets)
                        {
                            scr.joints.forceByMass = conn.joints.forceByMass;
                            SetDirty (scr);
                        }
                    
                    GUILayout.Space (space);
                    
                    GUILayout.Label ("  Angular", EditorStyles.boldLabel);

                    EditorGUI.BeginChangeCheck();
                    conn.joints.angleLimit = EditorGUILayout.IntSlider ("Limit", conn.joints.angleLimit, 0, 50);
                    if (EditorGUI.EndChangeCheck() == true)
                        foreach (RayfireConnectivity scr in targets)
                        {
                            scr.joints.angleLimit = conn.joints.angleLimit;
                            SetDirty (scr);

                            if (Application.isPlaying == true)
                                RFJointProperties.SetAngularMotion (scr.joints.angleLimit, scr.joints.angleLimitVar, scr.joints.jointList);
                        }

                    GUILayout.Space (space);
                    
                    EditorGUI.BeginChangeCheck();
                    conn.joints.angleLimitVar = EditorGUILayout.IntSlider ("Variation", conn.joints.angleLimitVar, 0, 100);
                    if (EditorGUI.EndChangeCheck() == true)
                        foreach (RayfireConnectivity scr in targets)
                        {
                            scr.joints.angleLimitVar = conn.joints.angleLimitVar;
                            SetDirty (scr);

                            if (Application.isPlaying == true)
                                RFJointProperties.SetAngularMotion (scr.joints.angleLimit, scr.joints.angleLimitVar, scr.joints.jointList);
                        }

                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    conn.joints.damper = EditorGUILayout.IntSlider ("Damper", conn.joints.damper, 0, 10000);
                    if (EditorGUI.EndChangeCheck() == true)
                        foreach (RayfireConnectivity scr in targets)
                        {
                            scr.joints.damper = conn.joints.damper;
                            SetDirty (scr);

                            if (Application.isPlaying == true)
                                RFJointProperties.SetSpring (scr.joints.damper, scr.joints.jointList);
                        }

                    GUILayout.Space (space);
                    
                    GUILayout.Label ("  Deformation", EditorStyles.boldLabel);
                    
                    EditorGUI.BeginChangeCheck();
                    conn.joints.deformEnable = EditorGUILayout.Toggle ("Enable", conn.joints.deformEnable);
                    if (EditorGUI.EndChangeCheck())
                        foreach (RayfireConnectivity scr in targets)
                        {
                            scr.joints.deformEnable = conn.joints.deformEnable;
                            SetDirty (scr);
                        }

                    if (conn.joints.deformEnable == true)
                    {
                        GUILayout.Space (space);

                        // Stiffness
                        if (conn.joints.breakType == RFJointProperties.RFJointBreakType.Breakable)
                        {
                            EditorGUI.BeginChangeCheck();
                            conn.joints.stiffFrc = EditorGUILayout.Slider ("Stiffness", conn.joints.stiffFrc, 0.05f, 0.95f);
                            if (EditorGUI.EndChangeCheck() == true)
                                foreach (RayfireConnectivity scr in targets)
                                {
                                    scr.joints.stiffFrc = conn.joints.stiffFrc;
                                    SetDirty (scr);
                                }
                        }
                        else
                        {
                            EditorGUI.BeginChangeCheck();
                            conn.joints.stiffAbs = EditorGUILayout.IntSlider ("Stiffness", conn.joints.stiffAbs, 1, 500);
                            if (EditorGUI.EndChangeCheck() == true)
                                foreach (RayfireConnectivity scr in targets)
                                {
                                    scr.joints.stiffAbs = conn.joints.stiffAbs;
                                    SetDirty (scr);
                                }
                        }

                        GUILayout.Space (space);

                        EditorGUI.BeginChangeCheck();
                        conn.joints.bend = EditorGUILayout.IntSlider ("Bending", conn.joints.bend, 0, 10);
                        if (EditorGUI.EndChangeCheck() == true)
                            foreach (RayfireConnectivity scr in targets)
                            {
                                scr.joints.bend = conn.joints.bend;
                                SetDirty (scr);
                            }

                        GUILayout.Space (space);

                        EditorGUI.BeginChangeCheck();
                        conn.joints.weakening = EditorGUILayout.Slider ("Weakening", conn.joints.weakening, 0, 0.9f);
                        if (EditorGUI.EndChangeCheck() == true)
                            foreach (RayfireConnectivity scr in targets)
                            {
                                scr.joints.weakening = conn.joints.weakening;
                                SetDirty (scr);
                            }

                        GUILayout.Space (space);

                        EditorGUI.BeginChangeCheck();
                        conn.joints.percentage = EditorGUILayout.IntSlider ("Percentage", conn.joints.percentage, 1, 100);
                        if (EditorGUI.EndChangeCheck() == true)
                            foreach (RayfireConnectivity scr in targets)
                            {
                                scr.joints.percentage = conn.joints.percentage;
                                SetDirty (scr);
                            }

                        GUILayout.Space (space);

                        EditorGUI.BeginChangeCheck();
                        conn.joints.deformCount = EditorGUILayout.IntSlider ("Iterations", conn.joints.deformCount, 1, 100);
                        if (EditorGUI.EndChangeCheck() == true)
                            foreach (RayfireConnectivity scr in targets)
                            {
                                scr.joints.deformCount = conn.joints.deformCount;
                                SetDirty (scr);
                            }

                        if (conn.joints.HasDeforms == true)
                        {
                            GUILayout.Space (space);
                            GUILayout.Label ("Deformable joints: " + conn.joints.deformList.Count + "/" + conn.joints.jointList.Count);
                        }
                           
                    }

           
                    
                    // GUILayout.Label ("CurrentForce");
                    
                    EditorGUI.indentLevel--;
                }
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Draw
        /// /////////////////////////////////////////////////////////
        
        // Draw Connections and Nodes by act and uny states
        static void ClusterDraw(RFCluster cluster, bool showNodes, bool showConnections)
        {
            if (showNodes == true || showConnections == true)
            {
                if (cluster != null && cluster.shards.Count > 0)
                {
                    for (int i = 0; i < cluster.shards.Count; i++)
                    {
                        if (cluster.shards[i].tm != null)
                        {
                            // Color
                            if (cluster.shards[i].rigid == null)
                                SetColor (cluster.shards[i].uny, cluster.shards[i].act);
                            else 
                            {
                                if (cluster.shards[i].rigid.objectType == ObjectType.Mesh)
                                    SetColor (cluster.shards[i].rigid.activation.unyielding, cluster.shards[i].rigid.activation.activatable);
                                else if (cluster.shards[i].rigid.objectType == ObjectType.MeshRoot)
                                    SetColor (cluster.shards[i].uny, cluster.shards[i].act);
                            }

                            // Nodes
                            if (showNodes == true)
                                Gizmos.DrawWireSphere (cluster.shards[i].tm.position, cluster.shards[i].sz / 12f);
                            
                            // Connection
                            if (showConnections == true)
                            {
                                // Debug.Log (cluster.shards[i].nIds.Count);
                                
                                // Has no neibs
                                if (cluster.shards[i].nIds.Count == 0)
                                    continue;
                                
                                // Shard has neibs but neib shards not initialized by nIds
                                if (cluster.shards[i].neibShards == null)
                                    cluster.shards[i].neibShards = new List<RFShard>();
                                
                                // Reinit
                                if (cluster.shards[i].neibShards.Count == 0)
                                    for (int n = 0; n < cluster.shards[i].nIds.Count; n++)
                                        cluster.shards[i].neibShards.Add (cluster.shards[cluster.shards[i].nIds[n]]);
                                
                                // Preview
                                for (int j = 0; j < cluster.shards[i].neibShards.Count; j++)
                                    if (cluster.shards[i].neibShards[j].tm != null)
                                    {
                                        Gizmos.DrawLine (cluster.shards[i].tm.position, 
                                            (cluster.shards[i].neibShards[j].tm.position - cluster.shards[i].tm.position) / 2f + cluster.shards[i].tm.position);
                                    }
                            }
                        }
                    }
                }
            }
        }
        
        // Draw Connections and Nodes by act and uny states
        static void ClusterDraw(RayfireConnectivity targ)
        {
            if (targ.showNodes == true || targ.showConnections == true)
            {
                if (targ.cluster != null && targ.cluster.shards.Count > 0)
                {
                    for (int i = 0; i < targ.cluster.shards.Count; i++)
                    {
                        if (targ.cluster.shards[i].tm != null)
                        {
                            // Color
                            if (targ.cluster.shards[i].rigid == null)
                                SetColor (targ.cluster.shards[i].uny, targ.cluster.shards[i].act);
                            else 
                            {
                                if (targ.cluster.shards[i].rigid.objectType == ObjectType.Mesh)
                                    SetColor (targ.cluster.shards[i].rigid.activation.unyielding, targ.cluster.shards[i].rigid.activation.activatable);
                                else if (targ.cluster.shards[i].rigid.objectType == ObjectType.MeshRoot)
                                    SetColor (targ.cluster.shards[i].uny, targ.cluster.shards[i].act);
                            }

                            // Nodes
                            if (targ.showNodes == true)
                                Gizmos.DrawWireSphere (targ.cluster.shards[i].tm.position, targ.cluster.shards[i].sz / 12f);
                            
                            // Connection
                            if (targ.showConnections == true)
                            {
                                // has no neibs
                                if (targ.cluster.shards[i].nIds.Count == 0)
                                    continue;
                                
                                // Shard has neibs but neib shards not initialized by nIds
                                if (targ.cluster.shards[i].neibShards == null)
                                    targ.cluster.shards[i].neibShards = new List<RFShard>();
                                
                                // Reinit
                                if (targ.cluster.shards[i].neibShards.Count == 0)
                                    for (int n = 0; n < targ.cluster.shards[i].nIds.Count; n++)
                                        targ.cluster.shards[i].neibShards.Add (targ.cluster.shards[targ.cluster.shards[i].nIds[n]]);
                                
                                // Preview
                                for (int j = 0; j < targ.cluster.shards[i].neibShards.Count; j++)
                                    if (targ.cluster.shards[i].neibShards[j].tm != null)
                                    {
                                        Gizmos.DrawLine (targ.cluster.shards[i].tm.position, 
                                            (targ.cluster.shards[i].neibShards[j].tm.position - targ.cluster.shards[i].tm.position) / 2f + targ.cluster.shards[i].tm.position);
                                    }
                            }
                        }
                    }
                }
            }
        }
        
        // Draw stressed connections
        static void StressDraw (RayfireConnectivity targ)
        {
            if (targ.showStress == true && targ.stress != null && targ.stress.inProgress == true)
            {
                if (targ.cluster != null && targ.cluster.shards.Count > 0)
                {
                    Vector3 pos;
                    for (int i = 0; i < targ.cluster.shards.Count; i++)
                    {
                        if (targ.cluster.shards[i].tm != null)
                        {
                            // Show Path stress
                            /*
                            if (false)
                                if (targ.stress.bySize == true)
                                {
                                    Gizmos.color = ColorByValue (stressColor, targ.cluster.shards[i].sSt, 1f);
                                    Gizmos.DrawWireSphere (targ.cluster.shards[i].tm.position, targ.cluster.shards[i].sz / 12f);
                                }
                            */
                            
                            if (targ.cluster.shards[i].StressState == true)
                            {
                                for (int n = 0; n < targ.cluster.shards[i].nSt.Count / 3; n++)
                                {
                                    if (targ.cluster.shards[i].uny == true)
                                    {
                                        Gizmos.color = Color.yellow;
                                    }
                                    else
                                    {
                                        Gizmos.color = targ.cluster.shards[i].sIds.Count > 0 
                                            ? Color.yellow 
                                            : ColorByValue (stressColor, targ.cluster.shards[i].nSt[n * 3], targ.stress.threshold);
                                    }
                                    
                                    pos = (targ.cluster.shards[i].neibShards[n].tm.position - targ.cluster.shards[i].tm.position) / 2.5f + targ.cluster.shards[i].tm.position;
                                    Gizmos.DrawLine (targ.cluster.shards[i].tm.position, pos);
                                }
                            }
                        }
                    }
                }
            }
        }

        // Set gizmo color by uny and act states
        static void SetColor (bool uny, bool act)
        {
            if (uny == false)
                Gizmos.color = Color.green;
            else
                Gizmos.color = act == true ? Color.magenta : Color.red;
        }
        
        // Color by value
        static Color ColorByValue(Color color, float val, float threshold)
        {
            val     /= threshold;
            color.g =  1f - val;
            color.r =  val;
            return color;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////
        
        [DrawGizmo (GizmoType.Selected | GizmoType.NonSelected | GizmoType.Pickable)]
        static void DrawGizmosSelected (RayfireConnectivity targ, GizmoType gizmoType)
        {
            // Draw bounding gizmo
            GizmoDraw (targ);
            
            // Has no shards
            if (targ.cluster == null)
                return;
            
            /*
            // Missing shards
            // if (targ.cluster.shards.Count > 0)
            {
                if (RFCluster.IntegrityCheck (targ.cluster) == false)
                    Debug.Log ("RayFire Connectivity: " + targ.name + " has missing shards. Reset or Setup cluster.", targ.gameObject);
                else
                    targ.integrityCheck = false;
            }
            */
            
            // Draw for MeshRoot and RigidRoot in runtime
            if (Application.isPlaying == true || targ.meshRootHost != null)
            {
                ClusterDraw (targ.cluster, targ.showNodes, targ.showConnections);
            }

            // Draw for RigidRoot because Connectivity do not store same shard list
            else if (targ.rigidRootHost != null)
                ClusterDraw (targ.rigidRootHost.cluster, targ.showNodes, targ.showConnections);

            // Draw stresses connections
            StressDraw (targ);
        }

        static void GizmoDraw (RayfireConnectivity targ)
        {
            if (targ.showGizmo == true)
            {
                // Gizmo properties
                Gizmos.color = wireColor;
                if (targ.transform.childCount > 0)
                {
                    Bounds bound = RFCluster.GetChildrenBound (targ.transform);
                    Gizmos.DrawWireCube (bound.center, bound.size);
                }
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////
        
        void SetDirty (RayfireConnectivity scr)
        {
            if (Application.isPlaying == false)
            {
                EditorUtility.SetDirty (scr);
                EditorSceneManager.MarkSceneDirty (scr.gameObject.scene);
                SceneView.RepaintAll();
            }
        }
        
        void SetFoldoutPref (ref bool val, string pref, string caption) 
        {
            EditorGUI.BeginChangeCheck();
            val = EditorGUILayout.Foldout (val, caption, true);
            if (EditorGUI.EndChangeCheck() == true)
                EditorPrefs.SetBool (pref, val);
        }
    }
}

        /*
        
        /// /////////////////////////////////////////////////////////
        /// Handle selection
        /// /////////////////////////////////////////////////////////
        
        static         Vector2Int currentShardConnection;
		private static int        s_ButtonHash = "ConnectionHandle".GetHashCode();
        
        void OnSceneGUI()
		{
            var targ = conn;
			if (targ == null)
				return;
            
			if (targ.showConnections == true)
			{
				if (targ.cluster != null && targ.cluster.shards.Count > 0)
				{
					int count = targ.cluster.shards.Count;
					for (int i = 0; i < count; i++)
					{
						if (targ.cluster.shards[i].tm != null)
						{
							if (targ.cluster.shards[i].nIds.Count == 0)
								continue;

							if (targ.cluster.shards[i].neibShards != null && targ.cluster.shards[i].neibShards.Count != 0)
							{
								int nCount = targ.cluster.shards[i].neibShards.Count;
								for (int j = 0; j < nCount; j++)
								{
									if (targ.cluster.shards[i].neibShards[j].tm != null)
									{
										Vector3 start = targ.cluster.shards[i].tm.position;
										Vector3 end = start + (targ.cluster.shards[i].neibShards[j].tm.position - start) * 0.5f;
										HandleClick(start, end, targ.cluster.shards[i].id, targ.cluster.shards[i].neibShards[j].id);
                                        
                                        
									}
								}
							}
						}
					}
				}
			}
		}
        
		private static void HandleClick(Vector3 start, Vector3 end, int id1, int id2)
		{
			int id = GUIUtility.GetControlID(s_ButtonHash, FocusType.Passive);
			Event evt = Event.current;

			switch (evt.GetTypeForControl(id))
			{
				case EventType.Layout:
				{
					HandleUtility.AddControl(id, HandleUtility.DistanceToLine(start, end));
					break;
				}
                case EventType.MouseMove:
                {
                    if (id == HandleUtility.nearestControl)
                        HandleUtility.Repaint();
                    break;
                }
                case EventType.MouseDown:
				{
					if (HandleUtility.nearestControl == id && evt.button == 0)
					{
						GUIUtility.hotControl = id; // Grab mouse focus
						HandleClickSelection(evt, id1, id2);
						evt.Use();
					}
					break;
				}
			}
		}

		public static void HandleClickSelection(Event evt, int id1, int id2)
		{
			currentShardConnection.x = id1;
			currentShardConnection.y = id2;
            
            
		}
        
        private void DeleteSelectedConnection()
        {
            var targ = conn;
            if (targ.showConnections == true)
            {
                if (targ.cluster != null && targ.cluster.shards.Count > 0)
                {
                    int count = targ.cluster.shards.Count;
                    for (int i = 0; i < count; i++)
                    {
                        if (targ.cluster.shards[i].tm != null)
                        {
                            if (targ.cluster.shards[i].nIds.Count == 0)
                                continue;

                            if (targ.cluster.shards[i].neibShards != null && targ.cluster.shards[i].neibShards.Count != 0)
                            {
                                int nCount = targ.cluster.shards[i].neibShards.Count - 1;
                                for (int j = nCount; j >= 0; --j)
                                {
                                    if (targ.cluster.shards[i].neibShards[j].tm != null)
                                    {
                                        var id  = targ.cluster.shards[i].id;
                                        var nId = targ.cluster.shards[i].neibShards[j].id;
                                        if (currentShardConnection.x == id && currentShardConnection.y == nId || currentShardConnection.y == id && currentShardConnection.x == nId)
                                            targ.cluster.shards[i].RemoveNeibAt(j);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        
        */

/*
 if (targ.cluster.shards[i].uny == true)
    {
        Gizmos.color = Color.yellow;
    }
    else
    {
        //if (targ.cluster.shards[i].sIds.Count > 0)
        //{
            if (targ.cluster.shards[i].neibShards[n].sIds.Contains (targ.cluster.shards[i].id) == true || targ.cluster.shards[i].sIds.Contains (targ.cluster.shards[i].neibShards[n].id) == true)
            {
                Gizmos.color = Color.yellow;
            }
        //}
            else
                Gizmos.color     = ColorByValue (stressColor, targ.cluster.shards[i].nStr[n * 3], targ.stress.threshold);
    }




                                    if (targ.cluster.shards[i].uny == true || targ.cluster.shards[i].sIds.Count > 0)
                                        Gizmos.color = Color.yellow;
                                    else
                                        Gizmos.color = ColorByValue (stressColor, targ.cluster.shards[i].nStr[n*3], targ.stress.threshold);

*/