namespace DrawModePlusMLS.Editor
{
    public class Depth10DrawMode : CustomDrawModeBase
    {
        public override void OnInitialize()
        {
            DrawModeName = "Depth(10m)";
            FullScreenShaderName = "DrawModePlus/DepthView10";
            usePostProcessingShader = true;
            replaceSceneViewShader = false;

            base.OnInitialize();
        }
    }
}