#if UNITY_EDITOR

using Aleab.LoopbackAudioVisualizer.Scripts.Visualizers.Visualizer01;
using Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Extensions;
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
        private SerializedProperty yScaleMultiplier;
        private SerializedProperty maxYScale;

        protected override void OnEnable()
        {
            base.OnEnable();

            this.cubePrefab = this.serializedObject.FindProperty("cubePrefab");
            this.center = this.serializedObject.FindProperty("center");
            this.radius = this.serializedObject.FindProperty("radius");
            this.yScaleMultiplier = this.serializedObject.FindProperty("yScaleMultiplier");
            this.maxYScale = this.serializedObject.FindProperty("maxYScale");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            this.serializedObject.Update();

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            GUILayout.Space(12.0f);
            EditorExtension.DrawPropertyFieldSafe(this.cubePrefab, nameof(this.cubePrefab), new GUIContent("Cube Prefab"));

            GUILayout.Space(6.0f);
            EditorExtension.DrawPropertyFieldSafe(this.center, nameof(this.center), new GUIContent("Center", "The center of the circumference the cubes-frequencies are going to be placed upon."));
            EditorExtension.DrawRangeFieldSafe(this.radius, nameof(this.radius), new GUIContent("Radius", "The radius of the circumference the cubes-frequencies are going to be placed upon."));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
            EditorExtension.DrawRangeFieldSafe(this.yScaleMultiplier, nameof(this.yScaleMultiplier), new GUIContent("Y Scale Multiplier (k)", "Before being displayed, each FFT value will be scaled by this amount (multiplied by 1000)."));

            if (!EditorExtension.DrawTogglePropertyField(this.maxYScale, new GUIContent("Max Y Scale")))
                this.maxYScale.floatValue = -1.0f;

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif