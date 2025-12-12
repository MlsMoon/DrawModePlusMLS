namespace DrawModePlusMLS.Editor
{
    public class UV0Checker : CustomDrawModeBase
    {
        public override void OnInitialize()
        {
            DrawModeName = "UV0Checker";
            FullScreenShaderName = "DrawModePlus/UV0Checker";
            usePostProcessingShader = false;
            replaceSceneViewShader = true;

            base.OnInitialize();
        }
    }
}