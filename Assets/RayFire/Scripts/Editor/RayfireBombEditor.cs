using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

namespace RayFire
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireBomb))]
    public class RayfireBombEditor : Editor
    {
        RayfireBomb        bomb;
        List<string> layerNames;
        
        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////
        
        // Static
        static Color wireColor = new Color (0.58f, 0.77f, 1f);
        static Color sphColor  = new Color (1.0f,  0.60f, 0f);
        static int   space     = 3;
        
        // Contents
        static GUIContent gui_rangeShow     = new GUIContent ("Show",          "");
        static GUIContent gui_rangeType     = new GUIContent ("Type",          "Explosion direction.");
        static GUIContent gui_rangeFade     = new GUIContent ("Fade",          "Explosion strength decay over distance.");
        static GUIContent gui_rangeRange    = new GUIContent ("Range",         "Only objects in Range distance will be affected by explosion.");
        static GUIContent gui_rangeDeletion = new GUIContent ("Deletion",      "Destroy objects close to Bomb. Measures in percentage relative to Range value.");
        static GUIContent gui_impulseStr    = new GUIContent ("Strength",      "Maximum explosion impulse which will be applied to objects.");
        static GUIContent gui_impulseCrv    = new GUIContent ("Curve",         "");
        static GUIContent gui_impulseVar    = new GUIContent ("Variation",     "Random variation to final explosion strength for every object in percents relative to Strength value.");
        static GUIContent gui_impulseChaos  = new GUIContent ("Chaos",         "Random rotation velocity to exploded objects.");
        static GUIContent gui_impulseForce  = new GUIContent ("Force By Mass", "Add different final explosion impulse to objects with different mass.");
        static GUIContent gui_impulseIna    = new GUIContent ("Inactive",      "Activate Inactive objects and explode them as well.");
        static GUIContent gui_impulseKin    = new GUIContent ("Kinematic",     "Activate Kinematic objects and explode them as well.");
        static GUIContent gui_detonHeight   = new GUIContent ("Height Offset", "Allows to offset downward Explosion position over global Y axis.");
        static GUIContent gui_detonDelay    = new GUIContent ("Delay",         "Explosion delay in seconds.");
        static GUIContent gui_detonStart    = new GUIContent ("At Start",      "Automatically explode Bomb at Gameobject activation.");
        static GUIContent gui_detonDestroy  = new GUIContent ("Destroy",       "Destroy Gameobject after explosion.");
        static GUIContent gui_damageApply   = new GUIContent ("Apply",         "Apply damage to objects with Rigid component in case they have enabled Damage.");
        static GUIContent gui_damageValue   = new GUIContent ("Value",         "Damage value  which will take object at explosion.");
        static GUIContent gui_audioPlay     = new GUIContent ("Play",          "Play audio clip at explosion.");
        static GUIContent gui_audioVolume   = new GUIContent ("Volume",        "");
        static GUIContent gui_audioClip     = new GUIContent ("Clip",          "Audio Clip to play at explosion.");
        
        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////
        
        [DrawGizmo (GizmoType.Selected | GizmoType.NonSelected | GizmoType.Pickable)]
        static void DrawGizmosSelected (RayfireBomb bomb, GizmoType gizmoType)
        {
            if (bomb.showGizmo == true)
                DrawGizmo (bomb);
        }

        void OnSceneGUI()
        {
            var bomb = target as RayfireBomb;
            if (bomb == null)
                return;

            if (bomb.enabled == true)
            {
                Handles.matrix = bomb.transform.localToWorldMatrix;
                Vector3 ho = Vector3.zero;
                ho.y += bomb.heightOffset;

                // Draw handles
                EditorGUI.BeginChangeCheck();
                bomb.range = Handles.RadiusHandle (Quaternion.identity, ho, bomb.range);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    Undo.RecordObject (bomb, "Change Range");
                    SetDirty (bomb);
                }
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Inspector
        /// /////////////////////////////////////////////////////////
        
        public override void OnInspectorGUI()
        {
            bomb = target as RayfireBomb;
            if (bomb == null)
                return;
            
            GUILayout.Space (8);

            UI_Actions();
            
            GUILayout.Space (space);

            UI_Range();

            GUILayout.Space (space);

            UI_Impulse();

            GUILayout.Space (space);

            UI_Activation();            

            GUILayout.Space (space);

            UI_Detonation();

            GUILayout.Space (space);

            UI_Damage();
            
            GUILayout.Space (space);

            UI_Audio();
            
            GUILayout.Space (space);

            UI_Filters();

            GUILayout.Space (8);
        }

        /// /////////////////////////////////////////////////////////
        /// Buttons
        /// /////////////////////////////////////////////////////////

        void UI_Actions()
        {
            if (Application.isPlaying == true)
            {
                GUILayout.Label ("  Actions", EditorStyles.boldLabel);
                
                GUILayout.BeginHorizontal();
                
                if (GUILayout.Button ("Explode", GUILayout.Height (25)))
                {
                    foreach (RayfireBomb script in targets)
                    {
                        script.Explode (script.delay);
                        SetDirty (script);
                    }
                }
                
                if (GUILayout.Button ("Restore", GUILayout.Height (25)))
                {
                    foreach (RayfireBomb script in targets)
                    {
                        script.Restore ();
                        SetDirty (script);
                    }
                }
                
                EditorGUILayout.EndHorizontal();
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Range
        /// /////////////////////////////////////////////////////////
        
        void UI_Range()
        {
            GUILayout.Label ("  Range", EditorStyles.boldLabel);

            UI_RangeShow();
            
            GUILayout.Space (space);
            
            UI_RangeType();

            GUILayout.Space (space);

            UI_RangeRange();
            
            GUILayout.Space (space);

            UI_RangeDeletion();
        }
        
        void UI_RangeShow()
        {
            EditorGUI.BeginChangeCheck();
            bomb.showGizmo = EditorGUILayout.Toggle (gui_rangeShow, bomb.showGizmo);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireBomb script in targets)
                {
                    script.showGizmo = bomb.showGizmo;
                    SetDirty (script);
                }
            }
        }
        
        void UI_RangeType()
        {
            EditorGUI.BeginChangeCheck();
            bomb.rangeType = (RayfireBomb.RangeType)EditorGUILayout.EnumPopup (gui_rangeType, bomb.rangeType);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireBomb scr in targets)
                {
                    scr.rangeType = bomb.rangeType;
                    SetDirty (scr);
                }
            }
        }
        
        void UI_RangeRange()
        {
            EditorGUI.BeginChangeCheck();
            bomb.range = EditorGUILayout.Slider (gui_rangeRange, bomb.range, 0.01f, 50f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireBomb scr in targets)
                {
                    scr.range = bomb.range;
                    SetDirty (scr);
                }
            }
        }
        
        void UI_RangeDeletion()
        {
            EditorGUI.BeginChangeCheck();
            bomb.deletion = EditorGUILayout.IntSlider (gui_rangeDeletion, bomb.deletion, 0, 100);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireBomb scr in targets)
                {
                    scr.deletion = bomb.deletion;
                    SetDirty (scr);
                }
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Impulse
        /// /////////////////////////////////////////////////////////
        
        void UI_Impulse()
        {
            GUILayout.Label ("  Impulse", EditorStyles.boldLabel);

            UI_ImpulseFade();
            
            if (bomb.fadeType == RayfireBomb.FadeType.ByCurve)
            {
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                bomb.curve = EditorGUILayout.CurveField (gui_impulseCrv, bomb.curve);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    foreach (RayfireBomb scr in targets)
                    {
                        scr.curve = bomb.curve;
                        SetDirty (scr);
                    }
                }
            }
            
            GUILayout.Space (space);

            UI_ImpulseStr();
            
            GUILayout.Space (space);
            
            UI_ImpulseVar();
            
            GUILayout.Space (space);
                
            UI_ImpulseChaos();

            GUILayout.Space (space);

            UI_ImpulseForce();
        }
        
        void UI_ImpulseFade()
        {
            EditorGUI.BeginChangeCheck();
            bomb.fadeType = (RayfireBomb.FadeType)EditorGUILayout.EnumPopup (gui_rangeFade, bomb.fadeType);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireBomb scr in targets)
                {
                    scr.fadeType = bomb.fadeType;
                    SetDirty (scr);
                }
            }
        }
        
        void UI_ImpulseStr()
        {
            EditorGUI.BeginChangeCheck();
            bomb.strength = EditorGUILayout.Slider (gui_impulseStr, bomb.strength, 0, 10f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireBomb scr in targets)
                {
                    scr.strength = bomb.strength;
                    SetDirty (scr);
                }
            }
        }
        
        void UI_ImpulseVar()
        {
            EditorGUI.BeginChangeCheck();
            bomb.variation = EditorGUILayout.IntSlider (gui_impulseVar, bomb.variation, 0, 100);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireBomb scr in targets)
                {
                    scr.variation = bomb.variation;
                    SetDirty (scr);
                }
            }
        }
        
        void UI_ImpulseChaos()
        {
            EditorGUI.BeginChangeCheck();
            bomb.chaos = EditorGUILayout.IntSlider (gui_impulseChaos, bomb.chaos, 0, 90);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireBomb scr in targets)
                {
                    scr.chaos = bomb.chaos;
                    SetDirty (scr);
                }
            }
        }
        
        void UI_ImpulseForce()
        {
            EditorGUI.BeginChangeCheck();
            bomb.forceByMass = EditorGUILayout.Toggle (gui_impulseForce, bomb.forceByMass);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireBomb scr in targets)
                {
                    scr.forceByMass = bomb.forceByMass;
                    SetDirty (scr);
                }
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Activation
        /// /////////////////////////////////////////////////////////

        void UI_Activation()
        {
            GUILayout.Label ("  Activate", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            bomb.affectInactive = EditorGUILayout.Toggle (gui_impulseIna, bomb.affectInactive);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireBomb scr in targets)
                {
                    scr.affectInactive = bomb.affectInactive;
                    SetDirty (scr);
                }
            }

            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            bomb.affectKinematic = EditorGUILayout.Toggle (gui_impulseKin, bomb.affectKinematic);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireBomb scr in targets)
                {
                    scr.affectKinematic = bomb.affectKinematic;
                    SetDirty (scr);
                }
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Detonation
        /// /////////////////////////////////////////////////////////
        
        void UI_Detonation()
        {
            GUILayout.Label ("  Detonation", EditorStyles.boldLabel);

            UI_DetonHeight();
            
            GUILayout.Space (space);
            
            UI_DetonDelay();
            
            GUILayout.Space (space);
                
            UI_DetonStart();

            GUILayout.Space (space);

            UI_DetonDestroy();
        }
        
        void UI_DetonHeight()
        {
            EditorGUI.BeginChangeCheck();
            bomb.heightOffset = EditorGUILayout.Slider (gui_detonHeight, bomb.heightOffset, -10f, 10f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireBomb scr in targets)
                {
                    scr.heightOffset = bomb.heightOffset;
                    SetDirty (scr);
                }
            }
        }
        
        void UI_DetonDelay()
        {
            EditorGUI.BeginChangeCheck();
            bomb.delay = EditorGUILayout.Slider (gui_detonDelay, bomb.delay, 0, 10f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireBomb scr in targets)
                {
                    scr.delay = bomb.delay;
                    SetDirty (scr);
                }
            }
        }
        
        void UI_DetonStart()
        {
            EditorGUI.BeginChangeCheck();
            bomb.atStart = EditorGUILayout.Toggle (gui_detonStart, bomb.atStart);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireBomb scr in targets)
                {
                    scr.atStart = bomb.atStart;
                    SetDirty (scr);
                }
            }
        }
        
        void UI_DetonDestroy()
        {
            EditorGUI.BeginChangeCheck();
            bomb.destroy = EditorGUILayout.Toggle (gui_detonDestroy, bomb.destroy);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireBomb scr in targets)
                {
                    scr.destroy = bomb.destroy;
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

            UI_DamageApply();

            if (bomb.applyDamage == true)
            {
                GUILayout.Space (space);

                UI_DamageValue();
            }
        }

        void UI_DamageApply()
        {
            EditorGUI.BeginChangeCheck();
            bomb.applyDamage = EditorGUILayout.Toggle (gui_damageApply, bomb.applyDamage);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireBomb scr in targets)
                {
                    scr.applyDamage = bomb.applyDamage;
                    SetDirty (scr);
                }
            }
        }
        
        void UI_DamageValue()
        {
            EditorGUI.BeginChangeCheck();
            bomb.damageValue = EditorGUILayout.Slider (gui_damageValue, bomb.damageValue, 0, 100f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireBomb scr in targets)
                {
                    scr.damageValue = bomb.damageValue;
                    SetDirty (scr);
                }
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Audio
        /// /////////////////////////////////////////////////////////
        
        void UI_Audio()
        {
            GUILayout.Label ("  Audio", EditorStyles.boldLabel);

            UI_AudioPlay();

            if (bomb.play == true)
            {
                GUILayout.Space (space);

                UI_AudioVolume();
                
                GUILayout.Space (space);

                UI_AudioClip();
            }
        }

        void UI_AudioPlay()
        {
            EditorGUI.BeginChangeCheck();
            bomb.play = EditorGUILayout.Toggle (gui_audioPlay, bomb.play);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireBomb scr in targets)
                {
                    scr.play = bomb.play;
                    SetDirty (scr);
                }
            }
        }
        
        void UI_AudioVolume()
        {
            EditorGUI.BeginChangeCheck();
            bomb.volume = EditorGUILayout.Slider (gui_audioVolume, bomb.volume, 0.01f, 1f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireBomb scr in targets)
                {
                    scr.volume = bomb.volume;
                    SetDirty (scr);
                }
            }
        }
        
        void UI_AudioClip()
        {
            EditorGUI.BeginChangeCheck();
            bomb.clip = (AudioClip)EditorGUILayout.ObjectField (gui_audioClip, bomb.clip, typeof(AudioClip), true);
            if (EditorGUI.EndChangeCheck() == true)
                SetDirty (bomb);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Filters
        /// /////////////////////////////////////////////////////////
        
        void UI_Filters()
        {
            GUILayout.Label ("  Filters", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            bomb.tagFilter = EditorGUILayout.TagField ("Tag", bomb.tagFilter);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireBomb scr in targets)
                {
                    scr.tagFilter = bomb.tagFilter;
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
            bomb.mask = EditorGUILayout.MaskField ("Layer", bomb.mask, layerNames.ToArray());
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireBomb scr in targets)
                {
                    scr.mask = bomb.mask;
                    SetDirty (scr);
                }
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Draw
        /// /////////////////////////////////////////////////////////

        static void DrawGizmo (RayfireBomb bomb)
        {
            // Vars
            float       rate          = 0f;
            const int   size          = 45;
            const float scale         = 1f / size;
            Vector3     previousPoint = Vector3.zero;
            Vector3     nextPoint     = Vector3.zero;
            float       h             = bomb.heightOffset;

            // Gizmo properties
            Gizmos.color  = wireColor;
            Gizmos.matrix = bomb.transform.localToWorldMatrix;

            // Draw top eye
            rate            = 0f;
            nextPoint.y     = h;
            previousPoint.y = h;
            previousPoint.x = bomb.range * Mathf.Cos (rate);
            previousPoint.z = bomb.range * Mathf.Sin (rate);
            for (int i = 0; i < size; i++)
            {
                rate        += 2.0f * Mathf.PI * scale;
                nextPoint.x =  bomb.range * Mathf.Cos (rate);
                nextPoint.z =  bomb.range * Mathf.Sin (rate);
                Gizmos.DrawLine (previousPoint, nextPoint);
                previousPoint = nextPoint;
            }

            // Draw top eye
            rate            = 0f;
            nextPoint.x     = 0f;
            previousPoint.x = 0f;
            previousPoint.y = bomb.range * Mathf.Cos (rate) + h;
            previousPoint.z = bomb.range * Mathf.Sin (rate);
            for (int i = 0; i < size; i++)
            {
                rate        += 2.0f * Mathf.PI * scale;
                nextPoint.y =  bomb.range * Mathf.Cos (rate) + h;
                nextPoint.z =  bomb.range * Mathf.Sin (rate);
                Gizmos.DrawLine (previousPoint, nextPoint);
                previousPoint = nextPoint;
            }

            // Draw top eye
            rate            = 0f;
            nextPoint.z     = 0f;
            previousPoint.z = 0f;
            previousPoint.y = bomb.range * Mathf.Cos (rate) + h;
            previousPoint.x = bomb.range * Mathf.Sin (rate);
            for (int i = 0; i < size; i++)
            {
                rate        += 2.0f * Mathf.PI * scale;
                nextPoint.y =  bomb.range * Mathf.Cos (rate) + h;
                nextPoint.x =  bomb.range * Mathf.Sin (rate);
                Gizmos.DrawLine (previousPoint, nextPoint);
                previousPoint = nextPoint;
            }

            // Selectable sphere
            float sphereSize = bomb.range * 0.07f;
            if (sphereSize < 0.1f)
                sphereSize = 0.1f;
            Gizmos.color = sphColor;
            Gizmos.DrawSphere (new Vector3 (0f,          bomb.range + h,  0f),          sphereSize);
            Gizmos.DrawSphere (new Vector3 (0f,          -bomb.range + h, 0f),          sphereSize);
            Gizmos.DrawSphere (new Vector3 (bomb.range,  h,               0f),          sphereSize);
            Gizmos.DrawSphere (new Vector3 (-bomb.range, h,              0f),          sphereSize);
            Gizmos.DrawSphere (new Vector3 (0f,          h,              bomb.range),  sphereSize);
            Gizmos.DrawSphere (new Vector3 (0f,          h,              -bomb.range), sphereSize);

            // Center helper
            Gizmos.color = Color.red;
            Gizmos.DrawSphere (new Vector3 (0f, 0f, 0f), sphereSize / 3f);

            // Height offset helper
            if (bomb.heightOffset != 0)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere (new Vector3 (0f, bomb.heightOffset, 0f), sphereSize / 3f);
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////
        
        void SetDirty (RayfireBomb scr)
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