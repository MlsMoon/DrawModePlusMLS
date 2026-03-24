using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DrawModePlusMLS
{
    internal sealed class FullscreenDebugPass : ScriptableRenderPass
    {
        private readonly ProfilingSampler fullscreenProfilingSampler = new ProfilingSampler("DrawModePlusMLS Fullscreen");
        private RTHandle tempTexture;
        private Material material;

        public FullscreenDebugPass(RenderPassEvent passEvent)
        {
            renderPassEvent = passEvent;
        }

        public void Setup(DrawModePlusMode drawMode, Material drawMaterial)
        {
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
                    return ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal;
                default:
                    return ScriptableRenderPassInput.None;
            }
        }
    }
}
