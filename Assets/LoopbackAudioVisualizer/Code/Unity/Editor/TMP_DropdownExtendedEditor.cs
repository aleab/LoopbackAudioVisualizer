#if UNITY_EDITOR

using Aleab.LoopbackAudioVisualizer.Scripts.UI;
using TMPro.EditorUtilities;
using UnityEditor;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Unity.UnityEditor
{
    // ReSharper disable once InconsistentNaming
    [CustomEditor(typeof(TMP_DropdownExtended))]
    public class TMP_DropdownExtendedEditor : DropdownEditor
    {
        private SerializedProperty arrow;

        protected override void OnEnable()
        {
            base.OnEnable();
            this.arrow = this.serializedObject.FindProperty("arrow");
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(this.serializedObject.FindProperty("m_Script"));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
            EditorExtension.DrawPropertyFieldSafe(this.arrow, nameof(this.arrow), new GUIContent("Arrow"));
            EditorGUILayout.Space();

            this.serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();
        }
    }
}

#endif