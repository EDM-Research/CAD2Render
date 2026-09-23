using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public sealed class PipeBoneSceneImporter : EditorWindow
{
    [SerializeField] private GameObject model;
    [SerializeField] private DefaultAsset outputFolder;

    [MenuItem("Tools/Import Pipe Bone Scene")]
    private static void Open() => GetWindow<PipeBoneSceneImporter>("Import Pipe Bone Scene");

    private void OnGUI()
    {
        EditorGUILayout.HelpBox(
            "Copy the exported FBX and its .pipebones.json file into Assets, then select the FBX. " +
            "The prefab includes the entire scene and configured cable randomizers.", MessageType.Info);
        model = (GameObject)EditorGUILayout.ObjectField("Model", model, typeof(GameObject), false);
        outputFolder = (DefaultAsset)EditorGUILayout.ObjectField(
            "Prefab folder", outputFolder, typeof(DefaultAsset), false);
        string folder = AssetDatabase.GetAssetPath(outputFolder);
        using (new EditorGUI.DisabledScope(model == null || !AssetDatabase.IsValidFolder(folder)))
        {
            if (GUILayout.Button("Import Scene"))
            {
                try
                {
                    Selection.activeObject = Import(AssetDatabase.GetAssetPath(model), folder);
                    EditorGUIUtility.PingObject(Selection.activeObject);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                    EditorUtility.DisplayDialog("Pipe Bone import failed", exception.Message, "OK");
                }
            }
        }
    }

    public static GameObject Import(string modelPath, string outputPath)
    {
        if (!string.Equals(Path.GetExtension(modelPath), ".fbx", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Select the exported FBX model.");
        string jsonPath = modelPath + ".pipebones.json";
        var data = JsonUtility.FromJson<SceneData>(File.ReadAllText(jsonPath));
        if (data == null || data.version != 1 || data.rigs == null)
            throw new InvalidDataException("Invalid Pipe Bones scene metadata.");
        if (!AssetDatabase.IsValidFolder(outputPath))
            throw new InvalidDataException("Select an existing folder inside Assets.");

        PrepareModel(modelPath);

        GameObject instance = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(modelPath));
        instance.name = Path.GetFileNameWithoutExtension(modelPath);
        try
        {
            foreach (RigData rig in data.rigs)
            {
                Transform armature = FindTransform(instance.transform, rig.name);
                Transform[] chain = rig.bones.Select(name => FindTransform(armature, name)).ToArray();
                Transform tip = FindTransform(armature, rig.bones.Last() + "_end");
                Vector3 tipPosition = tip.position;
                var frame = new GameObject(rig.name + "_Bending");
                frame.transform.SetParent(instance.transform, false);
                var randomizer = frame.AddComponent<BentCableRandomizer>();
                randomizer.Initialize(chain, tipPosition, rig.lengths);
                foreach (string meshName in rig.meshes)
                {
                    var renderer = FindRenderer(instance, meshName) as SkinnedMeshRenderer;
                    if (renderer == null)
                        throw new InvalidDataException($"{meshName} has no imported skin weights.");
                    // Allow bends outside the original import bounds to remain visible.
                    renderer.updateWhenOffscreen = true;
                }
            }

            string path = AssetDatabase.GenerateUniqueAssetPath($"{outputPath}/{instance.name}.prefab");
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, path, out bool success);
            if (!success)
                throw new IOException($"Could not save {path}.");
            Debug.Log($"Imported Pipe Bones scene: {data.rigs.Length} cable rigs. Prefab: {path}");
            return prefab;
        }
        finally
        {
            DestroyImmediate(instance);
        }
    }

    private static void PrepareModel(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as ModelImporter;
        if (importer == null)
            throw new InvalidDataException($"Unity could not load model {path}.");
        importer.animationType = ModelImporterAnimationType.Generic;
        importer.importAnimation = false;
        importer.optimizeGameObjects = false;
        importer.preserveHierarchy = true;
        importer.globalScale = 1f;
        importer.useFileScale = true;
        importer.SaveAndReimport();
    }

    private static Transform FindTransform(Transform root, string name)
    {
        Transform[] matches = root.GetComponentsInChildren<Transform>(true)
            .Where(item => item.name == name).ToArray();
        if (matches.Length != 1)
            throw new InvalidDataException($"Expected one transform named {name}, found {matches.Length}.");
        return matches[0];
    }

    private static Renderer FindRenderer(GameObject root, string name)
    {
        Renderer[] matches = root.GetComponentsInChildren<Renderer>(true)
            .Where(item => item.name == name).ToArray();
        if (matches.Length != 1)
            throw new InvalidDataException($"Expected one mesh named {name}, found {matches.Length}.");
        return matches[0];
    }

    [Serializable] private class SceneData
    {
        public int version;
        public RigData[] rigs;
    }

    [Serializable] private class RigData
    {
        public string name;
        public string[] bones;
        public string[] meshes;
        public float[] lengths;
    }
}
