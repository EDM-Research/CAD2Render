using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;

namespace RayFire
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireShatter))]
    public class RayfireShatterEditor : Editor
    {
        RayfireShatter shat;
        Vector3        centerWorldPos;
        Quaternion     centerWorldQuat;
        
        
        SerializedProperty custTmProp;
        ReorderableList    custTmList; 
        SerializedProperty custPointProp;
        ReorderableList    custPointList;       
        SerializedProperty sliceTmProp;
        ReorderableList    sliceTmList;
        
        
        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////
        
        static int  space    = 3;
        static bool exp_deb;
        static bool exp_lim;
        static bool exp_fil;
        
        static GUIContent gui_tp = new GUIContent ("Type", "Defines fragmentation type for object.");
        
        static GUIContent gui_tp_vor          = new GUIContent ("      Voronoi",     "Low poly, convex, physics friendly fragments.");
        static GUIContent gui_tp_vor_amount   = new GUIContent ("Amount",            "Defines amount of points in point cloud, every point represent rough center of  fragment.");
        static GUIContent gui_tp_vor_bias     = new GUIContent ("Center Bias",       "Defines offset of points in point cloud towards Center.");
        static GUIContent gui_tp_spl          = new GUIContent ("      Splinters",   "Low poly, convex, physics friendly fragments, stretched along one axis.");
        static GUIContent gui_tp_spl_axis     = new GUIContent ("Axis",              "Fragments will be stretched over defined axis.");
        static GUIContent gui_tp_spl_str      = new GUIContent ("Strength",          "Defines sharpness of stretched fragments.");
        static GUIContent gui_tp_slb          = new GUIContent ("      Slabs",       "Low poly, convex, physics friendly fragments, stretched along two axes.");
        static GUIContent gui_tp_rad          = new GUIContent ("      Radial",      "Low poly, convex, physics friendly fragments, creates radial fragments pattern.");
        static GUIContent gui_tp_rad_axis     = new GUIContent ("Center Axis",       "");
        static GUIContent gui_tp_rad_radius   = new GUIContent ("Radius",            "");
        static GUIContent gui_tp_rad_div      = new GUIContent ("Divergence",        "");
        static GUIContent gui_tp_rad_rest     = new GUIContent ("Restrict To Plane", "");
        static GUIContent gui_tp_rad_rings    = new GUIContent ("Rings",             "");
        static GUIContent gui_tp_rad_focus    = new GUIContent ("Focus",             "");
        static GUIContent gui_tp_rad_str      = new GUIContent ("Focus Strength",    "");
        static GUIContent gui_tp_rad_randRing = new GUIContent ("Random Rings",      "");
        static GUIContent gui_tp_rad_rays     = new GUIContent ("Rays",              "");
        static GUIContent gui_tp_rad_randRay  = new GUIContent ("Random Rays",       "");
        static GUIContent gui_tp_rad_twist    = new GUIContent ("Twist",             "");
        static GUIContent gui_tp_cus          = new GUIContent ("      Custom",      "Low poly, convex, physics friendly fragments, allows to use custom point cloud for fragments distribution.");
        static GUIContent gui_tp_cus_src      = new GUIContent ("Source",            "");
        static GUIContent gui_tp_cus_use      = new GUIContent ("Use As",            "");
        static GUIContent gui_tp_cus_am       = new GUIContent ("Amount",            "");
        static GUIContent gui_tp_cus_rad      = new GUIContent ("Radius",            "");
        static GUIContent gui_tp_cus_en       = new GUIContent ("Enable",            "");
        static GUIContent gui_tp_cus_sz       = new GUIContent ("Size",              "");
        static GUIContent gui_tp_mir          = new GUIContent ("      Mirrored",    "Low poly, convex, physics friendly fragments, generate custom point cloud mirrored at the edges over defined axes.");
        static GUIContent gui_tp_slc          = new GUIContent ("      Slice",       "Slice object by planes.");
        static GUIContent gui_tp_slc_pl       = new GUIContent ("Plane",             "Slicing plane.");
        static GUIContent gui_tp_brk          = new GUIContent ("      Bricks",      "");
        static GUIContent gui_tp_brk_type     = new GUIContent ("Lattice",           "");
        static GUIContent gui_tp_brk_mult     = new GUIContent ("Multiplier",        "");
        static GUIContent gui_tp_brk_am_X     = new GUIContent ("X axis",            "");
        static GUIContent gui_tp_brk_am_Y     = new GUIContent ("Y axis",            "");
        static GUIContent gui_tp_brk_am_Z     = new GUIContent ("Z axis",            "");
        static GUIContent gui_tp_brk_lock     = new GUIContent ("Lock",              "");
        static GUIContent gui_tp_brk_sp_prob  = new GUIContent ("Probability",       "");
        static GUIContent gui_tp_brk_sp_offs  = new GUIContent ("Offset",            "");
        static GUIContent gui_tp_brk_sp_rot   = new GUIContent ("Rotation",          "");
        static GUIContent gui_tp_vxl          = new GUIContent ("      Voxels",      "");
        static GUIContent gui_tp_tet          = new GUIContent ("      Tets", "Tetrahedron based fragments, this type is mostly useless as is and should be used with Gluing, " +
                                                                          "in this case it creates high poly concave fragments.");
        static GUIContent gui_tp_tetDn      = new GUIContent ("Density",        "");
        static GUIContent gui_tp_tetNs      = new GUIContent ("Noise",          "");
        static GUIContent gui_pr_mode       = new GUIContent ("Mode",           "");
        static GUIContent gui_mat_in        = new GUIContent ("Inner Material", "Defines material for fragment's inner surface.");
        static GUIContent gui_mat_scl       = new GUIContent ("Mapping Scale",  "Defines mapping scale for inner surface.");
        static GUIContent gui_pr_exp        = new GUIContent ("  Export",       "Export fragments meshes to Unity Asset and reference to this asset.");
        static GUIContent gui_pr_exp_src    = new GUIContent ("Source",         "");
        static GUIContent gui_pr_exp_sfx    = new GUIContent ("Suffix",         "");
        
        static GUIContent gui_pr_cls        = new GUIContent ("  Clusters",     "Allows to glue groups of fragments into single mesh by deleting shared faces.");
        static GUIContent gui_pr_cls_en     = new GUIContent ("Enable",         "Allows to glue groups of fragments into single mesh by deleting shared faces.");
        static GUIContent gui_pr_cls_cnt    = new GUIContent ("Count",          "Amount of clusters defined by random point cloud.");
        static GUIContent gui_pr_cls_seed   = new GUIContent ("Seed",           "Random seed for clusters point cloud generator.");
        static GUIContent gui_pr_cls_rel    = new GUIContent ("Relax",          "Smooth strength for cluster inner surface.");
        static GUIContent gui_pr_cls_debris = new GUIContent ("Debris",   "Preserve some fragments at the edges of clusters to create small debris around big chunks.");
        static GUIContent gui_pr_cls_amount = new GUIContent ("Amount",         "Amount of debris in last layer in percents relative to amount of fragments in cluster.");
        static GUIContent gui_pr_cls_layers = new GUIContent ("Layers",         "Amount of debris layers at cluster border.");
        static GUIContent gui_pr_cls_scale  = new GUIContent ("Layers",         "Scale variation for inner debris.");
        static GUIContent gui_pr_cls_min    = new GUIContent ("Minimum",        "Minimum amount of fragments in debris cluster.");
        static GUIContent gui_pr_cls_max    = new GUIContent ("Maximum",        "Maximum amount of fragments in debris cluster.");
        
        static GUIContent gui_pr_adv_seed     = new GUIContent ("Seed",              "Seed for point cloud generator. Set to 0 to get random point cloud every time.");
        static GUIContent gui_pr_adv_dec      = new GUIContent ("Decompose",         "Check output fragments and separate not connected parts of meshes into separate fragments.");
        static GUIContent gui_pr_adv_col      = new GUIContent ("Collinear",         "Remove vertices which lay on straight edge.");
        static GUIContent gui_pr_adv_copy     = new GUIContent ("Copy",              "Copy components from original object to fragments");
        static GUIContent gui_pr_adv_smooth   = new GUIContent ("Smooth",            "Smooth fragments inner surface.");
        static GUIContent gui_pr_adv_combine  = new GUIContent ("Combine",           "Combine all children meshes into one mesh and fragment this mesh.");
        static GUIContent gui_pr_adv_input    = new GUIContent ("Input Precap",      "Create extra triangles to connect open edges and close mesh volume for correct fragmentation.");
        static GUIContent gui_pr_adv_output   = new GUIContent ("    Output Precap", "Keep fragment's faces created by Input Precap.");
        static GUIContent gui_pr_adv_remove   = new GUIContent ("Double Faces",      "Delete faces which overlap with each other.");
        static GUIContent gui_pr_adv_element  = new GUIContent ("Element Size",      "Input mesh will be separated to not connected mesh elements, every element will be fragmented separately." + "This threshold value measures in percentage relative to original objects size and prevent element from being fragmented if its size is less.");
        static GUIContent gui_pr_adv_inner    = new GUIContent ("Inner",             "Do not output inner fragments which has no outer surface.");
        static GUIContent gui_pr_adv_planar   = new GUIContent ("Planar",            "Do not output planar fragments which mesh vertices lie in the same plane.");
        static GUIContent gui_pr_adv_rel      = new GUIContent ("Relative Size",     "Do not output small fragments. Measures is percentage relative to original object size.");
        static GUIContent gui_pr_adv_abs      = new GUIContent ("Absolute Size",     "Do not output small fragments which size in world units is less than this value.");
        
        static GUIContent gui_pr_adv_size_lim = new GUIContent ("Size",   "All fragments with size bigger than Max Size value will be fragmented to few more fragments.");
        static GUIContent gui_pr_adv_size_am  = new GUIContent ("    Max Size",      "");
        static GUIContent gui_pr_adv_vert_lim = new GUIContent ("Vertex",   "All fragments with vertex amount higher than Max Amount value will be fragmented to few more fragments.");
        static GUIContent gui_pr_adv_vert_am  = new GUIContent ("    Max Amount",      "");
        static GUIContent gui_pr_adv_tri_lim  = new GUIContent ("Triangle", "All fragments with triangle amount higher than Max Amount value will be fragmented to few more fragments.");
        static GUIContent gui_pr_adv_tri_am   = new GUIContent ("    Max Amount",      "");
        
        /// /////////////////////////////////////////////////////////
        /// Enable
        /// /////////////////////////////////////////////////////////

        private void OnEnable()
        {
            custTmProp                     = serializedObject.FindProperty("custom.transforms");
            custTmList                     = new ReorderableList(serializedObject, custTmProp, true, true, true, true);
            custTmList.drawElementCallback = DrawCustTmListItems;
            custTmList.drawHeaderCallback  = DrawCustTmHeader;
            custTmList.onAddCallback       = AddCustTm;
            custTmList.onRemoveCallback    = RemoveCustTm;

            custPointProp                     = serializedObject.FindProperty("custom.vector3");
            custPointList                     = new ReorderableList(serializedObject, custPointProp, true, true, true, true);
            custPointList.drawElementCallback = DrawCustPointListItems;
            custPointList.drawHeaderCallback  = DrawCustPointHeader;
            custPointList.onAddCallback       = AddCustPoint;
            custPointList.onRemoveCallback    = RemoveCustPoint;
            
            sliceTmProp                     = serializedObject.FindProperty("slice.sliceList");
            sliceTmList                     = new ReorderableList(serializedObject, sliceTmProp, true, true, true, true);
            sliceTmList.drawElementCallback = DrawSliceTmListItems;
            sliceTmList.drawHeaderCallback  = DrawSliceTmHeader;
            sliceTmList.onAddCallback       = AddSliceTm;
            sliceTmList.onRemoveCallback    = RemoveSliceTm;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Inspector
        /// /////////////////////////////////////////////////////////
        
        public override void OnInspectorGUI()
        {
            shat = target as RayfireShatter;
            if (shat == null)
                return;

            // Get inspector width
            // float width = EditorGUIUtility.currentViewWidth - 20f;

            // Space
            GUILayout.Space (8);

            UI_Fragment();
            UI_Preview();
            
            // Reset scale if fragments were deleted
            shat.ResetScale (shat.previewScale);
            
            UI_Types();

            GUILayout.Space (space);
            
            UI_Material();
            
            GUILayout.Space (space);

            UI_Cluster();
            
            GUILayout.Space (space);
            
            UI_Advanced();
            
            GUILayout.Space (space);

            UI_Export();
            
            GUILayout.Space (space);

            UI_Collider();
            
            GUILayout.Space (space);
            
            UI_Center();
            
            GUILayout.Space (space);
            
            InfoUI();
            
            GUILayout.Space (8);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Types
        /// /////////////////////////////////////////////////////////
        
        void UI_Types()
        {
            GUILayout.Label ("  Fragments", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            shat.type = (FragType)EditorGUILayout.EnumPopup (gui_tp, shat.type);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.type = shat.type;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.indentLevel++;
            
            if (shat.type == FragType.Voronoi)
                UI_Type_Voronoi();
            else if (shat.type == FragType.Splinters)
                UI_Type_Splinters();
            else if (shat.type == FragType.Slabs)
                UI_Type_Slabs();
            else if (shat.type == FragType.Radial)
                UI_Type_Radial();
            else if (shat.type == FragType.Custom)
                UI_Type_Custom();            
            else if (shat.type == FragType.Slices)
                UI_Type_Slices();      
            else if (shat.type == FragType.Bricks)
                UI_Type_Bricks();  
            else if (shat.type == FragType.Voxels)
                UI_Type_Voxels();  
            else if (shat.type == FragType.Tets)
                UI_Type_Tets();
            
            EditorGUI.indentLevel--;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Voronoi
        /// /////////////////////////////////////////////////////////

        void UI_Type_Voronoi()
        {
            GUILayout.Label (gui_tp_vor, EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            shat.voronoi.amount = EditorGUILayout.IntField (gui_tp_vor_amount, shat.voronoi.amount);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.voronoi.amount = shat.voronoi.amount;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            shat.voronoi.centerBias = EditorGUILayout.Slider (gui_tp_vor_bias, shat.voronoi.centerBias, 0f, 1f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.voronoi.centerBias = shat.voronoi.centerBias;
                    SetDirty (scr);
                }
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Splinters
        /// /////////////////////////////////////////////////////////
        
        void UI_Type_Splinters()
        {
            GUILayout.Label (gui_tp_spl, EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            shat.splinters.axis = (AxisType)EditorGUILayout.EnumPopup (gui_tp_spl_axis, shat.splinters.axis);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.splinters.axis = shat.splinters.axis;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            shat.splinters.amount = EditorGUILayout.IntField (gui_tp_vor_amount, shat.splinters.amount);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.splinters.amount = shat.splinters.amount;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            shat.splinters.strength = EditorGUILayout.Slider (gui_tp_spl_str, shat.splinters.strength, 0f, 1f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.splinters.strength = shat.splinters.strength;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            shat.splinters.centerBias = EditorGUILayout.Slider (gui_tp_vor_bias, shat.splinters.centerBias, 0f, 1f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.splinters.centerBias = shat.splinters.centerBias;
                    SetDirty (scr);
                }
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Slabs
        /// /////////////////////////////////////////////////////////
        
        void UI_Type_Slabs()
        {
            GUILayout.Label (gui_tp_slb, EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            shat.slabs.axis = (AxisType)EditorGUILayout.EnumPopup (gui_tp_spl_axis, shat.slabs.axis);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.slabs.axis = shat.slabs.axis;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            shat.slabs.amount = EditorGUILayout.IntField (gui_tp_vor_amount, shat.slabs.amount);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.slabs.amount = shat.slabs.amount;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            shat.slabs.strength = EditorGUILayout.Slider (gui_tp_spl_str, shat.slabs.strength, 0f, 1f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.slabs.strength = shat.slabs.strength;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            shat.slabs.centerBias = EditorGUILayout.Slider (gui_tp_vor_bias, shat.slabs.centerBias, 0f, 1f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.slabs.centerBias = shat.slabs.centerBias;
                    SetDirty (scr);
                }
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Radial
        /// /////////////////////////////////////////////////////////
        
        void UI_Type_Radial()
        {
            GUILayout.Label (gui_tp_rad, EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            shat.radial.centerAxis = (AxisType)EditorGUILayout.EnumPopup (gui_tp_rad_axis, shat.radial.centerAxis);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.radial.centerAxis = shat.radial.centerAxis;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            shat.radial.radius = EditorGUILayout.Slider (gui_tp_rad_radius, shat.radial.radius, 0.01f, 30f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.radial.radius = shat.radial.radius;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            shat.radial.divergence = EditorGUILayout.Slider (gui_tp_rad_div, shat.radial.divergence, 0f, 1f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.radial.divergence = shat.radial.divergence;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);

            if (shat.radial.divergence > 0)
            {
                EditorGUI.BeginChangeCheck();
                //EditorGUI.indentLevel++;
                shat.radial.restrictToPlane = EditorGUILayout.Toggle (gui_tp_rad_rest, shat.radial.restrictToPlane);
                //EditorGUI.indentLevel--;
                if (EditorGUI.EndChangeCheck())
                {
                    foreach (RayfireShatter scr in targets)
                    {
                        scr.radial.restrictToPlane = shat.radial.restrictToPlane;
                        SetDirty (scr);
                    }
                }
            }

            GUILayout.Space (space);
            
            GUILayout.Label ("      Rings", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            shat.radial.rings = EditorGUILayout.IntSlider (gui_tp_rad_rings, shat.radial.rings, 3, 60);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.radial.rings = shat.radial.rings;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            shat.radial.randomRings = EditorGUILayout.IntSlider (gui_tp_rad_randRing, shat.radial.randomRings, 0, 100);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.radial.randomRings = shat.radial.randomRings;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            shat.radial.focusStr = EditorGUILayout.IntSlider (gui_tp_rad_str, shat.radial.focusStr, 0, 100);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.radial.focusStr = shat.radial.focusStr;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            shat.radial.focus = EditorGUILayout.IntSlider (gui_tp_rad_focus, shat.radial.focus, 0, 100);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.radial.focus = shat.radial.focus;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            GUILayout.Label ("      Rays", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            shat.radial.rays = EditorGUILayout.IntSlider (gui_tp_rad_rays, shat.radial.rays, 3, 60);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.radial.rays = shat.radial.rays;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            shat.radial.randomRays = EditorGUILayout.IntSlider (gui_tp_rad_randRay, shat.radial.randomRays, 0, 100);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.radial.randomRays = shat.radial.randomRays;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            shat.radial.twist = EditorGUILayout.IntSlider (gui_tp_rad_twist, shat.radial.twist, -90, 90);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.radial.twist = shat.radial.twist;
                    SetDirty (scr);
                }
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Custom
        /// /////////////////////////////////////////////////////////
        
        void UI_Type_Custom()
        {
            GUILayout.Label (gui_tp_cus, EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            shat.custom.source = (RFCustom.RFPointCloudSourceType)EditorGUILayout.EnumPopup (gui_tp_cus_src, shat.custom.source);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.custom.source = shat.custom.source;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            shat.custom.useAs = (RFCustom.RFPointCloudUseType)EditorGUILayout.EnumPopup (gui_tp_cus_use, shat.custom.useAs);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.custom.useAs = shat.custom.useAs;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            if (shat.custom.source == RFCustom.RFPointCloudSourceType.TransformList)
            {
                GUILayout.Label ("      List", EditorStyles.boldLabel);
                
                serializedObject.Update();
                custTmList.DoLayoutList();
                serializedObject.ApplyModifiedProperties();
            }

            if (shat.custom.source == RFCustom.RFPointCloudSourceType.Vector3List)
            {
                GUILayout.Label ("      List", EditorStyles.boldLabel);
                
                serializedObject.Update();
                custPointList.DoLayoutList();
                serializedObject.ApplyModifiedProperties();
            }
            
            GUILayout.Space (space);

            if (shat.custom.useAs == RFCustom.RFPointCloudUseType.VolumePoints)
            {
                GUILayout.Label ("      Volume", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                shat.custom.amount = EditorGUILayout.IntSlider (gui_tp_cus_am, shat.custom.amount, 3, 999);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireShatter scr in targets)
                    {
                        scr.custom.amount = shat.custom.amount;
                        SetDirty (scr);
                    }
                }

                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                shat.custom.radius = EditorGUILayout.Slider (gui_tp_cus_rad, shat.custom.radius, 0.01f, 4f);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireShatter scr in targets)
                    {
                        scr.custom.radius = shat.custom.radius;
                        SetDirty (scr);
                    }
                }

                if (shat.custom.inBoundPoints.Count > 0)
                {
                    GUILayout.Space (space);
                    GUILayout.Label ("    In/Out points: " + shat.custom.inBoundPoints.Count + "/" + shat.custom.outBoundPoints.Count);
                }
            }

            GUILayout.Label ("      Preview", EditorStyles.boldLabel);
                
            EditorGUI.BeginChangeCheck();
            shat.custom.enable = EditorGUILayout.Toggle (gui_tp_cus_en, shat.custom.enable);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.custom.enable = shat.custom.enable;
                    SetDirty (scr);
                }
            }

            if (shat.custom.enable == true)
            {
                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                shat.custom.size = EditorGUILayout.Slider (gui_tp_cus_sz, shat.custom.size, 0.01f, 0.4f);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireShatter scr in targets)
                    {
                        scr.custom.size = shat.custom.size;
                        SetDirty (scr);
                    }
                }
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Slices
        /// /////////////////////////////////////////////////////////
        
        void UI_Type_Slices()
        {
            GUILayout.Label (gui_tp_slc, EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            shat.slice.plane = (PlaneType)EditorGUILayout.EnumPopup (gui_tp_slc_pl, shat.slice.plane);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.slice.plane = shat.slice.plane;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            serializedObject.Update();
            sliceTmList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Bricks
        /// /////////////////////////////////////////////////////////
        
        void UI_Type_Bricks()
        {
            GUILayout.Label (gui_tp_brk, EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            shat.bricks.amountType = (RFBricks.RFBrickType)EditorGUILayout.EnumPopup (gui_tp_brk_type, shat.bricks.amountType);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.bricks.amountType = shat.bricks.amountType;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            shat.bricks.mult = EditorGUILayout.Slider (gui_tp_brk_mult, shat.bricks.mult, 0.1f, 10);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.bricks.mult = shat.bricks.mult;
                    SetDirty (scr);
                }
            }
            
            if (shat.bricks.amountType == RFBricks.RFBrickType.ByAmount)
                UI_Type_Bricks_Amount();
            else
                UI_Type_Bricks_Size();

            GUILayout.Space (space);
            
            UI_Type_Bricks_Size_Variation();

            GUILayout.Space (space);

            UI_Type_Bricks_Offset();

            GUILayout.Space (space);
            
            UI_Type_Bricks_Split();
        }
        
        void UI_Type_Bricks_Amount()
        {
            GUILayout.Label ("      Amount", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            shat.bricks.amount_X = EditorGUILayout.IntSlider (gui_tp_brk_am_X, shat.bricks.amount_X, 0, 50);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.bricks.amount_X = shat.bricks.amount_X;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            shat.bricks.amount_Y = EditorGUILayout.IntSlider (gui_tp_brk_am_Y, shat.bricks.amount_Y, 0, 50);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.bricks.amount_Y = shat.bricks.amount_Y;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            shat.bricks.amount_Z = EditorGUILayout.IntSlider (gui_tp_brk_am_Z, shat.bricks.amount_Z, 0, 50);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.bricks.amount_Z = shat.bricks.amount_Z;
                    SetDirty (scr);
                }
            }
        }
        
        void UI_Type_Bricks_Size()
        {
            GUILayout.Label ("      Size", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            shat.bricks.size_X = EditorGUILayout.Slider (gui_tp_brk_am_X, shat.bricks.size_X, 0.01f, 10);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.bricks.size_X = shat.bricks.size_X;
                    if (shat.bricks.size_Lock == true)
                    {
                        scr.bricks.size_Z = shat.bricks.size_X;
                        scr.bricks.size_Y = shat.bricks.size_X;
                    }
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            shat.bricks.size_Y = EditorGUILayout.Slider (gui_tp_brk_am_Y, shat.bricks.size_Y, 0.01f, 10);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.bricks.size_Y = shat.bricks.size_Y;
                    if (shat.bricks.size_Lock == true)
                    {
                        scr.bricks.size_X = shat.bricks.size_Y;
                        scr.bricks.size_Z = shat.bricks.size_Y;
                    }
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            shat.bricks.size_Z = EditorGUILayout.Slider (gui_tp_brk_am_Z, shat.bricks.size_Z, 0.01f, 10);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.bricks.size_Z = shat.bricks.size_Z;
                    if (shat.bricks.size_Lock == true)
                    {
                        scr.bricks.size_X = shat.bricks.size_Z;
                        scr.bricks.size_Y = shat.bricks.size_Z;
                    }
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            shat.bricks.size_Lock = EditorGUILayout.Toggle (gui_tp_brk_lock, shat.bricks.size_Lock);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.bricks.size_Lock = shat.bricks.size_Lock;
                    SetDirty (scr);
                }
            }
        }
        
        void UI_Type_Bricks_Size_Variation()
        {
            GUILayout.Label ("      Size Variation", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            shat.bricks.sizeVar_X = EditorGUILayout.IntSlider (gui_tp_brk_am_X, shat.bricks.sizeVar_X, 0, 100);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.bricks.sizeVar_X = shat.bricks.sizeVar_X;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            shat.bricks.sizeVar_Y = EditorGUILayout.IntSlider (gui_tp_brk_am_Y, shat.bricks.sizeVar_Y, 0, 100);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.bricks.sizeVar_Y = shat.bricks.sizeVar_Y;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            shat.bricks.sizeVar_Z = EditorGUILayout.IntSlider (gui_tp_brk_am_Z, shat.bricks.sizeVar_Z, 0, 100);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.bricks.sizeVar_Z = shat.bricks.sizeVar_Z;
                    SetDirty (scr);
                }
            }
        }
        
        void UI_Type_Bricks_Offset()
        {
            GUILayout.Label ("      Offset", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            shat.bricks.offset_X = EditorGUILayout.Slider (gui_tp_brk_am_X, shat.bricks.offset_X, 0f, 1f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.bricks.offset_X = shat.bricks.offset_X;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            shat.bricks.offset_Y = EditorGUILayout.Slider (gui_tp_brk_am_Y, shat.bricks.offset_Y, 0f, 1f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.bricks.offset_Y = shat.bricks.offset_Y;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            shat.bricks.offset_Z = EditorGUILayout.Slider (gui_tp_brk_am_Z, shat.bricks.offset_Z, 0f, 1f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.bricks.offset_Z = shat.bricks.offset_Z;
                    SetDirty (scr);
                }
            }
        }

        void UI_Type_Bricks_Split()
        {
            GUILayout.Label ("      Split", EditorStyles.boldLabel);
            
            UI_Type_Bricks_Split_Axes();
            
            EditorGUI.BeginChangeCheck();
            shat.bricks.split_probability = EditorGUILayout.IntSlider (gui_tp_brk_sp_prob, shat.bricks.split_probability, 0, 100);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.bricks.split_probability = shat.bricks.split_probability;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);

            EditorGUI.BeginChangeCheck();
            shat.bricks.split_rotation = EditorGUILayout.IntSlider (gui_tp_brk_sp_rot, shat.bricks.split_rotation, 0, 90);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.bricks.split_rotation = shat.bricks.split_rotation;
                    SetDirty (scr);
                }
            }

            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            shat.bricks.split_offset = EditorGUILayout.Slider (gui_tp_brk_sp_offs, shat.bricks.split_offset, 0f, 0.95f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.bricks.split_offset = shat.bricks.split_offset;
                    SetDirty (scr);
                }
            }
        }

        void UI_Type_Bricks_Split_Axes()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PrefixLabel ("X Y Z");
            
            EditorGUI.BeginChangeCheck();
            shat.bricks.split_X = EditorGUILayout.Toggle ("", shat.bricks.split_X, GUILayout.Width (40));
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.bricks.split_X = shat.bricks.split_X;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            shat.bricks.split_Y = EditorGUILayout.Toggle ("", shat.bricks.split_Y, GUILayout.Width (40));
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.bricks.split_Y = shat.bricks.split_Y;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            shat.bricks.split_Z = EditorGUILayout.Toggle ("", shat.bricks.split_Z, GUILayout.Width (40));
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.bricks.split_Z = shat.bricks.split_Z;
                    SetDirty (scr);
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }

        /// /////////////////////////////////////////////////////////
        /// Voxels
        /// /////////////////////////////////////////////////////////

        void UI_Type_Voxels()
        {
            GUILayout.Label (gui_tp_vxl, EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            shat.voxels.size = EditorGUILayout.Slider (gui_tp_cus_sz, shat.voxels.size, 0.05f, 10);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.voxels.size = shat.voxels.size;
                    SetDirty (scr);
                }
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Tets
        /// /////////////////////////////////////////////////////////
        
        void UI_Type_Tets()
        {
            GUILayout.Label (gui_tp_tet, EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            shat.tets.density = EditorGUILayout.IntSlider (gui_tp_tetDn, shat.tets.density, 1, 50);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.tets.density = shat.tets.density;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            shat.tets.noise = EditorGUILayout.IntSlider (gui_tp_tetNs, shat.tets.noise, 0, 100);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.tets.noise = shat.tets.noise;
                    SetDirty (scr);
                }
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Properties
        /// /////////////////////////////////////////////////////////
        
        void UI_Material()
        {
            // Not for decompose
            if (shat.type == FragType.Decompose )
                return;
            
            GUILayout.Label ("  Material", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            shat.material.innerMaterial = (Material)EditorGUILayout.ObjectField (gui_mat_in, shat.material.innerMaterial, typeof(Material), true);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.material.innerMaterial = shat.material.innerMaterial;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space); 
            
            EditorGUI.BeginChangeCheck();
            shat.material.mappingScale = EditorGUILayout.Slider (gui_mat_scl, shat.material.mappingScale, 0.01f, 2f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.material.mappingScale = shat.material.mappingScale;
                    SetDirty (scr);
                }
            }
        }
        
        void UI_Cluster()
        {
            // Not for bricks, slices and decompose
            if (shat.type == FragType.Bricks || shat.type == FragType.Decompose || shat.type == FragType.Voxels 
                || shat.type == FragType.Slices)
                return;
            
            GUILayout.Label (gui_pr_cls, EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            shat.clusters.enable = EditorGUILayout.Toggle (gui_pr_cls_en, shat.clusters.enable);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.clusters.enable = shat.clusters.enable;
                    SetDirty (scr);
                }
            }
            
            if (shat.clusters.enable == true)
            {
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                shat.clusters.count = EditorGUILayout.IntSlider (gui_pr_cls_cnt, shat.clusters.count, 2, 200);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireShatter scr in targets)
                    {
                        scr.clusters.count = shat.clusters.count;
                        SetDirty (scr);
                    }
                }
            
                GUILayout.Space (space);
            
                EditorGUI.BeginChangeCheck();
                shat.clusters.seed = EditorGUILayout.IntSlider (gui_pr_cls_seed, shat.clusters.seed, 0, 100);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireShatter scr in targets)
                    {
                        scr.clusters.seed = shat.clusters.seed;
                        SetDirty (scr);
                    }
                }
                
                GUILayout.Space (space);
            
                EditorGUI.BeginChangeCheck();
                shat.clusters.relax = EditorGUILayout.Slider (gui_pr_cls_rel, shat.clusters.relax, 0f, 1f);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireShatter scr in targets)
                    {
                        scr.clusters.relax = shat.clusters.relax;
                        SetDirty (scr);
                    }
                }
                
                GUILayout.Space (space);
                
                exp_deb = EditorGUILayout.Foldout (exp_deb, gui_pr_cls_debris, true);
                if (exp_deb == true)
                {
                    GUILayout.Space (space);

                    EditorGUI.indentLevel++;

                    EditorGUI.BeginChangeCheck();
                    shat.clusters.amount = EditorGUILayout.IntSlider (gui_pr_cls_amount, shat.clusters.amount, 0, 100);
                    if (EditorGUI.EndChangeCheck() == true)
                    {
                        foreach (RayfireShatter scr in targets)
                        {
                            scr.clusters.amount = shat.clusters.amount;
                            SetDirty (scr);
                        }
                    }

                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    shat.clusters.layers = EditorGUILayout.IntSlider (gui_pr_cls_layers, shat.clusters.layers, 0, 5);
                    if (EditorGUI.EndChangeCheck() == true)
                    {
                        foreach (RayfireShatter scr in targets)
                        {
                            scr.clusters.layers = shat.clusters.layers;
                            SetDirty (scr);
                        }
                    }

                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    shat.clusters.scale = EditorGUILayout.Slider (gui_pr_cls_scale, shat.clusters.scale, 0.1f, 1f);
                    if (EditorGUI.EndChangeCheck() == true)
                    {
                        foreach (RayfireShatter scr in targets)
                        {
                            scr.clusters.scale = shat.clusters.scale;
                            SetDirty (scr);
                        }
                    }

                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    shat.clusters.min = EditorGUILayout.IntSlider (gui_pr_cls_min, shat.clusters.min, 1, 20);
                    if (EditorGUI.EndChangeCheck() == true)
                    {
                        foreach (RayfireShatter scr in targets)
                        {
                            scr.clusters.min = shat.clusters.min;
                            SetDirty (scr);
                        }
                    }

                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    shat.clusters.max = EditorGUILayout.IntSlider (gui_pr_cls_max, shat.clusters.max, 1, 20);
                    if (EditorGUI.EndChangeCheck() == true)
                    {
                        foreach (RayfireShatter scr in targets)
                        {
                            scr.clusters.max = shat.clusters.max;
                            SetDirty (scr);
                        }
                    }

                    EditorGUI.indentLevel--;
                }
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Advanced
        /// /////////////////////////////////////////////////////////
        
        void UI_Advanced()
        {
            UI_Advanced_Properties();
            
            GUILayout.Space (space);
            
            UI_Advanced_Filters();
            
            GUILayout.Space (space);
            
            if (shat.mode == FragmentMode.Editor)
                UI_Advanced_Editor();
        }
        
        void UI_Advanced_Properties()
        {
            GUILayout.Label ("  Properties", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            shat.mode = (FragmentMode)EditorGUILayout.EnumPopup (gui_pr_mode, shat.mode);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.mode = shat.mode;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            shat.advanced.seed = EditorGUILayout.IntSlider (gui_pr_adv_seed, shat.advanced.seed, 0, 100);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.advanced.seed = shat.advanced.seed;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            shat.advanced.copyComponents = EditorGUILayout.Toggle (gui_pr_adv_copy, shat.advanced.copyComponents);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.advanced.copyComponents = shat.advanced.copyComponents;
                    SetDirty (scr);
                }
            } 
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            shat.advanced.smooth = EditorGUILayout.Toggle (gui_pr_adv_smooth, shat.advanced.smooth);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.advanced.smooth = shat.advanced.smooth;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);

            EditorGUI.BeginChangeCheck();
            shat.advanced.combineChildren = EditorGUILayout.Toggle (gui_pr_adv_combine, shat.advanced.combineChildren);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.advanced.combineChildren = shat.advanced.combineChildren;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            shat.advanced.removeCollinear = EditorGUILayout.Toggle (gui_pr_adv_col, shat.advanced.removeCollinear);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.advanced.removeCollinear = shat.advanced.removeCollinear;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            shat.advanced.decompose = EditorGUILayout.Toggle (gui_pr_adv_dec, shat.advanced.decompose);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.advanced.decompose = shat.advanced.decompose;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            shat.advanced.inputPrecap = EditorGUILayout.Toggle (gui_pr_adv_input, shat.advanced.inputPrecap);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.advanced.inputPrecap = shat.advanced.inputPrecap;
                    SetDirty (scr);
                }
            }

            if (shat.advanced.inputPrecap == true)
            {
                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                shat.advanced.outputPrecap = EditorGUILayout.Toggle (gui_pr_adv_output, shat.advanced.outputPrecap);
                if (EditorGUI.EndChangeCheck())
                {
                    foreach (RayfireShatter scr in targets)
                    {
                        scr.advanced.outputPrecap = shat.advanced.outputPrecap;
                        SetDirty (scr);
                    }
                }
            }
            
            GUILayout.Space (space);

            UI_Advanced_Limits();
        }
        
        void UI_Advanced_Limits()
        {
            exp_lim = EditorGUILayout.Foldout (exp_lim, "Limitations", true);
            if (exp_lim == true)
            {
                GUILayout.Space (space);

                EditorGUI.indentLevel++;

                EditorGUI.BeginChangeCheck();
                shat.advanced.sizeLimitation = EditorGUILayout.Toggle (gui_pr_adv_size_lim, shat.advanced.sizeLimitation);
                if (EditorGUI.EndChangeCheck())
                {
                    foreach (RayfireShatter scr in targets)
                    {
                        scr.advanced.sizeLimitation = shat.advanced.sizeLimitation;
                        SetDirty (scr);
                    }
                }

                if (shat.advanced.sizeLimitation == true)
                {
                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    shat.advanced.sizeAmount = EditorGUILayout.Slider (gui_pr_adv_size_am, shat.advanced.sizeAmount, 0.1f, 100f);
                    if (EditorGUI.EndChangeCheck() == true)
                    {
                        foreach (RayfireShatter scr in targets)
                        {
                            scr.advanced.sizeAmount = shat.advanced.sizeAmount;
                            SetDirty (scr);
                        }
                    }
                }

                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                shat.advanced.vertexLimitation = EditorGUILayout.Toggle (gui_pr_adv_vert_lim, shat.advanced.vertexLimitation);
                if (EditorGUI.EndChangeCheck())
                {
                    foreach (RayfireShatter scr in targets)
                    {
                        scr.advanced.vertexLimitation = shat.advanced.vertexLimitation;
                        SetDirty (scr);
                    }
                }

                if (shat.advanced.vertexLimitation == true)
                {
                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    shat.advanced.vertexAmount = EditorGUILayout.IntSlider (gui_pr_adv_vert_am, shat.advanced.vertexAmount, 100, 1900);
                    if (EditorGUI.EndChangeCheck() == true)
                    {
                        foreach (RayfireShatter scr in targets)
                        {
                            scr.advanced.vertexAmount = shat.advanced.vertexAmount;
                            SetDirty (scr);
                        }
                    }
                }

                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                shat.advanced.triangleLimitation = EditorGUILayout.Toggle (gui_pr_adv_tri_lim, shat.advanced.triangleLimitation);
                if (EditorGUI.EndChangeCheck())
                {
                    foreach (RayfireShatter scr in targets)
                    {
                        scr.advanced.triangleLimitation = shat.advanced.triangleLimitation;
                        SetDirty (scr);
                    }
                }

                if (shat.advanced.triangleLimitation == true)
                {
                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    shat.advanced.triangleAmount = EditorGUILayout.IntSlider (gui_pr_adv_tri_am, shat.advanced.triangleAmount, 100, 1900);
                    if (EditorGUI.EndChangeCheck() == true)
                    {
                        foreach (RayfireShatter scr in targets)
                        {
                            scr.advanced.triangleAmount = shat.advanced.triangleAmount;
                            SetDirty (scr);
                        }
                    }
                }
                
                EditorGUI.indentLevel--;
            }
        }
        
        void UI_Advanced_Editor()
        {
            GUILayout.Label ("  Editor", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            shat.advanced.elementSizeThreshold = EditorGUILayout.IntSlider (gui_pr_adv_element, shat.advanced.elementSizeThreshold, 1, 100);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.advanced.seed = shat.advanced.seed;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            shat.advanced.removeDoubleFaces = EditorGUILayout.Toggle (gui_pr_adv_remove, shat.advanced.removeDoubleFaces);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireShatter scr in targets)
                {
                    scr.advanced.removeDoubleFaces = shat.advanced.removeDoubleFaces;
                    SetDirty (scr);
                }
            }
        }

        void UI_Advanced_Filters()
        {
            exp_fil = EditorGUILayout.Foldout (exp_fil, "Filters", true);
            if (exp_fil == true)
            {
                GUILayout.Space (space);

                EditorGUI.indentLevel++;
                
                EditorGUI.BeginChangeCheck();
                shat.advanced.inner = EditorGUILayout.Toggle (gui_pr_adv_inner, shat.advanced.inner);
                if (EditorGUI.EndChangeCheck())
                {
                    foreach (RayfireShatter scr in targets)
                    {
                        scr.advanced.inner = shat.advanced.inner;
                        SetDirty (scr);
                    }
                }

                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                shat.advanced.planar = EditorGUILayout.Toggle (gui_pr_adv_planar, shat.advanced.planar);
                if (EditorGUI.EndChangeCheck())
                {
                    foreach (RayfireShatter scr in targets)
                    {
                        scr.advanced.planar = shat.advanced.planar;
                        SetDirty (scr);
                    }
                }

                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                shat.advanced.relativeSize = EditorGUILayout.IntSlider (gui_pr_adv_rel, shat.advanced.relativeSize, 0, 10);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireShatter scr in targets)
                    {
                        scr.advanced.relativeSize = shat.advanced.relativeSize;
                        SetDirty (scr);
                    }
                }

                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                shat.advanced.absoluteSize = EditorGUILayout.Slider (gui_pr_adv_abs, shat.advanced.absoluteSize, 0, 1f);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireShatter scr in targets)
                    {
                        scr.advanced.absoluteSize = shat.advanced.absoluteSize;
                        SetDirty (scr);
                    }
                }
                
                EditorGUI.indentLevel--;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Fragment / delete
        /// /////////////////////////////////////////////////////////
        
        void UI_Fragment()
        {
            if (GUILayout.Button ("Fragment", GUILayout.Height (25)))
            {
                foreach (var targ in targets)
                    if (targ as RayfireShatter != null)
                    {
                        (targ as RayfireShatter).Fragment();

                        // TODO APPLY LOCAL SHATTER PREVIEW PROPS TO ALL SELECTED
                    }

                // Scale preview if preview turn on
                if (shat.previewScale > 0 && shat.scalePreview == true)
                    ScalePreview (shat);
            }
            
            GUILayout.Space (1);
            
            GUILayout.BeginHorizontal();

            // Delete last
            if (shat.fragmentsLast.Count > 0) // TODO SUPPORT MASS CHECK
            {
                if (GUILayout.Button ("Fragment to Last", GUILayout.Height (22)))
                {
                    foreach (var targ in targets)
                        if (targ as RayfireShatter != null)
                        {
                            (targ as RayfireShatter).DeleteFragmentsLast (1);
                            (targ as RayfireShatter).resetState = true;
                            (targ as RayfireShatter).Fragment (RayfireShatter.FragLastMode.ToLast);

                            // Scale preview if preview turn on
                            if ((targ as RayfireShatter).previewScale > 0 && (targ as RayfireShatter).scalePreview == true)
                                ScalePreview (targ as RayfireShatter);
                        }
                }

                if (GUILayout.Button ("    Delete Last    ", GUILayout.Height (22)))
                {
                    foreach (var targ in targets)
                        if (targ as RayfireShatter != null)
                        {
                            (targ as RayfireShatter).DeleteFragmentsLast();
                            (targ as RayfireShatter).resetState = true;
                            (targ as RayfireShatter).ResetScale (0f);
                        }
                }
            }

            // Delete all fragments
            if (shat.fragmentsAll.Count > 0 && shat.fragmentsAll.Count > shat.fragmentsLast.Count)
            {
                if (GUILayout.Button (" Delete All ", GUILayout.Height (22)))
                {
                    foreach (var targ in targets)
                        if (targ as RayfireShatter != null)
                        {
                            (targ as RayfireShatter).DeleteFragmentsAll();
                            (targ as RayfireShatter).resetState = true;
                            (targ as RayfireShatter).ResetScale (0f);
                        }
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Info
        /// /////////////////////////////////////////////////////////

        // Info
        void InfoUI()
        {
            if (shat.fragmentsLast.Count > 0 || shat.fragmentsAll.Count > 0)
            {
                GUILayout.Label ("  Info", EditorStyles.boldLabel);

                GUILayout.BeginHorizontal();

                GUILayout.Label ("Roots: " + shat.rootChildList.Count);
                GUILayout.Label ("Last Fragments: " + shat.fragmentsLast.Count);
                GUILayout.Label ("Total Fragments: " + shat.fragmentsAll.Count);

                EditorGUILayout.EndHorizontal();
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Export
        /// /////////////////////////////////////////////////////////
        
        void UI_Export ()
        {
            if (CanExport() == true)
            {
                GUILayout.Label (gui_pr_exp, EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                shat.export.source = (RFMeshExport.MeshExportType)EditorGUILayout.EnumPopup (gui_pr_exp_src, shat.export.source);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireShatter scr in targets)
                    {
                        scr.export.source = shat.export.source;
                        SetDirty (scr);
                    }
                }
                
                if (HasToExport() == true)
                {
                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    shat.export.suffix = EditorGUILayout.TextField (gui_pr_exp_sfx, shat.export.suffix);
                    if (EditorGUI.EndChangeCheck() == true)
                    {
                        foreach (RayfireShatter scr in targets)
                        {
                            scr.export.suffix = shat.export.suffix;
                            SetDirty (scr);
                        }
                    }
                }

                GUILayout.Space (space);

                // Export Last fragments
                if (shat.export.source == RFMeshExport.MeshExportType.LastFragments && shat.fragmentsLast.Count > 0)
                    if (GUILayout.Button ("Export Last Fragments", GUILayout.Height (25)))
                        RFMeshAsset.SaveFragments (shat, RFMeshAsset.shatterPath);

                // Export children
                if (shat.export.source == RFMeshExport.MeshExportType.Children && shat.transform.childCount > 0)
                    if (GUILayout.Button ("Export Children", GUILayout.Height (25)))
                        RFMeshAsset.SaveFragments (shat, RFMeshAsset.shatterPath);
            }
        }

        bool CanExport()
        {
            if (shat.fragmentsLast.Count > 0 || shat.transform.childCount > 0)
                return true;
            return false;
        }
        
        bool HasToExport()
        {
            if (shat.export.source == RFMeshExport.MeshExportType.LastFragments && shat.fragmentsLast.Count > 0)
                return true;
            if (shat.export.source == RFMeshExport.MeshExportType.Children && shat.transform.childCount > 0)
                return true;
            return false;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Center
        /// /////////////////////////////////////////////////////////

        // Center
        void UI_Center()
        {
            if ((int)shat.type <= 5)
            {
                GUILayout.Label ("  Center", EditorStyles.boldLabel);

                GUILayout.BeginHorizontal();
                shat.showCenter = GUILayout.Toggle (shat.showCenter, " Show   ", "Button");
                if (GUILayout.Button ("Reset "))
                {
                    foreach (var targ in targets)
                        if (targ as RayfireShatter != null)
                        {
                            (targ as RayfireShatter).ResetCenter();
                            SetDirty (targ as RayfireShatter);
                        }
                    SceneView.RepaintAll();
                }

                EditorGUILayout.EndHorizontal();
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Preview
        /// /////////////////////////////////////////////////////////

        // Preview UI
        void UI_Preview()
        {
            // Preview
            if (shat.fragmentsLast.Count == 0)
                return;
            
            GUILayout.Label ("  Preview", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
            
            // Start check for scale toggle change
            EditorGUI.BeginChangeCheck();
            shat.scalePreview = GUILayout.Toggle (shat.scalePreview, "Scale", "Button");
            if (EditorGUI.EndChangeCheck() == true)
            {
                if (shat.scalePreview == true)
                    ScalePreview (shat);
                else
                {
                    shat.resetState = true;
                    shat.ResetScale (0f);
                }
                SetDirty (shat);
            }
            
            // Color preview toggle
            shat.colorPreview = GUILayout.Toggle (shat.colorPreview, "Color", "Button");

            EditorGUILayout.EndHorizontal();

            GUILayout.Space (3);

            GUILayout.BeginHorizontal();

            GUILayout.Label ("Scale Preview", GUILayout.Width (90));

            // Start check for slider change
            EditorGUI.BeginChangeCheck();
            shat.previewScale = GUILayout.HorizontalSlider (shat.previewScale, 0f, 0.99f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                if (shat.scalePreview == true)
                    ScalePreview (shat);
                SetDirty (shat);
            }

            EditorGUILayout.EndHorizontal();
        }
        
        // Color preview
        static void ColorPreview (RayfireShatter scr)
        {
            if (scr.fragmentsLast.Count > 0)
            {
                Random.InitState (1);
                foreach (Transform root in scr.rootChildList)
                {
                    if (root != null)
                    {
                        MeshFilter[] meshFilters = root.GetComponentsInChildren<MeshFilter>();
                        foreach (var mf in meshFilters)
                        {
                            Gizmos.color = new Color (Random.Range (0.2f, 0.8f), Random.Range (0.2f, 0.8f), Random.Range (0.2f, 0.8f));
                            Gizmos.DrawMesh (mf.sharedMesh, mf.transform.position, mf.transform.rotation, mf.transform.lossyScale * 1.01f);
                        }
                    }
                }
            }
        }

        // Scale fragments
        static void ScalePreview (RayfireShatter scr)
        {
            if (scr.fragmentsLast.Count > 0 && scr.previewScale > 0f)
            {
                // Do not scale
                if (scr.skinnedMeshRend != null)
                    scr.skinnedMeshRend.enabled = false;
                if (scr.meshRenderer != null)
                    scr.meshRenderer.enabled = false;

                foreach (GameObject fragment in scr.fragmentsLast)
                    if (fragment != null)
                        fragment.transform.localScale = Vector3.one * Mathf.Lerp (1f, 0.3f, scr.previewScale);
                scr.resetState = true;
            }

            if (scr.previewScale == 0f)
            {
                scr.ResetScale (0f);
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Colliders
        /// /////////////////////////////////////////////////////////
        
        // Collider UI
        void UI_Collider()
        {
            if (shat.fragmentsLast.Count == 0)
                return;
            
            GUILayout.Label ("  Colliders", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button ("Add Mesh Colliders"))
            {
                foreach (var targ in targets)
                    if (targ as RayfireShatter != null)
                        AddColliders (targ as RayfireShatter);
                SceneView.RepaintAll();
            }
            
            if (GUILayout.Button (" Remove Colliders "))
            {
                foreach (var targ in targets)
                    if (targ as RayfireShatter != null)
                        RemoveColliders (targ as RayfireShatter);
                SceneView.RepaintAll();
            }

            EditorGUILayout.EndHorizontal();
        }
        
        // Add collider
        static void AddColliders (RayfireShatter scr)
        {
            if (scr.fragmentsLast.Count > 0)
            {
                foreach (var frag in scr.fragmentsLast)
                {
                    MeshCollider mc = frag.GetComponent<MeshCollider>();
                    if (mc == null)
                        mc = frag.AddComponent<MeshCollider>();
                    mc.convex = true;
                }
            }
        }
        
        // Remove collider
        static void RemoveColliders (RayfireShatter scr)
        {
            if (scr.fragmentsLast.Count > 0)
            {
                foreach (var frag in scr.fragmentsLast)
                {
                    MeshCollider mc = frag.gameObject.GetComponent<MeshCollider>();
                    if (mc != null)
                        DestroyImmediate (mc);
                }
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Draw
        /// /////////////////////////////////////////////////////////
        
        [DrawGizmo (GizmoType.Selected | GizmoType.NonSelected | GizmoType.Pickable)]
        static void DrawGizmosSelected (RayfireShatter shatter, GizmoType gizmoType)
        {
            // Color preview
            if (shatter.colorPreview == true)
                ColorPreview (shatter);

            // Custom point cloud preview
            if (shatter.type == FragType.Custom)
            {
                if (shatter.custom.enable == true)
                {
                    // Get bounds for preview
                    Bounds bound = shatter.GetBound();
                    if (bound.size.magnitude > 0)
                    {
                        // Collect point cloud
                        RFCustom.GetCustomPointCLoud (shatter.custom, shatter.transform, shatter.advanced.seed, bound);

                        // In bound points
                        if (shatter.custom.inBoundPoints != null && shatter.custom.inBoundPoints.Count > 0)
                        {
                            Gizmos.color = Color.green;
                            for (int i = 0; i < shatter.custom.inBoundPoints.Count; i++)
                                Gizmos.DrawSphere (shatter.custom.inBoundPoints[i], shatter.custom.size);
                        }

                        // Outbound points
                        if (shatter.custom.outBoundPoints != null && shatter.custom.outBoundPoints.Count > 0)
                        {
                            Gizmos.color = Color.red;
                            for (int i = 0; i < shatter.custom.outBoundPoints.Count; i++)
                                Gizmos.DrawSphere (shatter.custom.outBoundPoints[i], shatter.custom.size / 2f);
                        }
                    }
                }
            }
        }

        // Show center move handle
        private void OnSceneGUI()
        {
            // Get shatter
            shat = target as RayfireShatter;
            if (shat == null)
                return;

            Transform transform = shat.transform;
            centerWorldPos  = transform.TransformPoint (shat.centerPosition);
            centerWorldQuat = transform.rotation * shat.centerDirection;

            // Point3 handle
            if (shat.showCenter == true)
            {
                EditorGUI.BeginChangeCheck();
                centerWorldPos = Handles.PositionHandle (centerWorldPos, centerWorldQuat.RFNormalize());
                if (EditorGUI.EndChangeCheck() == true)
                {
                    Undo.RecordObject (shat, "Center Move");
                    SetDirty (shat);
                }

                EditorGUI.BeginChangeCheck();
                centerWorldQuat = Handles.RotationHandle (centerWorldQuat, centerWorldPos);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    Undo.RecordObject (shat, "Center Rotate");
                    SetDirty (shat);
                }
            }

            shat.centerDirection = Quaternion.Inverse (transform.rotation) * centerWorldQuat;
            shat.centerPosition  = transform.InverseTransformPoint (centerWorldPos);
        }

        /// /////////////////////////////////////////////////////////
        /// ReorderableList Custom Transform
        /// /////////////////////////////////////////////////////////
        
        void DrawCustTmListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = custTmList.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y+2, EditorGUIUtility.currentViewWidth - 80f, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        }
        
        void DrawCustTmHeader(Rect rect)
        {
            rect.x += 10;
            EditorGUI.LabelField(rect, "Transform List");
        }

        void AddCustTm(ReorderableList list)
        {
            if (shat.custom.transforms == null)
                shat.custom.transforms = new List<Transform>();
            shat.custom.transforms.Add (null);
            list.index = list.count;
        }
        
        void RemoveCustTm(ReorderableList list)
        {
            if (shat.custom.transforms != null)
            {
                shat.custom.transforms.RemoveAt (list.index);
                list.index = list.index - 1;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// ReorderableList Custom Point 3
        /// /////////////////////////////////////////////////////////
        
        void DrawCustPointListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = custPointList.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y+2, EditorGUIUtility.currentViewWidth - 80f, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        }
        
        void DrawCustPointHeader(Rect rect)
        {
            rect.x += 10;
            EditorGUI.LabelField(rect, "Vector3 List");
        }

        void AddCustPoint(ReorderableList list)
        {
            if (shat.custom.vector3 == null)
                shat.custom.vector3 = new List<Vector3>();
            shat.custom.vector3.Add (Vector3.zero);
            list.index = list.count;
        }
        
        void RemoveCustPoint(ReorderableList list)
        {
            if (shat.custom.vector3 != null)
            {
                shat.custom.vector3.RemoveAt (list.index);
                list.index = list.index - 1;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// ReorderableList Slice Transform
        /// /////////////////////////////////////////////////////////
        
        void DrawSliceTmListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = sliceTmList.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y+2, EditorGUIUtility.currentViewWidth - 80f, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        }
        
        void DrawSliceTmHeader(Rect rect)
        {
            rect.x += 10;
            EditorGUI.LabelField(rect, "Transform List");
        }

        void AddSliceTm(ReorderableList list)
        {
            if (shat.slice.sliceList == null)
                shat.slice.sliceList = new List<Transform>();
            shat.slice.sliceList.Add (null);
            list.index = list.count;
        }
        
        void RemoveSliceTm(ReorderableList list)
        {
            if (shat.slice.sliceList != null)
            {
                shat.slice.sliceList.RemoveAt (list.index);
                list.index = list.index - 1;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////
        
        // Set dirty
        void SetDirty (RayfireShatter scr)
        {
            if (Application.isPlaying == false)
            {
                EditorUtility.SetDirty (scr);
                EditorSceneManager.MarkSceneDirty (scr.gameObject.scene);
                SceneView.RepaintAll();
            }
        }
        
    }
    
    // Normalize quat in order to support Unity 2018.1
    public static class RFQuaternionExtension
    {
        public static Quaternion RFNormalize (this Quaternion q)
        {
            float f = 1f / Mathf.Sqrt (q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
            return new Quaternion (q.x * f, q.y * f, q.z * f, q.w * f);
        }
    }
}

/*
public class ExampleClass: EditorWindow
{
    GameObject gameObject;
    Editor     gameObjectEditor;

    [MenuItem("Example/GameObject Editor")]
    static void ShowWindow()
    {
        GetWindowWithRect<ExampleClass>(new Rect(0, 0, 256, 256));
    }

    void OnGUI()
    {
        gameObject = (GameObject) EditorGUILayout.ObjectField(gameObject, typeof(GameObject), true);

        GUIStyle bgColor = new GUIStyle();
        bgColor.normal.background = EditorGUIUtility.whiteTexture;

        if (gameObject != null)
        {
            if (gameObjectEditor == null)
                gameObjectEditor = Editor.CreateEditor(gameObject);

            gameObjectEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(256, 256), bgColor);
        }
    }
}


[CustomPreview(typeof(GameObject))]
public class MyPreview : ObjectPreview
{
    public override bool HasPreviewGUI()
    {
        return true;
    }

    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        GUI.Label(r, target.name + " is being previewed");
    }
}
*/