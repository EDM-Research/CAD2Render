using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine.Audio;

namespace RayFire
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireSound))]
    public class RayfireSoundEditor : Editor
    {
        RayfireSound       sound;
        SerializedProperty сlipsInitProp;
        ReorderableList    clipsInitList; 
        SerializedProperty сlipsActProp;
        ReorderableList    clipsActList;  
        SerializedProperty сlipsDmlProp;
        ReorderableList    clipsDmlList;  
        
        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////
        
        static int space = 3;
        
        static GUIContent gui_volBase    = new GUIContent ("Base Volume",     "Base volume. Can be increased by Size Volume property.");
        static GUIContent gui_colSize    = new GUIContent ("Size Volume",     "Additional volume per one unit size.");
        static GUIContent gui_eventsInit = new GUIContent ("Initialization",  "Enable Initialization sound.");
        static GUIContent gui_eventsAct  = new GUIContent ("Activation",      "Enable Activation sound");
        static GUIContent gui_eventsDml  = new GUIContent ("Demolition",      "Enable Demolition sound");
        static GUIContent gui_once       = new GUIContent ("Play Once",       "");
        static GUIContent gui_soundMult  = new GUIContent ("Multiplier",      "Sound volume multiplier for this event.");
        static GUIContent gui_clip       = new GUIContent ("Clip",            "");
        static GUIContent gui_group      = new GUIContent ("Output Group",    "");
        static GUIContent gui_filterSize = new GUIContent ("Minimum Size",    "Objects with size lower than defined value will not make sound.");
        static GUIContent gui_filterDist = new GUIContent ("Camera Distance", "Objects with distance to main camera higher than defined value will not make sound.");

        /// /////////////////////////////////////////////////////////
        /// Enable
        /// /////////////////////////////////////////////////////////
        
        private void OnEnable()
        {
            сlipsInitProp                     = serializedObject.FindProperty("initialization.clips");
            clipsInitList                     = new ReorderableList(serializedObject, сlipsInitProp, true, true, true, true);
            clipsInitList.drawElementCallback = DrawInitListItems;
            clipsInitList.drawHeaderCallback  = DrawInitHeader;
            clipsInitList.onAddCallback       = AddInit;
            clipsInitList.onRemoveCallback    = RemoveInit;

            сlipsActProp                     = serializedObject.FindProperty("activation.clips");
            clipsActList                     = new ReorderableList(serializedObject, сlipsActProp, true, true, true, true);
            clipsActList.drawElementCallback = DrawActListItems;
            clipsActList.drawHeaderCallback  = DrawActHeader;
            clipsActList.onAddCallback       = AddAct;
            clipsActList.onRemoveCallback    = RemoveAct;

            сlipsDmlProp                     = serializedObject.FindProperty("demolition.clips");
            clipsDmlList                     = new ReorderableList(serializedObject, сlipsDmlProp, true, true, true, true);
            clipsDmlList.drawElementCallback = DrawDmlListItems;
            clipsDmlList.drawHeaderCallback  = DrawDmlHeader;
            clipsDmlList.onAddCallback       = AddDml;
            clipsDmlList.onRemoveCallback    = RemoveDml;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Inspector
        /// /////////////////////////////////////////////////////////

        public override void OnInspectorGUI()
        {
            sound = target as RayfireSound;
            if (sound == null)
                return;
            
            GUILayout.Space (8);
            
            UI_Vol();
            
            GUILayout.Space (space);
            
            UI_Events();
            
            GUILayout.Space (space);

            UI_Filters();
            
            GUILayout.Space (8);
            
            if (Application.isPlaying == true)
            {
                GUILayout.Label ("Info", EditorStyles.boldLabel);

                GUILayout.Label ("  Volume: " + RFSound.GeVolume(sound, 0f));
                
                GUILayout.Space (5);
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Volume
        /// /////////////////////////////////////////////////////////
        
        void UI_Vol()
        {
            GUILayout.Label ("  Volume", EditorStyles.boldLabel);

            UI_VolBase();
            
            GUILayout.Space (space);
                
            UI_VolSize();
        }
        
        void UI_VolBase()
        {

            EditorGUI.BeginChangeCheck();
            sound.baseVolume = EditorGUILayout.Slider (gui_volBase, sound.baseVolume, 0.01f, 1f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireSound scr in targets)
                {
                    scr.baseVolume = sound.baseVolume;
                    SetDirty (scr);
                }
            }
            
            //EditorGUILayout.MinMaxSlider (gui_volBase, ref sound.baseVolume, ref sound.sizeVolume, 0f, 1f);
            //EditorGUILayout.BeginFadeGroup ()

        }
        
        void UI_VolSize()
        {
            EditorGUI.BeginChangeCheck();
            sound.sizeVolume = EditorGUILayout.Slider (gui_colSize, sound.sizeVolume, 0f, 1f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireSound scr in targets)
                {
                    scr.sizeVolume = sound.sizeVolume;
                    SetDirty (scr);
                }
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Events
        /// /////////////////////////////////////////////////////////
        
        void UI_Events()
        {
            GUILayout.Label ("  Events", EditorStyles.boldLabel);

            UI_EventsInit();
            
            GUILayout.Space (space);
                
            UI_EventsAct();
            
            GUILayout.Space (space);
                
            UI_EventsDml();
        }
        
        void UI_EventsInit()
        {
            EditorGUI.BeginChangeCheck();
            sound.initialization.enable = EditorGUILayout.Toggle (gui_eventsInit, sound.initialization.enable);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireSound scr in targets)
                {
                    scr.initialization.enable = sound.initialization.enable;
                    SetDirty (scr);
                }
            }

            if (sound.initialization.enable == true)
                UI_PropsInit();
        }
        
        void UI_EventsAct()
        {
            EditorGUI.BeginChangeCheck();
            sound.activation.enable = EditorGUILayout.Toggle (gui_eventsAct, sound.activation.enable);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireSound scr in targets)
                {
                    scr.activation.enable = sound.activation.enable;
                    SetDirty (scr);
                }
            }

            if (sound.activation.enable == true)
                UI_PropsAct();
        }
        
        void UI_EventsDml()
        {
            EditorGUI.BeginChangeCheck();
            sound.demolition.enable = EditorGUILayout.Toggle (gui_eventsDml, sound.demolition.enable);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireSound scr in targets)
                {
                    scr.demolition.enable = sound.demolition.enable;
                    SetDirty (scr);
                }
            }

            if (sound.demolition.enable == true)
                UI_PropsDml();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Properties
        /// /////////////////////////////////////////////////////////
        
        void UI_PropsInit()
        {
            if (Application.isPlaying == true)
            {
                GUILayout.Space (space);
                
                if (GUILayout.Button ("Initialization Sound", GUILayout.Height (25)))
                    foreach (var targ in targets)
                        if (targ as RayfireSound != null)
                        {
                            RFSound.InitializationSound (targ as RayfireSound, 0f);
                            (targ as RayfireSound).initialization.played = false;
                        }
            }

            GUILayout.Space (space);
            
            EditorGUI.indentLevel++;
            
            EditorGUI.BeginChangeCheck();
            sound.initialization.once = EditorGUILayout.Toggle (gui_once, sound.initialization.once);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireSound scr in targets)
                {
                    scr.initialization.once = sound.initialization.once;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            sound.initialization.multiplier = EditorGUILayout.Slider (gui_soundMult, sound.initialization.multiplier, 0.01f, 1f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireSound scr in targets)
                {
                    scr.initialization.multiplier = sound.initialization.multiplier;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            sound.initialization.outputGroup = (AudioMixerGroup)EditorGUILayout.ObjectField (gui_group, sound.initialization.outputGroup, typeof(AudioMixerGroup), true);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireSound scr in targets)
                {
                    scr.initialization.outputGroup = sound.initialization.outputGroup;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            sound.initialization.clip = (AudioClip)EditorGUILayout.ObjectField (gui_clip, sound.initialization.clip, typeof(AudioClip), true);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireSound scr in targets)
                {
                    scr.initialization.clip = sound.initialization.clip;
                    SetDirty (scr);
                }
            }
            
            EditorGUI.indentLevel--;
            
            GUILayout.Space (space);
            
            serializedObject.Update();
            clipsInitList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
        
        void UI_PropsAct()
        {
            // Initialize
            if (Application.isPlaying == true)
            {
                GUILayout.Space (space);
                
                if (GUILayout.Button ("Activation Sound", GUILayout.Height (25)))
                    foreach (var targ in targets)
                        if (targ as RayfireSound != null)
                        {
                            RFSound.ActivationSound (targ as RayfireSound, 0f);
                            (targ as RayfireSound).activation.played = false;
                        }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.indentLevel++;
            
            EditorGUI.BeginChangeCheck();
            sound.activation.once = EditorGUILayout.Toggle (gui_once, sound.activation.once);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireSound scr in targets)
                {
                    scr.activation.once = sound.activation.once;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            sound.activation.multiplier = EditorGUILayout.Slider (gui_soundMult, sound.activation.multiplier, 0.01f, 1f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireSound scr in targets)
                {
                    scr.activation.multiplier = sound.activation.multiplier;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            sound.activation.outputGroup = (AudioMixerGroup)EditorGUILayout.ObjectField (gui_group, sound.activation.outputGroup, typeof(AudioMixerGroup), true);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireSound scr in targets)
                {
                    scr.activation.outputGroup = sound.activation.outputGroup;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            sound.activation.clip = (AudioClip)EditorGUILayout.ObjectField (gui_clip, sound.activation.clip, typeof(AudioClip), true);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireSound scr in targets)
                {
                    scr.activation.clip = sound.activation.clip;
                    SetDirty (scr);
                }
            }
            
            EditorGUI.indentLevel--;
            
            GUILayout.Space (space);
        
            serializedObject.Update();
            clipsActList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
        
        void UI_PropsDml()
        {
            // Initialize
            if (Application.isPlaying == true)
            {
                GUILayout.Space (space);
                
                if (GUILayout.Button ("Demolition Sound", GUILayout.Height (25)))
                    foreach (var targ in targets)
                        if (targ as RayfireSound != null)
                        {
                            RFSound.DemolitionSound (targ as RayfireSound, 0f);
                            (targ as RayfireSound).demolition.played = false;
                        }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.indentLevel++;
            
            EditorGUI.BeginChangeCheck();
            sound.demolition.once = EditorGUILayout.Toggle (gui_once, sound.demolition.once);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireSound scr in targets)
                {
                    scr.demolition.once = sound.demolition.once;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            sound.demolition.multiplier = EditorGUILayout.Slider (gui_soundMult, sound.demolition.multiplier, 0.01f, 1f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireSound scr in targets)
                {
                    scr.demolition.multiplier = sound.demolition.multiplier;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            sound.demolition.outputGroup = (AudioMixerGroup)EditorGUILayout.ObjectField (gui_group, sound.demolition.outputGroup, typeof(AudioMixerGroup), true);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireSound scr in targets)
                {
                    scr.demolition.outputGroup = sound.demolition.outputGroup;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            sound.demolition.clip = (AudioClip)EditorGUILayout.ObjectField (gui_clip, sound.demolition.clip, typeof(AudioClip), true);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireSound scr in targets)
                {
                    scr.demolition.clip = sound.demolition.clip;
                    SetDirty (scr);
                }
            }
            
            EditorGUI.indentLevel--;
            
            GUILayout.Space (space);
        
            serializedObject.Update();
            clipsDmlList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Filters
        /// /////////////////////////////////////////////////////////
        
        void UI_Filters()
        {
            GUILayout.Label ("  Filters", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            
            sound.minimumSize = EditorGUILayout.Slider (gui_filterSize, sound.minimumSize, 0f, 1f);
            
            GUILayout.Space (space);
            
            sound.cameraDistance = EditorGUILayout.Slider (gui_filterDist, sound.cameraDistance, 0f, 999f);
            
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireSound scr in targets)
                {
                    scr.minimumSize    = sound.minimumSize;
                    scr.cameraDistance = sound.cameraDistance;
                    SetDirty (scr);
                }
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// ReorderableList draw
        /// /////////////////////////////////////////////////////////
        
        void DrawInitListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = clipsInitList.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y+2, EditorGUIUtility.currentViewWidth - 80f, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        }
        
        void DrawInitHeader(Rect rect)
        {
            rect.x += 10;
            EditorGUI.LabelField(rect, "Random Clips");
        }

        void AddInit(ReorderableList list)
        {
            if (sound.initialization.clips == null)
                sound.initialization.clips = new List<AudioClip>();
            sound.initialization.clips.Add (null);
            list.index = list.count;
        }
        
        void RemoveInit(ReorderableList list)
        {
            if (sound.initialization.clips != null)
            {
                sound.initialization.clips.RemoveAt (list.index);
                list.index = list.index - 1;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// ReorderableList draw
        /// /////////////////////////////////////////////////////////
        
        void DrawActListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = clipsActList.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y+2, EditorGUIUtility.currentViewWidth - 80f, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        }
        
        void DrawActHeader(Rect rect)
        {
            rect.x += 10;
            EditorGUI.LabelField(rect, "Random Clips");
        }

        void AddAct(ReorderableList list)
        {
            if (sound.activation.clips == null)
                sound.activation.clips = new List<AudioClip>();
            sound.activation.clips.Add (null);
            list.index = list.count;
        }
        
        void RemoveAct(ReorderableList list)
        {
            if (sound.activation.clips != null)
            {
                sound.activation.clips.RemoveAt (list.index);
                list.index = list.index - 1;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// ReorderableList draw
        /// /////////////////////////////////////////////////////////
        
        void DrawDmlListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = clipsDmlList.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y+2, EditorGUIUtility.currentViewWidth - 80f, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        }
        
        void DrawDmlHeader(Rect rect)
        {
            rect.x += 10;
            EditorGUI.LabelField(rect, "Random Clips");
        }

        void AddDml(ReorderableList list)
        {
            if (sound.demolition.clips == null)
                sound.demolition.clips = new List<AudioClip>();
            sound.demolition.clips.Add (null);
            list.index = list.count;
        }
        
        void RemoveDml(ReorderableList list)
        {
            if (sound.demolition.clips != null)
            {
                sound.demolition.clips.RemoveAt (list.index);
                list.index = list.index - 1;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////

        void SetDirty (RayfireSound scr)
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