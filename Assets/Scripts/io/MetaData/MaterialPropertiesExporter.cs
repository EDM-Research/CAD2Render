using RayFire;
using SimpleJSON;
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
        private ImageSaver imageSaverMaterials;
        GameObject generator;
        public override IEnumerator exportFrame(List<GameObject> instantiated_models, Camera camera, int fileID)
        {
            yield return new WaitForEndOfFrame();
            if(generator == null) { generator = GameObject.FindWithTag("Generator"); }

            int textureCounter = 0;
            foreach (MaterialRandomizeHandler handler in generator.GetComponentsInChildren<MaterialRandomizeHandler>())
            {
                int i = 0;
                MaterialTextures textures = handler.getTextures(i);
                if (imageSaverMaterials == null) { imageSaverMaterials = new ImageSaver(textures.resolution.x, textures.resolution.y); }
                while (textures != null)
                {
                    ++i;
                    ++textureCounter;

                    switch (textures.rend.material.shader.name)
                    {
                        case "HDRP/Lit":
                            exportLidShader(textures, fileID, textureCounter); break;
                        case "HDRP/LayeredLit":
                            exportLayeredLitShader(textures, fileID, textureCounter); break;
                        default:
                            Debug.LogWarning("Unsuported material export: " + textures.rend.material.shader.name);
                            break;
                    }

                    textures = handler.getTextures(i);
                }
            }
        }

        private void exportLayeredLitShader(MaterialTextures textures, int fileID, int textureCounter)
        {
            var materialparameters = new JSONObject();
            string pathPrefix = getFullPath() + fileID.ToString() + "_" + textureCounter.ToString("D6") + "/";
            ensureDir(pathPrefix);
            ensureDir(pathPrefix + "albedo/");
            ensureDir(pathPrefix + "normal/");
            ensureDir(pathPrefix + "maskMap/");

            for (int layerIndex = 0; layerIndex < textures.GetCurrentLinkedInt("_LayerCount"); ++layerIndex)
            {
                string filename = layerIndex.ToString();

                imageSaverMaterials.Save(textures.GetCurrentLinkedTexture("_BaseColorMap" + layerIndex.ToString()), pathPrefix + "albedo/" + filename, ImageSaver.Extension.png, true);
                imageSaverMaterials.Save(textures.GetCurrentLinkedTexture("_NormalMap" + layerIndex.ToString()), pathPrefix + "normal/" + filename, ImageSaver.Extension.png, false);
                imageSaverMaterials.Save(textures.GetCurrentLinkedTexture("_MaskMap" + layerIndex.ToString()), pathPrefix + "maskMap/" + filename, ImageSaver.Extension.png, true);

                materialparameters["_BaseColorMap" + layerIndex.ToString() + "_ST"] = textures.GetCurrentLinkedVector("_BaseColorMap" + layerIndex.ToString() + "_ST");
                materialparameters["_BaseColor" + layerIndex.ToString()] = textures.GetCurrentLinkedVector("_BaseColor" + layerIndex.ToString());
                materialparameters["_Metallic" + layerIndex.ToString()] = textures.GetCurrentLinkedFloat("_Metallic" + layerIndex.ToString());
                materialparameters["_Smoothness" + layerIndex.ToString()] = textures.GetCurrentLinkedFloat("_Smoothness" + layerIndex.ToString());

            }

            imageSaverMaterials.Save(textures.get(MaterialTextures.MapTypes.defectMap), pathPrefix + "defectMask", ImageSaver.Extension.png, true);
            imageSaverMaterials.Save(textures.get(MaterialTextures.MapTypes.layerMask), pathPrefix + "layerMask", ImageSaver.Extension.png, true);
            materialparameters["_LayerMaskMap_ST"] = textures.GetCurrentLinkedVector("_LayerMaskMap_ST");
            appendToJSON(pathPrefix + "materialProperties.json", materialparameters.ToString(), true);


        }
        private void exportLidShader(MaterialTextures textures, int fileID, int textureCounter)
        {
            imageSaverMaterials.Save(textures.get(MaterialTextures.MapTypes.colorMap), getFullPath() + "albedo/" + fileID.ToString("D6") + "_" + textureCounter.ToString("D6"), ImageSaver.Extension.png, true);
            imageSaverMaterials.Save(textures.get(MaterialTextures.MapTypes.normalMap), getFullPath() + "normal/" + fileID.ToString("D6") + "_" + textureCounter.ToString("D6"), ImageSaver.Extension.png, false);
            imageSaverMaterials.Save(textures.get(MaterialTextures.MapTypes.defectMap), getFullPath() + "defectMask/" + fileID.ToString("D6") + "_" + textureCounter.ToString("D6"), ImageSaver.Extension.png, true);
            imageSaverMaterials.Save(textures.get(MaterialTextures.MapTypes.maskMap), getFullPath() + "maskMap/" + fileID.ToString("D6") + "_" + textureCounter.ToString("D6"), ImageSaver.Extension.png, true);
        }
        protected override void setupCustomPasses(Camera mainCamera){}

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
