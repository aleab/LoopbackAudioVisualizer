#if UNITY_EDITOR

using Aleab.LoopbackAudioVisualizer.Scripts.UI;
using Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Extensions;
using UnityEditor;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Unity.UnityEditor
{
    [CustomEditor(typeof(ShowTooltipOnHover))]
    public class ShowTooltipOnHoverEditor : Editor
    {
        private ShowTooltipOnHover targetObject;

        private SerializedProperty useTextSource;
        private SerializedProperty text;
        private SerializedProperty textSource;
        private SerializedProperty delayMilliseconds;
        private SerializedProperty fadeDurationMilliseconds;
        private SerializedProperty position;
        private SerializedProperty tooltipPrefab;
        private SerializedProperty alwaysRecreateTooltip;

        private void OnEnable()
        {
            this.targetObject = this.target as ShowTooltipOnHover;

            this.useTextSource = this.serializedObject.FindProperty(nameof(this.targetObject.useTextSource));
            this.text = this.serializedObject.FindProperty("text");
            this.textSource = this.serializedObject.FindProperty("textSource");
            this.delayMilliseconds = this.serializedObject.FindProperty("delayMilliseconds");
            this.fadeDurationMilliseconds = this.serializedObject.FindProperty("fadeDurationMilliseconds");
            this.position = this.serializedObject.FindProperty("position");
            this.tooltipPrefab = this.serializedObject.FindProperty("tooltipPrefab");
            this.alwaysRecreateTooltip = this.serializedObject.FindProperty("alwaysRecreateTooltip");
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(this.serializedObject.FindProperty("m_Script"));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
            EditorExtension.DrawPropertyFieldSafe(this.tooltipPrefab, nameof(this.tooltipPrefab), new GUIContent("Tooltip Prefab"));

            // [ Text ]––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––
            EditorExtension.DrawHeader("Text");
            EditorExtension.DrawPropertyFieldSafe(this.useTextSource, nameof(this.useTextSource), new GUIContent("Use Text Source"));
            if (this.useTextSource.boolValue)
            {
                EditorExtension.DrawPropertyFieldSafe(this.textSource, nameof(this.textSource), new GUIContent("Text Source"));
                if (!this.targetObject.SetTextFromTextSource())
                    this.text.stringValue = null;

                EditorGUI.BeginDisabledGroup(true);
                EditorExtension.DrawPropertyFieldSafe(this.text, nameof(this.text));
                EditorGUI.EndDisabledGroup();
            }
            else
                EditorExtension.DrawPropertyFieldSafe(this.text, nameof(this.text));


            // [ Tooltip Properties ]––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––
            EditorExtension.DrawHeader("Tooltip Properties");
            EditorExtension.DrawPropertyFieldSafe(this.position, nameof(this.position), new GUIContent("Position"));
            EditorExtension.DrawPropertyFieldSafe(this.delayMilliseconds, nameof(this.delayMilliseconds), new GUIContent("Delay (ms)"));
            EditorExtension.DrawPropertyFieldSafe(this.fadeDurationMilliseconds, nameof(this.fadeDurationMilliseconds), new GUIContent("Fade Duration (ms)"));


            EditorGUILayout.Space();
            EditorExtension.DrawPropertyFieldSafe(this.alwaysRecreateTooltip, nameof(this.alwaysRecreateTooltip), new GUIContent("Re-Create Always", "Re-create the tooltip every time it needs to be shown; the default behaviour is to create it once and leave it hidden in the scene if not shown.\n\nSome hoverable components (e.g. dropdown items) might be re-created periodically themselves, thus causing a large number of unused objects to be created."));

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif