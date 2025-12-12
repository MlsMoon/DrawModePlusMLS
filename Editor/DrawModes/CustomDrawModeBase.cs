using UnityEditor;
using UnityEngine;

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

        private Material material;
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

            if (material == null)
                InitializeMaterial();

            Graphics.Blit(src, temp, material);
            Graphics.Blit(temp, src);

            RenderTexture.ReleaseTemporary(temp);
        }

        protected virtual void OnSceneViewSelected()
        {
            SceneView currentSceneView = SceneView.lastActiveSceneView;
            currentSceneView.SetSceneViewShaderReplace(null, null);
        }

        protected virtual void OnSceneViewUnselected()
        {
            SceneView currentSceneView = SceneView.lastActiveSceneView;
            currentSceneView.SetSceneViewShaderReplace(null, null);
        }

        private void InitializeMaterial()
        {
            Shader shader = Shader.Find(GetShaderName());
            if (shader == null)
            {
                Debug.LogError("Shader not found: " + FullScreenShaderName);
            }

            material = new Material(shader);
        }
    }
}