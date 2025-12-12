namespace DrawModePlusMLS.Editor
{
    public class WorldNormalDrawMode : CustomDrawModeBase
    {
        public override void OnInitialize()
        {
            DrawModeName = "WorldNormal";
            ShaderName = "DrawModePlus/WorldNormal";

            base.OnInitialize();
        }
    }
}