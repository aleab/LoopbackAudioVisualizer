#if UNITY_EDITOR

using Aleab.LoopbackAudioVisualizer.Helpers;
using Aleab.LoopbackAudioVisualizer.Scripts.Visualizers.Visualizer01;
using Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Extensions;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Visualizers.Visualizer01
{
    [CustomEditor(typeof(LightsTuner))]
    public sealed class LightsTunerEditor : BaseLightsTunerEditor
    {
        private static readonly Dictionary<LightsTuner, bool> intMeanSpAFoldouts;
        private static readonly Dictionary<LightsTuner, bool> onOffMeanSpAFoldouts;

        private LightsTuner lightsTuner;
        private MethodInfo intMeanSpAFunction;

        private SerializedProperty spectrumVisualizer;
        private SerializedProperty intMeanSpAMin;
        private SerializedProperty intMeanSpAMax;
        private SerializedProperty intMeanSpAThreshold;
        private SerializedProperty intMeanSpASigma;

        private SerializedProperty onOffMeanSpAThreshold;

        static LightsTunerEditor()
        {
            IEqualityComparer<LightsTuner> comparer = new AnonymousComparer<LightsTuner>((v1, v2) => v1.GetInstanceID() == v2.GetInstanceID());

            if (intMeanSpAFoldouts != null)
                intMeanSpAFoldouts.Clear();
            else
                intMeanSpAFoldouts = new Dictionary<LightsTuner, bool>(comparer);

            if (onOffMeanSpAFoldouts != null)
                onOffMeanSpAFoldouts.Clear();
            else
                onOffMeanSpAFoldouts = new Dictionary<LightsTuner, bool>(comparer);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            this.lightsTuner = (LightsTuner)this.target;
            this.intMeanSpAFunction = typeof(LightsTuner).GetMethod("IntensityTuningOnSpectrumMeanAmplitudeChangeFunction", BindingFlags.Instance | BindingFlags.NonPublic);

            this.spectrumVisualizer = this.serializedObject.FindProperty("spectrumVisualizer");
            this.intMeanSpAMin = this.serializedObject.FindProperty("intMeanSpAMin");
            this.intMeanSpAMax = this.serializedObject.FindProperty("intMeanSpAMax");
            this.intMeanSpAThreshold = this.serializedObject.FindProperty("intMeanSpAThreshold");
            this.intMeanSpASigma = this.serializedObject.FindProperty("intMeanSpASigma");

            this.onOffMeanSpAThreshold = this.serializedObject.FindProperty("onOffMeanSpAThreshold");

            if (!intMeanSpAFoldouts.ContainsKey(this.lightsTuner))
                intMeanSpAFoldouts.Add(this.lightsTuner, true);
            if (!onOffMeanSpAFoldouts.ContainsKey(this.lightsTuner))
                onOffMeanSpAFoldouts.Add(this.lightsTuner, true);
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

                GUILayout.Space(4.0f);
                float actualMin = (float)this.intMeanSpAFunction.Invoke(this.lightsTuner, new object[] { 0.0f });
                float actualMax = (float)this.intMeanSpAFunction.Invoke(this.lightsTuner, new object[] { 1.0f });
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.LabelField(new GUIContent("Actual Minimum"), new GUIContent($"{actualMin}"), EditorStyles.miniLabel);
                EditorGUILayout.LabelField(new GUIContent("Actual Maximum"), new GUIContent($"{actualMax}"), EditorStyles.miniLabel);
                EditorGUI.EndDisabledGroup();
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

            // ================[ On/Off Threshold ⨯ Mean Spectrum Amplitude ]================
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(Styles.HelpBoxForFoldout);
            onOffMeanSpAFoldouts[this.lightsTuner] = EditorGUILayout.Foldout(onOffMeanSpAFoldouts[this.lightsTuner], new GUIContent("On/Off Threshold – Mean Spectrum Amplitude"), true, Styles.FoldoutWithBoldLabel);
            if (onOffMeanSpAFoldouts[this.lightsTuner])
            {
                EditorGUI.indentLevel++;
                EditorExtension.DrawPropertyFieldSafe(this.onOffMeanSpAThreshold, nameof(this.onOffMeanSpAThreshold), new GUIContent("Threshold"));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif