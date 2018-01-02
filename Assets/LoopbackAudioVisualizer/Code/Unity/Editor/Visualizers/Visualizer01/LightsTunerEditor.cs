#if UNITY_EDITOR

using Aleab.LoopbackAudioVisualizer.Helpers;
using Aleab.LoopbackAudioVisualizer.Scripts.Visualizers.Visualizer01;
using Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Extensions;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Visualizers.Visualizer01
{
    [CustomEditor(typeof(LightsTuner))]
    public sealed class LightsTunerEditor : BaseLightsTunerEditor
    {
        private static readonly Dictionary<LightsTuner, bool> onOffMeanSpAFoldouts;

        private LightsTuner lightsTuner;

        private SerializedProperty spectrumVisualizer;
        private SerializedProperty onOffMeanSpAThreshold;

        static LightsTunerEditor()
        {
            IEqualityComparer<LightsTuner> comparer = new AnonymousComparer<LightsTuner>((v1, v2) => v1.GetInstanceID() == v2.GetInstanceID());

            if (onOffMeanSpAFoldouts != null)
                onOffMeanSpAFoldouts.Clear();
            else
                onOffMeanSpAFoldouts = new Dictionary<LightsTuner, bool>(comparer);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            this.lightsTuner = (LightsTuner)this.target;

            this.spectrumVisualizer = this.serializedObject.FindProperty("spectrumVisualizer");
            this.onOffMeanSpAThreshold = this.serializedObject.FindProperty("onOffMeanSpAThreshold");

            if (!onOffMeanSpAFoldouts.ContainsKey(this.lightsTuner))
                onOffMeanSpAFoldouts.Add(this.lightsTuner, true);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            this.serializedObject.Update();

            EditorGUILayout.Space();
            EditorExtension.DrawPropertyFieldSafe(this.spectrumVisualizer, nameof(this.spectrumVisualizer), new GUIContent("Spectrum Visualizer"));

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