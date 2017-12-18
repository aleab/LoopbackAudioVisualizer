#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Unity.UnityEditor
{
    [CustomEditor(typeof(LoopbackAudioSource))]
    public class LoopbackAudioSourceEditor : Editor
    {
        private SerializedProperty loopbackDeviceName;
        private SerializedProperty currentStereoBlock;

        private void OnEnable()
        {
            this.loopbackDeviceName = this.serializedObject.FindProperty("loopbackDeviceName");
            this.currentStereoBlock = this.serializedObject.FindProperty("currentStereoBlock");
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(this.serializedObject.FindProperty("m_Script"));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(true);
            EditorExtension.DrawPropertyFieldSafe(this.loopbackDeviceName, nameof(this.loopbackDeviceName), new GUIContent("Loopback Device"));
            EditorExtension.DrawPropertyFieldSafe(this.currentStereoBlock, nameof(this.currentStereoBlock), new GUIContent("Current Stereo Block"));
            EditorGUI.EndDisabledGroup();

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif