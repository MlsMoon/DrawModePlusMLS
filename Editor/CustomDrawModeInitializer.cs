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

        private static string CurrentDrawName;

        static CustomDrawModeInitializer()
        {
            Debug.Log("DrawModePlusMLS: Initialize");
            EditorApplication.update += OnUpdateEditor;

            // 注册DrawMode
            DepthDrawMode depthDrawMode = new DepthDrawMode();
            drawModes.Add(depthDrawMode);
            WorldNormalDrawMode worldNormalDrawMode = new WorldNormalDrawMode();
            drawModes.Add(worldNormalDrawMode);
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