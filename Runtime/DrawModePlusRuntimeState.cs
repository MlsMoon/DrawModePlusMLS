using UnityEngine;

namespace DrawModePlusMLS
{
    public enum DrawModePlusMode
    {
        None = 0,
        Depth = 1,
        WorldNormalForward = 2,
        WorldNormalDeferred = 3,
        BaseColorDeferred = 4,
        MaterialAO = 5,
        MetallicDeferred = 6,
        RoughnessDeferred = 7,
        TexelDensity = 8,
        UV0 = 9,
        Stencil = 10,
        Reflection = 11
    }

    public static class DrawModePlusRuntimeState
    {
        private static readonly int DepthMeterId = Shader.PropertyToID("_DepthMeter");
        private static readonly int ReflectionRoughnessId = Shader.PropertyToID("_DrawModePlusReflectionRoughness");

        /// <summary>Force Mirror 关闭时使用的全局 perceptual roughness，不是原材质粗糙度。</summary>
        private const float DefaultReflectionRoughness = 0.35f;

        public static DrawModePlusMode CurrentMode { get; private set; }
        public static float DepthMeter { get; private set; } = 50f;

        /// <summary>强制 chrome（perceptual roughness = 0），所有物体看同一套 Probe/Sky。</summary>
        public static bool ForceMirror { get; private set; } = true;

        /// <summary>Force Mirror 关闭时的全局粗糙度。</summary>
        public static float ReflectionRoughness { get; private set; } = DefaultReflectionRoughness;

        public static bool IsActive => CurrentMode != DrawModePlusMode.None;

        static DrawModePlusRuntimeState()
        {
            Shader.SetGlobalFloat(DepthMeterId, DepthMeter);
            ApplyReflectionRoughness();
        }

        public static void SetMode(DrawModePlusMode mode)
        {
            CurrentMode = mode;
            if (mode == DrawModePlusMode.Reflection)
                ApplyReflectionRoughness();
        }

        public static void SetDepthMeter(float value)
        {
            DepthMeter = Mathf.Max(0.01f, value);
            Shader.SetGlobalFloat(DepthMeterId, DepthMeter);
        }

        /// <summary>切换 Force Mirror，并立刻写入全局 roughness。</summary>
        public static void SetForceMirror(bool value)
        {
            if (ForceMirror == value)
                return;

            ForceMirror = value;
            ApplyReflectionRoughness();
        }

        /// <summary>设置全局 roughness；仅在 Force Mirror 关闭时生效。</summary>
        public static void SetReflectionRoughness(float value)
        {
            ReflectionRoughness = Mathf.Clamp01(value);
            ApplyReflectionRoughness();
        }

        public static bool IsFullscreenMode(DrawModePlusMode mode)
        {
            return mode == DrawModePlusMode.Depth
                || mode == DrawModePlusMode.WorldNormalForward
                || mode == DrawModePlusMode.WorldNormalDeferred;
        }

        private static void ApplyReflectionRoughness()
        {
            Shader.SetGlobalFloat(ReflectionRoughnessId, ForceMirror ? 0f : ReflectionRoughness);
        }
    }
}
