using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DrawModePlusMLS
{
    internal sealed class StencilDebugPass : SceneObjectDebugPass
    {
        private readonly ProfilingSampler fullscreenSampler = new ProfilingSampler("DrawModePlusMLS Stencil View");
        private RenderStateBlock stencilDepthState = new RenderStateBlock(RenderStateMask.Depth)
        {
            depthState = new DepthState(true, CompareFunction.LessEqual)
        };

        private Material stencilWriteOverride;
        private Material stencilViewMaterial;
        private RTHandle tempTexture;

        public StencilDebugPass(RenderPassEvent passEvent)
            : base("DrawModePlusMLS Stencil Write", passEvent)
        {
        }

        public void Setup(Material writeMaterial, Material viewMaterial)
        {
            stencilWriteOverride = writeMaterial;
            stencilViewMaterial = viewMaterial;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;
            descriptor.msaaSamples = 1;
            RenderingUtils.ReAllocateIfNeeded(ref tempTexture, descriptor, name: "_DrawModePlusStencilTemp");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (stencilWriteOverride == null || stencilViewMaterial == null)
                return;

            BeginDebugRender(context, ref renderingData, out var cmd);
            DrawStencilWriters(context, ref renderingData);

            using (new ProfilingScope(cmd, fullscreenSampler))
            {
                var colorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
                Blitter.BlitCameraTexture(cmd, colorTarget, tempTexture, stencilViewMaterial, 0);
                Blitter.BlitCameraTexture(cmd, tempTexture, colorTarget);
            }

            EndDebugRender(context, cmd);
        }

        public void Dispose()
        {
            tempTexture?.Release();
        }

        private void DrawStencilWriters(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var opaqueFiltering = new FilteringSettings(RenderQueueRange.opaque, renderingData.cameraData.camera.cullingMask);
            var transparentFiltering = new FilteringSettings(RenderQueueRange.transparent, renderingData.cameraData.camera.cullingMask);

            var opaqueDrawingSettings = CreateDrawingSettings(
                ShaderTagIds,
                ref renderingData,
                renderingData.cameraData.defaultOpaqueSortFlags);
            opaqueDrawingSettings.overrideMaterial = stencilWriteOverride;
            opaqueDrawingSettings.overrideMaterialPassIndex = 0;

            var transparentDrawingSettings = CreateDrawingSettings(
                ShaderTagIds,
                ref renderingData,
                SortingCriteria.CommonTransparent);
            transparentDrawingSettings.overrideMaterial = stencilWriteOverride;
            transparentDrawingSettings.overrideMaterialPassIndex = 0;

            context.DrawRenderers(renderingData.cullResults, ref opaqueDrawingSettings, ref opaqueFiltering, ref stencilDepthState);
            context.DrawRenderers(renderingData.cullResults, ref transparentDrawingSettings, ref transparentFiltering, ref stencilDepthState);
        }
    }
}
