using System.Collections;
using UnityEngine;

namespace Assets.Scripts.io.FM
{
    [CreateAssetMenu(fileName = "Untitled Dataset", menuName = "Cad2Render/Export settings/New FM export settings")]

    public class FMExportSettings : ScriptableObject
    {
        public bool exportRender = true;
        public ImageSaver.Extension outputExt = ImageSaver.Extension.png;
        public bool exportSegmentationMasks = true;
        public bool exportCameraData = true ;
        public bool exportSubModels = false;
        public bool exportWorldposition = true;
        public bool exportImagePosition = false;
        public bool exportKeypoints = false;
        public bool exportDepth = true;
        public ImageSaver.Extension depthMapExt = ImageSaver.Extension.png;
        public bool applyGammaCorrection = true;
    }
}