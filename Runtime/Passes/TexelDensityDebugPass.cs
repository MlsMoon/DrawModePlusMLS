using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DrawModePlusMLS
{
    internal sealed class TexelDensityDebugPass : SceneObjectDebugPass
    {
        private static readonly List<ShaderTagId> TexelDensityShaderTagIds = new List<ShaderTagId>
        {
            new ShaderTagId("DrawModePlusTexelDensity")
        };

        private Material grayMaterial;

        public TexelDensityDebugPass(RenderPassEvent passEvent)
            : base("DrawModePlusMLS Texel Density", passEvent)
        {
        }

        public void Setup(Material fallbackGrayMaterial)
        {
            grayMaterial = fallbackGrayMaterial;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (grayMaterial == null)
                return;

            BeginDebugRender(context, ref renderingData, out var cmd);
            DrawSceneObjects(context, ref renderingData, grayMaterial, 0);
            DrawCommonShaderObjects(context, ref renderingData);
            EndDebugRender(context, cmd);
        }

        private void DrawCommonShaderObjects(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var opaqueFiltering = new FilteringSettings(RenderQueueRange.opaque, renderingData.cameraData.camera.cullingMask);
            var transparentFiltering = new FilteringSettings(RenderQueueRange.transparent, renderingData.cameraData.camera.cullingMask);

            var opaqueDrawingSettings = CreateDrawingSettings(
                TexelDensityShaderTagIds,
                ref renderingData,
                renderingData.cameraData.defaultOpaqueSortFlags);

            var transparentDrawingSettings = CreateDrawingSettings(
                TexelDensityShaderTagIds,
                ref renderingData,
                SortingCriteria.CommonTransparent);

            context.DrawRenderers(renderingData.cullResults, ref opaqueDrawingSettings, ref opaqueFiltering);
            context.DrawRenderers(renderingData.cullResults, ref transparentDrawingSettings, ref transparentFiltering);
        }
    }
}
