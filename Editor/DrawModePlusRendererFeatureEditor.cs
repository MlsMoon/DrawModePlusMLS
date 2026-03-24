using UnityEditor;
using UnityEngine;

namespace DrawModePlusMLS.Editor
{
    [CustomEditor(typeof(DrawModePlusRendererFeature))]
    public class DrawModePlusRendererFeatureEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var enableGameViewProp = serializedObject.FindProperty("enableGameView");
            EditorGUILayout.PropertyField(enableGameViewProp, new GUIContent("Enable Game View"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
