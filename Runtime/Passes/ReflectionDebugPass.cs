using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DrawModePlusMLS
{
    /// <summary>
    /// Clears the camera, then redraws the scene with an override material so Probe/IBL
    /// can be inspected without running the original materials.
    /// </summary>
    internal sealed class ReflectionDebugPass : SceneObjectDebugPass
    {
        private Material overrideMaterial;

        public ReflectionDebugPass(RenderPassEvent passEvent)
            : base("DrawModePlusMLS Reflection", passEvent)
        {
        }

        public void Setup(Material material)
        {
            overrideMaterial = material;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (overrideMaterial == null)
                return;

            BeginDebugRender(context, ref renderingData, out var cmd);
            DrawSceneObjects(context, ref renderingData, overrideMaterial, 0);
            EndDebugRender(context, cmd);
        }
    }
}
