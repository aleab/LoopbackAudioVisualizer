#if UNITY_EDITOR

using Aleab.LoopbackAudioVisualizer.Maths;
using Aleab.LoopbackAudioVisualizer.Scripts.Visualizers.Visualizer01;
using Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Extensions;
using System;
using UnityEditor;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Visualizers.Visualizer01
{
    [CustomEditor(typeof(SpectrumVisualizer))]
    public class SpectrumVisualizerEditor : BaseSpectrumVisualizerEditor
    {
        private SerializedProperty cubePrefab;
        private SerializedProperty center;
        private SerializedProperty radius;
        private SerializedProperty maxYScale;
        private SerializedProperty lowFreqGain;
        private SerializedProperty highFreqGain;
        private SerializedProperty equalizationFunctionType;
        private SerializedProperty gaussStdDeviation;
        private SerializedProperty logSteepness;

        protected override void OnEnable()
        {
            base.OnEnable();

            this.cubePrefab = this.serializedObject.FindProperty("cubePrefab");
            this.center = this.serializedObject.FindProperty("center");
            this.radius = this.serializedObject.FindProperty("radius");
            this.maxYScale = this.serializedObject.FindProperty("maxYScale");
            this.lowFreqGain = this.serializedObject.FindProperty("lowFreqGain");
            this.highFreqGain = this.serializedObject.FindProperty("highFreqGain");
            this.equalizationFunctionType = this.serializedObject.FindProperty("equalizationFunctionType");
            this.gaussStdDeviation = this.serializedObject.FindProperty("gaussStdDeviation");
            this.logSteepness = this.serializedObject.FindProperty("logSteepness");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            this.serializedObject.Update();

            GUILayout.Space(16.0f);
            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            EditorExtension.DrawPropertyFieldSafe(this.cubePrefab, nameof(this.cubePrefab), new GUIContent("Cube Prefab"));

            EditorExtension.DrawHeader("Cubes' circumference");
            EditorExtension.DrawPropertyFieldSafe(this.center, nameof(this.center), new GUIContent("Center", "The center of the circumference the cubes-frequencies are going to be placed upon."));
            EditorExtension.DrawRangeFieldSafe(this.radius, nameof(this.radius), new GUIContent("Radius", "The radius of the circumference the cubes-frequencies are going to be placed upon."));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
            if (!EditorExtension.DrawTogglePropertyField(this.maxYScale, new GUIContent("Max Y Scale", "Maximum Y scale of each cube.")))
                this.maxYScale.floatValue = -1.0f;

            EditorExtension.DrawHeader("Equalization");
            EditorExtension.DrawPropertyFieldSafe(this.lowFreqGain, nameof(this.lowFreqGain), new GUIContent("LF Gain", "Gain at low frequencies (multiplied by 1000)."));
            EditorExtension.DrawPropertyFieldSafe(this.highFreqGain, nameof(this.highFreqGain), new GUIContent("HF Gain", "Gain at high frequencies (multiplied by 1000)."));

            GUILayout.Space(6.0f);
            EditorExtension.DrawPropertyFieldSafe(this.equalizationFunctionType, nameof(this.equalizationFunctionType), new GUIContent("Function Type"));
            FunctionType functionType = (FunctionType)Enum.GetValues(typeof(FunctionType)).GetValue(this.equalizationFunctionType.enumValueIndex);
            EditorGUI.indentLevel++;
            switch (functionType)
            {
                case FunctionType.Gaussian:
                    EditorExtension.DrawPropertyFieldSafe(this.gaussStdDeviation, nameof(this.gaussStdDeviation), new GUIContent("\u03C3", "Standard deviation.\nThe gaussian is upside-down; the lower this vaule, the more rapidly the gaussian grows."));
                    break;

                case FunctionType.Logarithm:
                    EditorExtension.DrawPropertyFieldSafe(this.logSteepness, nameof(this.logSteepness), new GUIContent("Steepness", "The lower this value, the more rapidly the logarithm will increase."));
                    break;
            }
            EditorGUI.indentLevel--;

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif