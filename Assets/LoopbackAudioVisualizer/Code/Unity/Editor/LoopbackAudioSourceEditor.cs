#if UNITY_EDITOR

using Aleab.LoopbackAudioVisualizer.Scripts;
using UnityEditor;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Unity.UnityEditor
{
    [CustomEditor(typeof(LoopbackAudioSource))]
    public class LoopbackAudioSourceEditor : Editor
    {
        private SerializedProperty loopbackDeviceName;
        private SerializedProperty audioEndpointVolumeLevels;
        private SerializedProperty currentStereoBlock;

        private void OnEnable()
        {
            this.loopbackDeviceName = this.serializedObject.FindProperty("loopbackDeviceName");
            this.audioEndpointVolumeLevels = this.serializedObject.FindProperty("audioEndpointVolumeLevels");
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
            EditorExtension.DrawPropertyFieldSafe(this.audioEndpointVolumeLevels, nameof(this.audioEndpointVolumeLevels), new GUIContent("Volume"));
            EditorExtension.DrawPropertyFieldSafe(this.currentStereoBlock, nameof(this.currentStereoBlock), new GUIContent("Current Stereo Block"));
            EditorGUI.EndDisabledGroup();

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif