namespace DrawModePlusMLS.Editor
{
    public class WorldNormalDrawMode : CustomDrawModeBase
    {
        public override void OnInitialize()
        {
            DrawModeName = "WorldNormal(Forward)";
            Mode = global::DrawModePlusMLS.DrawModePlusMode.WorldNormalForward;

            base.OnInitialize();
        }
    }
}
