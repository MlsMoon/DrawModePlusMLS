using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace DrawModePlusMLS.Editor
{
    public class CustomDrawModeBase
    {
        public string GetDrawModeName() => DrawModeName;
        public string GetShaderName() => FullScreenShaderName;

        protected string DrawModeName = "Default";
        protected string FullScreenShaderName = "DrawModePlus/DefaultFallback";
        protected string SceneViewReplaceShaderName = "DrawModePlus/DefaultFallback";

        protected bool replaceSceneViewShader = false;
        protected bool usePostProcessingShader = false;

        protected Material postProcessingMaterial;
        protected Material sceneViewReplaceMaterial;
        protected Shader sceneViewReplaceShader;

        private RenderTexture temp;

        public virtual void OnInitialize()
        {
            // 在菜单中添加自定义绘制模式
            SceneView.AddCameraMode(GetDrawModeName(), "DrawModePlusMLS");
            InitializeMaterial();
        }

        public virtual void OnSceneGUIDraw(SceneView sceneView)
        {
            if (!usePostProcessingShader)
                return;

            var cam = sceneView.camera;
            var src = cam.targetTexture;
            temp = RenderTexture.GetTemporary(src.descriptor);

            if (src == null)
            {
                Debug.Log("OnSceneGUIDraw : RT src is null");
                return;
            }

            if (temp == null)
            {
                Debug.Log("OnSceneGUIDraw : RT temp is null");
                return;
            }

            if (postProcessingMaterial == null)
                InitializeMaterial();

            Graphics.Blit(src, temp, postProcessingMaterial);
            Graphics.Blit(temp, src);

            RenderTexture.ReleaseTemporary(temp);
        }

        public virtual void OnSceneViewSelected()
        {
            SceneView currentSceneView = SceneView.lastActiveSceneView;
            currentSceneView.SetSceneViewShaderReplace(sceneViewReplaceShader,null);
        }

        public virtual void OnSceneViewUnselected()
        {
            SceneView currentSceneView = SceneView.lastActiveSceneView;
            currentSceneView.SetSceneViewShaderReplace(null, null);
        }

        protected void InitializeMaterial()
        {
            if (usePostProcessingShader)
            {
                Shader shaderFullScreen = Shader.Find(GetShaderName());
                if (shaderFullScreen == null)
                {
                    Debug.LogError("Shader not found: " + FullScreenShaderName);
                    return;
                }
                postProcessingMaterial = new Material(shaderFullScreen);
            }

            if (replaceSceneViewShader)
            {
                sceneViewReplaceShader = Shader.Find(SceneViewReplaceShaderName);
                if (sceneViewReplaceShader == null)
                {
                    Debug.LogError("Shader not found: " + SceneViewReplaceShaderName);
                    return;
                }
                sceneViewReplaceMaterial = new Material(sceneViewReplaceShader);
            }
        }
    }
}