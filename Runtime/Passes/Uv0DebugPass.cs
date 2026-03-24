using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DrawModePlusMLS
{
    internal sealed class Uv0DebugPass : SceneObjectDebugPass
    {
        public static readonly int BaseTextureId = Shader.PropertyToID("_BaseTexture");

        private Material overrideMaterial;

        public Uv0DebugPass(RenderPassEvent passEvent)
            : base("DrawModePlusMLS UV0", passEvent)
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
