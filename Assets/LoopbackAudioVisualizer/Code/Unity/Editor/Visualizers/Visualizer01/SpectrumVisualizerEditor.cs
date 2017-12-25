#if UNITY_EDITOR

using Aleab.LoopbackAudioVisualizer.Maths;
using Aleab.LoopbackAudioVisualizer.Scripts.Visualizers.Visualizer01;
using Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Extensions;
using System;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Visualizers.Visualizer01
{
    [CustomEditor(typeof(SpectrumVisualizer))]
    public class SpectrumVisualizerEditor : BaseSpectrumVisualizerEditor
    {
        private SerializedProperty cubePrefab;
        private SerializedProperty cubesContainer;
        private SerializedProperty center;
        private SerializedProperty radius;
        private SerializedProperty maxYScale;
        private SerializedProperty equalizationFunctionType;

        private SerializedProperty gaussStdDeviation;
        private SerializedProperty gaussLowFreqGain;
        private SerializedProperty gaussHighFreqGain;

        private SerializedProperty logSteepness;
        private SerializedProperty logHighFreq;
        private SerializedProperty logLowFreqGain;
        private SerializedProperty logHighFreqGain;

        protected override void OnEnable()
        {
            base.OnEnable();

            this.cubePrefab = this.serializedObject.FindProperty("cubePrefab");
            this.cubesContainer = this.serializedObject.FindProperty("cubesContainer");
            this.center = this.serializedObject.FindProperty("center");
            this.radius = this.serializedObject.FindProperty("radius");
            this.maxYScale = this.serializedObject.FindProperty("maxYScale");
            this.equalizationFunctionType = this.serializedObject.FindProperty("equalizationFunctionType");

            this.gaussStdDeviation = this.serializedObject.FindProperty("gaussStdDeviation");
            this.gaussLowFreqGain = this.serializedObject.FindProperty("gaussLowFreqGain");
            this.gaussHighFreqGain = this.serializedObject.FindProperty("gaussHighFreqGain");

            this.logSteepness = this.serializedObject.FindProperty("logSteepness");
            this.logHighFreq = this.serializedObject.FindProperty("logHighFreq");
            this.logLowFreqGain = this.serializedObject.FindProperty("logLowFreqGain");
            this.logHighFreqGain = this.serializedObject.FindProperty("logHighFreqGain");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            this.serializedObject.Update();

            GUILayout.Space(16.0f);
            EditorExtension.DrawPropertyFieldSafe(this.cubePrefab, nameof(this.cubePrefab), new GUIContent("Cube Prefab"));
            EditorExtension.DrawPropertyFieldSafe(this.cubesContainer, nameof(this.cubesContainer), new GUIContent("Cubes Containers"));

            EditorExtension.DrawHeader("Circumference");
            EditorExtension.DrawPropertyFieldSafe(this.center, nameof(this.center), new GUIContent("Center", "The center of the circumference the cube-frequencies are going to be placed upon."));
            EditorExtension.DrawPropertyFieldSafe(this.radius, nameof(this.radius), new GUIContent("Radius", "The radius of the circumference the cubes-frequencies are going to be placed upon."));

            EditorGUILayout.Space();
            if (!EditorExtension.DrawTogglePropertyField(this.maxYScale, new GUIContent("Max. Y Scale", "Maximum Y scale of each cube, relative to the original scale of the prefab.")))
                this.maxYScale.floatValue = -1.0f;

            EditorExtension.DrawHeader("Equalization");
            EditorExtension.DrawPropertyFieldSafe(this.equalizationFunctionType, nameof(this.equalizationFunctionType), new GUIContent("Function Type"));
            FunctionType functionType = (FunctionType)Enum.GetValues(typeof(FunctionType)).GetValue(this.equalizationFunctionType.enumValueIndex);
            EditorGUI.indentLevel++;
            SerializedProperty lowFreqGainProperty, highFreqGainProperty;
            string lowFreqGainPropertyName, highFreqGainPropertyName;
            switch (functionType)
            {
                case FunctionType.Gaussian:
                    EditorExtension.DrawPropertyFieldSafe(this.gaussStdDeviation, nameof(this.gaussStdDeviation), new GUIContent("\u03C3", "Standard deviation.\nThe gaussian is upside-down; the lower this vaule, the more rapidly the gaussian grows."));
                    lowFreqGainProperty = this.gaussLowFreqGain;
                    highFreqGainProperty = this.gaussHighFreqGain;
                    lowFreqGainPropertyName = nameof(this.gaussLowFreqGain);
                    highFreqGainPropertyName = nameof(this.gaussHighFreqGain);
                    break;

                case FunctionType.Logarithm:
                    EditorExtension.DrawPropertyFieldSafe(this.logSteepness, nameof(this.logSteepness), new GUIContent("Steepness", "The lower this value, the more rapidly the logarithm will increase."));
                    EditorExtension.DrawPropertyFieldSafe(this.logHighFreq, nameof(this.logHighFreq), new GUIContent("High Freq. (KHz)", $"The frequency that is considered \"high\"; values at this frequency will get the \"HF Gain\"."));
                    lowFreqGainProperty = this.logLowFreqGain;
                    highFreqGainProperty = this.logHighFreqGain;
                    lowFreqGainPropertyName = nameof(this.logLowFreqGain);
                    highFreqGainPropertyName = nameof(this.logHighFreqGain);
                    break;

                default:
                    throw new InvalidEnumArgumentException(nameof(functionType), (int)functionType, typeof(FunctionType));
            }
            EditorGUI.indentLevel--;

            GUILayout.Space(6.0f);
            EditorExtension.DrawPropertyFieldSafe(lowFreqGainProperty, lowFreqGainPropertyName, new GUIContent("LF Gain", "Gain at low frequencies (multiplied by 1000)."));
            EditorExtension.DrawPropertyFieldSafe(highFreqGainProperty, highFreqGainPropertyName, new GUIContent("HF Gain", "Gain at high frequencies (multiplied by 1000)."));

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif