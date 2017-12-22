#if UNITY_EDITOR

using Aleab.LoopbackAudioVisualizer.Scripts.Visualizers;
using Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Extensions;
using UnityEditor;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Visualizers
{
    [CustomEditor(typeof(BaseSpectrumVisualizer))]
    public class BaseSpectrumVisualizerEditor : Editor
    {
        private SerializedProperty fftSize;
        private SerializedProperty rawFftDataBuffer;
        private SerializedProperty fftDataBuffer;

        protected virtual void OnEnable()
        {
            this.fftSize = this.serializedObject.FindProperty("fftSize");
            this.rawFftDataBuffer = this.serializedObject.FindProperty("rawFftDataBuffer");
            this.fftDataBuffer = this.serializedObject.FindProperty("fftDataBuffer");
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(this.serializedObject.FindProperty("m_Script"));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
            EditorExtension.DrawPropertyFieldSafe(this.fftSize, nameof(this.fftSize), new GUIContent("FFT Size"));

            EditorGUI.BeginDisabledGroup(true);
            EditorExtension.DrawCompactArray(this.rawFftDataBuffer, nameof(this.rawFftDataBuffer), new GUIContent("Raw FFT Data"), 4);
            GUILayout.Space(4.0f);
            EditorExtension.DrawCompactArray(this.fftDataBuffer, nameof(this.fftDataBuffer), new GUIContent("FFT Data"), 4);
            EditorGUI.EndDisabledGroup();

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif