#if UNITY_EDITOR

using Aleab.LoopbackAudioVisualizer.Scripts.Visualizers.Visualizer01;
using Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Extensions;
using UnityEditor;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Visualizers.Visualizer01
{
    [CustomEditor(typeof(Visualizer))]
    public sealed class VisualizerEditor : Editor
    {
        private Visualizer visualizer;

        private SerializedProperty spectrumVisualizer;
        private SerializedProperty cubePrefab;
        private SerializedProperty cubesContainer;
        private SerializedProperty center;
        private SerializedProperty radius;
        private SerializedProperty maxYScale;

        private void OnEnable()
        {
            this.visualizer = this.target as Visualizer;

            this.spectrumVisualizer = this.serializedObject.FindProperty("spectrumVisualizer");
            this.cubePrefab = this.serializedObject.FindProperty("cubePrefab");
            this.cubesContainer = this.serializedObject.FindProperty("cubesContainer");
            this.center = this.serializedObject.FindProperty("center");
            this.radius = this.serializedObject.FindProperty("radius");
            this.maxYScale = this.serializedObject.FindProperty("maxYScale");
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(this.serializedObject.FindProperty("m_Script"));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
            EditorExtension.DrawPropertyFieldSafe(this.spectrumVisualizer, nameof(this.spectrumVisualizer), new GUIContent("Spectrum Visualizer"));

            EditorGUILayout.Space();
            EditorExtension.DrawPropertyFieldSafe(this.cubePrefab, nameof(this.cubePrefab), new GUIContent("Cube Prefab"));
            EditorExtension.DrawPropertyFieldSafe(this.cubesContainer, nameof(this.cubesContainer), new GUIContent("Cubes Container"));

            EditorExtension.DrawHeader("Circumference");
            EditorExtension.DrawPropertyFieldSafe(this.center, nameof(this.center), new GUIContent("Center", "The center of the circumference the cube-frequencies are going to be placed upon."));
            EditorExtension.DrawPropertyFieldSafe(this.radius, nameof(this.radius), new GUIContent("Radius", "The radius of the circumference the cubes-frequencies are going to be placed upon."));

            EditorGUILayout.Space();
            if (!EditorExtension.DrawTogglePropertyField(this.maxYScale, new GUIContent("Max. Y Scale", "Maximum Y scale of each cube, relative to the original scale of the prefab.")))
                this.maxYScale.floatValue = -1.0f;

            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorGUILayout.Space();
                if (GUILayout.Button(new GUIContent("Refresh Preview")))
                    this.visualizer.CreateEditorCubes();
            }

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif