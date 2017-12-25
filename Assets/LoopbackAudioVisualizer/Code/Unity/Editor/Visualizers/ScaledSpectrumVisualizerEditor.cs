#if UNITY_EDITOR

using Aleab.LoopbackAudioVisualizer.Maths;
using Aleab.LoopbackAudioVisualizer.Scripts.Visualizers;
using Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Extensions;
using System;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Visualizers
{
    [CustomEditor(typeof(ScaledSpectrumVisualizer))]
    public class ScaledSpectrumVisualizerEditor : BaseSpectrumVisualizerEditor
    {
        private SerializedProperty scalingFunctionType;

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

            this.scalingFunctionType = this.serializedObject.FindProperty("scalingFunctionType");

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

            EditorExtension.DrawHeader("Spectrum Scaling");
            EditorExtension.DrawPropertyFieldSafe(this.scalingFunctionType, nameof(this.scalingFunctionType), new GUIContent("Function Type"));
            FunctionType functionType = (FunctionType)Enum.GetValues(typeof(FunctionType)).GetValue(this.scalingFunctionType.enumValueIndex);
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