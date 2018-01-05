#if UNITY_EDITOR

using Aleab.LoopbackAudioVisualizer.Scripts.Visualizers.Visualizer01;
using Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Extensions;
using UnityEditor;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Visualizers.Visualizer01
{
    [CustomEditor(typeof(LightsTuner))]
    public sealed class LightsTunerEditor : BaseLightsTunerEditor
    {
        private SerializedProperty spectrumVisualizer;

        protected override void OnEnable()
        {
            base.OnEnable();

            this.spectrumVisualizer = this.serializedObject.FindProperty("spectrumVisualizer");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            this.serializedObject.Update();

            EditorGUILayout.Space();
            EditorExtension.DrawPropertyFieldSafe(this.spectrumVisualizer, nameof(this.spectrumVisualizer), new GUIContent("Spectrum Visualizer"));

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif