namespace DrawModePlusMLS.Editor
{
    public class ObjectWorldNormalDrawMode : CustomDrawModeBase
    {
        public override void OnInitialize()
        {
            DrawModeName = "ObjectWorldNormal";
            FullScreenShaderName = "DrawModePlus/ObjectWorldNormal";
            usePostProcessingShader = true;
            replaceSceneViewShader = false;

            base.OnInitialize();
        }
    }
}