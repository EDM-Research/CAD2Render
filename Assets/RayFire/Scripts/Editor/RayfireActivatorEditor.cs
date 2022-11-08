using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;

namespace RayFire
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireActivator))]
    public class RayfireActivatorEditor : Editor
    {
        RayfireActivator activator;
        BoxBoundsHandle  m_BoundsHandle = new BoxBoundsHandle();
        
        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////
        
        static Color wireColor = new Color (0.58f, 0.77f, 1f);
        static Color sphColor  = new Color (1.0f,  0.60f, 0f);
        static int   space     = 3;
        static bool  expand;
        
        static GUIContent gui_gizmoShow      = new GUIContent ("Show",          "");
        static GUIContent gui_gizmoType      = new GUIContent ("Type:",         "Gizmo which will be used to create collider to activate objects.");
        static GUIContent gui_gizmoSphere    = new GUIContent ("Radius", "Defines size of Sphere gizmo.");
        static GUIContent gui_gizmoBox       = new GUIContent ("Size",      "Defines size of Box gizmo.");
        static GUIContent gui_compRigid      = new GUIContent ("Rigid",         "Activate objects with Rigid component with Inactive or Kinematic simulation type.");
        static GUIContent gui_compRoot       = new GUIContent ("RigidRoot",     "Activate RigidRoot component objects with Inactive or Kinematic simulation type.");
        static GUIContent gui_activationType = new GUIContent ("Type:",          " On Enter: Object will be activated when Activator trigger collider will enter object's collider.\n" +
                                                                                 " On Exit: Object will be activated when Activator trigger collider will exit object's collider.");
        static GUIContent gui_activationDelay    = new GUIContent ("Delay",            "Activation Delay in seconds.");
        static GUIContent gui_activationDemolish = new GUIContent ("Demolish Cluster", "Allows to demolish Connected Cluster and detach it's children into separate objects.");
        static GUIContent gui_forceApply         = new GUIContent ("Apply",            "Add velocity and spin to activated objects");
        static GUIContent gui_forceVelocity      = new GUIContent ("Velocity",         "Applied Velocity in world coordinates.");
        static GUIContent gui_forceSpin          = new GUIContent ("Spin",             "Applied Angular Velocity in world coordinates.");
        static GUIContent gui_forceMode          = new GUIContent ("Mode",             "");
        static GUIContent gui_forceCoord         = new GUIContent ("Local Space",      "");
        static GUIContent gui_animShow           = new GUIContent ("Show Animation",   "Show animation properties.");
        static GUIContent gui_animDuration       = new GUIContent ("Duration",         "Total animation duration.");
        static GUIContent gui_animScale          = new GUIContent ("Scale Animation",  "Animate scale of Activator gizmo.");
        static GUIContent gui_animPosition = new GUIContent ("Position Animation", " By Global Position List: Use Position list of Vector3 points. Object will be animated from one point to another starting from the first point in global world coordinates.\n" +
                                                                                   " By Static Line: Use predefined Line. Path will be cached at start. \n" +
                                                                                   " By Dynamic Line: Use predefined Line. Path will be calculated at every frame by Line. \n" +
                                                                                   " By Local Position List: Use Position list of Vector3 points. Object will be animated from one point to another starting from the first point in local coordinates.");
        static GUIContent gui_animLine = new GUIContent ("Line",          "Line which will be used as animation path.");
        static GUIContent gui_animList = new GUIContent ("Position List", "List of Vector3 points in global space. Object will be animated from one point to another starting from the first point in list.");
        
        /// /////////////////////////////////////////////////////////
        /// Inspector
        /// /////////////////////////////////////////////////////////
        
        public override void OnInspectorGUI()
        {
            activator = (RayfireActivator)target;
            if (activator == null)
                return;
            
            GUILayout.Space (8);

            UI_Components();
            
            GUILayout.Space (space);
            
            UI_Gizmo();
            
            GUILayout.Space (space);

            UI_Activation();

            GUILayout.Space (space);
            
            UI_Force();
            
            GUILayout.Space (space);
            
            UI_Animation();
            
            GUILayout.Space (8);
        }

        /// /////////////////////////////////////////////////////////
        /// Components
        /// /////////////////////////////////////////////////////////
        
        void UI_Components()
        {
            GUILayout.Label ("  Components", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
            
            EditorGUI.BeginChangeCheck();
            activator.checkRigid     = GUILayout.Toggle (activator.checkRigid,     gui_compRigid, "Button", GUILayout.Height (22));
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireActivator scr in targets)
                {
                    scr.checkRigid     = activator.checkRigid;
                    SetDirty (scr);
                }

            EditorGUI.BeginChangeCheck();
            activator.checkRigidRoot = GUILayout.Toggle (activator.checkRigidRoot, gui_compRoot,  "Button", GUILayout.Height (22));
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireActivator scr in targets)
                {
                    scr.checkRigidRoot = activator.checkRigidRoot;
                    SetDirty (scr);
                }

            GUILayout.EndHorizontal();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Gizmo
        /// /////////////////////////////////////////////////////////

        void UI_Gizmo()
        {
            GUILayout.Label ("  Gizmo", EditorStyles.boldLabel);
            
            UI_GizmoType();
            
            GUILayout.Space (space);

            UI_GizmoProperties();
            
            if (activator.gizmoType == RayfireActivator.GizmoType.Box ||
                activator.gizmoType == RayfireActivator.GizmoType.Sphere)
            {
                GUILayout.Space (space);
                UI_GizmoShow();
            }
        }
        
        void UI_GizmoShow()
        {
            EditorGUI.BeginChangeCheck();
            activator.showGizmo = EditorGUILayout.Toggle (gui_gizmoShow, activator.showGizmo);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireActivator scr in targets)
                {
                    scr.showGizmo = activator.showGizmo;
                    SetDirty (scr);
                }
        }
        
        void UI_GizmoType()
        {
            EditorGUI.BeginChangeCheck();
            activator.gizmoType = (RayfireActivator.GizmoType)EditorGUILayout.EnumPopup (gui_gizmoType, activator.gizmoType);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireActivator scr in targets)
                {
                    scr.gizmoType = activator.gizmoType;
                    SetDirty (scr);
                }

                // Runtime size change
                if (Application.isPlaying == true)
                    activator.SetGizmoType (activator.gizmoType);
            }
        }
        
        void UI_GizmoProperties()
        {
            EditorGUI.BeginChangeCheck();
            if (activator.gizmoType == RayfireActivator.GizmoType.Sphere)
                activator.sphereRadius = EditorGUILayout.Slider (gui_gizmoSphere, activator.sphereRadius, 0.1f, 100f);
            else if (activator.gizmoType == RayfireActivator.GizmoType.Box)
                activator.boxSize = EditorGUILayout.Vector3Field (gui_gizmoBox, activator.boxSize);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireActivator script in targets)
                {
                    script.sphereRadius = activator.sphereRadius;
                    script.boxSize      = activator.boxSize;
                    SetDirty (script);
                }

                // Runtime size change
                if (Application.isPlaying == true && activator.activatorCollider != null)
                {
                    if (activator.gizmoType == RayfireActivator.GizmoType.Sphere)
                        (activator.activatorCollider as SphereCollider).radius = activator.sphereRadius;
                    else if (activator.gizmoType == RayfireActivator.GizmoType.Box)
                        (activator.activatorCollider as BoxCollider).size = activator.boxSize;
                }
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Activation
        /// /////////////////////////////////////////////////////////
        
        void UI_Activation()
        {
            GUILayout.Label ("  Activation", EditorStyles.boldLabel);

            // Enter Exit not supported by particle system
            if (activator.gizmoType != RayfireActivator.GizmoType.ParticleSystem)
            {
                UI_ActivationType();

                GUILayout.Space (space);
            }

            UI_ActivationDelay();

            GUILayout.Space (space);

            UI_ActivationDemolish();
        }
        
        void UI_ActivationType()
        {
            EditorGUI.BeginChangeCheck();
            activator.type = (RayfireActivator.ActivationType)EditorGUILayout.EnumPopup (gui_activationType, activator.type);
            if (EditorGUI.EndChangeCheck() == true)
                foreach (RayfireActivator scr in targets)
                {
                    scr.type = activator.type;
                    SetDirty (scr);
                }
        }
        
        void UI_ActivationDelay()
        {
            EditorGUI.BeginChangeCheck();
            activator.delay = EditorGUILayout.Slider (gui_activationDelay, activator.delay, 0f, 100f);
            if (EditorGUI.EndChangeCheck() == true)
                foreach (RayfireActivator scr in targets)
                {
                    scr.delay = activator.delay;
                    SetDirty (scr);
                }
        }
        
        void UI_ActivationDemolish()
        {
            EditorGUI.BeginChangeCheck();
            activator.demolishCluster = EditorGUILayout.Toggle (gui_activationDemolish, activator.demolishCluster);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireActivator scr in targets)
                {
                    scr.demolishCluster = activator.demolishCluster;
                    SetDirty (scr);
                }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Force
        /// /////////////////////////////////////////////////////////
        
        void UI_Force()
        {
            GUILayout.Label ("  Force", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            activator.apply = EditorGUILayout.Toggle (gui_forceApply, activator.apply);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireActivator scr in targets)
                {
                    scr.apply = activator.apply;
                    SetDirty (scr);
                }

            if (activator.apply == true)
            {
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                activator.coord = EditorGUILayout.Toggle (gui_forceCoord, activator.coord);
                if (EditorGUI.EndChangeCheck())
                    foreach (RayfireActivator scr in targets)
                    {
                        scr.coord = activator.coord;
                        SetDirty (scr);
                    }
                
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                activator.velocity = EditorGUILayout.Vector3Field (gui_forceVelocity, activator.velocity);
                if (EditorGUI.EndChangeCheck())
                    foreach (RayfireActivator scr in targets)
                    {
                        scr.velocity = activator.velocity;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);
            
                EditorGUI.BeginChangeCheck();
                activator.spin = EditorGUILayout.Vector3Field (gui_forceSpin, activator.spin);
                if (EditorGUI.EndChangeCheck())
                    foreach (RayfireActivator scr in targets)
                    {
                        scr.spin = activator.spin;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                activator.mode = (ForceMode)EditorGUILayout.EnumPopup (gui_forceMode, activator.mode);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireActivator scr in targets)
                    {
                        scr.mode = activator.mode;
                        SetDirty (scr);
                    }
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Animation
        /// /////////////////////////////////////////////////////////

        void UI_Animation()
        {
            GUILayout.Label ("  Animation", EditorStyles.boldLabel);

            UI_AnimationShow();

            if (activator.showAnimation)
            {
                GUILayout.Space (space);

                UI_AnimationDurationScale();

                GUILayout.Space (space);

                UI_AnimationType();

                GUILayout.Space (space);

                UI_AnimationLine();
                UI_AnimationList();
                UI_AnimationAddRemoveClear();
                UI_AnimationPlay();
            }
        }
        
        void UI_AnimationShow()
        {
            EditorGUI.BeginChangeCheck();
            activator.showAnimation = EditorGUILayout.Toggle (gui_animShow, activator.showAnimation);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireActivator script in targets)
                {
                    script.showAnimation = activator.showAnimation;
                    SetDirty (script);
                }
        }
        
        void UI_AnimationDurationScale()
        {
            EditorGUI.BeginChangeCheck();
            activator.duration       = EditorGUILayout.Slider (gui_animDuration, activator.duration, 0.1f, 100f);
            if (EditorGUI.EndChangeCheck() == true)
                foreach (RayfireActivator scr in targets)
                {
                    scr.duration       = activator.duration;
                    SetDirty (scr);
                }

            GUILayout.Space (space);
           
            EditorGUI.BeginChangeCheck();
            activator.scaleAnimation = EditorGUILayout.Slider (gui_animScale, activator.scaleAnimation, 1f, 50f);
            if (EditorGUI.EndChangeCheck() == true)
                foreach (RayfireActivator scr in targets)
                {
                    scr.scaleAnimation = activator.scaleAnimation;
                    SetDirty (scr);
                }
        }
        
        void UI_AnimationType()
        {
            EditorGUI.BeginChangeCheck();
            activator.positionAnimation = (RayfireActivator.AnimationType)EditorGUILayout.EnumPopup (gui_animPosition, activator.positionAnimation);
            if (EditorGUI.EndChangeCheck() == true)
                SetDirty (activator);
        }
        
        void UI_AnimationLine()
        {
            if (activator.ByLine == true)
            {
                EditorGUI.BeginChangeCheck();
                activator.line = (LineRenderer)EditorGUILayout.ObjectField (gui_animLine, activator.line, typeof(LineRenderer), true);
                if (EditorGUI.EndChangeCheck() == true)
                    SetDirty (activator);
            }
        }
        
        void UI_AnimationList()
        {
            if (activator.ByPositions == true)
            {
                EditorGUI.BeginChangeCheck();
                expand = EditorGUILayout.Foldout (expand, gui_animList, true);
                if (expand == true && activator.positionList != null && activator.positionList.Count > 0)
                    for (int i = 0; i < activator.positionList.Count; i++)
                    {
                        activator.positionList[i] = EditorGUILayout.Vector3Field ("  " + (i+1).ToString(), activator.positionList[i]); 
                        GUILayout.Space (space);
                    }

                if (EditorGUI.EndChangeCheck() == true)
                    SetDirty (activator);
            }
        }
        
        void UI_AnimationAddRemoveClear()
        {
            if (Application.isPlaying == false)
            {
                if (activator.positionAnimation == RayfireActivator.AnimationType.ByGlobalPositionList ||
                    activator.positionAnimation == RayfireActivator.AnimationType.ByLocalPositionList)
                {
                    GUILayout.Space (space);
                    EditorGUI.BeginChangeCheck();
                    GUILayout.BeginHorizontal();

                    if (GUILayout.Button ("Add", GUILayout.Height (25)))
                        activator.AddPosition (activator.transform.position);

                    if (GUILayout.Button ("Remove", GUILayout.Height (25)))
                        if (activator.positionList.Count > 0)
                            activator.positionList.RemoveAt (activator.positionList.Count - 1);

                    if (GUILayout.Button ("Clear", GUILayout.Height (25)))
                        activator.positionList.Clear();

                    EditorGUILayout.EndHorizontal();

                    if (EditorGUI.EndChangeCheck() == true)
                        SetDirty (activator);
                }
            }
        }
        
        void UI_AnimationPlay()
        {
            if (Application.isPlaying == true)
            {
                GUILayout.Space (space);
                
                GUILayout.Label ("  Playback", EditorStyles.boldLabel);
                
                GUILayout.BeginHorizontal();
                
                if (GUILayout.Button ("Start", GUILayout.Height (25)))
                    foreach (var targ in targets)
                        if (targ as RayfireActivator != null)
                            (targ as RayfireActivator).TriggerAnimation();
                
                if (GUILayout.Button ("Stop", GUILayout.Height (25)))
                    foreach (var targ in targets)
                        if (targ as RayfireActivator != null)
                            (targ as RayfireActivator).StopAnimation();
                
                if (GUILayout.Button ("Reset", GUILayout.Height (25)))
                    foreach (var targ in targets)
                        if (targ as RayfireActivator != null)
                            (targ as RayfireActivator).ResetAnimation();
                
                EditorGUILayout.EndHorizontal();
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Draw
        /// /////////////////////////////////////////////////////////
        
        [DrawGizmo (GizmoType.Selected | GizmoType.NonSelected | GizmoType.Pickable)]
        static void DrawGizmosSelected (RayfireActivator targ, GizmoType gizmoType)
        {
            if (targ.enabled && targ.showGizmo == true)
                DrawGizmo (targ);
        }
        
        void OnSceneGUI()
        {
            activator = target as RayfireActivator;
            if (activator == null)
                return;

            if (activator.enabled == true && activator.showGizmo == true)
            {
                if (activator.gizmoType == RayfireActivator.GizmoType.Sphere)
                {
                    var transform = activator.transform;

                    // Draw handles
                    EditorGUI.BeginChangeCheck();
                    activator.sphereRadius = Handles.RadiusHandle (transform.rotation, transform.position, activator.sphereRadius, true);
                    if (EditorGUI.EndChangeCheck() == true)
                    {
                        // TODO change sphere collider size

                        SetDirty (activator);
                        Undo.RecordObject (activator, "Change Radius");
                    }
                }

                if (activator.gizmoType == RayfireActivator.GizmoType.Box)
                {
                    Handles.matrix                = activator.transform.localToWorldMatrix;
                    m_BoundsHandle.wireframeColor = wireColor;
                    m_BoundsHandle.center         = Vector3.zero;
                    m_BoundsHandle.size           = activator.boxSize;

                    // draw the handle
                    EditorGUI.BeginChangeCheck();
                    m_BoundsHandle.DrawHandle();
                    if (EditorGUI.EndChangeCheck())
                    {
                        SetDirty (activator);
                        Undo.RecordObject (activator, "Change Bounds");
                        activator.boxSize = m_BoundsHandle.size;
                    }
                }
            }
        }
        
        static void DrawGizmo (RayfireActivator targ)
        {
            // Gizmo properties
            Gizmos.color  = wireColor;
            Gizmos.matrix = targ.transform.localToWorldMatrix;

            // Box gizmo
            if (targ.gizmoType == RayfireActivator.GizmoType.Box)
                Gizmos.DrawWireCube (Vector3.zero, targ.boxSize);

            // Sphere gizmo
            if (targ.gizmoType == RayfireActivator.GizmoType.Sphere)
            {
                // Vars
                int   size   = 45;
                float scale  = 1f / size;
                float radius = targ.sphereRadius;

                Vector3 previousPoint = Vector3.zero;
                Vector3 nextPoint     = Vector3.zero;

                // Draw top eye
                float rate      = 0f;
                nextPoint.y     = 0f;
                previousPoint.y = 0f;
                previousPoint.x = radius * Mathf.Cos (rate);
                previousPoint.z = radius * Mathf.Sin (rate);
                for (int i = 0; i < size; i++)
                {
                    rate        += 2.0f * Mathf.PI * scale;
                    nextPoint.x =  radius * Mathf.Cos (rate);
                    nextPoint.z =  radius * Mathf.Sin (rate);
                    Gizmos.DrawLine (previousPoint, nextPoint);
                    previousPoint = nextPoint;
                }

                // Draw top eye
                rate            = 0f;
                nextPoint.x     = 0f;
                previousPoint.x = 0f;
                previousPoint.y = radius * Mathf.Cos (rate);
                previousPoint.z = radius * Mathf.Sin (rate);
                for (int i = 0; i < size; i++)
                {
                    rate        += 2.0f * Mathf.PI * scale;
                    nextPoint.y =  radius * Mathf.Cos (rate);
                    nextPoint.z =  radius * Mathf.Sin (rate);
                    Gizmos.DrawLine (previousPoint, nextPoint);
                    previousPoint = nextPoint;
                }

                // Draw top eye
                rate            = 0f;
                nextPoint.z     = 0f;
                previousPoint.z = 0f;
                previousPoint.y = radius * Mathf.Cos (rate);
                previousPoint.x = radius * Mathf.Sin (rate);
                for (int i = 0; i < size; i++)
                {
                    rate        += 2.0f * Mathf.PI * scale;
                    nextPoint.y =  radius * Mathf.Cos (rate);
                    nextPoint.x =  radius * Mathf.Sin (rate);
                    Gizmos.DrawLine (previousPoint, nextPoint);
                    previousPoint = nextPoint;
                }

                // Selectable sphere
                float sphereSize = radius * 0.07f;
                if (sphereSize < 0.1f)
                    sphereSize = 0.1f;
                Gizmos.color = sphColor;
                Gizmos.DrawSphere (new Vector3 (0f,      radius,  0f),      sphereSize);
                Gizmos.DrawSphere (new Vector3 (0f,      -radius, 0f),      sphereSize);
                Gizmos.DrawSphere (new Vector3 (radius,  0f,      0f),      sphereSize);
                Gizmos.DrawSphere (new Vector3 (-radius, 0f,      0f),      sphereSize);
                Gizmos.DrawSphere (new Vector3 (0f,      0f,      radius),  sphereSize);
                Gizmos.DrawSphere (new Vector3 (0f,      0f,      -radius), sphereSize);
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////

        void SetDirty (RayfireActivator scr)
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