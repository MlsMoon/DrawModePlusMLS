using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DrawModePlusMLS
{
    internal abstract class SceneObjectDebugPass : ScriptableRenderPass
    {
        protected static readonly List<ShaderTagId> ShaderTagIds = new List<ShaderTagId>
        {
            new ShaderTagId("SRPDefaultUnlit"),
            new ShaderTagId("UniversalForward"),
            new ShaderTagId("UniversalForwardOnly"),
            new ShaderTagId("UniversalGBuffer")
        };

        private readonly ProfilingSampler debugProfilingSampler;

        protected SceneObjectDebugPass(string name, RenderPassEvent passEvent)
        {
            debugProfilingSampler = new ProfilingSampler(name);
            renderPassEvent = passEvent;
        }

        protected void DrawSceneObjects(
            ScriptableRenderContext context,
            ref RenderingData renderingData,
            Material overrideMaterial,
            int overridePassIndex)
        {
            var opaqueFiltering = new FilteringSettings(RenderQueueRange.opaque, renderingData.cameraData.camera.cullingMask);
            var transparentFiltering = new FilteringSettings(RenderQueueRange.transparent, renderingData.cameraData.camera.cullingMask);

            var opaqueDrawingSettings = CreateDrawingSettings(
                ShaderTagIds,
                ref renderingData,
                renderingData.cameraData.defaultOpaqueSortFlags);
            opaqueDrawingSettings.overrideMaterial = overrideMaterial;
            opaqueDrawingSettings.overrideMaterialPassIndex = overridePassIndex;

            var transparentDrawingSettings = CreateDrawingSettings(
                ShaderTagIds,
                ref renderingData,
                SortingCriteria.CommonTransparent);
            transparentDrawingSettings.overrideMaterial = overrideMaterial;
            transparentDrawingSettings.overrideMaterialPassIndex = overridePassIndex;

            context.DrawRenderers(renderingData.cullResults, ref opaqueDrawingSettings, ref opaqueFiltering);
            context.DrawRenderers(renderingData.cullResults, ref transparentDrawingSettings, ref transparentFiltering);
        }

        protected void BeginDebugRender(
            ScriptableRenderContext context,
            ref RenderingData renderingData,
            out CommandBuffer cmd)
        {
            cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, debugProfilingSampler))
            {
                CoreUtils.SetRenderTarget(
                    cmd,
                    renderingData.cameraData.renderer.cameraColorTargetHandle,
                    renderingData.cameraData.renderer.cameraDepthTargetHandle,
                    ClearFlag.All,
                    Color.black);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
        }

        protected void EndDebugRender(
            ScriptableRenderContext context,
            CommandBuffer cmd)
        {
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}
