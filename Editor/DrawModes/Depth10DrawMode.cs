namespace DrawModePlusMLS.Editor
{
    public class Depth10DrawMode : CustomDrawModeBase
    {
        public override void OnInitialize()
        {
            DrawModeName = "Depth(10m)";
            ShaderName = "DrawModePlus/DepthView10";

            base.OnInitialize();
        }
    }
}