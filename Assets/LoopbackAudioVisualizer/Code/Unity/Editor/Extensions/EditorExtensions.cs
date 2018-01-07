#if UNITY_EDITOR

using Aleab.LoopbackAudioVisualizer.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Extensions
{
    public static class EditorExtension
    {
        public static float SingleLineHeight { get { return EditorGUIUtility.singleLineHeight + 2.0f; } }

        public static float OneCharButtonWidth { get { return 18.0f; } }

        public static GUILayoutOption[] CalcMinMaxWidth(GUIContent content, GUIStyle style)
        {
            float minWidth, maxWidth;
            style.CalcMinMaxWidth(content, out minWidth, out maxWidth);
            return new[] { GUILayout.MinWidth(minWidth), GUILayout.MaxWidth(maxWidth) };
        }

        public static float CalcMaxWitdh(GUIContent content, GUIStyle style)
        {
            float _, maxWidth;
            style.CalcMinMaxWidth(content, out _, out maxWidth);
            return maxWidth;
        }

        public static void DrawHeader(string text, GUIStyle style = null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(new GUIContent(text), style ?? EditorStyles.boldLabel);
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

        #region DrawIntRangeFieldSafe

        public static bool DrawIntRangeFieldSafe(SerializedProperty property, string propertyName, int min, int max, params GUILayoutOption[] options)
        {
            if (property != null)
            {
                EditorGUILayout.IntSlider(property, min, max, options);
                return true;
            }
            return DrawErrorLabel(propertyName);
        }

        public static bool DrawIntRangeFieldSafe(SerializedProperty property, string propertyName, int min, int max, GUIContent label, params GUILayoutOption[] options)
        {
            if (property != null)
            {
                EditorGUILayout.IntSlider(property, min, max, label, options);
                return true;
            }
            return DrawErrorLabel(propertyName, $"{label.text}\n{label.tooltip}");
        }

        public static bool DrawIntRangeFieldSafe(SerializedProperty property, string propertyName, int min, int max, int[] steps, GUIContent label, params GUILayoutOption[] options)
        {
            if (property != null)
            {
                int selected = EditorGUILayout.IntSlider(label, property.intValue, min, max, options);
                property.intValue = steps.Where(step => selected >= step).Max();
                return true;
            }
            return DrawErrorLabel(propertyName, $"{label.text}\n{label.tooltip}");
        }

        #endregion DrawIntRangeFieldSafe

        public static bool DrawEnumPopupSafe<TEnum>(SerializedProperty property, string propertyName, GUIContent label, ICollection<TEnum> values, Func<TEnum, string> displayedOptionsSelector = null, params GUILayoutOption[] options)
            where TEnum : struct, IConvertible, IComparable, IFormattable
        {
            if (property != null)
            {
                var displayedOptions = values.Select((e, guiContent) => new GUIContent(displayedOptionsSelector?.Invoke(e) ?? e.ToString(CultureInfo.InvariantCulture))).ToArray();
                property.enumValueIndex = EditorGUILayout.Popup(label, property.enumValueIndex, displayedOptions);
                return true;
            }
            return DrawErrorLabel(propertyName);
        }

        #region DrawCompactArray

        public static bool DrawCompactArray(SerializedProperty property, string propertyName, GUIContent label, int maxLineItems = 3, bool includeChildren = true, params GUILayoutOption[] options)
        {
            if (property != null)
            {
                if (property.isArray)
                {
                    if (maxLineItems <= 0)
                        maxLineItems = 3;

                    // Foldout + Size or Label if array length <= 0
                    EditorGUILayout.BeginHorizontal();
                    property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, label, true, property.arraySize > 0 ? EditorStyles.foldout : Styles.FoldoutNoArrow);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent("Size"), EditorStyles.label, CalcMinMaxWidth(new GUIContent("Size"), EditorStyles.label));
                    property.arraySize = Mathf.Abs(EditorGUILayout.IntField(new GUIContent(string.Empty), property.arraySize, GUILayout.MaxWidth(48.0f)));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndHorizontal();

                    // Draw array items if foldout is expanded and array length > 0
                    if (property.isExpanded && property.arraySize > 0)
                    {
                        int i = 0;
                        SerializedProperty emptyProperty = property.GetArrayElementAtIndex(0).Copy();
                        emptyProperty.ClearValue();

                        float minLineLabelWidth, maxLineLabelWidth;
                        Styles.IndentedRightAlignedMiniLabel.CalcMinMaxWidth(new GUIContent((property.arraySize - 1).ToString()), out minLineLabelWidth, out maxLineLabelWidth);

                        EditorGUILayout.BeginVertical();

                        while (i < property.arraySize)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(new GUIContent(i.ToString()), Styles.IndentedRightAlignedMiniLabel, GUILayout.MinWidth(minLineLabelWidth), GUILayout.MaxWidth(maxLineLabelWidth));
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

        #region DrawArraySafe

        public static bool DrawArraySafe(SerializedProperty property, string propertyName, GUIContent label, bool includeChildren = true, params GUILayoutOption[] options)
        {
            if (property != null)
            {
                if (property.isArray)
                {
                    // Foldout + Size or Label if array length <= 0
                    EditorGUILayout.BeginHorizontal();
                    property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, label, true, property.arraySize > 0 ? EditorStyles.foldout : Styles.FoldoutNoArrow);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent($"Size: {property.arraySize}"), EditorStyles.label);
                    if (GUILayout.Button(new GUIContent("+"), GUILayout.MaxWidth(24.0f)))
                        property.arraySize++;
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndHorizontal();

                    if (property.isExpanded && property.arraySize > 0)
                    {
                        int i = 0;
                        float minLineLabelWidth, maxLineLabelWidth;
                        new GUIStyle(EditorStyles.miniLabel)
                        {
                            alignment = TextAnchor.MiddleRight,
                            padding = { left = 12 * (EditorGUI.indentLevel + 1) }
                        }.CalcMinMaxWidth(new GUIContent((property.arraySize - 1).ToString()), out minLineLabelWidth, out maxLineLabelWidth);

                        GUILayout.Space(4.0f);
                        EditorGUILayout.BeginVertical();
                        while (i < property.arraySize)
                        {
                            SerializedProperty arrayElement = property.GetArrayElementAtIndex(i);
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(new GUIContent($"{i}"), EditorStyles.miniLabel, GUILayout.MinWidth(minLineLabelWidth), GUILayout.MaxWidth(maxLineLabelWidth));
                            EditorGUILayout.PropertyField(arrayElement, GUIContent.none, true);
                            if (GUILayout.Button(new GUIContent("-"), GUILayout.MaxWidth(24.0f)))
                            {
                                if (arrayElement.propertyType == SerializedPropertyType.ObjectReference && arrayElement.objectReferenceValue != null)
                                    property.DeleteArrayElementAtIndex(i);
                                property.DeleteArrayElementAtIndex(i);
                                i--;
                            }
                            EditorGUILayout.EndHorizontal();
                            i++;
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

        public static bool DrawArraySafe(Rect totalPosition, SerializedProperty property, string propertyName, GUIContent label, bool includeChildren = true, string elementLabel = "")
        {
            if (property != null)
            {
                if (property.isArray)
                {
                    EditorGUI.BeginProperty(totalPosition, label, property);

                    // Foldout | Size | [+]
                    GUIContent sizeLabel = new GUIContent($"Size: {property.arraySize}");
                    float sizeLabelWidth = CalcMaxWitdh(sizeLabel, EditorStyles.label) + 18.0f;

                    Rect foldoutLabelRect = new Rect(totalPosition.x, totalPosition.y, CalcMaxWitdh(label, EditorStyles.label), SingleLineHeight);
                    Rect sizeLabelRect = new Rect(totalPosition.x + totalPosition.width - sizeLabelWidth - 8.0f - OneCharButtonWidth - 8.0f, foldoutLabelRect.y, sizeLabelWidth, foldoutLabelRect.height);
                    Rect addBtnRect = new Rect(sizeLabelRect.x + sizeLabelRect.width + 8.0f, foldoutLabelRect.y, OneCharButtonWidth, SingleLineHeight - 2.0f);

                    property.isExpanded = EditorGUI.Foldout(foldoutLabelRect, property.isExpanded, label, true, property.arraySize > 0 ? EditorStyles.foldout : Styles.FoldoutNoArrow);
                    EditorGUI.LabelField(sizeLabelRect, sizeLabel, EditorStyles.label);
                    if (GUI.Button(addBtnRect, new GUIContent("+")))
                        property.InsertNewArrayElement();

                    // Array elements
                    if (property.isExpanded && property.arraySize > 0)
                    {
                        int i = 0;
                        float indexLabelWidth = CalcMaxWitdh(new GUIContent($"{property.arraySize - 1}"), Styles.IndentedRightAlignedMiniLabel) + 16.0f;

                        Rect indentedRect = EditorGUI.IndentedRect(new Rect(totalPosition.x, totalPosition.y, totalPosition.width, totalPosition.height));
                        float currentChildrenHeight = 0.0f;
                        while (i < property.arraySize)
                        {
                            SerializedProperty arrayElement = property.GetArrayElementAtIndex(i);
                            float height = EditorGUI.GetPropertyHeight(arrayElement, true) + (arrayElement.isExpanded ? SingleLineHeight : 0.0f);
                            Rect elementRect = new Rect(indentedRect.x, indentedRect.y + currentChildrenHeight + SingleLineHeight, indentedRect.width, height);
                            currentChildrenHeight += height;

                            EditorGUI.BeginProperty(elementRect, GUIContent.none, arrayElement);
                            // Index | Property | [-]
                            Rect indexRect = new Rect(elementRect.x, elementRect.y, indexLabelWidth, SingleLineHeight);
                            Rect removeBtnRect = new Rect(elementRect.x + elementRect.width - OneCharButtonWidth - 8.0f, elementRect.y, OneCharButtonWidth, SingleLineHeight - 2.0f);
                            Rect propertyRect = new Rect(elementRect.x + indexRect.width, elementRect.y, elementRect.width - indexRect.width - removeBtnRect.width - 16.0f, elementRect.height);

                            EditorGUI.LabelField(indexRect, new GUIContent($"{i}"), Styles.IndentedRightAlignedMiniLabel);
                            EditorGUI.PropertyField(propertyRect, arrayElement, string.IsNullOrWhiteSpace(elementLabel) ? GUIContent.none : new GUIContent($"{elementLabel}{i}"), true);
                            if (GUI.Button(removeBtnRect, new GUIContent("-")))
                            {
                                if (arrayElement.propertyType == SerializedPropertyType.ObjectReference && arrayElement.objectReferenceValue != null)
                                    property.DeleteArrayElementAtIndex(i);
                                property.DeleteArrayElementAtIndex(i);
                                i--;
                            }
                            i++;

                            EditorGUI.EndProperty();
                        }
                    }
                    EditorGUI.EndProperty();
                }
                else
                    EditorGUI.PropertyField(totalPosition, property, label, includeChildren);
                return true;
            }
            return DrawErrorLabel(propertyName, $"{label.text}\n{label.tooltip}");
        }

        #endregion DrawArraySafe

        #region DrawReadonlyArraySafe

        public static bool DrawReadonlyArraySafe(SerializedProperty property, string propertyName, GUIContent label, bool includeChildren = true, params GUILayoutOption[] options)
        {
            if (property != null)
            {
                if (property.isArray)
                {
                    // Foldout + Size or Label if array length <= 0
                    EditorGUILayout.BeginHorizontal();
                    property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, label, true, property.arraySize > 0 ? EditorStyles.foldout : Styles.FoldoutNoArrow);
                    EditorGUILayout.LabelField(new GUIContent($"Size: {property.arraySize}"), EditorStyles.label);
                    EditorGUILayout.EndHorizontal();

                    if (property.isExpanded && property.arraySize > 0)
                    {
                        int i = 0;
                        float minLineLabelWidth, maxLineLabelWidth;
                        GUIStyle style = new GUIStyle(EditorStyles.miniLabel)
                        {
                            alignment = TextAnchor.MiddleRight,
                            padding = { left = 12 * (EditorGUI.indentLevel + 1) }
                        };
                        style.CalcMinMaxWidth(new GUIContent($"{property.arraySize - 1}"), out minLineLabelWidth, out maxLineLabelWidth);

                        GUILayout.Space(4.0f);
                        EditorGUILayout.BeginVertical();
                        while (i < property.arraySize)
                        {
                            SerializedProperty arrayElement = property.GetArrayElementAtIndex(i);
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(new GUIContent($"{i}"), EditorStyles.miniLabel, GUILayout.MinWidth(minLineLabelWidth), GUILayout.MaxWidth(maxLineLabelWidth));
                            EditorGUILayout.PropertyField(arrayElement, GUIContent.none, true);
                            EditorGUILayout.EndHorizontal();
                            i++;
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

        #endregion

        public static bool DrawTogglePropertyField(SerializedProperty property, GUIContent label, bool includeChildren = true, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginHorizontal();
            property.isExpanded = EditorGUILayout.ToggleLeft(GUIContent.none, property.isExpanded, GUILayout.MaxWidth(12.0f));
            EditorGUILayout.LabelField(label, GUILayout.MaxWidth(123.0f));
            EditorGUI.BeginDisabledGroup(!property.isExpanded);
            EditorGUILayout.PropertyField(property, GUIContent.none, includeChildren, options);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            return property.isExpanded;
        }

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