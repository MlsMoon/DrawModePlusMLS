namespace DrawModePlusMLS.Editor
{
    public class DepthDrawMode : CustomDrawModeBase
    {
        public override void OnInitialize()
        {
            DrawModeName = "Depth";
            FullScreenShaderName = "DrawModePlus/DepthView";
            usePostProcessingShader = true;
            replaceSceneViewShader = false;

            base.OnInitialize();
        }
    }
}