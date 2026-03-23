using System.Collections.Generic;
using DrawModePlusMLS;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DrawModePlusMLS.Editor
{
    [InitializeOnLoad]
    public static class CustomDrawModeInitializer
    {
        private static SceneView currentSceneView;
        private static List<CustomDrawModeBase> drawModes = new List<CustomDrawModeBase>();
        private static readonly int DrawModeIsForwardId = Shader.PropertyToID("_DrawModeIsForward");

        private static RenderPipelineAsset lastRenderPipelineAsset;
        static CustomDrawModeInitializer()
        {
            Debug.Log("DrawModePlusMLS: Initialize");

            UpdateDrawModeIsForwardFlag();
            EnsureRendererFeaturesInjected();
            lastRenderPipelineAsset = GraphicsSettings.currentRenderPipeline;
            EditorApplication.projectChanged += OnProjectChanged;

            EditorApplication.update += OnUpdateEditor;

            // 注册DrawMode
            DepthDrawMode depthDrawMode = new DepthDrawMode();
            drawModes.Add(depthDrawMode);
            WorldNormalDrawMode worldNormalDrawMode = new WorldNormalDrawMode();
            drawModes.Add(worldNormalDrawMode);
            DeferredNormalBufferDrawMode deferredNormalBufferDrawMode = new DeferredNormalBufferDrawMode();
            drawModes.Add(deferredNormalBufferDrawMode);
            DeferredAmbientOcclusionDrawMode deferredAmbientOcclusionDrawMode = new DeferredAmbientOcclusionDrawMode();
            drawModes.Add(deferredAmbientOcclusionDrawMode);
            UV0Checker uv0Checker = new UV0Checker();
            drawModes.Add(uv0Checker);
            StencilDrawMode stencilDrawMode = new StencilDrawMode();
            drawModes.Add(stencilDrawMode);

            foreach (var drawMode in drawModes)
            {
                drawMode.OnInitialize();
            }
        }

        private static void OnDrawModeChanged(SceneView.CameraMode mode)
        {
            ResetDebugDraw();
            string sceneViewModeName = mode.name;

            for (int i = 0; i < drawModes.Count; i++)
            {
                string currentDrawModeName = drawModes[i].GetDrawModeName();
                if (sceneViewModeName == currentDrawModeName)
                {
                    drawModes[i].OnSceneViewSelected();
                }
            }
        }

        private static void OnProjectChanged()
        {
            UpdateDrawModeIsForwardFlag();
            EnsureRendererFeaturesInjected();
        }

        public static void OnUpdateEditor()
        {
            if (GraphicsSettings.currentRenderPipeline != lastRenderPipelineAsset)
            {
                lastRenderPipelineAsset = GraphicsSettings.currentRenderPipeline;
                UpdateDrawModeIsForwardFlag();
                EnsureRendererFeaturesInjected();
            }

            if (SceneView.lastActiveSceneView != currentSceneView)
            {
                if (currentSceneView != null)
                {
                    currentSceneView.onCameraModeChanged -= OnDrawModeChanged;
                }

                if (SceneView.lastActiveSceneView != null)
                {
                    currentSceneView = SceneView.lastActiveSceneView;
                    currentSceneView.onCameraModeChanged += OnDrawModeChanged;
                }
            }
        }

        private static void UpdateDrawModeIsForwardFlag()
        {
            int isForward = 1;

            var rpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (rpAsset == null)
            {
                Shader.SetGlobalInt(DrawModeIsForwardId, isForward);
                return;
            }

            try
            {
                var rpSerializedObject = new SerializedObject(rpAsset);
                SerializedProperty rendererDataListProp = rpSerializedObject.FindProperty("m_RendererDataList");
                SerializedProperty singleRendererDataProp = null;

                ScriptableRendererData rendererData = null;

                if (rendererDataListProp != null && rendererDataListProp.isArray && rendererDataListProp.arraySize > 0)
                {
                    for (int i = 0; i < rendererDataListProp.arraySize; i++)
                    {
                        var element = rendererDataListProp.GetArrayElementAtIndex(i);
                        var obj = element.objectReferenceValue as ScriptableRendererData;
                        if (obj != null)
                        {
                            rendererData = obj;
                            break;
                        }
                    }
                }
                else
                {
                    singleRendererDataProp = rpSerializedObject.FindProperty("m_RendererData");
                    if (singleRendererDataProp != null)
                    {
                        rendererData = singleRendererDataProp.objectReferenceValue as ScriptableRendererData;
                    }
                }

                if (rendererData != null && rendererData.GetType().Name.Contains("UniversalRendererData"))
                {
                    var rendererSO = new SerializedObject(rendererData);
                    var renderingModeProp = rendererSO.FindProperty("m_RenderingPath") ?? rendererSO.FindProperty("m_RenderingMode");

                    if (renderingModeProp != null)
                    {
                        int mode = renderingModeProp.intValue;
                        isForward = mode == 1 ? 0 : 1;
                    }
                    else
                    {
                        Debug.LogWarning("DrawModePlusMLS: Could not find URP rendering mode property, fallback to Forward.");
                    }
                }
                else if (rendererData != null)
                {
                    Debug.LogWarning($"DrawModePlusMLS: Unknown renderer data type {rendererData.GetType().Name}, fallback to Forward.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"DrawModePlusMLS: Failed to detect URP Rendering Path, fallback to Forward. {e.Message}");
            }

            Shader.SetGlobalInt(DrawModeIsForwardId, isForward);
        }

        private static void EnsureRendererFeaturesInjected()
        {
            var rpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (rpAsset == null)
                return;

            try
            {
                foreach (var rendererData in EnumerateRendererData(rpAsset))
                {
                    EnsureRendererFeatureInjected(rendererData);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"DrawModePlusMLS: Failed to inject renderer feature. {e.Message}");
            }
        }

        private static IEnumerable<ScriptableRendererData> EnumerateRendererData(UniversalRenderPipelineAsset rpAsset)
        {
            var rpSerializedObject = new SerializedObject(rpAsset);
            var rendererDataListProp = rpSerializedObject.FindProperty("m_RendererDataList");

            if (rendererDataListProp != null && rendererDataListProp.isArray && rendererDataListProp.arraySize > 0)
            {
                for (int i = 0; i < rendererDataListProp.arraySize; i++)
                {
                    var element = rendererDataListProp.GetArrayElementAtIndex(i);
                    var rendererData = element.objectReferenceValue as ScriptableRendererData;
                    if (rendererData != null)
                        yield return rendererData;
                }

                yield break;
            }

            var singleRendererDataProp = rpSerializedObject.FindProperty("m_RendererData");
            var singleRendererData = singleRendererDataProp?.objectReferenceValue as ScriptableRendererData;
            if (singleRendererData != null)
                yield return singleRendererData;
        }

        private static void EnsureRendererFeatureInjected(ScriptableRendererData rendererData)
        {
            if (rendererData == null)
                return;

            for (int i = 0; i < rendererData.rendererFeatures.Count; i++)
            {
                if (rendererData.rendererFeatures[i] is DrawModePlusRendererFeature)
                    return;
            }

            var serializedObject = new SerializedObject(rendererData);
            var featuresProp = serializedObject.FindProperty("m_RendererFeatures");
            var featureMapProp = serializedObject.FindProperty("m_RendererFeatureMap");
            if (featuresProp == null || featureMapProp == null)
            {
                Debug.LogWarning($"DrawModePlusMLS: Renderer data format is not supported on {rendererData.name}.");
                return;
            }

            var feature = ScriptableObject.CreateInstance<DrawModePlusRendererFeature>();
            feature.name = nameof(DrawModePlusRendererFeature);
            feature.SetEditorResources(ResourceFinder.LoadTexture("Common/ColorUV.png"));

            AssetDatabase.AddObjectToAsset(feature, rendererData);
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(feature, out _, out long localId);

            int insertIndex = featuresProp.arraySize;
            featuresProp.arraySize++;
            featuresProp.GetArrayElementAtIndex(insertIndex).objectReferenceValue = feature;

            featureMapProp.arraySize++;
            featureMapProp.GetArrayElementAtIndex(insertIndex).longValue = localId;

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            feature.Create();

            rendererData.SetDirty();
            EditorUtility.SetDirty(feature);
            EditorUtility.SetDirty(rendererData);
            AssetDatabase.SaveAssets();

            Debug.Log($"DrawModePlusMLS: Injected DrawModePlusRendererFeature into {rendererData.name}");
        }

        private static void ResetDebugDraw()
        {
            for (int i = 0; i < drawModes.Count; i++)
            {
                CustomDrawModeBase drawModeBase = drawModes[i];
                drawModeBase.OnSceneViewUnselected();
            }
        }
    }
}
