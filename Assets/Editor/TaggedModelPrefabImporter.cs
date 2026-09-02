using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SimpleJSON;
using UnityEditor;
using UnityEngine;

public sealed class TaggedModelPrefabImporter : EditorWindow
{
    private const int SchemaVersion = 4;

    [SerializeField] private GameObject model;
    [SerializeField] private string tagsJsonPath;
    [SerializeField] private DefaultAsset outputRoot;
    [SerializeField] private float scale = 1f;
    [SerializeField] private Vector3 rotation = new Vector3(-90f, 0f, 0f);

    [MenuItem("Tools/Import Tagged Model")]
    private static void Open()
    {
        GetWindow<TaggedModelPrefabImporter>("Import Tagged Model");
    }

    private void OnGUI()
    {
        model = (GameObject)EditorGUILayout.ObjectField("Model", model, typeof(GameObject), false);

        using (new EditorGUILayout.HorizontalScope())
        {
            tagsJsonPath = EditorGUILayout.TextField("Object tags JSON", tagsJsonPath);
            if (GUILayout.Button("Browse", GUILayout.Width(70f)))
            {
                string folder = File.Exists(tagsJsonPath)
                    ? Path.GetDirectoryName(tagsJsonPath)
                    : Application.dataPath;
                string selected = EditorUtility.OpenFilePanel("Select object tags JSON", folder, "json");
                if (!string.IsNullOrEmpty(selected))
                    tagsJsonPath = selected;
            }
        }

        outputRoot = (DefaultAsset)EditorGUILayout.ObjectField(
            "Output root", outputRoot, typeof(DefaultAsset), false);
        scale = EditorGUILayout.FloatField("Scale", scale);
        rotation = EditorGUILayout.Vector3Field("Rotation", rotation);

        string outputPath = AssetDatabase.GetAssetPath(outputRoot);
        bool isObj = model != null
            && string.Equals(
                Path.GetExtension(AssetDatabase.GetAssetPath(model)),
                ".obj",
                StringComparison.OrdinalIgnoreCase);
        if (model != null && !isObj)
            EditorGUILayout.HelpBox("Select an OBJ model.", MessageType.Warning);

        bool canImport = isObj
            && File.Exists(tagsJsonPath)
            && AssetDatabase.IsValidFolder(outputPath);

        using (new EditorGUI.DisabledScope(!canImport))
        {
            if (GUILayout.Button("Import"))
                RunImport(outputPath);
        }
    }

    private void RunImport(string outputPath)
    {
        try
        {
            ImportResult result = Import(model, tagsJsonPath, outputPath, scale, rotation);
            Selection.activeObject = result.Prefab;
            EditorGUIUtility.PingObject(result.Prefab);
            Debug.Log($"Imported {result.TaggedObjects} tagged objects to {result.PrefabPath}");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorUtility.DisplayDialog("Tagged model import failed", exception.Message, "OK");
        }
    }

    internal static ImportResult Import(
        GameObject modelAsset,
        string jsonPath,
        string outputPath,
        float modelScale,
        Vector3 modelRotation)
    {
        if (!string.Equals(
                Path.GetExtension(AssetDatabase.GetAssetPath(modelAsset)),
                ".obj",
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("The selected model must be an OBJ file.");

        ParsedTags tags = Parse(File.ReadAllText(jsonPath));
        string prefabFolder = EnsureFolder(outputPath, "Prefabs");
        string dataFolder = EnsureFolder(outputPath, "ObjectTagData");
        var warnings = new List<string>();
        GameObject importAsset = EnsureObjectGroups(modelAsset);
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(importAsset);

        if (instance == null)
            throw new InvalidOperationException("The selected model could not be instantiated as a prefab asset.");

        try
        {
            Dictionary<string, GameObject> targets = ResolveTargets(tags.Objects, instance);

            foreach (MaterialDefinition material in tags.Materials.Values)
            {
                material.Dataset = CreateDataset(material, dataFolder, warnings);
            }

            MissingPartData missingPart = tags.Objects.Any(item => item.MissingPart)
                ? CreateMissingPartDataset(dataFolder)
                : null;

            foreach (TaggedObject taggedObject in tags.Objects)
            {
                GameObject target = targets[taggedObject.Name];

                if (taggedObject.MaterialId != null)
                {
                    MaterialDefinition material = tags.Materials[taggedObject.MaterialId];
                    SetDataset(target, material.HandlerType, material.Dataset);
                }

                if (taggedObject.MissingPart)
                    SetDataset(target, typeof(MissingPartHandler), missingPart);
            }

            instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.Euler(modelRotation));
            instance.transform.localScale = Vector3.one * modelScale;

            string prefabPath = $"{prefabFolder}/{modelAsset.name}.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath, out bool success);
            if (!success || prefab == null)
                throw new IOException($"Unity could not save the prefab at {prefabPath}.");

            AssetDatabase.SaveAssets();
            if (warnings.Count > 0)
                Debug.LogWarning("Tagged model import warnings:\n- " + string.Join("\n- ", warnings));

            return new ImportResult(prefab, prefabPath, tags.Objects.Count);
        }
        finally
        {
            DestroyImmediate(instance);
        }
    }

    private static ParsedTags Parse(string json)
    {
        JSONNode root = JSON.Parse(json);
        if (root == null || !root.IsObject)
            throw new InvalidDataException("The tag file must contain a JSON object.");
        if (root["schema_version"].AsInt != SchemaVersion)
            throw new InvalidDataException($"Only object tag schema version {SchemaVersion} is supported.");

        JSONObject materialNodes = root["material_assets"].AsObject;
        JSONArray objectNodes = root["objects"].AsArray;
        if (materialNodes == null || objectNodes == null)
            throw new InvalidDataException("The tag file is missing material_assets or objects.");

        var materials = new Dictionary<string, MaterialDefinition>(StringComparer.Ordinal);
        foreach (KeyValuePair<string, JSONNode> pair in materialNodes.Linq)
        {
            JSONNode settings = pair.Value["settings"];
            string scriptGuid = settings["m_Script"]["guid"].Value;
            Type dataType = ResolveDataType(pair.Key, scriptGuid);
            materials.Add(pair.Key, new MaterialDefinition(
                pair.Key,
                settings,
                dataType,
                ResolveHandlerType(dataType)));
        }

        var objects = new List<TaggedObject>();
        foreach (JSONNode objectNode in objectNodes.Children)
        {
            JSONObject objectTags = objectNode["tags"].AsObject;
            if (objectTags == null)
                continue;

            string materialId = objectTags.HasKey("material")
                ? objectTags["material"].Value
                : null;
            bool missingPart = objectTags.HasKey("missing_part")
                && objectTags["missing_part"].AsBool;
            if (materialId == null && !missingPart)
                continue;
            if (materialId != null && !materials.ContainsKey(materialId))
                throw new InvalidDataException($"Object {objectNode["name"].Value} references unknown material {materialId}.");

            objects.Add(new TaggedObject(objectNode["name"].Value, materialId, missingPart));
        }

        return new ParsedTags(materials, objects);
    }

    private static Type ResolveDataType(string materialId, string scriptGuid)
    {
        string scriptPath = AssetDatabase.GUIDToAssetPath(scriptGuid);
        MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);
        Type dataType = script == null ? null : script.GetClass();

        if (dataType == null || !typeof(ScriptableObject).IsAssignableFrom(dataType))
            throw new InvalidDataException(
                $"Material {materialId} references unresolved ScriptableObject GUID {scriptGuid}.");
        return dataType;
    }

    private static Type ResolveHandlerType(Type dataType)
    {
        Type[] handlers = TypeCache.GetTypesDerivedFrom<MaterialRandomizerInterface>()
            .Where(type => !type.IsAbstract)
            .Where(type =>
            {
                FieldInfo field = type.GetField(
                    "dataset",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                return field != null && field.FieldType == dataType;
            })
            .ToArray();

        Type[] directHandlers = handlers.Where(type =>
        {
            FieldInfo field = type.GetField(
                "dataset",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.DeclaredOnly);
            return field != null && field.FieldType == dataType;
        }).ToArray();

        if (directHandlers.Length == 1)
            return directHandlers[0];

        string handlerName = dataType.Name.EndsWith("Data", StringComparison.Ordinal)
            ? dataType.Name.Substring(0, dataType.Name.Length - 4) + "Handler"
            : null;
        Type[] namedHandlers = handlers.Where(type => type.Name == handlerName).ToArray();

        if (namedHandlers.Length == 1)
            return namedHandlers[0];
        if (handlers.Length == 1)
            return handlers[0];

        string candidates = handlers.Length == 0
            ? "none"
            : string.Join(", ", handlers.Select(type => type.FullName));
        throw new InvalidDataException(
            $"Could not uniquely resolve a material randomizer handler for {dataType.Name}. " +
            $"Candidates: {candidates}.");
    }

    private static ScriptableObject CreateDataset(
        MaterialDefinition material,
        string dataFolder,
        ICollection<string> warnings)
    {
        ScriptableObject dataset = CreateInstance(material.DataType);
        ApplySettings(dataset, material.Id, material.Settings, warnings);
        return SaveDataset(dataset, $"{dataFolder}/{SafeFileName(material.Id)}.asset");
    }

    private static MissingPartData CreateMissingPartDataset(string dataFolder)
    {
        var dataset = CreateInstance<MissingPartData>();
        dataset.missingChance = 1f;
        return (MissingPartData)SaveDataset(dataset, $"{dataFolder}/missing_part.asset");
    }

    private static void ApplySettings(
        ScriptableObject dataset,
        string materialId,
        JSONNode settings,
        ICollection<string> warnings)
    {
        var serialized = new SerializedObject(dataset);
        serialized.Update();

        foreach (KeyValuePair<string, JSONNode> pair in settings.Linq)
        {
            if (pair.Key.StartsWith("m_", StringComparison.Ordinal))
                continue;

            SerializedProperty property = serialized.FindProperty(pair.Key);
            if (property == null)
            {
                warnings.Add($"{materialId}: ignored unsupported setting {pair.Key}");
                continue;
            }

            if (!ApplySetting(property, pair.Value))
                warnings.Add($"{materialId}: could not apply setting {pair.Key}");
        }

        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static bool ApplySetting(SerializedProperty property, JSONNode value)
    {
        switch (property.propertyType)
        {
            case SerializedPropertyType.Boolean:
                property.boolValue = value.AsBool;
                return true;
            case SerializedPropertyType.Integer:
                property.intValue = value.AsInt;
                return true;
            case SerializedPropertyType.Float:
                property.floatValue = value.AsFloat;
                return true;
            case SerializedPropertyType.Vector2:
                property.vector2Value = new Vector2(value["x"].AsFloat, value["y"].AsFloat);
                return true;
            case SerializedPropertyType.Color:
                property.colorValue = new Color(
                    value["r"].AsFloat,
                    value["g"].AsFloat,
                    value["b"].AsFloat,
                    value["a"].AsFloat);
                return true;
            case SerializedPropertyType.ObjectReference:
                string guid = value["guid"].Value;
                property.objectReferenceValue = string.IsNullOrEmpty(guid)
                    ? null
                    : AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid));
                return string.IsNullOrEmpty(guid) || property.objectReferenceValue != null;
            default:
                return false;
        }
    }

    private static ScriptableObject SaveDataset(ScriptableObject source, string assetPath)
    {
        source.name = Path.GetFileNameWithoutExtension(assetPath);
        ScriptableObject existing = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
        if (existing == null)
        {
            AssetDatabase.CreateAsset(source, assetPath);
            return source;
        }
        if (existing.GetType() != source.GetType())
            throw new InvalidDataException(
                $"Existing dataset {assetPath} has type {existing.GetType().Name}, expected {source.GetType().Name}.");

        EditorUtility.CopySerialized(source, existing);
        existing.name = source.name;
        DestroyImmediate(source);
        EditorUtility.SetDirty(existing);
        return existing;
    }

    private static void SetDataset(GameObject target, Type handlerType, ScriptableObject dataset)
    {
        Component handler = target.GetComponent(handlerType) ?? target.AddComponent(handlerType);
        var serialized = new SerializedObject(handler);
        SerializedProperty property = serialized.FindProperty("dataset");
        if (property == null)
            throw new InvalidDataException($"{handlerType.Name} has no serialized dataset field.");

        property.objectReferenceValue = dataset;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static Dictionary<string, GameObject> ResolveTargets(
        IReadOnlyCollection<TaggedObject> taggedObjects,
        GameObject instance)
    {
        Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
        var result = new Dictionary<string, GameObject>(StringComparer.Ordinal);
        var failures = new List<string>();

        foreach (TaggedObject taggedObject in taggedObjects)
        {
            GameObject[] matches = FindTargets(renderers, taggedObject.Name, false);
            if (matches.Length == 0)
                matches = FindTargets(renderers, taggedObject.Name, true);

            if (matches.Length == 1)
                result.Add(taggedObject.Name, matches[0]);
            else
                failures.Add($"{taggedObject.Name} ({(matches.Length == 0 ? "not found" : "ambiguous")})");
        }

        if (failures.Count == 0)
            return result;

        string importedNames = string.Join(", ", renderers
            .SelectMany(renderer => new[] { renderer.name, GetMesh(renderer)?.name })
            .Where(name => !string.IsNullOrEmpty(name))
            .Distinct()
            .OrderBy(name => name));
        throw new InvalidDataException(
            $"Could not match tagged OBJ objects: {string.Join(", ", failures)}. " +
            $"Imported transform/mesh names: {importedNames}");
    }

    private static GameObject[] FindTargets(Renderer[] renderers, string name, bool normalized)
    {
        string expected = normalized ? name.Replace('.', '_') : name;
        return renderers
            .Where(renderer =>
                MatchName(renderer.name, expected, normalized) ||
                MatchName(GetMesh(renderer)?.name, expected, normalized))
            .Select(renderer => renderer.gameObject)
            .Distinct()
            .ToArray();
    }

    private static bool MatchName(string actual, string expected, bool normalized)
    {
        if (normalized && actual != null)
            actual = actual.Replace('.', '_');
        return string.Equals(actual, expected, StringComparison.Ordinal);
    }

    private static Mesh GetMesh(Renderer renderer)
    {
        return renderer is SkinnedMeshRenderer skinned
            ? skinned.sharedMesh
            : renderer.GetComponent<MeshFilter>()?.sharedMesh;
    }

    private static GameObject EnsureObjectGroups(GameObject modelAsset)
    {
        string sourcePath = AssetDatabase.GetAssetPath(modelAsset);
        bool hasObjects = false;
        bool hasGroups = false;
        foreach (string line in File.ReadLines(sourcePath))
        {
            hasObjects |= IsObjRecord(line, 'o');
            hasGroups |= IsObjRecord(line, 'g');
            if (hasObjects && hasGroups)
                break;
        }

        if (hasGroups)
            return modelAsset;
        if (!hasObjects)
            throw new InvalidDataException("The OBJ contains no named objects or groups.");

        string folder = Path.GetDirectoryName(sourcePath).Replace('\\', '/');
        string groupedPath = $"{folder}/{Path.GetFileNameWithoutExtension(sourcePath)}_grouped.obj";
        using (var writer = new StreamWriter(groupedPath, false))
        {
            foreach (string line in File.ReadLines(sourcePath))
                writer.WriteLine(IsObjRecord(line, 'o') ? "g" + line.Substring(1) : line);
        }

        AssetDatabase.ImportAsset(
            groupedPath,
            ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        GameObject groupedAsset = AssetDatabase.LoadAssetAtPath<GameObject>(groupedPath);
        if (groupedAsset == null)
            throw new IOException($"Unity could not import grouped OBJ {groupedPath}.");
        return groupedAsset;
    }

    private static bool IsObjRecord(string line, char type)
    {
        return line.Length > 2 && line[0] == type && char.IsWhiteSpace(line[1]);
    }

    private static string EnsureFolder(string parent, string name)
    {
        string path = $"{parent}/{name}";
        if (!AssetDatabase.IsValidFolder(path))
            AssetDatabase.CreateFolder(parent, name);
        return path;
    }

    private static string SafeFileName(string value)
    {
        char[] invalid = Path.GetInvalidFileNameChars();
        return new string(value.Select(character => invalid.Contains(character) ? '_' : character).ToArray());
    }

    internal sealed class ImportResult
    {
        public GameObject Prefab { get; }
        public string PrefabPath { get; }
        public int TaggedObjects { get; }

        public ImportResult(GameObject prefab, string prefabPath, int taggedObjects)
        {
            Prefab = prefab;
            PrefabPath = prefabPath;
            TaggedObjects = taggedObjects;
        }
    }

    private sealed class ParsedTags
    {
        public Dictionary<string, MaterialDefinition> Materials { get; }
        public List<TaggedObject> Objects { get; }

        public ParsedTags(
            Dictionary<string, MaterialDefinition> materials,
            List<TaggedObject> objects)
        {
            Materials = materials;
            Objects = objects;
        }
    }

    private sealed class MaterialDefinition
    {
        public string Id { get; }
        public JSONNode Settings { get; }
        public Type DataType { get; }
        public Type HandlerType { get; }
        public ScriptableObject Dataset { get; set; }

        public MaterialDefinition(string id, JSONNode settings, Type dataType, Type handlerType)
        {
            Id = id;
            Settings = settings;
            DataType = dataType;
            HandlerType = handlerType;
        }
    }

    private sealed class TaggedObject
    {
        public string Name { get; }
        public string MaterialId { get; }
        public bool MissingPart { get; }

        public TaggedObject(string name, string materialId, bool missingPart)
        {
            Name = name;
            MaterialId = materialId;
            MissingPart = missingPart;
        }
    }
}
