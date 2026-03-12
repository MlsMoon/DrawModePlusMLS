using System.Collections.Generic;
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

        private static string CurrentDrawName;

        static CustomDrawModeInitializer()
        {
            Debug.Log("DrawModePlusMLS: Initialize");

            UpdateDrawModeIsForwardFlag();
            lastRenderPipelineAsset = GraphicsSettings.currentRenderPipeline;
            EditorApplication.projectChanged += UpdateDrawModeIsForwardFlag;

            EditorApplication.update += OnUpdateEditor;

            // 注册DrawMode
            DepthDrawMode depthDrawMode = new DepthDrawMode();
            drawModes.Add(depthDrawMode);
            WorldNormalDrawMode worldNormalDrawMode = new WorldNormalDrawMode();
            drawModes.Add(worldNormalDrawMode);
            DeferredNormalBufferDrawMode deferredNormalBufferDrawMode = new DeferredNormalBufferDrawMode();
            drawModes.Add(deferredNormalBufferDrawMode);
            UV0Checker uv0Checker = new UV0Checker();
            drawModes.Add(uv0Checker);
            // StencilDrawMode stencilDrawMode = new StencilDrawMode();
            // drawModes.Add(stencilDrawMode);

            foreach (var drawMode in drawModes)
            {
                drawMode.OnInitialize();
            }
        }

        private static void OnDrawModeChanged(SceneView.CameraMode mode)
        {
            ResetDebugDraw();

            //Fill this later
            string sceneViewModeName = mode.name;

            for (int i = 0; i < drawModes.Count; i++)
            {
                string currentDrawModeName = drawModes[i].GetDrawModeName();
                if (sceneViewModeName == currentDrawModeName)
                {
                    SceneView.duringSceneGui += drawModes[i].OnSceneGUIDraw;
                    drawModes[i].OnSceneViewSelected();
                }
            }
        }

        public static void OnUpdateEditor()
        {
            if (GraphicsSettings.currentRenderPipeline != lastRenderPipelineAsset)
            {
                lastRenderPipelineAsset = GraphicsSettings.currentRenderPipeline;
                UpdateDrawModeIsForwardFlag();
            }

            if (SceneView.lastActiveSceneView != currentSceneView)
            {
                if (currentSceneView != null)
                {
                    //Make sure we subtract our drawing mode 
                    //from the previous scene view if changed
                    currentSceneView.onCameraModeChanged -= OnDrawModeChanged;
                }

                if (SceneView.lastActiveSceneView != null)
                {
                    currentSceneView = SceneView.lastActiveSceneView;
                    //Add callback function to OnDrawModeChanged
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

        private static void ResetDebugDraw()
        {
            for (int i = 0; i < drawModes.Count; i++)
            {
                CustomDrawModeBase drawModeBase = drawModes[i];
                SceneView.duringSceneGui -= drawModeBase.OnSceneGUIDraw;
                drawModeBase.OnSceneViewUnselected();
            }
        }
    }
}