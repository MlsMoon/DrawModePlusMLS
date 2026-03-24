using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DrawModePlusMLS
{
    internal sealed class MaterialAOCompositePass : ScriptableRenderPass
    {
        private static readonly int BlitTextureId = Shader.PropertyToID("_BlitTexture");
        private static readonly int BlitScaleBiasId = Shader.PropertyToID("_BlitScaleBias");
        private static readonly MaterialPropertyBlock SharedPropertyBlock = new MaterialPropertyBlock();

        private readonly ProfilingSampler materialAOCompositeProfilingSampler = new ProfilingSampler("DrawModePlusMLS MaterialAO Composite");

        private MaterialAOCapturePass capturePass;
        private Material material;
        private RTHandle tempTexture;
        private int materialPassIndex;

        public MaterialAOCompositePass(RenderPassEvent passEvent)
        {
            renderPassEvent = passEvent;
        }

        public void Setup(MaterialAOCapturePass sourcePass, Material debugMaterial, int passIndex)
        {
            capturePass = sourcePass;
            material = debugMaterial;
            materialPassIndex = passIndex;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;
            descriptor.msaaSamples = 1;
            RenderingUtils.ReAllocateIfNeeded(ref tempTexture, descriptor, name: "_DrawModePlusMaterialAOTemp");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var debugTexture = capturePass?.DebugTexture;
            if (material == null || debugTexture == null || tempTexture == null)
                return;

            var cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, materialAOCompositeProfilingSampler))
            {
                cmd.SetGlobalTexture(MaterialAOCapturePass.MaterialAODebugTextureId, debugTexture.nameID);

                var colorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
                CoreUtils.SetRenderTarget(cmd, tempTexture, ClearFlag.Color, Color.black);
                SharedPropertyBlock.Clear();
                SharedPropertyBlock.SetTexture(BlitTextureId, debugTexture);
                SharedPropertyBlock.SetVector(BlitScaleBiasId, new Vector4(1f, 1f, 0f, 0f));
                cmd.DrawProcedural(Matrix4x4.identity, material, materialPassIndex, MeshTopology.Triangles, 3, 1, SharedPropertyBlock);
                Blitter.BlitCameraTexture(cmd, tempTexture, colorTarget);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public void Dispose()
        {
            tempTexture?.Release();
        }
    }
}
