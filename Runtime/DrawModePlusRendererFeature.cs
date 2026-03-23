using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DrawModePlusMLS
{
    [DisallowMultipleRendererFeature("DrawModePlusMLS")]
    public sealed class DrawModePlusRendererFeature : ScriptableRendererFeature
    {
        [SerializeField] private RenderPassEvent fullscreenPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        [SerializeField] private RenderPassEvent sceneRedrawPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        [SerializeField] private Texture2D uv0Texture;

        private FullscreenDebugPass fullscreenPass;
        private Uv0DebugPass uv0Pass;
        private StencilDebugPass stencilPass;

        private Material depthMaterial;
        private Material worldNormalMaterial;
        private Material deferredNormalMaterial;
        private Material deferredAoMaterial;
        private Material uv0Material;
        private Material stencilWriteMaterial;
        private Material stencilViewMaterial;

        public void SetEditorResources(Texture2D texture)
        {
            uv0Texture = texture;
        }

        public override void Create()
        {
            fullscreenPass?.Dispose();
            stencilPass?.Dispose();

            fullscreenPass = new FullscreenDebugPass(fullscreenPassEvent);
            uv0Pass = new Uv0DebugPass(sceneRedrawPassEvent);
            stencilPass = new StencilDebugPass(sceneRedrawPassEvent);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType != CameraType.SceneView || renderingData.cameraData.isPreviewCamera)
                return;

            var mode = DrawModePlusRuntimeState.CurrentMode;
            if (mode == DrawModePlusMode.None)
                return;

            if (DrawModePlusRuntimeState.IsFullscreenMode(mode))
            {
                var material = GetFullscreenMaterial(mode);
                if (material == null)
                    return;

                fullscreenPass.Setup(mode, material);
                renderer.EnqueuePass(fullscreenPass);
                return;
            }

            if (mode == DrawModePlusMode.UV0)
            {
                var material = GetUv0Material();
                if (material == null)
                    return;

                uv0Pass.Setup(material);
                renderer.EnqueuePass(uv0Pass);
                return;
            }

            if (mode == DrawModePlusMode.Stencil)
            {
                var writeMaterial = GetStencilWriteMaterial();
                var viewMaterial = GetStencilViewMaterial();
                if (writeMaterial == null || viewMaterial == null)
                    return;

                stencilPass.Setup(writeMaterial, viewMaterial);
                renderer.EnqueuePass(stencilPass);
            }
        }

        protected override void Dispose(bool disposing)
        {
            fullscreenPass?.Dispose();
            stencilPass?.Dispose();

            DestroyMaterial(depthMaterial);
            DestroyMaterial(worldNormalMaterial);
            DestroyMaterial(deferredNormalMaterial);
            DestroyMaterial(deferredAoMaterial);
            DestroyMaterial(uv0Material);
            DestroyMaterial(stencilWriteMaterial);
            DestroyMaterial(stencilViewMaterial);
        }

        private Material GetFullscreenMaterial(DrawModePlusMode mode)
        {
            switch (mode)
            {
                case DrawModePlusMode.Depth:
                    return GetOrCreateMaterial(ref depthMaterial, "DrawModePlus/DepthView");
                case DrawModePlusMode.WorldNormalForward:
                    return GetOrCreateMaterial(ref worldNormalMaterial, "DrawModePlus/WorldNormal");
                case DrawModePlusMode.WorldNormalDeferred:
                    return GetOrCreateMaterial(ref deferredNormalMaterial, "DrawModePlus/DeferredNormalBuffer");
                case DrawModePlusMode.AmbientOcclusionDeferred:
                    return GetOrCreateMaterial(ref deferredAoMaterial, "DrawModePlus/DeferredAOBuffer");
                default:
                    return null;
            }
        }

        private Material GetUv0Material()
        {
            var material = GetOrCreateMaterial(ref uv0Material, "DrawModePlus/UV0Checker");
            if (material != null && uv0Texture != null)
            {
                material.SetTexture(Uv0DebugPass.BaseTextureId, uv0Texture);
            }

            return material;
        }

        private Material GetStencilWriteMaterial()
        {
            return GetOrCreateMaterial(ref stencilWriteMaterial, "DrawModePlus/StencilWriter");
        }

        private Material GetStencilViewMaterial()
        {
            return GetOrCreateMaterial(ref stencilViewMaterial, "DrawModePlus/StencilChecker");
        }

        private static Material GetOrCreateMaterial(ref Material material, string shaderName)
        {
            if (material != null)
                return material;

            var shader = Shader.Find(shaderName);
            if (shader == null)
            {
                Debug.LogWarning($"DrawModePlusMLS: Shader not found: {shaderName}");
                return null;
            }

            material = CoreUtils.CreateEngineMaterial(shader);
            return material;
        }

        private static void DestroyMaterial(Material material)
        {
            if (material == null)
                return;

            CoreUtils.Destroy(material);
        }

        private sealed class FullscreenDebugPass : ScriptableRenderPass
        {
            private readonly ProfilingSampler fullscreenProfilingSampler = new ProfilingSampler("DrawModePlusMLS Fullscreen");
            private RTHandle tempTexture;
            private Material material;
            private DrawModePlusMode mode;

            public FullscreenDebugPass(RenderPassEvent passEvent)
            {
                renderPassEvent = passEvent;
            }

            public void Setup(DrawModePlusMode drawMode, Material drawMaterial)
            {
                mode = drawMode;
                material = drawMaterial;
                ConfigureInput(GetRequiredInputs(drawMode));
            }

            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                var descriptor = renderingData.cameraData.cameraTargetDescriptor;
                descriptor.depthBufferBits = 0;
                descriptor.msaaSamples = 1;
                RenderingUtils.ReAllocateIfNeeded(ref tempTexture, descriptor, name: "_DrawModePlusTemp");
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (material == null)
                    return;

                var cmd = CommandBufferPool.Get();
                using (new ProfilingScope(cmd, fullscreenProfilingSampler))
                {
                    var colorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
                    Blitter.BlitCameraTexture(cmd, colorTarget, tempTexture, material, 0);
                    Blitter.BlitCameraTexture(cmd, tempTexture, colorTarget);
                }

                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }

            public void Dispose()
            {
                tempTexture?.Release();
            }

            private static ScriptableRenderPassInput GetRequiredInputs(DrawModePlusMode drawMode)
            {
                switch (drawMode)
                {
                    case DrawModePlusMode.Depth:
                        return ScriptableRenderPassInput.Depth;
                    case DrawModePlusMode.WorldNormalForward:
                    case DrawModePlusMode.WorldNormalDeferred:
                    case DrawModePlusMode.AmbientOcclusionDeferred:
                        return ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal;
                    default:
                        return ScriptableRenderPassInput.None;
                }
            }
        }

        private abstract class SceneObjectDebugPass : ScriptableRenderPass
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

        private sealed class Uv0DebugPass : SceneObjectDebugPass
        {
            public static readonly int BaseTextureId = Shader.PropertyToID("_BaseTexture");

            private Material overrideMaterial;

            public Uv0DebugPass(RenderPassEvent passEvent)
                : base("DrawModePlusMLS UV0", passEvent)
            {
            }

            public void Setup(Material material)
            {
                overrideMaterial = material;
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (overrideMaterial == null)
                    return;

                BeginDebugRender(context, ref renderingData, out var cmd);
                DrawSceneObjects(context, ref renderingData, overrideMaterial, 0);
                EndDebugRender(context, cmd);
            }
        }

        private sealed class StencilDebugPass : SceneObjectDebugPass
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
}
