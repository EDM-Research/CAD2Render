using System.Collections;
using UnityEngine;

namespace Assets.Scripts.io
{
    public enum LidarProjection
    {
        Spherical = 0, // equal angle steps, like a spinning LiDAR scanner
        Pinhole = 1,   // even grid on a flat image plane, like a depth camera (RealSense)
    }

    public enum LidarCoordinateSystem
    {
        World = 0,  // Unity world coordinates, Unity units
        Camera = 1, // camera at the origin, OpenCV axes (+X right, +Y down, +Z forward), millimetres (same frame as the BOP export)
    }

    [CreateAssetMenu(fileName = "Untitled Dataset", menuName = "Cad2Render/Export settings/New Lidar export settings", order = 3)]
    public class LidarExportSettings : ScriptableObject
    {
        [Tooltip("Number of samples horizontally (x) and vertically (y)")]
        public Vector2Int resolution;
        [Tooltip("Spherical = equal angle steps (spinning LiDAR). Pinhole = even pixel grid on an image plane (depth camera such as a RealSense).")]
        public LidarProjection projection = LidarProjection.Spherical;
        public bool binary = true;
        [Tooltip("World = Unity world coordinates in Unity units. Camera = relative to the camera with OpenCV axes (+X right, +Y down, +Z forward) in mm, the same frame as the BOP export.")]
        public LidarCoordinateSystem coordinateSystem = LidarCoordinateSystem.World;

        [Tooltip("Colour the points with 6 path-traced views around the scanner. They build up samples during the render frames like the main camera, which costs about 6x extra render time. Off = plain rasterized views.")]
        public bool pathTracedColor = true;
        [Tooltip("Resolution of each of the 6 colour views. 0 = the largest scan resolution.")]
        [Min(0)]
        public int colorFaceResolution = 0;

        [Tooltip("Keep scanning for the Distance preview in the GUI (also while not recording), every previewUpdateInterval frames.")]
        public bool livePreview = true;
        [Tooltip("Live preview: rebuild the ray tracing scene and scan only every N frames (the Distance preview updates at that rate). Export scans always run on an up-to-date scene.")]
        [Min(1)]
        [UnityEngine.Serialization.FormerlySerializedAs("previewSceneUpdateInterval")]
        public int previewUpdateInterval = 10;
        [Tooltip("Distance preview range in mm: black at x (near), white at y (far). Rays that hit nothing are dark red.")]
        public Vector2 distancePreviewRange = new Vector2(0.0f, 3000.0f);

        [Tooltip("Min vertical angle, relative to the camera: 0 = straight down from the camera, 90 = the look direction, 180 = straight up from the camera")]
        [Range(0.0f, 180.0f)]
        public float minHorizonAngle = 25.0f;
        [Tooltip("Max vertical angle, relative to the camera: 0 = straight down from the camera, 90 = the look direction, 180 = straight up from the camera")]
        [Range(0.0f, 180.0f)]
        public float maxHorizonAngle = 180.0f;

        [Tooltip("Min horizontal angle, relative to the camera: 0 = the look direction, positive = to the right")]
        [Range(-180.0f, 180.0f)]
        public float minPhi = -180.0f;
        [Tooltip("Max horizontal angle, relative to the camera: 0 = the look direction, positive = to the right")]
        [Range(-180.0f, 180.0f)]
        public float maxPhi = 180.0f;
    }
}