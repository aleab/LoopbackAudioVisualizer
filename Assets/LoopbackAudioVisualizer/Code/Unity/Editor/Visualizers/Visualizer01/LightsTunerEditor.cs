#if UNITY_EDITOR

using Aleab.LoopbackAudioVisualizer.Helpers;
using Aleab.LoopbackAudioVisualizer.Scripts.Visualizers.Visualizer01;
using Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Extensions;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Visualizers.Visualizer01
{
    [CustomEditor(typeof(LightsTuner))]
    public sealed class LightsTunerEditor : BaseLightsTunerEditor
    {
        private static readonly Dictionary<LightsTuner, bool> intMeanSpAFoldouts;

        private LightsTuner lightsTuner;

        private SerializedProperty spectrumVisualizer;
        private SerializedProperty intMeanSpAMin;
        private SerializedProperty intMeanSpAMax;
        private SerializedProperty intMeanSpAThreshold;
        private SerializedProperty intMeanSpASigma;

        static LightsTunerEditor()
        {
            IEqualityComparer<LightsTuner> comparer = new AnonymousComparer<LightsTuner>((v1, v2) => v1.GetInstanceID() == v2.GetInstanceID());

            if (intMeanSpAFoldouts != null)
                intMeanSpAFoldouts.Clear();
            else
                intMeanSpAFoldouts = new Dictionary<LightsTuner, bool>(comparer);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            this.lightsTuner = (LightsTuner)this.target;

            this.spectrumVisualizer = this.serializedObject.FindProperty("spectrumVisualizer");
            this.intMeanSpAMin = this.serializedObject.FindProperty("intMeanSpAMin");
            this.intMeanSpAMax = this.serializedObject.FindProperty("intMeanSpAMax");
            this.intMeanSpAThreshold = this.serializedObject.FindProperty("intMeanSpAThreshold");
            this.intMeanSpASigma = this.serializedObject.FindProperty("intMeanSpASigma");

            if (!intMeanSpAFoldouts.ContainsKey(this.lightsTuner))
                intMeanSpAFoldouts.Add(this.lightsTuner, true);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            this.serializedObject.Update();

            EditorGUILayout.Space();
            EditorExtension.DrawPropertyFieldSafe(this.spectrumVisualizer, nameof(this.spectrumVisualizer), new GUIContent("Spectrum Visualizer"));

            // ================[ Intensity ⨯ Mean Spectrum Amplitude ]================
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(Styles.HelpBoxForFoldout);
            intMeanSpAFoldouts[this.lightsTuner] = EditorGUILayout.Foldout(intMeanSpAFoldouts[this.lightsTuner], new GUIContent("Intensity – Mean Spectrum Amplitude"), true, Styles.FoldoutWithBoldLabel);
            if (intMeanSpAFoldouts[this.lightsTuner])
            {
                EditorGUI.indentLevel++;
                EditorExtension.DrawPropertyFieldSafe(this.intMeanSpAMin, nameof(this.intMeanSpAMin), new GUIContent("Min. Intensity"));
                EditorExtension.DrawPropertyFieldSafe(this.intMeanSpAMax, nameof(this.intMeanSpAMax), new GUIContent("Max. Intensity"));
                EditorExtension.DrawPropertyFieldSafe(this.intMeanSpAThreshold, nameof(this.intMeanSpAThreshold), new GUIContent("Value Threshold"));
                EditorExtension.DrawPropertyFieldSafe(this.intMeanSpASigma, nameof(this.intMeanSpASigma), new GUIContent("\u03C3"));

                Func<float, float> intMeanSpA = value =>
                {
                    float f = Mathf.Exp(-(value - 1.0f) * (value - 1.0f) / (2.0f * this.intMeanSpASigma.floatValue * this.intMeanSpASigma.floatValue));
                    return (this.intMeanSpAMax.floatValue - this.intMeanSpAMin.floatValue) * Mathf.Clamp(f, 0.0f, float.PositiveInfinity) + this.intMeanSpAMin.floatValue;
                };
                float actualMin = intMeanSpA(0.0f);
                float actualMax = intMeanSpA(1.0f);
                GUILayout.Space(4.0f);
                EditorGUILayout.LabelField(new GUIContent("Actual Min / Max"), new GUIContent($"{actualMin} / {actualMax}"));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif