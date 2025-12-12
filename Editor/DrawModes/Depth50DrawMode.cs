namespace DrawModePlusMLS.Editor
{
    public class Depth50DrawMode : CustomDrawModeBase
    {
        public override void OnInitialize()
        {
            DrawModeName = "Depth(50m)";
            FullScreenShaderName = "DrawModePlus/DepthView50";
            usePostProcessingShader = true;
            replaceSceneViewShader = false;

            base.OnInitialize();
        }
    }
}