using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace DrawModePlusMLS.Editor
{
    internal static class DrawModePlusModeRegistry
    {
        private const string SectionName = "DrawModePlusMLS";
        private const BindingFlags ReflectionFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private static bool sceneViewModesRegistered;
        private static readonly Dictionary<DrawModePlusMode, SceneView.CameraMode> RegisteredCameraModes = new Dictionary<DrawModePlusMode, SceneView.CameraMode>();

        private static readonly DrawModePlusMode[] OrderedModes =
        {
            DrawModePlusMode.None,
            DrawModePlusMode.Depth,
            DrawModePlusMode.WorldNormalForward,
            DrawModePlusMode.WorldNormalDeferred,
            DrawModePlusMode.BaseColorDeferred,
            DrawModePlusMode.MaterialAO,
            DrawModePlusMode.UV0,
            DrawModePlusMode.Stencil
        };

        private static readonly Dictionary<DrawModePlusMode, string> DisplayNames = new Dictionary<DrawModePlusMode, string>
        {
            { DrawModePlusMode.None, "None" },
            { DrawModePlusMode.Depth, "Depth" },
            { DrawModePlusMode.WorldNormalForward, "WorldNormal(Forward)" },
            { DrawModePlusMode.WorldNormalDeferred, "WorldNormal(Deferred)" },
            { DrawModePlusMode.BaseColorDeferred, "BaseColor(Deferred)" },
            { DrawModePlusMode.MaterialAO, "MaterialAO" },
            { DrawModePlusMode.UV0, "UV0" },
            { DrawModePlusMode.Stencil, "Stencil" }
        };

        public static IReadOnlyList<DrawModePlusMode> Modes => OrderedModes;

        public static string GetDisplayName(DrawModePlusMode mode)
        {
            return DisplayNames.TryGetValue(mode, out var displayName) ? displayName : mode.ToString();
        }

        public static int GetIndex(DrawModePlusMode mode)
        {
            for (int i = 0; i < OrderedModes.Length; i++)
            {
                if (OrderedModes[i] == mode)
                    return i;
            }

            return 0;
        }

        public static DrawModePlusMode GetModeAt(int index)
        {
            if (index < 0 || index >= OrderedModes.Length)
                return DrawModePlusMode.None;

            return OrderedModes[index];
        }

        public static string[] GetDisplayNames()
        {
            var names = new string[OrderedModes.Length];
            for (int i = 0; i < OrderedModes.Length; i++)
            {
                names[i] = GetDisplayName(OrderedModes[i]);
            }

            return names;
        }

        public static void RegisterSceneViewMode(DrawModePlusMode mode)
        {
            EnsureSceneViewModesRegistered();
        }

        public static bool TryGetMode(SceneView.CameraMode cameraMode, out DrawModePlusMode mode)
        {
            if (cameraMode.section == SectionName)
            {
                for (int i = 0; i < OrderedModes.Length; i++)
                {
                    var currentMode = OrderedModes[i];
                    if (currentMode != DrawModePlusMode.None && cameraMode.name == GetDisplayName(currentMode))
                    {
                        mode = currentMode;
                        return true;
                    }
                }
            }

            mode = DrawModePlusMode.None;
            return false;
        }

        public static void ApplyMode(DrawModePlusMode mode)
        {
            EnsureSceneViewModesRegistered();
            DrawModePlusRuntimeState.SetMode(mode);

            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
                ApplyModeToSceneView(sceneView, mode);

            RepaintAllViews();
        }

        private static void EnsureSceneViewModesRegistered()
        {
            if (sceneViewModesRegistered)
                return;

            for (int i = 0; i < OrderedModes.Length; i++)
            {
                var mode = OrderedModes[i];
                if (mode == DrawModePlusMode.None)
                    continue;

                SceneView.AddCameraMode(GetDisplayName(mode), SectionName);
            }

            sceneViewModesRegistered = true;
        }

        private static void ApplyModeToSceneView(SceneView sceneView, DrawModePlusMode mode)
        {
            if (sceneView == null)
                return;

            if (mode == DrawModePlusMode.None)
            {
                TryResetSceneViewCameraMode(sceneView);
                return;
            }

            if (TryGetRegisteredSceneViewMode(mode, out var registeredMode))
            {
                sceneView.cameraMode = registeredMode;
                return;
            }

            Debug.LogWarning($"DrawModePlusMLS: Failed to locate registered SceneView camera mode for {GetDisplayName(mode)}.");
        }

        private static void TryResetSceneViewCameraMode(SceneView sceneView)
        {
            var getBuiltinCameraModeMethod = typeof(SceneView).GetMethod("GetBuiltinCameraMode", ReflectionFlags, null, new[] { typeof(DrawCameraMode) }, null);
            if (getBuiltinCameraModeMethod != null)
            {
                var defaultMode = (SceneView.CameraMode)getBuiltinCameraModeMethod.Invoke(null, new object[] { DrawCameraMode.Textured });
                sceneView.cameraMode = defaultMode;
            }
        }

        private static void RepaintAllViews()
        {
            SceneView.RepaintAll();
            InternalEditorUtility.RepaintAllViews();
        }

        private static bool TryGetRegisteredSceneViewMode(DrawModePlusMode mode, out SceneView.CameraMode cameraMode)
        {
            if (mode == DrawModePlusMode.None)
            {
                cameraMode = default(SceneView.CameraMode);
                return false;
            }

            if (RegisteredCameraModes.TryGetValue(mode, out cameraMode))
                return true;

            CacheRegisteredCameraModes();
            return RegisteredCameraModes.TryGetValue(mode, out cameraMode);
        }

        private static void CacheRegisteredCameraModes()
        {
            RegisteredCameraModes.Clear();

            var fields = typeof(SceneView).GetFields(ReflectionFlags);
            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                if (!ShouldInspectMember(field.FieldType, field.Name))
                    continue;

                object value;
                try
                {
                    value = field.GetValue(null);
                }
                catch
                {
                    continue;
                }

                CollectRegisteredCameraModes(value, 0);
            }
        }

        private static void CollectRegisteredCameraModes(object value, int depth)
        {
            if (value == null || depth > 3)
                return;

            if (value is SceneView.CameraMode cameraMode)
            {
                TryStoreCameraMode(cameraMode);
                return;
            }

            if (value is IEnumerable enumerable && !(value is string))
            {
                foreach (var item in enumerable)
                {
                    CollectRegisteredCameraModes(item, depth + 1);
                }

                return;
            }

            var type = value.GetType();
            if (type.IsPrimitive || type.IsEnum || type == typeof(string))
                return;

            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                if (!ShouldInspectMember(field.FieldType, field.Name))
                    continue;

                object nestedValue;
                try
                {
                    nestedValue = field.GetValue(value);
                }
                catch
                {
                    continue;
                }

                CollectRegisteredCameraModes(nestedValue, depth + 1);
            }
        }

        private static bool ShouldInspectMember(Type memberType, string memberName)
        {
            if (memberType == typeof(SceneView.CameraMode))
                return true;

            if (typeof(IEnumerable).IsAssignableFrom(memberType))
                return true;

            if (memberName.IndexOf("camera", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            return false;
        }

        private static void TryStoreCameraMode(SceneView.CameraMode cameraMode)
        {
            if (cameraMode.section != SectionName)
                return;

            for (int i = 0; i < OrderedModes.Length; i++)
            {
                var mode = OrderedModes[i];
                if (mode == DrawModePlusMode.None)
                    continue;

                if (cameraMode.name == GetDisplayName(mode))
                {
                    RegisteredCameraModes[mode] = cameraMode;
                    return;
                }
            }
        }
    }
}
