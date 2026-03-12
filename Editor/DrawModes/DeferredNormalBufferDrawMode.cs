namespace DrawModePlusMLS.Editor
{
    public class DeferredNormalBufferDrawMode : CustomDrawModeBase
    {
        public override void OnInitialize()
        {
            DrawModeName = "WorldNormal(Deferred)";
            FullScreenShaderName = "DrawModePlus/DeferredNormalBuffer";
            usePostProcessingShader = true;
            replaceSceneViewShader = false;

            base.OnInitialize();
        }
    }
}
