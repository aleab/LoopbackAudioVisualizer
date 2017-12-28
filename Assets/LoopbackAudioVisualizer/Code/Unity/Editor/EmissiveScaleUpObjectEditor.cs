#if UNITY_EDITOR

using Aleab.LoopbackAudioVisualizer.Scripts;
using Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Extensions;
using UnityEditor;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Unity.UnityEditor
{
    [CustomEditor(typeof(EmissiveScaleUpObject)), CanEditMultipleObjects]
    public class EmissiveScaleUpObjectEditor : ScaleUpObjectEditor
    {
        private EmissiveScaleUpObject targetObject;

        private SerializedProperty useSharedMaterial;
        private SerializedProperty bottomLight;
        private SerializedProperty minLightIntensity, maxLightIntensity;

        protected override void OnEnable()
        {
            base.OnEnable();
            this.targetObject = this.target as EmissiveScaleUpObject;

            this.useSharedMaterial = this.serializedObject.FindProperty("useSharedMaterial");
            this.bottomLight = this.serializedObject.FindProperty("bottomLight");
            this.minLightIntensity = this.serializedObject.FindProperty("minLightIntensity");
            this.maxLightIntensity = this.serializedObject.FindProperty("maxLightIntensity");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            this.serializedObject.Update();

            EditorGUILayout.Space();
            EditorExtension.DrawPropertyFieldSafe(this.useSharedMaterial, nameof(this.useSharedMaterial), new GUIContent("Use Shared Material"));

            EditorExtension.DrawHeader("Lights");
            EditorExtension.DrawPropertyFieldSafe(this.bottomLight, nameof(this.bottomLight), new GUIContent("Bottom Light"));
            GUILayout.Space(6.0f);
            EditorExtension.DrawRangeFieldSafe(this.minLightIntensity, nameof(this.minLightIntensity), 0.0f, this.maxLightIntensity.floatValue, new GUIContent("Min. Intensity", "Percentage of the original intensity."));
            EditorExtension.DrawRangeFieldSafe(this.maxLightIntensity, nameof(this.maxLightIntensity), this.minLightIntensity.floatValue, 5.0f, new GUIContent("Max. Intensity", "Percentage of the original intensity."));

            if (EditorApplication.isPlaying)
            {
                EditorExtension.DrawHeader("Colors");
                Color[] currentColors = this.targetObject.CurrentColors;
                Color baseColor = EditorGUILayout.ColorField(new GUIContent("Base Color"), currentColors[0], true, true, false, null);
                Color emissionColor = EditorGUILayout.ColorField(new GUIContent("Emission Color"), currentColors[1], true, false, true, new ColorPickerHDRConfig(0.0f, 10.0f, 0.0f, 3.0f));
                if (!baseColor.Equals(currentColors[0]))
                    this.targetObject.SetBaseColor(baseColor);
                if (!emissionColor.Equals(currentColors[1]))
                    this.targetObject.SetEmissionColor(emissionColor);
            }

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif