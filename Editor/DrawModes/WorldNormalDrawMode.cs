namespace DrawModePlusMLS.Editor
{
    public class WorldNormalDrawMode : CustomDrawModeBase
    {
        public override void OnInitialize()
        {
            DrawModeName = "ObjectWorldNormal";
            FullScreenShaderName = "DrawModePlus/WorldNormal";
            usePostProcessingShader = true;
            replaceSceneViewShader = false;

            base.OnInitialize();
        }
    }
}