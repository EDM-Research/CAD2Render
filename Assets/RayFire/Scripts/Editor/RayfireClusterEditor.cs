using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace RayFire
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireCluster))]
    public class RayfireClusterEditor : Editor
    {
        // Draw gizmo
        [DrawGizmo (GizmoType.Selected | GizmoType.NonSelected | GizmoType.Pickable)]
        static void DrawGizmosSelected (RayfireCluster cluster, GizmoType gizmoType)
        {
            // Color preview
            if (cluster.colorPreview == true)
                ColorPreview (cluster);

            // Show bounds
            if (cluster.showGizmo == true)
            {
                if (cluster.transform.childCount > 0)
                {
                    Bounds bound = RFCluster.GetChildrenBound (cluster.transform);
                    Gizmos.color = cluster.wireColor;
                    Gizmos.DrawWireCube (bound.center, bound.size);

                    float size = bound.size.magnitude * 0.02f;
                    Gizmos.color = new Color (1.0f, 0.60f, 0f);
                    Gizmos.DrawSphere (bound.min, size);
                    Gizmos.DrawSphere (bound.max, size);
                }
            }
        }

        // Preview variables
        float lowestScale = 0.85f;
        bool  resetState  = false;

        public override void OnInspectorGUI()
        {
            // Get cluster
            RayfireCluster cluster = target as RayfireCluster;

            // Space
            GUILayout.Space (8);

            // Fragment 
            if (GUILayout.Button ("Clusterize", GUILayout.Height (25)))
            {
                float temp = cluster.previewScale;
                cluster.previewScale = 0f;
                resetState           = true;
                ResetScale (cluster, cluster.previewScale);
                cluster.Clusterize();
                cluster.previewScale = temp;
                if (cluster.scalePreview == true)
                    ScalePreview (cluster);
            }

            // Space
            GUILayout.Space (1);

            // Fragmentation section Begin
            GUILayout.BeginHorizontal();

            // Delete all fragments
            if (GUILayout.Button ("Extract Shards", GUILayout.Height (22)))
            {
                cluster.previewScale = 0f;
                resetState           = true;
                ResetScale (cluster, cluster.previewScale);
                cluster.Extract();
            }

            // Delete last
            if (GUILayout.Button ("Select Shards", GUILayout.Height (22)))
                SelectShards (cluster);

            // Fragmentation section End
            EditorGUILayout.EndHorizontal();

            // Space
            GUILayout.Space (1);

            // Preview toggles begin
            GUILayout.BeginHorizontal();

            // Color preview toggle
            EditorGUI.BeginChangeCheck();
            cluster.colorPreview = GUILayout.Toggle (cluster.colorPreview, "  Color Preview  ", "Button");
            if (EditorGUI.EndChangeCheck() == true)
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();

            // Start check for scale toggle change
            EditorGUI.BeginChangeCheck();
            cluster.scalePreview = GUILayout.Toggle (cluster.scalePreview, "Scale Preview", "Button");
            if (EditorGUI.EndChangeCheck() == true)
            {
                if (cluster.scalePreview == true)
                    ScalePreview (cluster);
                else
                {
                    resetState = true;
                    ResetScale (cluster, 0f);
                }
            }

            // Show bounds toggle
            cluster.showGizmo = GUILayout.Toggle (cluster.showGizmo, "Show Gizmo ", "Button");

            // Preview toggles end
            EditorGUILayout.EndHorizontal();

            // Space
            GUILayout.Space (3);

            // Preview section Begin
            GUILayout.BeginHorizontal();

            // Label
            GUILayout.Label ("Preview", GUILayout.Width (50));

            // Start check for slider change
            EditorGUI.BeginChangeCheck();

            // Slider
            cluster.previewScale = GUILayout.HorizontalSlider (cluster.previewScale, 0f, 0.99f);
            if (cluster.previewScale > 0f)
                if (EditorGUI.EndChangeCheck() == true)
                {
                    if (cluster.scalePreview == true)
                        ScalePreview (cluster);
                    if (cluster.colorPreview == true)
                        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                }

            // Reset scale if fragments were deleted
            ResetScale (cluster, cluster.previewScale);

            // Preview section End
            EditorGUILayout.EndHorizontal();

            // Space
            GUILayout.Space (3);

            // Info section Begin
            GUILayout.BeginHorizontal();

            // Label
            GUILayout.Label ("Clusters: " + cluster.allClusters.Count);

            // Label
            GUILayout.Label ("       Layers: " + MaxLayer (cluster));

            // Label
            GUILayout.Label ("      Shards: " + cluster.allShards.Count);

            // Info section End
            EditorGUILayout.EndHorizontal();

            // Space
            GUILayout.Space (3);

            // Draw script UI
            DrawDefaultInspector();
        }

        // Color preview
        static void ColorPreview (RayfireCluster cluster)
        {
            Random.InitState (1);

            // Scale cluster roots
            if (cluster.allClusters.Count > 0)
            {
                // Get max depth
                int maxDepth = 1;
                foreach (RFCluster cls in cluster.allClusters)
                    if (cls.depth > maxDepth)
                        maxDepth = cls.depth;

                // Depth step
                float step       = 1f / maxDepth;
                int   colorDepth = (int)(cluster.previewScale / step) + 1;

                // Get clusters to colorize
                List<RFCluster> colorClusters = new List<RFCluster>();
                foreach (RFCluster cls in cluster.allClusters)
                {
                    if (cls.childClusters.Count == 0 || cls.shards.Count > 0)
                    {
                        if (cls.depth <= colorDepth)
                            colorClusters.Add (cls);
                    }
                    else
                    {
                        if (cls.depth == colorDepth)
                            colorClusters.Add (cls);
                    }
                }

                //Random.InitState(1);
                foreach (var cls in colorClusters)
                {
                    Random.InitState (cls.id);
                    Gizmos.color = new Color (Random.Range (0.2f, 0.8f), Random.Range (0.2f, 0.8f), Random.Range (0.2f, 0.8f));

                    //List<RFShard> nestedShards = cls.GetNestedShards(true);
                    if (cls.tm != null)
                    {
                        MeshFilter[] meshFilters = cls.tm.GetComponentsInChildren<MeshFilter>();
                        foreach (var mf in meshFilters)
                            Gizmos.DrawMesh (mf.sharedMesh, mf.transform.position, mf.transform.rotation, mf.transform.lossyScale * 1.01f);
                    }
                }
            }
        }

        // Set all clusters scale by preview
        void ScalePreview (RayfireCluster cluster)
        {
            // Scale cluster roots
            if (cluster.allClusters.Count > 0)
            {
                // Get max depth
                int maxDepth = 1;
                foreach (RFCluster cls in cluster.allClusters)
                    if (cls.depth > maxDepth)
                        maxDepth = cls.depth;

                // Depth step
                float step = 1f / maxDepth;

                foreach (RFCluster cls in cluster.allClusters)
                {
                    if (cls.depth > 0)
                    {
                        // Get multiplier
                        float scaleMult = lowestScale;

                        // Get local range
                        float max = step * cls.depth;
                        float min = max - step;

                        // Scale should be lowest possible
                        if (cluster.previewScale < min)
                            scaleMult = 1f;

                        // Scale should be highest possible
                        else if (cluster.previewScale > max)
                            scaleMult = lowestScale;

                        // Scale should interpolated
                        else
                        {
                            float k = Mathf.InverseLerp (min, max, cluster.previewScale);
                            scaleMult = Mathf.Lerp (1.0f, lowestScale, k);
                        }

                        // Set scale
                        cls.tm.localScale = Vector3.one * scaleMult;
                    }
                }

                resetState = true;
            }
        }

        // Reset original object and fragments scale
        void ResetScale (RayfireCluster cluster, float scaleValue)
        {
            // Rest scale
            if (resetState == true && scaleValue == 0f)
            {
                if (cluster.allClusters.Count > 0)
                {
                    foreach (var cls in cluster.allClusters)
                    {
                        if (cls.tm != null)
                        {
                            if (cls.depth > 0)
                            {
                                cls.tm.localScale = Vector3.one;
                            }
                        }
                    }
                }

                resetState = false;
            }
        }

        // Select shards
        static void SelectShards (RayfireCluster cluster)
        {
            List<RFShard> shards = new List<RFShard>();
            foreach (var cls in cluster.allClusters)
                shards.AddRange (cls.shards);
            GameObject[] objects = new GameObject[shards.Count];
            for (int i = 0; i < shards.Count; i++)
                objects[i] = shards[i].tm.gameObject;
            if (objects.Length > 1)
                Selection.objects = objects;
        }

        // Get max depth
        static int MaxLayer (RayfireCluster cluster)
        {
            int depth = 0;
            foreach (var cls in cluster.allClusters)
                if (cls.depth > depth)
                    depth = cls.depth;
            return depth;
        }
    }
}