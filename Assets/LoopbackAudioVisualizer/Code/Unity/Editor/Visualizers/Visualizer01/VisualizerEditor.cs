#if UNITY_EDITOR

using Aleab.LoopbackAudioVisualizer.Helpers;
using Aleab.LoopbackAudioVisualizer.Scripts.Visualizers.Visualizer01;
using Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Extensions;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Visualizers.Visualizer01
{
    [CustomEditor(typeof(Visualizer))]
    public sealed class VisualizerEditor : Editor
    {
        private static readonly Dictionary<Visualizer, bool> circSpectrumFoldouts;

        private Visualizer visualizer;

        private SerializedProperty spectrumVisualizer;
        private SerializedProperty circCubeTemplate;
        private SerializedProperty circCubesContainer;
        private SerializedProperty circCenter;
        private SerializedProperty circRadius;
        private SerializedProperty circCubeMaxHeight;

        static VisualizerEditor()
        {
            if (circSpectrumFoldouts != null)
                circSpectrumFoldouts.Clear();
            else
                circSpectrumFoldouts = new Dictionary<Visualizer, bool>(new AnonymousComparer<Visualizer>((v1, v2) => v1.GetInstanceID() == v2.GetInstanceID()));
        }

        private void OnEnable()
        {
            this.visualizer = (Visualizer)this.target;

            this.spectrumVisualizer = this.serializedObject.FindProperty("spectrumVisualizer");

            this.circCubeTemplate = this.serializedObject.FindProperty("circCubeTemplate");
            this.circCubesContainer = this.serializedObject.FindProperty("circCubesContainer");
            this.circCenter = this.serializedObject.FindProperty("circCenter");
            this.circRadius = this.serializedObject.FindProperty("circRadius");
            this.circCubeMaxHeight = this.serializedObject.FindProperty("circCubeMaxHeight");

            if (!circSpectrumFoldouts.ContainsKey(this.visualizer))
                circSpectrumFoldouts.Add(this.visualizer, true);
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(this.serializedObject.FindProperty("m_Script"));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
            EditorExtension.DrawPropertyFieldSafe(this.spectrumVisualizer, nameof(this.spectrumVisualizer), new GUIContent("Spectrum Visualizer"));

            // ===================[ Circumference Spectrum ]===================
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(Styles.HelpBoxForFoldout);
            circSpectrumFoldouts[this.visualizer] = EditorGUILayout.Foldout(circSpectrumFoldouts[this.visualizer], new GUIContent("[Spectrum] Circumference"), true, Styles.FoldoutWithBoldLabel);
            if (circSpectrumFoldouts[this.visualizer])
            {
                EditorGUI.indentLevel++;
                EditorExtension.DrawPropertyFieldSafe(this.circCubeTemplate, nameof(this.circCubeTemplate), new GUIContent("Cube Template"));
                EditorExtension.DrawPropertyFieldSafe(this.circCubesContainer, nameof(this.circCubesContainer), new GUIContent("Cubes Container"));

                EditorExtension.DrawHeader("Circumference");
                EditorExtension.DrawPropertyFieldSafe(this.circCenter, nameof(this.circCenter), new GUIContent("Center", "The center of the circumference the cube-frequencies are going to be placed upon."));
                EditorExtension.DrawPropertyFieldSafe(this.circRadius, nameof(this.circRadius), new GUIContent("Radius", "The radius of the circumference the cubes-frequencies are going to be placed upon."));

                EditorGUILayout.Space();
                EditorExtension.DrawPropertyFieldSafe(this.circCubeMaxHeight, nameof(this.circCubeMaxHeight), new GUIContent("Max. Height", "Maximum Y scale of each cube."));

                if (!EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    EditorGUILayout.Space();
                    if (GUILayout.Button(new GUIContent("Refresh Preview")))
                        this.visualizer.CreateEditorCubes();
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif