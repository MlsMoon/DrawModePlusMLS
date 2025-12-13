using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace DrawModePlusMLS.Editor
{
    public class StencilDrawMode : CustomDrawModeBase
    {
        public override void OnInitialize()
        {
            DrawModeName = "Stencil";
            FullScreenShaderName = "SG/StencilChecker";
            usePostProcessingShader = true;
            replaceSceneViewShader = false;

            base.OnInitialize();
        }

        public override void OnSceneViewSelected()
        {
            base.OnSceneViewSelected();
            
            SceneView currentSceneView = SceneView.lastActiveSceneView;
            CommandBuffer  dCommandBuffer = new CommandBuffer();
            dCommandBuffer.name = "DrawModePlusMLS";
            // dCommandBuffer.DrawMesh();
            Vector3 pos = currentSceneView.camera.transform.position;
            currentSceneView.camera.AddCommandBuffer(CameraEvent.AfterForwardAlpha,dCommandBuffer);
        }

        public override void OnSceneViewUnselected()
        {
            base.OnSceneViewUnselected();
            
        }
    }
}