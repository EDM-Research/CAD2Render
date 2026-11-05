using System.Collections.Generic;
using UnityEngine.Rendering;
using System.Data;
using UnityEngine.UIElements;
using System.Linq;
using GLTFast.Schema;
using Image = UnityEngine.UIElements.Image;
using Assets.Scripts.io;
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
        private int pendingScanFileId = 0;

        // Called (on the main thread, a few frames after the export scan) with the scan result:
        // file id, camera pose (camera coordinates: OpenCV axes, mm -> Unity world), colour per ray (sRGB bytes),
        // and position per ray (xyz in the export frame, w = 1 for a hit, 0 for a miss).
        public event System.Action<int, Matrix4x4, Color32[], Color[]> ScanCompleted;

        public Camera bakingCamera = null;
        public RenderTexture LidarCameraPositionTexture { get; private set; }
        public RenderTexture LidarCameraColorTexture { get; private set; }
        // World-space equirectangular preview of the colour views (GUI preview list)
        public RenderTexture equirect = null;
        // Greyscale distance image of the latest scan (GUI preview list)
        public RenderTexture distancePreview { get; private set; }
        private int framesSinceSceneBuild = int.MaxValue;

        // 6 90 degree cameras (one per cube face, rotating with the scanning camera) that provide the point colours.
        // Each keeps its own path tracing history, so they build up samples during the render frames.
        private static readonly (Vector3 forward, Vector3 up)[] faceDirections =
        {
            (Vector3.right, Vector3.up), (Vector3.left, Vector3.up),
            (Vector3.up, Vector3.back), (Vector3.down, Vector3.forward),
            (Vector3.forward, Vector3.up), (Vector3.back, Vector3.up),
        };
        private GameObject faceRig = null;
        private readonly Camera[] faceCameras = new Camera[6];
        private readonly RenderTexture[] faceTextures = new RenderTexture[6];
        private readonly Matrix4x4[] faceViewProj = new Matrix4x4[6];
        private RenderTexture faceArray = null;
        private ComputeShader previewShader = null;
        private ComputeShader distancePreviewShader = null;

        private RayTracingAccelerationStructure raytracingAccelerationStructure = null;
        private RayTracingShader lidarRaySpawner = null;
        private readonly Dictionary<SkinnedMeshRenderer, Mesh> bakedSkinnedMeshes = new Dictionary<SkinnedMeshRenderer, Mesh>();

        static ShaderTagId[] shaderTags;

        public LidarCustomPass(LidarExportSettings settings, Camera camera) : base()
        {
            exportSettings = settings;
            overrideMaterial = new Material(Shader.Find("Unlit/RayTracing/worldPosition"));
            lidarRaySpawner = MyResourceManager.loadRTXShader("LidarRaytracingShader");
            bakingCamera = camera;

            int width = Mathf.Max(1, exportSettings.resolution.x);
            int height = Mathf.Max(1, exportSettings.resolution.y);
            int res = Mathf.Max(width, height);

            LidarCameraPositionTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
            LidarCameraPositionTexture.enableRandomWrite = true;
            LidarCameraPositionTexture.Create();

            // Linear (no automatic conversion): the shader writes sRGB-encoded bytes, which go straight into the .ply
            LidarCameraColorTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            LidarCameraColorTexture.enableRandomWrite = true;
            LidarCameraColorTexture.Create();

            distancePreview = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear) { enableRandomWrite = true };
            distancePreview.Create();

            int faceRes = exportSettings.colorFaceResolution > 0 ? exportSettings.colorFaceResolution : res;
            setupFaceCameras(camera, faceRes);

            // Linear format so the linear colours written by the preview shader are displayed as-is
            equirect = new RenderTexture(faceRes * 2, faceRes, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            equirect.enableRandomWrite = true;
            equirect.Create();
            previewShader = MyResourceManager.loadComputeShader("LidarEnvironmentPreview");
            distancePreviewShader = MyResourceManager.loadComputeShader("LidarDistancePreview");
        }

        protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
        {
            shaderTags = new ShaderTagId[]
            {
                new ShaderTagId("RayTracing"),
            };

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

            foreach (var rend in GameObject.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
            {
                if (!rend.enabled)
                    continue;
                var mf = rend.GetComponent<MeshFilter>();
                if (mf == null || mf.sharedMesh == null)
                    continue;

                addMeshInstance(mf.sharedMesh, rend.transform.localToWorldMatrix);
            }

            // Skinned meshes have no MeshFilter: bake their current pose into a regular mesh.
            var usedSkinnedRenderers = new HashSet<SkinnedMeshRenderer>();
            foreach (var rend in GameObject.FindObjectsByType<SkinnedMeshRenderer>(FindObjectsSortMode.None))
            {
                if (!rend.enabled || rend.sharedMesh == null)
                    continue;
                if (!rend.sharedMesh.isReadable)
                {
                    Debug.LogWarning($"[Lidar] Skinned mesh '{rend.sharedMesh.name}' on '{rend.name}' is skipped: enable Read/Write in its model import settings.", rend);
                    continue;
                }

                if (!bakedSkinnedMeshes.TryGetValue(rend, out Mesh baked) || baked == null)
                {
                    baked = new Mesh { name = rend.name + "_lidarBaked", hideFlags = HideFlags.HideAndDontSave };
                    bakedSkinnedMeshes[rend] = baked;
                }
                rend.BakeMesh(baked, true);
                usedSkinnedRenderers.Add(rend);
                if (baked.vertexCount == 0)
                {
                    Debug.LogWarning($"[Lidar] Baking skinned mesh on '{rend.name}' produced no vertices.", rend);
                    continue;
                }

                addMeshInstance(baked, bakedMeshToWorld(rend, baked));
            }
            releaseUnusedBakedMeshes(usedSkinnedRenderers);

            raytracingAccelerationStructure.Build();
        }

        // A baked mesh is relative to the renderer's transform, but whether its scale is included is easy to get wrong
        // (Blender FBX imports often carry a 100 or 0.01 scale). Pick the matrix that puts the baked mesh where the
        // renderer's world bounds say it is.
        private Matrix4x4 bakedMeshToWorld(SkinnedMeshRenderer rend, Mesh baked)
        {
            var t = rend.transform;
            Matrix4x4 withScale = t.localToWorldMatrix;
            Matrix4x4 withoutScale = Matrix4x4.TRS(t.position, t.rotation, Vector3.one);

            baked.RecalculateBounds();
            Bounds target = rend.bounds;
            float errorWithScale = boundsError(transformBounds(baked.bounds, withScale), target);
            float errorWithoutScale = boundsError(transformBounds(baked.bounds, withoutScale), target);
            return errorWithScale <= errorWithoutScale ? withScale : withoutScale;
        }

        private static float boundsError(Bounds a, Bounds b)
        {
            return (a.center - b.center).magnitude + (a.size - b.size).magnitude;
        }

        private static Bounds transformBounds(Bounds local, Matrix4x4 m)
        {
            var result = new Bounds(m.MultiplyPoint3x4(local.min), Vector3.zero);
            for (int i = 0; i < 8; ++i)
            {
                var corner = new Vector3(
                    (i & 1) == 0 ? local.min.x : local.max.x,
                    (i & 2) == 0 ? local.min.y : local.max.y,
                    (i & 4) == 0 ? local.min.z : local.max.z);
                result.Encapsulate(m.MultiplyPoint3x4(corner));
            }
            return result;
        }

        private void addMeshInstance(Mesh mesh, Matrix4x4 localToWorld)
        {
            for (uint subMesh = 0; subMesh < mesh.subMeshCount; ++subMesh)
            {
                var config = new RayTracingMeshInstanceConfig(mesh, subMesh, overrideMaterial);
                raytracingAccelerationStructure.AddInstance(config, localToWorld);
            }
        }

        // Destroy baked meshes of skinned renderers that were removed from the scene
        private void releaseUnusedBakedMeshes(HashSet<SkinnedMeshRenderer> used)
        {
            var stale = bakedSkinnedMeshes.Keys.Where(r => r == null || !used.Contains(r)).ToList();
            foreach (var rend in stale)
            {
                Object.DestroyImmediate(bakedSkinnedMeshes[rend]);
                bakedSkinnedMeshes.Remove(rend);
            }
        }

        private void setupFaceCameras(Camera source, int faceRes)
        {
            faceRig = new GameObject("LidarColorCameras");
            faceRig.hideFlags = HideFlags.HideAndDontSave;
            // Child of the scanning camera: same position as the ray origin, and the forward face looks along
            // the look direction (a RealSense field of view then falls inside that one face, without seams).
            faceRig.transform.SetParent(source.transform, false);

            faceArray = new RenderTexture(faceRes, faceRes, 0, RenderTextureFormat.ARGB32);
            faceArray.dimension = TextureDimension.Tex2DArray;
            faceArray.volumeDepth = 6;
            faceArray.useMipMap = false;
            faceArray.Create();

            for (int i = 0; i < 6; ++i)
            {
                var go = new GameObject("LidarColorCamera_" + i);
                go.hideFlags = HideFlags.HideAndDontSave;
                go.transform.SetParent(faceRig.transform, false);

                var cam = go.AddComponent<Camera>();
                cam.CopyFrom(source); // background, clear flags, clip planes, culling mask
                go.transform.localPosition = Vector3.zero; // CopyFrom also copies the source's transform
                go.transform.localRotation = Quaternion.LookRotation(faceDirections[i].forward, faceDirections[i].up);
                cam.usePhysicalProperties = false;
                cam.ResetProjectionMatrix();
                cam.fieldOfView = 90.0f;
                cam.aspect = 1.0f;

                faceTextures[i] = new RenderTexture(faceRes, faceRes, 24, RenderTextureFormat.ARGB32);
                faceTextures[i].Create();
                cam.targetTexture = faceTextures[i];
                cam.enabled = false;

                setupFaceFrameSettings(go, source);
                faceCameras[i] = cam;
            }
        }

        private void setupFaceFrameSettings(GameObject cameraObject, Camera source)
        {
            if (!cameraObject.TryGetComponent(out HDAdditionalCameraData hdData))
                hdData = cameraObject.AddComponent<HDAdditionalCameraData>();
            if (source.TryGetComponent(out HDAdditionalCameraData sourceHdData))
                sourceHdData.CopyTo(hdData); // HDRP clear mode / sky, volume layers, exposure target
            hdData.customRenderingSettings = true;

            // Lens effects don't belong in a colour lookup
            var disabled = new List<FrameSettingsField>
            {
                FrameSettingsField.MotionBlur,
                FrameSettingsField.DepthOfField,
                FrameSettingsField.ChromaticAberration,
                FrameSettingsField.LensDistortion,
                FrameSettingsField.Vignette,
            };
            if (!exportSettings.pathTracedColor)
                disabled.Add(FrameSettingsField.RayTracing); // turns off the path tracer for these cameras only

            foreach (var field in disabled)
            {
                hdData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)field] = true;
                hdData.renderingPathCustomFrameSettings.SetEnabled(field, false);
            }
        }

        // The colour cameras render whenever the scanning camera does: every frame while not recording (live preview),
        // and during the render frames while recording, where they build up path tracing samples like the main camera.
        // Call once per frame, after the scanning camera has been switched on or off.
        public void setColorCamerasEnabled(bool enabled)
        {
            foreach (var cam in faceCameras)
                if (cam != null && cam.enabled != enabled)
                    cam.enabled = enabled;
        }

        // Unity world -> camera frame of the BOP export (see GeometryUtils.getModelViewMatrix):
        // camera at the origin, +X right, +Y down, +Z forward, millimetres. The camera's scale is ignored.
        private static Matrix4x4 worldToExportCamera(Transform camera)
        {
            Matrix4x4 cameraPose = Matrix4x4.TRS(camera.position, camera.rotation, Vector3.one);
            Matrix4x4 flipY = Matrix4x4.Scale(new Vector3(1, -1, 1));
            Matrix4x4 toMm = Matrix4x4.Scale(Vector3.one * GeometryUtils.convertUnityToMm(1.0f));
            return toMm * flipY * cameraPose.inverse;
        }

        // Call at render end: scan during this frame's render. The result arrives later through ScanCompleted.
        public void requestScan(int fileId)
        {
            pendingScanFileId = fileId;
            renderNow = true;
        }

        // Queue the readback of the export scan on the same command list, right after the scan,
        // so no later (preview) scan can overwrite the textures before they are copied.
        private void queueScanReadback(CommandBuffer cmd, int fileId, Matrix4x4 cameraToWorld)
        {
            Color32[] colors = null;
            Color[] positions = null;
            bool failed = false;

            void tryComplete()
            {
                if (!failed && colors != null && positions != null)
                    ScanCompleted?.Invoke(fileId, cameraToWorld, colors, positions);
            }

            // GetData is only valid inside the callback, so copy it to managed arrays right away
            cmd.RequestAsyncReadback(LidarCameraColorTexture, 0, TextureFormat.RGBA32, request =>
            {
                if (request.hasError) { failed = true; Debug.LogError("[Lidar] Colour readback failed for file " + fileId); return; }
                colors = request.GetData<Color32>().ToArray();
                tryComplete();
            });
            cmd.RequestAsyncReadback(LidarCameraPositionTexture, 0, TextureFormat.RGBAFloat, request =>
            {
                if (request.hasError) { failed = true; Debug.LogError("[Lidar] Position readback failed for file " + fileId); return; }
                positions = request.GetData<Color>().ToArray();
                tryComplete();
            });
        }

        private void updateFaceInputs(CommandBuffer cmd)
        {
            for (int i = 0; i < 6; ++i)
            {
                faceViewProj[i] = faceCameras[i].projectionMatrix * faceCameras[i].worldToCameraMatrix;
                cmd.CopyTexture(faceTextures[i], 0, 0, faceArray, i, 0);
            }
        }

        // Greyscale distance image of the scan that was just dispatched (reads the scan's position texture)
        // scannerPosition: the scanner position in the export frame (Unity world, or the camera origin)
        private void updateDistancePreview(CommandBuffer cmd, Vector3 scannerPosition)
        {
            if (distancePreviewShader == null)
                return;
            float exportUnitsToMm = exportSettings.coordinateSystem == LidarCoordinateSystem.Camera
                ? 1.0f
                : GeometryUtils.convertUnityToMm(1.0f);

            int kernel = distancePreviewShader.FindKernel("CSMain");
            cmd.SetComputeTextureParam(distancePreviewShader, kernel, "positionTexture", LidarCameraPositionTexture);
            cmd.SetComputeTextureParam(distancePreviewShader, kernel, "distancePreviewTexture", distancePreview);
            cmd.SetComputeIntParams(distancePreviewShader, "previewSize", distancePreview.width, distancePreview.height);
            cmd.SetComputeVectorParam(distancePreviewShader, "scannerPosition", scannerPosition);
            cmd.SetComputeFloatParam(distancePreviewShader, "exportUnitsToMm", exportUnitsToMm);
            cmd.SetComputeVectorParam(distancePreviewShader, "distanceRangeMm", exportSettings.distancePreviewRange);
            cmd.DispatchCompute(distancePreviewShader, kernel, (distancePreview.width + 7) / 8, (distancePreview.height + 7) / 8, 1);
        }

        private void updatePreview(CommandBuffer cmd)
        {
            if (previewShader == null)
                return;
            int kernel = previewShader.FindKernel("CSMain");
            cmd.SetComputeTextureParam(previewShader, kernel, "g_FaceTex", faceArray);
            cmd.SetComputeMatrixArrayParam(previewShader, "g_FaceViewProj", faceViewProj);
            cmd.SetComputeTextureParam(previewShader, kernel, "previewTexture", equirect);
            cmd.SetComputeIntParams(previewShader, "previewSize", equirect.width, equirect.height);
            cmd.DispatchCompute(previewShader, kernel, (equirect.width + 7) / 8, (equirect.height + 7) / 8, 1);
        }
        /***
         * Render the segmentation mask
         * when targetTextureArray is set it wil render the objects in each slice seperatly without obstructions
         * only one render can be done each frame, so a min of targetTextureArray.volumeDepth +1 frames need to be renderd before all segmentation masks are completed!
         */
        protected override void Execute(CustomPassContext ctx)
        {
            // Copy the colour views and refresh the preview only when they have new content: while the colour
            // cameras render (not recording, or the render frames while recording), and for an export scan.
            // During the physics frames the cameras are off and their images don't change.
            bool colorCamerasOn = faceCameras[0] != null && faceCameras[0].enabled;
            if (colorCamerasOn || renderNow)
            {
                updateFaceInputs(ctx.cmd);
                updatePreview(ctx.cmd);
            }

            // Export scan when requested, otherwise a live preview scan (if enabled)
            bool exportScan = renderNow;
            if (!exportScan && !exportSettings.livePreview)
                return;
            renderNow = false;

            // Export scans always run on an up-to-date scene. Preview scans (scene rebuild + scan) only run
            // every N frames; in between, the Distance preview keeps showing the last scan.
            int interval = Mathf.Max(1, exportSettings.previewUpdateInterval);
            bool previewDue = raytracingAccelerationStructure == null || framesSinceSceneBuild >= interval;
            if (!exportScan && !previewDue)
            {
                framesSinceSceneBuild++;
                return;
            }
            buildAS();
            framesSinceSceneBuild = 1;

            ctx.cmd.SetRayTracingAccelerationStructure(lidarRaySpawner, "_RaytracingAccelerationStructure", raytracingAccelerationStructure);
            ctx.cmd.SetRayTracingShaderPass(lidarRaySpawner, "LidarPass"); 

            Vector4 position = bakingCamera.transform.position;
            float minVertical = exportSettings.minHorizonAngle - 90.0f;
            float maxVertical = exportSettings.maxHorizonAngle - 90.0f;
            float minHorizontal = exportSettings.minPhi;
            float maxHorizontal = exportSettings.maxPhi;
            bool pinhole = exportSettings.projection == LidarProjection.Pinhole;
            if (pinhole)
            {
                // An image plane can't reach 90 degrees or more from the look direction
                const float limit = 89.0f;
                if (Mathf.Abs(minVertical) > limit || Mathf.Abs(maxVertical) > limit || Mathf.Abs(minHorizontal) > limit || Mathf.Abs(maxHorizontal) > limit)
                    Debug.LogWarning("[Lidar] Pinhole projection needs all angles within 89 degrees of the look direction (vertical 1..179, horizontal -89..89); clamping.");
                minVertical = Mathf.Clamp(minVertical, -limit, limit);
                maxVertical = Mathf.Clamp(maxVertical, -limit, limit);
                minHorizontal = Mathf.Clamp(minHorizontal, -limit, limit);
                maxHorizontal = Mathf.Clamp(maxHorizontal, -limit, limit);
            }
            Vector4 lidarAngles = new Vector4(
                Mathf.Deg2Rad * minVertical, Mathf.Deg2Rad * maxVertical,
                Mathf.Deg2Rad * minHorizontal, Mathf.Deg2Rad * maxHorizontal);

            ctx.cmd.SetRayTracingVectorParam(lidarRaySpawner, Shader.PropertyToID("originPos"), position);
            ctx.cmd.SetRayTracingVectorParam(lidarRaySpawner, Shader.PropertyToID("lidarAngles"), lidarAngles);
            ctx.cmd.SetRayTracingFloatParam(lidarRaySpawner, Shader.PropertyToID("projectionMode"), pinhole ? 1.0f : 0.0f);

            Matrix4x4 worldToCamera = worldToExportCamera(bakingCamera.transform);
            Matrix4x4 exportMatrix = exportSettings.coordinateSystem == LidarCoordinateSystem.Camera ? worldToCamera : Matrix4x4.identity;
            ctx.cmd.SetRayTracingMatrixParam(lidarRaySpawner, Shader.PropertyToID("g_ExportMatrix"), exportMatrix);
            ctx.cmd.SetRayTracingMatrixParam(lidarRaySpawner, Shader.PropertyToID("g_InvViewMatrix"), bakingCamera.cameraToWorldMatrix);
            ctx.cmd.SetRayTracingTextureParam(lidarRaySpawner, Shader.PropertyToID("worldPosTexture"), LidarCameraPositionTexture);
            ctx.cmd.SetRayTracingTextureParam(lidarRaySpawner, Shader.PropertyToID("worldColorTexture"), LidarCameraColorTexture);
            ctx.cmd.SetRayTracingTextureParam(lidarRaySpawner, Shader.PropertyToID("g_FaceTex"), faceArray);
            ctx.cmd.SetRayTracingMatrixArrayParam(lidarRaySpawner, Shader.PropertyToID("g_FaceViewProj"), faceViewProj);

            ctx.cmd.DispatchRays(lidarRaySpawner, "MainRayGenShader", (uint)LidarCameraPositionTexture.width, (uint)LidarCameraPositionTexture.height, 1);
            updateDistancePreview(ctx.cmd, exportMatrix.MultiplyPoint3x4(bakingCamera.transform.position));

            if (exportScan)
                queueScanReadback(ctx.cmd, pendingScanFileId, worldToCamera.inverse);
        }

        /// <inheritdoc />
        public override IEnumerable<Material> RegisterMaterialForInspector() { yield return overrideMaterial; }

        protected override void Cleanup()
        {
            LidarCameraColorTexture?.Release();
            LidarCameraPositionTexture?.Release();
            distancePreview?.Release();
            equirect?.Release();
            faceArray?.Release();
            foreach (var cam in faceCameras)
                if (cam != null)
                    cam.targetTexture = null;
            foreach (var tex in faceTextures)
                tex?.Release();
            if (faceRig != null)
                Object.DestroyImmediate(faceRig);

            foreach (var mesh in bakedSkinnedMeshes.Values)
                if (mesh != null)
                    Object.DestroyImmediate(mesh);
            bakedSkinnedMeshes.Clear();
        }
    }


}
