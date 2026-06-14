using UnityEngine;

namespace DrawModePlusMLS.Editor
{
    public class MetallicDeferredDrawMode : CustomDrawModeBase
    {
        private static readonly int DrawModeIsForwardId = Shader.PropertyToID("_DrawModeIsForward");

        public override void OnInitialize()
        {
            Mode = global::DrawModePlusMLS.DrawModePlusMode.MetallicDeferred;
            base.OnInitialize();
        }

        public override void OnSceneViewSelected()
        {
            base.OnSceneViewSelected();

            if (Shader.GetGlobalInt(DrawModeIsForwardId) != 0)
                Debug.LogWarning("DrawModePlusMLS: Metallic(Deferred) only works with URP Deferred rendering.");
        }
    }
}
