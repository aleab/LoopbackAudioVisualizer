#if UNITY_EDITOR

using Aleab.LoopbackAudioVisualizer.Helpers;
using UnityEditor;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Unity.UnityEditor
{
    public static class EditorExtension
    {
        public static readonly GUIStyle FoldoutNoArrow = new GUIStyle(EditorStyles.label)
        {
            border = new RectOffset(EditorStyles.foldout.border.left, EditorStyles.foldout.border.right, EditorStyles.foldout.border.top, EditorStyles.foldout.border.bottom),
            margin = new RectOffset(EditorStyles.foldout.margin.left, EditorStyles.foldout.margin.right, EditorStyles.foldout.margin.top, EditorStyles.foldout.margin.bottom),
            padding = new RectOffset(EditorStyles.foldout.padding.left, EditorStyles.foldout.padding.right, EditorStyles.foldout.padding.top, EditorStyles.foldout.padding.bottom)
        };

        public static readonly GUIStyle RightAlignedLabel = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleRight };

        public static readonly GUIStyle IndentedMiniLabel = new GUIStyle(EditorStyles.miniLabel) { padding = { left = 12 } };

        public static readonly GUIStyle IndentedRightAlignedMiniLabel = new GUIStyle(IndentedMiniLabel) { alignment = TextAnchor.MiddleRight };

        public static GUILayoutOption[] CalcMinMaxWidth(GUIContent content, GUIStyle style)
        {
            float minWidth, maxWidth;
            style.CalcMinMaxWidth(content, out minWidth, out maxWidth);
            return new[] { GUILayout.MinWidth(minWidth), GUILayout.MaxWidth(maxWidth) };
        }

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

        #region DrawCompactArray

        public static bool DrawCompactArray(SerializedProperty property, string propertyName, GUIContent label, ref bool foldout, int maxLineItems = 3, bool includeChildren = true, params GUILayoutOption[] options)
        {
            if (property != null)
            {
                if (property.isArray)
                {
                    if (maxLineItems <= 0)
                        maxLineItems = 3;

                    // Foldout + Size or Label if array length <= 0
                    EditorGUILayout.BeginHorizontal();
                    foldout = EditorGUILayout.Foldout(foldout, label, true, property.arraySize > 0 ? EditorStyles.foldout : FoldoutNoArrow);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent("Size"), EditorStyles.label, CalcMinMaxWidth(new GUIContent("Size"), EditorStyles.label));
                    property.arraySize = Mathf.Abs(EditorGUILayout.IntField(new GUIContent(string.Empty), property.arraySize, GUILayout.MaxWidth(48.0f)));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndHorizontal();

                    // Draw array items if foldout is expanded and array length > 0
                    if (foldout && property.arraySize > 0)
                    {
                        int i = 0;
                        SerializedProperty emptyProperty = property.GetArrayElementAtIndex(0).Copy();
                        emptyProperty.ClearValues();

                        float minLineLabelWidth, maxLineLabelWidth;
                        IndentedRightAlignedMiniLabel.CalcMinMaxWidth(new GUIContent((property.arraySize - 1).ToString()), out minLineLabelWidth, out maxLineLabelWidth);
                        
                        EditorGUILayout.BeginVertical();

                        while (i < property.arraySize)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(new GUIContent(i.ToString()), IndentedRightAlignedMiniLabel, GUILayout.MinWidth(minLineLabelWidth), GUILayout.MaxWidth(maxLineLabelWidth));
                            for (int lineIndex = 0; lineIndex < maxLineItems; ++lineIndex, ++i)
                            {
                                if (i < property.arraySize)
                                    EditorGUILayout.PropertyField(property.GetArrayElementAtIndex(i), new GUIContent(string.Empty), true);
                                else
                                {
                                    EditorGUI.BeginDisabledGroup(true);
                                    EditorGUILayout.PropertyField(emptyProperty, new GUIContent(string.Empty), false);
                                    EditorGUI.EndDisabledGroup();
                                }
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        EditorGUILayout.EndVertical();
                    }
                }
                else
                    EditorGUILayout.PropertyField(property, label, includeChildren, options);
                return true;
            }
            return DrawErrorLabel(propertyName, $"{label.text}\n{label.tooltip}");
        }

        #endregion DrawCompactArray

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