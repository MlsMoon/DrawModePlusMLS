using UnityEngine;

namespace DrawModePlusMLS.Editor
{
    public class UV0Checker : CustomDrawModeBase
    {
        private static readonly int BaseTexture = Shader.PropertyToID("_BaseTexture");

        public override void OnInitialize()
        {
            DrawModeName = "UV0";
            SceneViewReplaceShaderName = "DrawModePlus/UV0Checker";
            usePostProcessingShader = false;
            replaceSceneViewShader = true;

            base.OnInitialize();
        }

        public override void OnSceneViewSelected()
        {
            base.OnSceneViewSelected();

            if (sceneViewReplaceShader == null)
                InitializeMaterial();

            Texture2D texture = ResourceFinder.LoadTexture("Common/ColorUV.png");
            if (texture == null)
            {
                Debug.LogWarning("UV0Checker : texture is null");
            }

            Shader.SetGlobalTexture(BaseTexture, texture);
        }
    }
}