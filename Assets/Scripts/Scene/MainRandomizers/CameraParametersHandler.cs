using UnityEngine;

// Sets the intrinsic parameters of the main camera (focal length, lens shift, sensor size).
// Camera positioning is handled separately by the view randomizers.
[AddComponentMenu("Cad2Render/Camera Parameters Handler")]
public class CameraParametersHandler : RandomizerInterface
{
    public CameraParametersData dataset;
    [InspectorButton("TriggerCloneClicked")]
    public bool clone;
    private void TriggerCloneClicked()
    {
        RandomizerInterface.CloneDataset(ref dataset);
    }

    private Camera _camera;
    private Camera mainCamera
    {
        get
        {
            if (!_camera)
            {
                _camera = Camera.main;
            }
            return _camera;
        }
    }
    private Matrix4x4 cameraMatrix;

    public void Start()
    {
        randomizerType = MainRandomizerData.RandomizerTypes.View;
        this.LinkGui();
        mainCamera.gateFit = Camera.GateFitMode.None;

        cameraMatrix = dataset.cameraMatrix;
        if (dataset.useCameraMatrix)
            SetCameraProperties();
    }

    private void SetCameraProperties()
    {
        float Fx = cameraMatrix[0, 0];
        float Cx = cameraMatrix[0, 2];
        float Cy = cameraMatrix[1, 2];

        mainCamera.usePhysicalProperties = true;
        float width = (float)mainCamera.pixelWidth;
        float height = (float)mainCamera.pixelHeight;

        Vector2 sensorSize = dataset.sensorSize;
        if (sensorSize.x <= 0 || sensorSize.y <= 0)
            sensorSize = mainCamera.sensorSize;

        float f = Fx * sensorSize.x / width;
        float shiftX = -(Cx - width / 2.0f) / width;
        float shiftY = (Cy - height / 2.0f) / height;

        mainCamera.focalLength = f;                            // in mm, ax = f * mx, ay = f * my
        mainCamera.lensShift = new Vector2(shiftX, shiftY);    // W/2,H/w for (0,0), 1.0 shift in full W/H in image plane
        mainCamera.sensorSize = sensorSize;
    }

    public override void Randomize(ref RandomNumberGenerator rng, SceneIteratorInterface sceneIterator = null)
    {
        if (!dataset.importFromSceneIterator)
            return;

        if (sceneIterator == null)
        {
            Debug.LogWarning("Import camera parameters from scene iterator is enabled but the scene iterator == null");
            return;
        }

        var projMat = sceneIterator.GetPose().projMat;
        cameraMatrix[0, 0] = projMat[0, 0];
        cameraMatrix[0, 2] = projMat[0, 2];
        cameraMatrix[1, 1] = projMat[1, 1];
        cameraMatrix[1, 2] = projMat[1, 2];
        SetCameraProperties();
        resetFrameAccumulation();
    }

    public override ScriptableObject getDataset()
    {
        return dataset;
    }
}
