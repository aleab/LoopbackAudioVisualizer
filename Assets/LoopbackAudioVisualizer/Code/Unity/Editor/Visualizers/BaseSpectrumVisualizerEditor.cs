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
        private SerializedProperty fftBuffer;

        private void OnEnable()
        {
            this.fftSize = this.serializedObject.FindProperty("fftSize");
            this.fftBuffer = this.serializedObject.FindProperty("fftBuffer");
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(this.serializedObject.FindProperty("m_Script"));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorExtension.DrawPropertyFieldSafe(this.fftSize, nameof(this.fftSize), new GUIContent("FFT Size"));
                EditorGUI.EndDisabledGroup();
            }
            else
                EditorExtension.DrawPropertyFieldSafe(this.fftSize, nameof(this.fftSize), new GUIContent("FFT Size"));

            EditorGUI.BeginDisabledGroup(true);
            EditorExtension.DrawCompactArray(this.fftBuffer, nameof(this.fftBuffer), new GUIContent("FFT Buffer"), 4);
            EditorGUI.EndDisabledGroup();

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif