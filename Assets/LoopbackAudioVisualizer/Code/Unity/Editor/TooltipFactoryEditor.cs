using Aleab.LoopbackAudioVisualizer.Scripts;
using UnityEditor;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Unity.UnityEditor
{
    [CustomEditor(typeof(TooltipFactory))]
    public class TooltipFactoryEditor : Editor
    {
        private SerializedProperty cleanupFrequency;

        private void OnEnable()
        {
            this.cleanupFrequency = this.serializedObject.FindProperty("cleanupFrequency");
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(this.serializedObject.FindProperty("m_Script"));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
            EditorExtension.DrawPropertyFieldSafe(this.cleanupFrequency, nameof(this.cleanupFrequency), new GUIContent("Clean-Up Frequency", "Clean-Up Frequency in seconds"));

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}