namespace DrawModePlusMLS.Editor
{
    /// <summary>
    /// SceneView Reflection 调试视图：驱动 URP Lighting Debug 隔离 Probe/IBL 反射。
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
