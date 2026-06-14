using UnityEngine;

namespace DrawModePlusMLS.Editor
{
    public class RoughnessDeferredDrawMode : CustomDrawModeBase
    {
        private static readonly int DrawModeIsForwardId = Shader.PropertyToID("_DrawModeIsForward");

        public override void OnInitialize()
        {
            Mode = global::DrawModePlusMLS.DrawModePlusMode.RoughnessDeferred;
            base.OnInitialize();
        }

        public override void OnSceneViewSelected()
        {
            base.OnSceneViewSelected();

            if (Shader.GetGlobalInt(DrawModeIsForwardId) != 0)
                Debug.LogWarning("DrawModePlusMLS: Roughness(Deferred) only works with URP Deferred rendering.");
        }
    }
}
