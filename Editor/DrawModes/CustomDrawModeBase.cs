using UnityEditor;
using DrawModePlusMLS;

namespace DrawModePlusMLS.Editor
{
    public class CustomDrawModeBase
    {
        public DrawModePlusMode DrawMode => Mode;
        public string GetDrawModeName() => DrawModeName;

        protected string DrawModeName = "Default";
        protected DrawModePlusMode Mode = DrawModePlusMode.None;

        public virtual void OnInitialize()
        {
            DrawModeName = DrawModePlusModeRegistry.GetDisplayName(Mode);
            DrawModePlusModeRegistry.RegisterSceneViewMode(Mode);
        }

        public virtual void OnSceneGUIDraw(SceneView sceneView) { }

        public virtual void OnSceneViewSelected()
        {
            DrawModePlusRuntimeState.SetMode(Mode);
            SceneView.RepaintAll();
        }

        public virtual void OnSceneViewUnselected()
        {
            if (DrawModePlusRuntimeState.CurrentMode != Mode)
                return;

            DrawModePlusRuntimeState.SetMode(DrawModePlusMode.None);
            SceneView.RepaintAll();
        }
    }
}
