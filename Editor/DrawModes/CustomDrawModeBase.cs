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
            InitializeMaterial();
        }

        public virtual void OnSceneGUIDraw(SceneView sceneView)
        {
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

        private void InitializeMaterial()
        {
            Shader shader = Shader.Find(GetShaderName());
            if (shader == null)
            {
                Debug.LogError("Shader not found: " + ShaderName);
            }

            material = new Material(shader);
        }
    }
}