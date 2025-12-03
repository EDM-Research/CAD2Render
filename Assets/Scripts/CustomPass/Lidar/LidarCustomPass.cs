using System.Collections.Generic;
using UnityEngine.Rendering;
using System.Data;
using UnityEngine.UIElements;
using System.Linq;
using GLTFast.Schema;
using Image = UnityEngine.UIElements.Image;
using Assets.Scripts.io.FM;
using System.Resources;
using UnityEditor.VersionControl;

namespace UnityEngine.Rendering.HighDefinition
{
    /// <summary>
    /// Create segmentation masks for objects with the false color component
    /// 
    /// </summary>
    [System.Serializable]
    class LidarCustomPass : CustomPass
    {
        // Override material
        private Material overrideMaterial = null;
        public LidarExportSettings exportSettings;

        public bool renderNow = false;
        public Camera bakingCamera = null;
        private GameObject cubeCameraGo = null;
        private Camera cubeCamera = null;
        public RenderTexture depth360Texture { get; private set; }
        public RenderTexture color360Texture { get; private set; }
        private RenderTexture cubemap = null;
        //public RenderTexture equirect = null;

        private RayTracingAccelerationStructure raytracingAccelerationStructure = null;
        private RayTracingShader lidarRaySpawner = null;

        static ShaderTagId[] shaderTags;
        Color backgroundColor;

        public LidarCustomPass(LidarExportSettings settings, Camera camera) : base()
        {
            exportSettings = settings;
            overrideMaterial = new Material(Shader.Find("Unlit/RayTracing/worldPosition"));
            lidarRaySpawner = Assets.Scripts.io.MyResourceManager.loadRTXShader("LidarRaytracingShader");
            bakingCamera = camera;
        }

        protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
        {
            Cleanup();

            backgroundColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
            shaderTags = new ShaderTagId[]
            {
                new ShaderTagId("RayTracing"),
            };
            depth360Texture = new RenderTexture(exportSettings.resolution.x, exportSettings.resolution.y, 24, RenderTextureFormat.ARGBFloat);
            depth360Texture.enableRandomWrite = true;
            depth360Texture.Create();
            
            color360Texture = new RenderTexture(exportSettings.resolution.x, exportSettings.resolution.y, 24, RenderTextureFormat.ARGB32);
            color360Texture.enableRandomWrite = true;
            color360Texture.Create();

            cubemap = new RenderTexture(exportSettings.resolution.x, exportSettings.resolution.y, 16);
            cubemap.enableRandomWrite = true;
            cubemap.dimension = TextureDimension.Cube;
            cubemap.useMipMap = false;
            cubemap.autoGenerateMips = false;
            cubemap.Create();

            cubeCameraGo = new GameObject("CubemapCamera");
            cubeCameraGo.hideFlags = HideFlags.HideAndDontSave;
            cubeCamera = cubeCameraGo.AddComponent<Camera>();
            cubeCamera.enabled = false;

            /*
            equirect = new RenderTexture(resolution*2, resolution, 16);
            var GUI = GameObject.FindGameObjectWithTag("GUI");
            if (!GUI)
            {
                Debug.LogWarning("GUI not found while linking buttons");
                return;
            }
            var UIDoc = GUI.GetComponent<UIDocument>();
            if (!UIDoc)
            {
                Debug.LogWarning("UIDocument not found in the GUI while linking buttons");
                return;
            }
            UIDoc.panelSettings.clearColor = true;
            var item = new Image();
            item.image = equirect;
            UIDoc.rootVisualElement.Q<VisualElement>("RenderDisplay").Add(item);*/

        }

        protected ShaderTagId[] GetShaderTagIds()
        {
            return shaderTags;
        }
        


        private void buildAS()
        {
            if (raytracingAccelerationStructure == null)
                raytracingAccelerationStructure = new RayTracingAccelerationStructure();
            //else return;

            raytracingAccelerationStructure.ClearInstances();
            var renderers = GameObject.FindObjectsByType<SkinnedMeshRenderer>(FindObjectsSortMode.None).Cast<Renderer>();
            renderers = renderers.Concat(GameObject.FindObjectsByType< MeshRenderer>(FindObjectsSortMode.None)).ToArray();
            foreach (var rend in renderers)
            {
                var mf = rend.GetComponent<MeshFilter>();
                if (mf == null || mf.sharedMesh == null)
                    continue;

                for (uint subMesh = 0; subMesh < mf.sharedMesh.subMeshCount; ++subMesh)
                {
                    var config = new RayTracingMeshInstanceConfig(mf.sharedMesh, subMesh, overrideMaterial);
                    raytracingAccelerationStructure.AddInstance(config, rend.gameObject.transform.localToWorldMatrix);
                }
            }

            raytracingAccelerationStructure.Build();
        }

        public void prepareExecute()
        {
            if (cubeCamera == null || bakingCamera == null)
                return;
            cubeCamera.transform.position = bakingCamera.transform.position;
            cubeCamera.transform.rotation = bakingCamera.transform.rotation;
            cubeCamera.RenderToCubemap(cubemap);
            renderNow = true;
        }
        /***
         * Render the segmentation mask
         * when targetTextureArray is set it wil render the objects in each slice seperatly without obstructions
         * only one render can be done each frame, so a min of targetTextureArray.volumeDepth +1 frames need to be renderd before all segmentation masks are completed!
         */
        protected override void Execute(CustomPassContext ctx)
        {
            if (!renderNow)
                return;
            renderNow = false;

            buildAS();
            ctx.cmd.SetRayTracingAccelerationStructure(lidarRaySpawner, "_RaytracingAccelerationStructure", raytracingAccelerationStructure);
            ctx.cmd.SetRayTracingShaderPass(lidarRaySpawner, "LidarPass");

            Vector4 position = bakingCamera.transform.position;
            Vector4 lidarAngles = new Vector4(
                Mathf.Deg2Rad * (exportSettings.minHorizonAngle - 90.0f), Mathf.Deg2Rad * (exportSettings.maxHorizonAngle - 90.0f)
                , Mathf.Deg2Rad * exportSettings.minPhi, Mathf.Deg2Rad * exportSettings.maxHorizonAngle);

            ctx.cmd.SetRayTracingVectorParam(lidarRaySpawner, Shader.PropertyToID("originPos"), position);
            ctx.cmd.SetRayTracingVectorParam(lidarRaySpawner, Shader.PropertyToID("lidarAngles"), lidarAngles);
            ctx.cmd.SetRayTracingMatrixParam(lidarRaySpawner, Shader.PropertyToID("g_InvViewMatrix"), bakingCamera.cameraToWorldMatrix);
            ctx.cmd.SetRayTracingTextureParam(lidarRaySpawner, Shader.PropertyToID("worldPosTexture"), depth360Texture);
            ctx.cmd.SetRayTracingTextureParam(lidarRaySpawner, Shader.PropertyToID("worldColorTexture"), color360Texture);
            ctx.cmd.SetRayTracingTextureParam(lidarRaySpawner, Shader.PropertyToID("g_EnvTex"), cubemap);

            ctx.cmd.DispatchRays(lidarRaySpawner, "MainRayGenShader", (uint)depth360Texture.width, (uint)depth360Texture.height, 1);

            //cubemap.ConvertToEquirect(equirect);
        }

        /// <inheritdoc />
        public override IEnumerable<Material> RegisterMaterialForInspector() { yield return overrideMaterial; }

        protected override void Cleanup()
        {
            color360Texture?.Release();
            depth360Texture?.Release();
            cubemap?.Release();
        }
    }


}
