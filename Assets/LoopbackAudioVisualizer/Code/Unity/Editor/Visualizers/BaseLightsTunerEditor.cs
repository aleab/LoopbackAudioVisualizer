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
        private BaseLightsTuner baseLightsTuner;

        private SerializedProperty autoUpdate;
        private SerializedProperty lightSets;
        private SerializedProperty lightSetMappings;

        protected virtual void OnEnable()
        {
            this.baseLightsTuner = this.serializedObject.targetObject as BaseLightsTuner;

            this.autoUpdate = this.serializedObject.FindProperty("autoUpdate");
            this.lightSets = this.serializedObject.FindProperty("lightSets");
            this.lightSetMappings = this.serializedObject.FindProperty("lightSetMappings");
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(this.serializedObject.FindProperty("m_Script"));
            EditorGUI.EndDisabledGroup();

            EditorExtension.DrawPropertyFieldSafe(this.autoUpdate, nameof(this.autoUpdate), new GUIContent("Auto Update"));
            EditorExtension.DrawPropertyFieldSafe(this.lightSets, nameof(this.lightSets), new GUIContent("Light Sets"));

            // FIX for `ArgumentException: GUILayout: Mismatched LayoutGroup.repaint`
            if (Event.current.type == EventType.Repaint)
                this.baseLightsTuner.PopulateLightSetMappingNames();
            EditorExtension.DrawReadonlyArraySafe(this.lightSetMappings, nameof(this.lightSetMappings), new GUIContent("Light Set Mappings"));

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif