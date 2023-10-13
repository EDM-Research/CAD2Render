using System.Collections.Generic;
using UnityEngine.Rendering;
using System;
using UnityEngine.Rendering.RendererUtils;

namespace UnityEngine.Rendering.HighDefinition
{
    /// <summary>
    /// Create segmentation masks for objects with the false color component
    /// 
    /// </summary>
    [System.Serializable]
    class DrawSegmentationObjectsCustomPass : CustomPass
    {
        // Override material
        public Material overrideMaterial = null;

        public Camera bakingCamera = null;
        public RenderTexture targetTexture = null;
        public RenderTexture targetTextureArray = null;

        static ShaderTagId[] shaderTags;
        Color backgroundColor;


        protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
        {
            backgroundColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
            shaderTags = new ShaderTagId[]
            {
                new ShaderTagId("DepthOnly"),
                new ShaderTagId("DepthForwardOnly"),
            };
        }

        protected ShaderTagId[] GetShaderTagIds()
        {
            return shaderTags;
        }

        static int renderIndex = -1;
        /***
         * Render the segmentation mask
         * when targetTextureArray is set it wil render the objects in each slice seperatly without obstructions
         * only one render can be done each frame, so a min of targetTextureArray.volumeDepth +1 frames need to be renderd before all segmentation masks are completed!
         */
        protected override void Execute(CustomPassContext ctx)
        {
            const int forwardOnlyPassIndex = 0;
            var result = new RendererListDesc(shaderTags, ctx.cullingResults, bakingCamera)
            {
                rendererConfiguration = PerObjectData.None,
                renderQueueRange = RenderQueueRange.all,

                overrideMaterial = overrideMaterial,
                overrideMaterialPassIndex = forwardOnlyPassIndex,
                sortingCriteria = SortingCriteria.BackToFront,
                excludeObjectMotionVectors = false,
                layerMask = 1,
            };

            //loop renderIndex between [-1: targetTextureArray.volumeDepth[
            int nrOfRenderTargetIds = targetTextureArray ? targetTextureArray.volumeDepth + 1 : 1;
            renderIndex = ((renderIndex + 1)% nrOfRenderTargetIds) - 1;
            //renderIndex = -1 => render the regular segmentation mask
            //renderIndex >= 0 only render the segmentation mask of the selected object (but keep occlusions)
            overrideMaterial.SetInteger("_currentObjectId", renderIndex);
            if(renderIndex >= 0)
                CoreUtils.SetRenderTarget(ctx.cmd, targetTextureArray, ClearFlag.All, backgroundColor, 0, CubemapFace.Unknown, renderIndex);
            else
                CoreUtils.SetRenderTarget(ctx.cmd, targetTexture, ClearFlag.All, backgroundColor);
            CoreUtils.DrawRendererList(ctx.renderContext, ctx.cmd, ctx.renderContext.CreateRendererList(result));
            renderIndex++;
        }

        /// <inheritdoc />
        public override IEnumerable<Material> RegisterMaterialForInspector() { yield return overrideMaterial; }
    }
}
