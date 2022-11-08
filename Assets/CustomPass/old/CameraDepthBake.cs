using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;

class CameraDepthBake : CustomPass
{
    public Camera           bakingCamera = null;
    public RenderTexture    targetTexture = null;
    public bool             render = true;
    ShaderTagId[]           shaderTags;

    // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
    // When empty this render pass will render to the active camera render target.
    // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
    // The render pipeline will ensure target setup and clearing happens in an performance manner.
    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        shaderTags = new ShaderTagId[2]
        {
            new ShaderTagId("DepthOnly"),
            new ShaderTagId("DepthForwardOnly"),
        };
    }

    protected override void Execute(CustomPassContext ctx)
    {
        // if (!render || camera.camera == bakingCamera)
        //     return;

        bakingCamera.TryGetCullingParameters(out var cullingParams);

        var result = new RendererListDesc(shaderTags, ctx.cullingResults, bakingCamera)
        {
            rendererConfiguration = PerObjectData.None,
            renderQueueRange = RenderQueueRange.all,
            sortingCriteria = SortingCriteria.BackToFront,
            excludeObjectMotionVectors = false,
            layerMask = -1,
        };

        var p = GL.GetGPUProjectionMatrix(bakingCamera.projectionMatrix, true);
        Matrix4x4 scaleMatrix = Matrix4x4.identity;
        scaleMatrix.m22 = -1.0f;
        var v = scaleMatrix * bakingCamera.transform.localToWorldMatrix.inverse;
        var vp = p * v;
        ctx.cmd.SetGlobalMatrix("_ViewMatrix", v);
        ctx.cmd.SetGlobalMatrix("_InvViewMatrix", v.inverse);
        ctx.cmd.SetGlobalMatrix("_ProjMatrix", p);
        ctx.cmd.SetGlobalMatrix("_InvProjMatrix", p.inverse);
        ctx.cmd.SetGlobalMatrix("_ViewProjMatrix", vp);
        ctx.cmd.SetGlobalMatrix("_InvViewProjMatrix", vp.inverse);
        ctx.cmd.SetGlobalMatrix("_CameraViewProjMatrix", vp);
        ctx.cmd.SetGlobalVector("_WorldSpaceCameraPos", Vector3.zero);

        CoreUtils.SetRenderTarget(ctx.cmd, targetTexture, ClearFlag.Depth);
        CoreUtils.DrawRendererList(ctx.renderContext, ctx.cmd, ctx.renderContext.CreateRendererList(result));
    }

    protected override void Cleanup()
    {
        // Cleanup code
    }
}