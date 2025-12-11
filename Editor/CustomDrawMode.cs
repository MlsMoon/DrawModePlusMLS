using UnityEditor;
using UnityEngine;

namespace DrawModePlusMLS.Editor
{
    [InitializeOnLoad]
    
    public static class CustomDrawMode
    {
        private static SceneView currentSceneView;

        static CustomDrawMode()
        {
            Debug.Log("CustomDrawMode: 初始化");
            EditorApplication.update += OnUpdateEditor;
            SceneView.AddCameraMode("Test", "DrawModePlusMLS");
        }

        public static void OnDrawModeChanged(SceneView.CameraMode mode)
        {
            //Fill this later
            Debug.Log(mode.name);
            
            if(mode.name == "Test")
            {
                DrawDebugTest();
            }
            else
            {
                DrawDebugReset();
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
        
        public static void DrawDebugTest()
        {
            Debug.Log("当前切换到DebugTest Draw Mode");
            //Replace the render shader of our scene to one we set
            // currentSceneView.SetSceneViewShaderReplace(Shader, null);
            string pluginPath = ResourceFinder.GetPluginPath();
            Debug.Log(pluginPath);
            Shader.SetGlobalTexture("_TestTexture", null);
            currentSceneView.SetSceneViewShaderReplace(Shader.Find("DrawModePlus/Unlit/TestUnlit"), null);
        }
        
        public static void DrawDebugReset()
        {
            //Reset to null, null
            currentSceneView.SetSceneViewShaderReplace(null, null);
        }
    }
}