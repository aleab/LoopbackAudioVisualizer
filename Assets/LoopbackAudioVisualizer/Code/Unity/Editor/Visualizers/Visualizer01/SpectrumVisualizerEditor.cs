#if UNITY_EDITOR

using Aleab.LoopbackAudioVisualizer.Scripts.Visualizers.Visualizer01;
using UnityEditor;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Visualizers
{
    [CustomEditor(typeof(SpectrumVisualizer))]
    public class SpectrumVisualizerEditor : ScaledSpectrumVisualizerEditor
    {
        private SpectrumVisualizer spectrumVisualizer;

        protected override void OnEnable()
        {
            base.OnEnable();

            this.spectrumVisualizer = this.target as SpectrumVisualizer;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            this.serializedObject.Update();

            if (this.spectrumVisualizer != null && EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorGUILayout.Space();
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.FloatField(new GUIContent("Avg. Amplitude"), this.spectrumVisualizer.SpectrumAverageAmplitude);
                EditorGUI.EndDisabledGroup();
            }

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif