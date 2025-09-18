using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.io.MISC
{
    class MaterialPropertiesExporter : ExportDatasetInterface
    {
        public int resolutionX = 2048;
        public int resolutionY = 2048;
        private ImageSaver imageSaverMaterials;
        GameObject generator;
        public override IEnumerator exportFrame(List<GameObject> instantiated_models, Camera camera, int fileID)
        {
            yield return new WaitForEndOfFrame();
            if (imageSaverMaterials == null) { imageSaverMaterials = new ImageSaver(resolutionX, resolutionY); }
            if(generator == null) { generator = GameObject.FindWithTag("Generator"); }

            int textureCounter = 0;
            foreach (MaterialRandomizeHandler handler in generator.GetComponentsInChildren<MaterialRandomizeHandler>())
            {
                int i = 0;
                MaterialTextures textures = handler.getTextures(i);
                while (textures != null)
                {
                    ++i;
                    ++textureCounter;
                    imageSaverMaterials.Save(textures.get(MaterialTextures.MapTypes.colorMap), getFullPath()  + "albedo/" + fileID.ToString("D6") + "_" + textureCounter.ToString("D6"), ImageSaver.Extension.png, true);
                    imageSaverMaterials.Save(textures.get(MaterialTextures.MapTypes.normalMap), getFullPath() + "normal/" + fileID.ToString("D6") + "_" + textureCounter.ToString("D6"), ImageSaver.Extension.png, false);
                    imageSaverMaterials.Save(textures.get(MaterialTextures.MapTypes.defectMap), getFullPath() + "defectMask/" + fileID.ToString("D6") + "_" + textureCounter.ToString("D6"), ImageSaver.Extension.png, true);
                    imageSaverMaterials.Save(textures.get(MaterialTextures.MapTypes.maskMap), getFullPath() + "maskMap/" + fileID.ToString("D6") + "_" + textureCounter.ToString("D6"), ImageSaver.Extension.png, true);
                    textures = handler.getTextures(i);
                }
            }
        }

        protected override void setupExportPath()
        {
            datasetPrefixPath = "Materials/";
            ensureDir(getFullPath() + "albedo/");
            ensureDir(getFullPath() + "normal/");
            ensureDir(getFullPath() + "defectMask/");
            ensureDir(getFullPath() + "maskMap/");
        }
    }
}
