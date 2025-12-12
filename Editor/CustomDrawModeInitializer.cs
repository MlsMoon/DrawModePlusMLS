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

        static CustomDrawModeInitializer()
        {
            Debug.Log("DrawModePlusMLS: Initialize");
            EditorApplication.update += OnUpdateEditor;

            // 注册DrawMode
            Depth10DrawMode depth10DrawMode = new Depth10DrawMode();
            drawModes.Add(depth10DrawMode);
            WorldNormalDrawMode worldNormalDrawMode = new WorldNormalDrawMode();
            drawModes.Add(worldNormalDrawMode);

            foreach (var drawMode in drawModes)
            {
                drawMode.OnInitialize();
            }
        }

        public static void OnDrawModeChanged(SceneView.CameraMode mode)
        {
            //Fill this later
            string sceneViewModeName = mode.name;

            bool isSelectedCustomMode = false;
            for (int i = 0; i < drawModes.Count; i++)
            {
                string currentDrawModeName = drawModes[i].GetDrawModeName();
                if (sceneViewModeName == currentDrawModeName)
                {
                    isSelectedCustomMode = true;
                    SceneView.duringSceneGui += drawModes[i].OnSceneGUIDraw;
                }
            }

            if (!isSelectedCustomMode)
            {
                ResetDebugDraw();
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
            }
        }
    }
}