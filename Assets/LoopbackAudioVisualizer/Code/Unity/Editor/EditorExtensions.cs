#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Unity.UnityEditor
{
    public static class EditorExtension
    {
        public static void DrawHeader(string text)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(new GUIContent(text), EditorStyles.boldLabel);
        }

        #region DrawPropertyFieldSafe

        public static bool DrawPropertyFieldSafe(SerializedProperty property, string propertyName, bool includeChildren = true, params GUILayoutOption[] options)
        {
            if (property != null)
            {
                EditorGUILayout.PropertyField(property, includeChildren, options);
                return true;
            }
            return DrawErrorLabel(propertyName);
        }

        public static bool DrawPropertyFieldSafe(SerializedProperty property, string propertyName, GUIContent label, bool includeChildren = true, params GUILayoutOption[] options)
        {
            if (property != null)
            {
                EditorGUILayout.PropertyField(property, label, includeChildren, options);
                return true;
            }
            return DrawErrorLabel(propertyName, $"{label.text}\n{label.tooltip}");
        }

        #endregion DrawPropertyFieldSafe

        #region DrawRangeFieldSafe

        public static bool DrawRangeFieldSafe(SerializedProperty property, string propertyName, GUIContent label, float maxLabelWidth = 137.0f, bool includeChildren = true, params GUILayoutOption[] options)
        {
            if (property != null)
            {
                EditorGUILayout.BeginHorizontal();
                if (maxLabelWidth >= 0.0f)
                    EditorGUILayout.LabelField(label, GUILayout.MaxWidth(maxLabelWidth));
                else
                    EditorGUILayout.LabelField(label);
                DrawPropertyFieldSafe(property, propertyName, new GUIContent(string.Empty), includeChildren, options);
                EditorGUILayout.EndHorizontal();
                return true;
            }
            return DrawErrorLabel(propertyName, $"{label.text}\n{label.tooltip}");
        }

        public static bool DrawRangeFieldSafe(SerializedProperty property, string propertyName, float min, float max, params GUILayoutOption[] options)
        {
            if (property != null)
            {
                EditorGUILayout.Slider(property, min, max, options);
                return true;
            }
            return DrawErrorLabel(propertyName);
        }

        public static bool DrawRangeFieldSafe(SerializedProperty property, string propertyName, float min, float max, GUIContent label, params GUILayoutOption[] options)
        {
            if (property != null)
            {
                EditorGUILayout.Slider(property, min, max, label, options);
                return true;
            }
            return DrawErrorLabel(propertyName, $"{label.text}\n{label.tooltip}");
        }

        #endregion DrawRangeFieldSafe

        private static bool DrawErrorLabel(string propertyName, string tooltip = "")
        {
            GUIContent content = new GUIContent()
            {
                text = $"[Error in laying out \"{propertyName}\"]",
                tooltip = tooltip
            };
            GUIStyle style = new GUIStyle(EditorStyles.label) { normal = { textColor = Color.Lerp(Color.red, Color.black, 0.25f) } };
            GUILayout.Label(content, style);
            return false;
        }
    }
}

#endif