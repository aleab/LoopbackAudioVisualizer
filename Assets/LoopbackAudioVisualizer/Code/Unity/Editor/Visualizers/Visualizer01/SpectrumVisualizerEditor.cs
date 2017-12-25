#if UNITY_EDITOR

using Aleab.LoopbackAudioVisualizer.Scripts.Visualizers.Visualizer01;
using Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Extensions;
using UnityEditor;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Visualizers.Visualizer01
{
    [CustomEditor(typeof(SpectrumVisualizer))]
    public sealed class SpectrumVisualizerEditor : Editor
    {
        private SerializedProperty scaledSpectrumVisualizer;
        private SerializedProperty cubePrefab;
        private SerializedProperty cubesContainer;
        private SerializedProperty center;
        private SerializedProperty radius;
        private SerializedProperty maxYScale;

        private void OnEnable()
        {
            this.scaledSpectrumVisualizer = this.serializedObject.FindProperty("scaledSpectrumVisualizer");
            this.cubePrefab = this.serializedObject.FindProperty("cubePrefab");
            this.cubesContainer = this.serializedObject.FindProperty("cubesContainer");
            this.center = this.serializedObject.FindProperty("center");
            this.radius = this.serializedObject.FindProperty("radius");
            this.maxYScale = this.serializedObject.FindProperty("maxYScale");
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            GUILayout.Space(16.0f);
            EditorExtension.DrawPropertyFieldSafe(this.scaledSpectrumVisualizer, nameof(this.scaledSpectrumVisualizer), new GUIContent("Spectrum Visualizer"));
            EditorExtension.DrawPropertyFieldSafe(this.cubePrefab, nameof(this.cubePrefab), new GUIContent("Cube Prefab"));
            EditorExtension.DrawPropertyFieldSafe(this.cubesContainer, nameof(this.cubesContainer), new GUIContent("Cubes Containers"));

            EditorExtension.DrawHeader("Circumference");
            EditorExtension.DrawPropertyFieldSafe(this.center, nameof(this.center), new GUIContent("Center", "The center of the circumference the cube-frequencies are going to be placed upon."));
            EditorExtension.DrawPropertyFieldSafe(this.radius, nameof(this.radius), new GUIContent("Radius", "The radius of the circumference the cubes-frequencies are going to be placed upon."));

            EditorGUILayout.Space();
            if (!EditorExtension.DrawTogglePropertyField(this.maxYScale, new GUIContent("Max. Y Scale", "Maximum Y scale of each cube, relative to the original scale of the prefab.")))
                this.maxYScale.floatValue = -1.0f;

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif