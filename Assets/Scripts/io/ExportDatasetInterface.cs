using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace Assets.Scripts.io
{
    public abstract class ExportDatasetInterface : MonoBehaviour
    {

        private int sceneId;
        private string baseOutputPath;
        protected string datasetPrefixPath;

        private void setupExportPath(string outputPath, int setSceneId) {
            baseOutputPath = outputPath; 
            sceneId = setSceneId; 
            setupExportPath(); 
        }
        public void incrementSceneId() { 
            sceneId++;  
            setupExportPath(); 
        }

        public void setup(Camera mainCamera, Camera bakingCamera, string outputPath, int setSceneId)
        {
            setupExportPath(outputPath, setSceneId);
            setCustomVolume(bakingCamera);

            setupCustomPasses(mainCamera);
        }
        protected static CustomPassVolume customPassVolume;
        private void setCustomVolume(Camera bakingCamera)
        {
            if (customPassVolume != null)
                return;

            var customPasses = GameObject.FindGameObjectWithTag("CustomPass");
            customPassVolume = customPasses?.GetComponent<CustomPassVolume>();
            if (customPassVolume != null)
                return;

            if (customPasses == null)
            {
                customPasses = new GameObject();
                customPasses.name = "CustomPasses";
                customPasses.tag = "CustomPass";
            }
            customPassVolume = customPasses.AddComponent<CustomPassVolume>();
            customPassVolume.injectionPoint = CustomPassInjectionPoint.BeforeRendering;
            customPassVolume.targetCamera = bakingCamera;
        }
        protected abstract void setupExportPath();
        protected abstract void setupCustomPasses(Camera mainCamera);

        public virtual List<(string, RenderTexture)> getTextureOutputs() { return new List<(string, RenderTexture)>(); }

        public abstract IEnumerator exportFrame(List<GameObject> instantiated_models, Camera camera, int fileID);
        public virtual void onRenderStart(Camera camera, int fileID) { }

        protected string getFullPath()
        {
            if (string.IsNullOrEmpty(baseOutputPath))
            {
                Debug.LogError("The output path is not specified");
                baseOutputPath = "../renderings/newDataset/";
            }
            if (baseOutputPath[baseOutputPath.Length - 1] != '/' && baseOutputPath[baseOutputPath.Length - 1] != '\\')
                baseOutputPath += "/";
            return baseOutputPath + datasetPrefixPath + String.Format("{0:000000}/", sceneId);
        }
        protected static void ensureDir(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        protected static void appendToJSON(string filename, string text, bool first_element)
        {
            // Append frame metadata to json file
            // if first frame, create the data, otherwise append to json
            // this approach will truncate the last '}' out of the json file and appends an element to the dictionary by adding a ','

            if (first_element)
            {
                using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
                {
                    StreamWriter sw = new StreamWriter(fs);
                    sw.Write(text);
                    sw.Flush();
                }
            }
            else
            {
                using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    StreamWriter sw = new StreamWriter(fs);
                    long endPoint = fs.Length;
                    // Set the stream position to the end of the file.   
                    fs.Seek(endPoint - 1, SeekOrigin.Begin);
                    sw.WriteLine(',');
                    sw.Write(text.Substring(1));
                    sw.Flush();
                }
            }

        }
    }
}
