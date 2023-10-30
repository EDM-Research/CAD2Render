
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class MyEditorWindow : EditorWindow
{
    [MenuItem("Tools/Convert Meshes To Prefabs")]
    public static void ShowExample()
    {
        MyEditorWindow wnd = GetWindow<MyEditorWindow>();
        wnd.titleContent = new GUIContent("Convert Meshes To Unity Prefabs");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Description label
        Label description = new Label("Converts all the meshes contained in a specified folder to Unity prefabs compatible with CAD2RENDER.");
        description.style.fontSize = 18;
        description.style.marginBottom = 18;
        description.style.marginTop = 18;
        root.Add(description);

        // VisualElements objects can contain other VisualElement following a tree hierarchy
        TextField textInputMeshes = new TextField("Path to the input meshes folder");
        root.Add(textInputMeshes);
        TextField textInputPrefabs = new TextField("Path to the output prefabs folder");
        root.Add(textInputPrefabs);

        FloatField scaleField = new FloatField("Scale");
        root.Add(scaleField);

        IntegerField xRotationField = new IntegerField("x-rotation");
        IntegerField yRotationField = new IntegerField("y-rotation");
        IntegerField zRotationField = new IntegerField("z-rotation");
        root.Add(xRotationField);
        root.Add(yRotationField);
        root.Add(zRotationField);
        Quaternion rotation = Quaternion.Euler(90, 0, 0);





        // Create button
        Button button = new Button();
        button.name = "convert";
        button.text = "Convert";
        button.clicked += () => HandleButtonClick(textInputMeshes.text, textInputPrefabs.text, scaleField.value, Quaternion.Euler(xRotationField.value, yRotationField.value, zRotationField.value));
        root.Add(button);
    }


    // Button click handler method
    private void HandleButtonClick(string meshesFolder, string prefabsFolder, float scale, Quaternion rotation)
    {

        // This code will be executed when the button is clicked
        ConvertFolder(meshesFolder, prefabsFolder, scale, rotation);


    }


    private void ConvertFolder(string meshesFolder, string prefabsFolder, float scale, Quaternion rotation)
    {
        // This code will be executed when the button is clicked
        string[] aFilePaths = Directory.GetFiles(meshesFolder);


        foreach (string sFilePath in aFilePaths)
        {
            if (Path.GetExtension(sFilePath) == ".OBJ" || Path.GetExtension(sFilePath) == ".obj")
            {
                ConvertMeshToPrefab(sFilePath, prefabsFolder, scale, rotation);
            }


        }
    }

    private void ConvertMeshToPrefab(string sFilePath, string prefabsFolder, float scale, Quaternion rotation)
    {
        Vector3 position = new Vector3(0, 0, 0);


        GameObject modelRootGO = (GameObject)AssetDatabase.LoadMainAssetAtPath(sFilePath);
        GameObject instanceRoot = (GameObject)PrefabUtility.InstantiatePrefab(modelRootGO);

        instanceRoot.transform.localScale = new Vector3(scale, scale, scale);
        instanceRoot.transform.SetPositionAndRotation(position, rotation);

        instanceRoot.AddComponent<Rigidbody>();

        MeshFilter meshFilter = instanceRoot.GetComponentInChildren<MeshFilter>();

        MeshCollider meshCollider = instanceRoot.GetComponentInChildren<MeshCollider>(); // Check if a MeshCollider already exists
        if (meshCollider == null)
        {
            meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>(); // Add Mesh Collider if not already present
            meshCollider.convex = true;
        }
        else
        {
            Debug.Log("meshcollider not found");
        }


        string objName = ExtractObjectName(sFilePath);
        Debug.Log(objName);
        
        GameObject variantRoot = PrefabUtility.SaveAsPrefabAsset(instanceRoot, prefabsFolder + '/' + objName + ".prefab");
    }


    public static string ExtractObjectName(string input)
    {
        // Find the last occurrence of "/"
        int lastSlashIndex = input.LastIndexOf("\\");

        // Find the last occurrence of "."
        int lastDotIndex = input.LastIndexOf(".");

        // Extract the substring between the last "/" and last "."
        string extractedString = input.Substring(lastSlashIndex + 1, lastDotIndex - lastSlashIndex - 1);

        return extractedString;
    }

}