namespace DrawModePlusMLS.Editor
{
    public class DepthDrawMode : CustomDrawModeBase
    {
        public override void OnInitialize()
        {
            DrawModeName = "Depth";
            Mode = global::DrawModePlusMLS.DrawModePlusMode.Depth;

            base.OnInitialize();
        }
    }
}
