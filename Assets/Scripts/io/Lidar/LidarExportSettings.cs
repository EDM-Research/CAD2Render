using System.Collections;
using UnityEngine;

namespace Assets.Scripts.io.FM
{
    [CreateAssetMenu(fileName = "Untitled Dataset", menuName = "Cad2Render/Export settings/New Lidar export settings")]

    public class LidarExportSettings : ScriptableObject
    {
        public Vector2Int resolution;
        public bool binary = true;

        [Tooltip("Min angle for the lidar with 0 being straight down,90 flat with the horizon and 180 straight up")]
        [Range(0.0f, 180.0f)]
        public float minHorizonAngle = 25.0f;
        [Tooltip("Max angle for the lidar with 0 being straight down,90 flat with the horizon and 180 straight up")]
        [Range(0.0f, 180.0f)]
        public float maxHorizonAngle = 180.0f;

        [Tooltip("Min phi")]
        [Range(-180.0f, 180.0f)]
        public float minPhi = -180.0f;
        [Tooltip("Max phi")]
        [Range(-180.0f, 180.0f)]
        public float maxPhi = 180.0f;
    }
}