using UnityEngine;

namespace DrawModePlusMLS.Editor
{
    public class DeferredAmbientOcclusionDrawMode : CustomDrawModeBase
    {
        private static readonly int DrawModeIsForwardId = Shader.PropertyToID("_DrawModeIsForward");

        public override void OnInitialize()
        {
            DrawModeName = "AmbientOcclusion(Deferred)";
            Mode = global::DrawModePlusMLS.DrawModePlusMode.AmbientOcclusionDeferred;

            base.OnInitialize();
        }

        public override void OnSceneViewSelected()
        {
            base.OnSceneViewSelected();

            if (Shader.GetGlobalInt(DrawModeIsForwardId) != 0)
                Debug.LogWarning("DrawModePlusMLS: AmbientOcclusion(Deferred) only works with URP Deferred rendering.");
        }
    }
}
