namespace DrawModePlusMLS.Editor
{
    public class DeferredNormalBufferDrawMode : CustomDrawModeBase
    {
        public override void OnInitialize()
        {
            Mode = global::DrawModePlusMLS.DrawModePlusMode.WorldNormalDeferred;

            base.OnInitialize();
        }
    }
}
