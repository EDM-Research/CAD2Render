using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Assets.Scripts.io.ExportDatasetInterface;

namespace Assets.Scripts.io.BOP
{
    [CreateAssetMenu(fileName = "Untitled Dataset", menuName = "Cad2Render/Export settings/New BOP export settings")]
    public class BOPExportSettings : ScriptableObject
    {
        [Tooltip("Extension of generated images and masks.")]
        public ImageSaver.Extension outputExt = ImageSaver.Extension.jpg;

        public bool exportRender = true;//
        public bool exportSegmentationMasks = true;//
        [Tooltip("If more objects need to be exported, a single colored segmentation mask is exported instead of seperate masks for each object.")]
        public int maxSegmentationObjects = 20;
        public bool exportCameraData = true;//
        public bool exportObjectData = true;//
        public bool exportKeypoints = false;//
        public bool exportDepth = true;//

        [Tooltip("Format of depth map export")]
        public ImageSaver.Extension depthMapExt = ImageSaver.Extension.png;
    }
}
