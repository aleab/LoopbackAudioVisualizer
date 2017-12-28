#if UNITY_EDITOR

using Aleab.LoopbackAudioVisualizer.Scripts;
using Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Extensions;
using UnityEditor;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Unity.UnityEditor
{
    [CustomEditor(typeof(ScaleUpObject)), CanEditMultipleObjects]
    public class ScaleUpObjectEditor : Editor
    {
        private SerializedProperty meshRenderer;
        private SerializedProperty minimumScale;
        private SerializedProperty smoothScaleDuration;

        protected virtual void OnEnable()
        {
            this.meshRenderer = this.serializedObject.FindProperty("meshRenderer");
            this.minimumScale = this.serializedObject.FindProperty("minimumScale");
            this.smoothScaleDuration = this.serializedObject.FindProperty("smoothScaleDuration");
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(this.serializedObject.FindProperty("m_Script"));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
            EditorExtension.DrawPropertyFieldSafe(this.meshRenderer, nameof(this.meshRenderer), new GUIContent("Mesh Renderer"));

            GUILayout.Space(6.0f);
            EditorExtension.DrawPropertyFieldSafe(this.minimumScale, nameof(this.minimumScale), new GUIContent("Minimum Scale"));
            EditorExtension.DrawPropertyFieldSafe(this.smoothScaleDuration, nameof(this.smoothScaleDuration), new GUIContent("Smooth Scale Duration (ms)"));

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif