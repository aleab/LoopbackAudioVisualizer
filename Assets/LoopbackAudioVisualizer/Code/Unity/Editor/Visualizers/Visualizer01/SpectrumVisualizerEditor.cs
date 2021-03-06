﻿#if UNITY_EDITOR

using Aleab.LoopbackAudioVisualizer.Helpers;
using Aleab.LoopbackAudioVisualizer.Scripts.Visualizers.Visualizer01;
using Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Extensions;
using UnityEditor;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Visualizers
{
    [CustomEditor(typeof(SpectrumVisualizer))]
    public class SpectrumVisualizerEditor : ScaledSpectrumVisualizerEditor
    {
        private SpectrumVisualizer spectrumVisualizer;

        private SerializedProperty numberOfBands;

        protected override void OnEnable()
        {
            base.OnEnable();

            this.spectrumVisualizer = this.target as SpectrumVisualizer;
            this.numberOfBands = this.serializedObject.FindProperty("numberOfBands");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            this.serializedObject.Update();

            EditorExtension.DrawHeader("ISpectrumMeanAmplitudeProvider", Styles.ItalicsBoldLabel);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.FloatField(new GUIContent("Mean Amplitude"), this.spectrumVisualizer.SpectrumMeanAmplitude);
            EditorGUI.EndDisabledGroup();

            EditorExtension.DrawHeader("IReducedBandsSpectrumProvider", Styles.ItalicsBoldLabel);
            EditorExtension.DrawEnumPopupSafe(this.numberOfBands, nameof(this.numberOfBands), new GUIContent("# of Bands"),
                this.spectrumVisualizer.FftSize.GetPossibleNumberOfFrequencyBands(), bands => $"{(int)bands} bands");

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif