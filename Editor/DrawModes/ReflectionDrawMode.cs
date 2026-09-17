namespace DrawModePlusMLS.Editor
{
    /// <summary>
    /// SceneView Reflection debug view. Drives the RenderFeature override redraw,
    /// not URP Lighting Debug.
    /// </summary>
    public class ReflectionDrawMode : CustomDrawModeBase
    {
        public override void OnInitialize()
        {
            Mode = global::DrawModePlusMLS.DrawModePlusMode.Reflection;
            base.OnInitialize();
        }
    }
}
