using UnityEngine;

[HelpURL("Documentation/DatasetInformation.html")]
[CreateAssetMenu(fileName = "Untitled Dataset", menuName = "Cad2Render/Camera settings/New Camera Parameters Data", order = 5)]
public class CameraParametersData : ScriptableObject
{
    [Tooltip("Use the intrinsics (projection matrix) from the scene iterator (e.g. a bop import)")]
    [UnityEngine.Serialization.FormerlySerializedAs("importFromBOP")]
    public bool importFromSceneIterator = false;

    [Header("Intrinsics")]
    [Tooltip("Apply the camera matrix below (Fx, Fy, Cx, Cy in pixels) to the main camera")]
    public bool useCameraMatrix = false;
    public Matrix4x4 cameraMatrix = new Matrix4x4();
    [Tooltip("Physical sensor size in mm. Leave at (0,0) to keep the camera's current sensor size")]
    public Vector2 sensorSize = new Vector2(0.0f, 0.0f);
}
