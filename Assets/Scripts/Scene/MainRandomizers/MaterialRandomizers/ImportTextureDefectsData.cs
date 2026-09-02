

using UnityEngine;

[CreateAssetMenu(fileName = "Untitled Dataset", menuName = "Cad2Render/Material randomizer Data/New import defect texture data")]
public class ImportTextureDefectsData : ScriptableObject
{
    [Header("Input")]
    [Tooltip("Path to textures (relative to Resources dir)")]
    public string TexturesPath;
    [Tooltip("Path to textures (relative to Resources dir)")]
    public string DefectTexturesPath;
}
