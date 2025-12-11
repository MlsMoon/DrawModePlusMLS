using UnityEditor;
using UnityEngine;

namespace DrawModePlusMLS.Editor
{
    public class CustomDrawModeBase
    {
        public string GetDrawModeName() => DrawModeName;
        public string GetShaderName() => ShaderName;

        protected string DrawModeName = "Default";
        protected string ShaderName = "DrawModePlus/DefaultFallback";

        private Material material;
        private RenderTexture temp;

        public virtual void OnInitialize()
        {
            // 在菜单中添加自定义绘制模式
            SceneView.AddCameraMode(GetDrawModeName(), "DrawModePlusMLS");

            Shader shader = Shader.Find(GetShaderName());
            if (shader == null)
            {
                Debug.LogError("Shader not found: " + ShaderName);
            }

            material = new Material(shader);
        }

        public virtual void OnSceneGUIDraw(SceneView sceneView)
        {
            var cam = sceneView.camera;
            var src = cam.targetTexture;
            if (src == null)
                return;

            temp = RenderTexture.GetTemporary(src.descriptor);
            Graphics.Blit(src, temp, material);
            Graphics.Blit(temp, src);

            RenderTexture.ReleaseTemporary(temp);
        }
    }
}