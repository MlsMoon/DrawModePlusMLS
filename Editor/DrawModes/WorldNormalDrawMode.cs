namespace DrawModePlusMLS.Editor
{
    public class WorldNormalDrawMode : CustomDrawModeBase
    {
        public override void OnInitialize()
        {
            Mode = global::DrawModePlusMLS.DrawModePlusMode.WorldNormalForward;

            base.OnInitialize();
        }
    }
}
