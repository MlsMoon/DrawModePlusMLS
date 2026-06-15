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
        Stencil = 10
    }

    public static class DrawModePlusRuntimeState
    {
        private static readonly int DepthMeterId = Shader.PropertyToID("_DepthMeter");

        public static DrawModePlusMode CurrentMode { get; private set; }
        public static float DepthMeter { get; private set; } = 50f;

        public static bool IsActive => CurrentMode != DrawModePlusMode.None;

        static DrawModePlusRuntimeState()
        {
            Shader.SetGlobalFloat(DepthMeterId, DepthMeter);
        }

        public static void SetMode(DrawModePlusMode mode)
        {
            CurrentMode = mode;
        }

        public static void SetDepthMeter(float value)
        {
            DepthMeter = Mathf.Max(0.01f, value);
            Shader.SetGlobalFloat(DepthMeterId, DepthMeter);
        }

        public static bool IsFullscreenMode(DrawModePlusMode mode)
        {
            return mode == DrawModePlusMode.Depth
                || mode == DrawModePlusMode.WorldNormalForward
                || mode == DrawModePlusMode.WorldNormalDeferred;
        }
    }
}
