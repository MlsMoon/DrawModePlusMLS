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
        private const string TexelDensityHintText = "Texel Density 512/m | <color=#7A0000><=128 x2 Low</color> | <color=#FF8A2A>256 x1 Low</color> | <color=#00FF00>512 OK</color> | <color=#00BFA5>1024 x1 High</color> | <color=#28106E>>=2048 x2 High</color> | <color=#9A9A9A>Gray non Common.shader</color>";

        private static RenderPipelineAsset lastRenderPipelineAsset;
        static CustomDrawModeInitializer()
        {
            Debug.Log("DrawModePlusMLS: Initialize");

            UpdateDrawModeIsForwardFlag();
            EnsureRendererFeaturesInjected();
            lastRenderPipelineAsset = GraphicsSettings.currentRenderPipeline;
            EditorApplication.projectChanged += OnProjectChanged;

            EditorApplication.update += OnUpdateEditor;
            SceneView.duringSceneGui += OnSceneGUI;

            // 注册DrawMode
            DepthDrawMode depthDrawMode = new DepthDrawMode();
            drawModes.Add(depthDrawMode);
            WorldNormalDrawMode worldNormalDrawMode = new WorldNormalDrawMode();
            drawModes.Add(worldNormalDrawMode);
            DeferredNormalBufferDrawMode deferredNormalBufferDrawMode = new DeferredNormalBufferDrawMode();
            drawModes.Add(deferredNormalBufferDrawMode);
            BaseColorDeferredDrawMode baseColorDeferredDrawMode = new BaseColorDeferredDrawMode();
            drawModes.Add(baseColorDeferredDrawMode);
            DeferredAmbientOcclusionDrawMode deferredAmbientOcclusionDrawMode = new DeferredAmbientOcclusionDrawMode();
            drawModes.Add(deferredAmbientOcclusionDrawMode);
            MetallicDeferredDrawMode metallicDeferredDrawMode = new MetallicDeferredDrawMode();
            drawModes.Add(metallicDeferredDrawMode);
            RoughnessDeferredDrawMode roughnessDeferredDrawMode = new RoughnessDeferredDrawMode();
            drawModes.Add(roughnessDeferredDrawMode);
            TexelDensityDrawMode texelDensityDrawMode = new TexelDensityDrawMode();
            drawModes.Add(texelDensityDrawMode);
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
            if (!DrawModePlusModeRegistry.TryGetMode(mode, out var selectedMode))
                return;

            for (int i = 0; i < drawModes.Count; i++)
            {
                if (drawModes[i].DrawMode == selectedMode)
                {
                    drawModes[i].OnSceneViewSelected();
                    break;
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
                    UpdateDrawModeIsForwardFlag();
                }
            }

            UpdateDrawModeIsForwardFlag();
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (DrawModePlusRuntimeState.CurrentMode != DrawModePlusMode.TexelDensity)
                return;

            Handles.BeginGUI();
            var rect = new Rect(12f, sceneView.position.height - 52f, sceneView.position.width - 24f, 28f);
            var style = new GUIStyle(EditorStyles.helpBox)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                richText = true,
                normal = { textColor = Color.white }
            };
            GUI.Label(rect, TexelDensityHintText, style);
            Handles.EndGUI();
        }
        private static void UpdateDrawModeIsForwardFlag()
        {
            var referenceCamera = GetReferenceCamera();
            int isForward = DrawModePlusRenderPipelineBridge.IsDeferred(referenceCamera) ? 0 : 1;
            Shader.SetGlobalInt(DrawModeIsForwardId, isForward);
        }

        private static void EnsureRendererFeaturesInjected()
        {
            var rpAsset = DrawModePlusRenderPipelineBridge.GetCurrentRenderPipelineAsset();
            if (rpAsset == null)
                return;

            try
            {
                foreach (var rendererData in DrawModePlusRenderPipelineBridge.EnumerateRendererData(rpAsset))
                {
                    EnsureRendererFeatureInjected(rendererData);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"DrawModePlusMLS: Failed to inject renderer feature. {e.Message}");
            }
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

        private static Camera GetReferenceCamera()
        {
            if (Selection.activeGameObject != null && Selection.activeGameObject.TryGetComponent(out Camera selectedCamera))
                return selectedCamera;

            if (Camera.main != null)
                return Camera.main;

            var cameras = Object.FindObjectsOfType<Camera>();
            for (int i = 0; i < cameras.Length; i++)
            {
                if (cameras[i] != null && cameras[i].cameraType == CameraType.Game)
                    return cameras[i];
            }

            return SceneView.lastActiveSceneView != null ? SceneView.lastActiveSceneView.camera : null;
        }
    }
}
