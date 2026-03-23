namespace DrawModePlusMLS.Editor
{
    public class DeferredNormalBufferDrawMode : CustomDrawModeBase
    {
        public override void OnInitialize()
        {
            DrawModeName = "WorldNormal(Deferred)";
            Mode = global::DrawModePlusMLS.DrawModePlusMode.WorldNormalDeferred;

            base.OnInitialize();
        }
    }
}
