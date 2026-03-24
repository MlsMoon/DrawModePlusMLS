using UnityEngine;

namespace DrawModePlusMLS.Editor
{
    public class BaseColorDeferredDrawMode : CustomDrawModeBase
    {
        private static readonly int DrawModeIsForwardId = Shader.PropertyToID("_DrawModeIsForward");

        public override void OnInitialize()
        {
            Mode = global::DrawModePlusMLS.DrawModePlusMode.BaseColorDeferred;
            base.OnInitialize();
        }

        public override void OnSceneViewSelected()
        {
            base.OnSceneViewSelected();

            if (Shader.GetGlobalInt(DrawModeIsForwardId) != 0)
                Debug.LogWarning("DrawModePlusMLS: BaseColor(Deferred) only works with URP Deferred rendering.");
        }
    }
}
