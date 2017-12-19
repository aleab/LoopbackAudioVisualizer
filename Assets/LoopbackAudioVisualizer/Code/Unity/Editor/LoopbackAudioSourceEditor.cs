#if UNITY_EDITOR

using Aleab.LoopbackAudioVisualizer.Scripts;
using Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Extensions;
using UnityEditor;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Unity.UnityEditor
{
    [CustomEditor(typeof(LoopbackAudioSource))]
    public class LoopbackAudioSourceEditor : Editor
    {
        private SerializedProperty loopbackDeviceName;
        private SerializedProperty audioEndpointVolumeLevels;
        private SerializedProperty currentAudioBlock;

        private void OnEnable()
        {
            this.loopbackDeviceName = this.serializedObject.FindProperty("loopbackDeviceName");
            this.audioEndpointVolumeLevels = this.serializedObject.FindProperty("audioEndpointVolumeLevels");
            this.currentAudioBlock = this.serializedObject.FindProperty("currentAudioBlock");
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
            EditorExtension.DrawPropertyFieldSafe(this.currentAudioBlock, nameof(this.currentAudioBlock), new GUIContent("Current Stereo Block"));
            EditorGUI.EndDisabledGroup();

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif