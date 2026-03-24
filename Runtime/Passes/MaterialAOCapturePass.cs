using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DrawModePlusMLS
{
    internal sealed class MaterialAOCapturePass : ScriptableRenderPass
    {
        internal static readonly int MaterialAODebugTextureId = Shader.PropertyToID("_DrawModePlusMaterialAODebugTexture");
        private static readonly int BlitScaleBiasId = Shader.PropertyToID("_BlitScaleBias");
        private static readonly MaterialPropertyBlock SharedPropertyBlock = new MaterialPropertyBlock();

        private readonly ProfilingSampler deferredDebugCaptureProfilingSampler = new ProfilingSampler("Deferred Debug View");
        private Material material;
        private RTHandle debugTexture;
        private int materialPassIndex;

        public RTHandle DebugTexture => debugTexture;

        public MaterialAOCapturePass(RenderPassEvent passEvent)
        {
            renderPassEvent = passEvent;
        }

        public void Setup(Material debugMaterial, int passIndex)
        {
            material = debugMaterial;
            materialPassIndex = passIndex;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;
            descriptor.msaaSamples = 1;
            descriptor.graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm;

            RenderingUtils.ReAllocateIfNeeded(
                ref debugTexture,
                descriptor,
                FilterMode.Point,
                TextureWrapMode.Clamp,
                name: "_DrawModePlusMaterialAODebugTexture");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (material == null || debugTexture == null)
                return;

            var cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, deferredDebugCaptureProfilingSampler))
            {
                CoreUtils.SetRenderTarget(cmd, debugTexture, ClearFlag.Color, Color.black);
                SharedPropertyBlock.Clear();
                SharedPropertyBlock.SetVector(BlitScaleBiasId, new Vector4(1f, 1f, 0f, 0f));
                cmd.DrawProcedural(Matrix4x4.identity, material, materialPassIndex, MeshTopology.Triangles, 3, 1, SharedPropertyBlock);
                cmd.SetGlobalTexture(MaterialAODebugTextureId, debugTexture.nameID);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public void Dispose()
        {
            debugTexture?.Release();
        }
    }
}
