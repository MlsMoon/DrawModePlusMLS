using UnityEditor;
namespace DrawModePlusMLS.Editor
{
    public class StencilDrawMode : CustomDrawModeBase
    {
        public override void OnInitialize()
        {
            DrawModeName = "Stencil";
            Mode = global::DrawModePlusMLS.DrawModePlusMode.Stencil;

            base.OnInitialize();
        }
    }
}
