using UnityEditor;
using UnityEngine;

namespace DrawModePlusMLS.Editor
{
    public class DrawModePlusControlWindow : EditorWindow
    {
        private const string WindowTitle = "DrawModeDisplayControl";
        private const string HelperText =
            "Use this panel to switch DrawModePlus debug display modes for SceneView and GameView.\n" +
            "Author: minlesheng\n" +
            "Modified: 2026-03-24";

        [MenuItem("Tools/DrawModePlus/DrawMode 显示控制面板", false, 181)]
        private static void OpenWindow()
        {
            var window = GetWindow<DrawModePlusControlWindow>(WindowTitle);
            window.minSize = new Vector2(320f, 190f);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("DrawMode Display Control", EditorStyles.boldLabel);
            GUILayout.Space(4f);
            EditorGUILayout.HelpBox(HelperText, MessageType.Info);
            GUILayout.Space(4f);

            var displayNames = DrawModePlusModeRegistry.GetDisplayNames();
            int currentIndex = DrawModePlusModeRegistry.GetIndex(DrawModePlusRuntimeState.CurrentMode);

            EditorGUI.BeginChangeCheck();
            int nextIndex = EditorGUILayout.Popup("Mode", currentIndex, displayNames);
            if (EditorGUI.EndChangeCheck())
            {
                DrawModePlusModeRegistry.ApplyMode(DrawModePlusModeRegistry.GetModeAt(nextIndex));
            }

            if (DrawModePlusRuntimeState.CurrentMode == DrawModePlusMode.Depth)
            {
                EditorGUI.BeginChangeCheck();
                float depthMeter = EditorGUILayout.Slider("Depth Range", DrawModePlusRuntimeState.DepthMeter, 1f, 500f);
                if (EditorGUI.EndChangeCheck())
                {
                    DrawModePlusRuntimeState.SetDepthMeter(depthMeter);
                    SceneView.RepaintAll();
                }
            }
        }
    }
}
