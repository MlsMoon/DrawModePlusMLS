using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DrawModePlusMLS
{
    public static class DrawModePlusRenderPipelineBridge
    {
        private const BindingFlags ReflectionFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly FieldInfo RendererDataListField = typeof(UniversalRenderPipelineAsset).GetField("m_RendererDataList", ReflectionFlags);
        private static readonly FieldInfo DefaultRendererIndexField = typeof(UniversalRenderPipelineAsset).GetField("m_DefaultRendererIndex", ReflectionFlags);
        private static readonly FieldInfo LegacyRendererDataField = typeof(UniversalRenderPipelineAsset).GetField("m_RendererData", ReflectionFlags);
        private static readonly FieldInfo CameraRendererIndexField = typeof(UniversalAdditionalCameraData).GetField("m_RendererIndex", ReflectionFlags);

        public static UniversalRenderPipelineAsset GetCurrentRenderPipelineAsset()
        {
            return GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        }

        public static IEnumerable<ScriptableRendererData> EnumerateRendererData(UniversalRenderPipelineAsset pipelineAsset)
        {
            if (pipelineAsset == null)
                yield break;

            var rendererDataList = GetRendererDataList(pipelineAsset);
            if (rendererDataList != null && rendererDataList.Length > 0)
            {
                for (int i = 0; i < rendererDataList.Length; i++)
                {
                    if (rendererDataList[i] != null)
                        yield return rendererDataList[i];
                }

                yield break;
            }

            if (LegacyRendererDataField?.GetValue(pipelineAsset) is ScriptableRendererData legacyRendererData && legacyRendererData != null)
                yield return legacyRendererData;
        }

        public static bool TryGetCurrentRendererData(Camera camera, out ScriptableRendererData rendererData)
        {
            rendererData = null;

            var pipelineAsset = GetCurrentRenderPipelineAsset();
            if (pipelineAsset == null)
                return false;

            return TryGetRendererData(pipelineAsset, camera, out rendererData);
        }

        public static bool TryGetRendererData(UniversalRenderPipelineAsset pipelineAsset, Camera camera, out ScriptableRendererData rendererData)
        {
            rendererData = null;
            if (pipelineAsset == null)
                return false;

            int rendererIndex = GetRendererIndex(camera);
            if (TryGetRendererDataByIndex(pipelineAsset, rendererIndex, out rendererData))
                return true;

            return TryGetRendererDataByIndex(pipelineAsset, -1, out rendererData);
        }

        public static bool TryGetRenderingMode(Camera camera, out RenderingMode renderingMode)
        {
            renderingMode = RenderingMode.Forward;
            return TryGetCurrentRendererData(camera, out var rendererData)
                && TryGetRenderingMode(rendererData, out renderingMode);
        }

        public static bool IsDeferred(Camera camera)
        {
            return TryGetRenderingMode(camera, out var renderingMode) && renderingMode == RenderingMode.Deferred;
        }

        private static int GetRendererIndex(Camera camera)
        {
            if (camera == null || !camera.TryGetComponent(out UniversalAdditionalCameraData additionalCameraData))
                return -1;

            if (CameraRendererIndexField?.GetValue(additionalCameraData) is int rendererIndex)
                return rendererIndex;

            return -1;
        }

        private static bool TryGetRendererDataByIndex(UniversalRenderPipelineAsset pipelineAsset, int rendererIndex, out ScriptableRendererData rendererData)
        {
            rendererData = null;

            var rendererDataList = GetRendererDataList(pipelineAsset);
            if (rendererDataList == null || rendererDataList.Length == 0)
            {
                if (LegacyRendererDataField?.GetValue(pipelineAsset) is ScriptableRendererData legacyRendererData)
                {
                    rendererData = legacyRendererData;
                    return rendererData != null;
                }

                return false;
            }

            int resolvedIndex = rendererIndex >= 0 ? rendererIndex : GetDefaultRendererIndex(pipelineAsset);
            if (resolvedIndex < 0 || resolvedIndex >= rendererDataList.Length || rendererDataList[resolvedIndex] == null)
                resolvedIndex = GetDefaultRendererIndex(pipelineAsset);

            if (resolvedIndex < 0 || resolvedIndex >= rendererDataList.Length)
                return false;

            rendererData = rendererDataList[resolvedIndex];
            return rendererData != null;
        }

        private static ScriptableRendererData[] GetRendererDataList(UniversalRenderPipelineAsset pipelineAsset)
        {
            return RendererDataListField?.GetValue(pipelineAsset) as ScriptableRendererData[];
        }

        private static int GetDefaultRendererIndex(UniversalRenderPipelineAsset pipelineAsset)
        {
            if (DefaultRendererIndexField?.GetValue(pipelineAsset) is int rendererIndex)
                return rendererIndex;

            return 0;
        }

        private static bool TryGetRenderingMode(ScriptableRendererData rendererData, out RenderingMode renderingMode)
        {
            renderingMode = RenderingMode.Forward;
            if (rendererData == null)
                return false;

            var rendererDataType = rendererData.GetType();
            var property = rendererDataType.GetProperty("renderingMode", ReflectionFlags);
            if (property != null && property.GetValue(rendererData, null) is RenderingMode modeFromProperty)
            {
                renderingMode = modeFromProperty;
                return true;
            }

            var field = rendererDataType.GetField("m_RenderingMode", ReflectionFlags)
                ?? rendererDataType.GetField("m_RenderingPath", ReflectionFlags);
            if (field?.GetValue(rendererData) is RenderingMode modeFromField)
            {
                renderingMode = modeFromField;
                return true;
            }

            if (field?.GetValue(rendererData) is int modeValue)
            {
                renderingMode = (RenderingMode)modeValue;
                return true;
            }

            return false;
        }
    }
}
