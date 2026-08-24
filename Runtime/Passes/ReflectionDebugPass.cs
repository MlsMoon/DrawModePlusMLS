using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DrawModePlusMLS
{
    /// <summary>
    /// 清屏后用 override 材质重绘场景，隔离 Probe/IBL，不跑原材质。
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
