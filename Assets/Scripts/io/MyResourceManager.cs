using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.Scripts.io
{

    internal static class MyResourceManager
    {
        private static Dictionary<string, UnityEngine.Object[]> LoadedData = new Dictionary<string, UnityEngine.Object[]>();

        private static string makeHashCode(string path, Type type)
        {
            return type.ToString() + "?" +  path;// ?  should be an illegal character in a path name so should never make conflicts
        }

        public static T[] LoadAll<T>(string  path)
        {
            UnityEngine.Object[] list;
            if (!LoadedData.TryGetValue(makeHashCode(path, typeof(T)), out list))
            {
                if (path != "")
                    list = Resources.LoadAll(path, typeof(T));
                else
                    list = new UnityEngine.Object[0];
                LoadedData.Add(makeHashCode(path, typeof(T)), list);
            }
            return list.Cast<T>().ToArray();
        }

        private static Dictionary<string, ComputeShader> loadedComputeShaders = new Dictionary<string, ComputeShader>();
        public static ComputeShader loadComputeShader(string shaderName)
        {
            ComputeShader shader;

            if (!loadedComputeShaders.TryGetValue(shaderName, out shader))
            {
                shader = (ComputeShader)Resources.Load("Shaders/ComputeShaders/" + shaderName);
                loadedComputeShaders.Add(shaderName, shader);
            }
            return shader;
        }

        private static Dictionary<string, RayTracingShader> loadedRTXShaders = new Dictionary<string, RayTracingShader>();
        public static RayTracingShader loadRTXShader(string shaderName)
        {
            RayTracingShader shader;

            if (!loadedRTXShaders.TryGetValue(shaderName, out shader))
            {
                shader = (RayTracingShader)Resources.Load("Shaders/RaytracingShaders/" + shaderName);
                loadedRTXShaders.Add(shaderName, shader);
            }
            return shader;
        }
    }
}
