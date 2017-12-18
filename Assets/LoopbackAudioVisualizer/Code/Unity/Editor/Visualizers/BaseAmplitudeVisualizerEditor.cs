#if UNITY_EDITOR

using Aleab.LoopbackAudioVisualizer.Scripts.Visualizers;
using System;
using UnityEditor;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Visualizers
{
    [CustomEditor(typeof(BaseAmplitudeVisualizer))]
    public class BaseAmplitudeVisualizerEditor : Editor
    {
        private SerializedProperty useFilter;
        private SerializedProperty sensitivity;
        private SerializedProperty filterBase;
        private SerializedProperty filterK;
        private SerializedProperty filterMean;
        private SerializedProperty filterStdDeviation;
        private SerializedProperty filterLambda;

        private void OnEnable()
        {
            this.useFilter = this.serializedObject.FindProperty("useFilter");
            this.sensitivity = this.serializedObject.FindProperty("sensitivity");
            this.filterBase = this.serializedObject.FindProperty("filterBase");
            this.filterK = this.serializedObject.FindProperty("filterK");
            this.filterMean = this.serializedObject.FindProperty("filterMean");
            this.filterStdDeviation = this.serializedObject.FindProperty("filterStdDeviation");
            this.filterLambda = this.serializedObject.FindProperty("filterLambda");
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(this.serializedObject.FindProperty("m_Script"));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
            EditorExtension.DrawPropertyFieldSafe(this.sensitivity, nameof(this.sensitivity), new GUIContent("Sensitivity"));

            // ============[ FILTER ]============
            GUILayout.Space(10.0f);
            EditorExtension.DrawPropertyFieldSafe(this.useFilter, nameof(this.useFilter), new GUIContent("Use Amplification Filter"));
            if (this.useFilter.boolValue)
            {
                EditorGUILayout.BeginHorizontal();
                EditorExtension.DrawRangeFieldSafe(this.filterBase, nameof(this.filterBase), new GUIContent("Base", "The base of exponentials in the filter's formula"));
                if (GUILayout.Button(new GUIContent("E", $"Set Base to the value of E (~{Math.E:N3})"), GUILayout.Width(20.0f), GUILayout.Height(18.0f)))
                    this.filterBase.floatValue = (float)Math.E;
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(3.0f);
                EditorExtension.DrawRangeFieldSafe(this.filterK, nameof(this.filterK), new GUIContent("K", "Commutation point from Gaussian function to Exponential function"));

                // --------[ Gaussian ]--------
                GUILayout.Space(6.0f);
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Gaussian", GUILayout.MaxWidth(80.0f));
                    EditorGUILayout.BeginVertical();
                    {
                        EditorExtension.DrawRangeFieldSafe(this.filterMean, nameof(this.filterMean), new GUIContent("\u03BC", "Mean"), 22.0f);
                        EditorExtension.DrawRangeFieldSafe(this.filterStdDeviation, nameof(this.filterStdDeviation), new GUIContent("\u03C3", "Standard Deviation"), 22.0f);
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();

                // --------[ Exponential ]--------
                GUILayout.Space(3.0f);
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Exponential", GUILayout.MaxWidth(80.0f));
                    EditorGUILayout.BeginVertical();
                    {
                        EditorExtension.DrawRangeFieldSafe(this.filterLambda, nameof(this.filterLambda), new GUIContent("\u03BB", "Rate or inverse scale"), 22.0f);
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();
            }

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif