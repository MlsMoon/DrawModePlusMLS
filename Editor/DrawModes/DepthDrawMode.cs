namespace DrawModePlusMLS.Editor
{
    public class DepthDrawMode : CustomDrawModeBase
    {
        public override void OnInitialize()
        {
            Mode = global::DrawModePlusMLS.DrawModePlusMode.Depth;

            base.OnInitialize();
        }
    }
}
