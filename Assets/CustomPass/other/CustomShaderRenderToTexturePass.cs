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
    class CustomShaderRenderToTexturePass : CustomPass
    {
        // Override material
        public Material overrideMaterial = null;

        public Camera bakingCamera = null;
        public RenderTexture targetTexture = null;

        static ShaderTagId[] shaderTags;
        public Color backgroundColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);


        protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
        {
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

            
            CoreUtils.SetRenderTarget(ctx.cmd, targetTexture, ClearFlag.All, backgroundColor);
            CoreUtils.DrawRendererList(ctx.renderContext, ctx.cmd, ctx.renderContext.CreateRendererList(result));
        }

        /// <inheritdoc />
        public override IEnumerable<Material> RegisterMaterialForInspector() { yield return overrideMaterial; }
    }
}
