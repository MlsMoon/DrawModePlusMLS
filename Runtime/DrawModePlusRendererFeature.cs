using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DrawModePlusMLS
{
    [DisallowMultipleRendererFeature("DrawModePlusMLS")]
    public sealed class DrawModePlusRendererFeature : ScriptableRendererFeature
    {
        [SerializeField] private bool enableGameView;
        [SerializeField, HideInInspector] private RenderPassEvent fullscreenPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        [SerializeField, HideInInspector] private RenderPassEvent sceneRedrawPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        [SerializeField, HideInInspector] private Texture2D uv0Texture;

        public bool EnableGameView => enableGameView;

        public void SetEnableGameView(bool value)
        {
            enableGameView = value;
        }

        private FullscreenDebugPass fullscreenPass;
        private Uv0DebugPass uv0Pass;
        private StencilDebugPass stencilPass;
        private MaterialAOCapturePass materialAOCapturePass;
        private MaterialAOCompositePass materialAOCompositePass;

        private Material depthMaterial;
        private Material worldNormalMaterial;
        private Material deferredNormalMaterial;
        private Material deferredDebugMaterial;
        private Material uv0Material;
        private Material stencilWriteMaterial;
        private Material stencilViewMaterial;

        public void SetEditorResources(Texture2D texture)
        {
            uv0Texture = texture;
        }

        public override void Create()
        {
#if UNITY_EDITOR
            fullscreenPass?.Dispose();
            stencilPass?.Dispose();
            materialAOCapturePass?.Dispose();
            materialAOCompositePass?.Dispose();

            fullscreenPass = new FullscreenDebugPass(fullscreenPassEvent);
            uv0Pass = new Uv0DebugPass(sceneRedrawPassEvent);
            stencilPass = new StencilDebugPass(sceneRedrawPassEvent);
            materialAOCapturePass = new MaterialAOCapturePass(RenderPassEvent.BeforeRenderingDeferredLights);
            materialAOCompositePass = new MaterialAOCompositePass(RenderPassEvent.AfterRenderingPostProcessing);
#endif
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
#if !UNITY_EDITOR
            return;
#else
            if (!ShouldRenderCamera(renderingData.cameraData.cameraType) || renderingData.cameraData.isPreviewCamera)
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

            if (mode == DrawModePlusMode.MaterialAO || mode == DrawModePlusMode.BaseColorDeferred)
            {
                if (!DrawModePlusRenderPipelineBridge.IsDeferred(renderingData.cameraData.camera))
                    return;

                var material = GetDeferredDebugMaterial();
                if (material == null)
                    return;

                materialAOCapturePass.Setup(material, GetDeferredDebugCapturePassIndex(mode));
                materialAOCompositePass.Setup(materialAOCapturePass, material, 2);
                renderer.EnqueuePass(materialAOCapturePass);
                renderer.EnqueuePass(materialAOCompositePass);
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
#endif
        }

        protected override void Dispose(bool disposing)
        {
            fullscreenPass?.Dispose();
            stencilPass?.Dispose();
            materialAOCapturePass?.Dispose();
            materialAOCompositePass?.Dispose();

            DestroyMaterial(depthMaterial);
            DestroyMaterial(worldNormalMaterial);
            DestroyMaterial(deferredNormalMaterial);
            DestroyMaterial(deferredDebugMaterial);
            DestroyMaterial(uv0Material);
            DestroyMaterial(stencilWriteMaterial);
            DestroyMaterial(stencilViewMaterial);
        }

        private bool ShouldRenderCamera(CameraType cameraType)
        {
            if (cameraType == CameraType.SceneView)
                return true;

            return enableGameView && cameraType == CameraType.Game;
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
                default:
                    return null;
            }
        }

        private Material GetDeferredDebugMaterial()
        {
            return GetOrCreateMaterial(ref deferredDebugMaterial, "DrawModePlus/DeferredDebugView");
        }

        private static int GetDeferredDebugCapturePassIndex(DrawModePlusMode mode)
        {
            switch (mode)
            {
                case DrawModePlusMode.MaterialAO:
                    return 0;
                case DrawModePlusMode.BaseColorDeferred:
                    return 1;
                default:
                    return 0;
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
    }
}
