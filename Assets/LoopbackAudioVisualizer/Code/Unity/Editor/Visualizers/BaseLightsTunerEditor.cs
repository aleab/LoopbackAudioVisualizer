#if UNITY_EDITOR

using Aleab.LoopbackAudioVisualizer.Scripts.Visualizers;
using Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Extensions;
using UnityEditor;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Visualizers
{
    [CustomEditor(typeof(BaseLightsTuner))]
    public class BaseLightsTunerEditor : Editor
    {
        private SerializedProperty autoUpdate;
        private SerializedProperty lightSets;

        protected virtual void OnEnable()
        {
            this.autoUpdate = this.serializedObject.FindProperty("autoUpdate");
            this.lightSets = this.serializedObject.FindProperty("lightSets");
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(this.serializedObject.FindProperty("m_Script"));
            EditorGUI.EndDisabledGroup();

            EditorExtension.DrawPropertyFieldSafe(this.autoUpdate, nameof(this.autoUpdate), new GUIContent("Auto Update"));
            EditorExtension.DrawPropertyFieldSafe(this.lightSets, nameof(this.lightSets), new GUIContent("Light Sets"));

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif