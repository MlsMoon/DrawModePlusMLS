using UnityEngine;

namespace DrawModePlusMLS
{
    public enum DrawModePlusMode
    {
        None = 0,
        Depth = 1,
        WorldNormalForward = 2,
        WorldNormalDeferred = 3,
        AmbientOcclusionDeferred = 4,
        UV0 = 5,
        Stencil = 6
    }

    public static class DrawModePlusRuntimeState
    {
        public static DrawModePlusMode CurrentMode { get; private set; }

        public static bool IsActive => CurrentMode != DrawModePlusMode.None;

        public static void SetMode(DrawModePlusMode mode)
        {
            CurrentMode = mode;
        }

        public static bool IsFullscreenMode(DrawModePlusMode mode)
        {
            return mode == DrawModePlusMode.Depth
                || mode == DrawModePlusMode.WorldNormalForward
                || mode == DrawModePlusMode.WorldNormalDeferred
                || mode == DrawModePlusMode.AmbientOcclusionDeferred;
        }
    }
}
