#if UNITY_EDITOR

using Aleab.LoopbackAudioVisualizer.LightTuning;
using Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Extensions;
using UnityEditor;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.LightTuning
{
    [CustomPropertyDrawer(typeof(LightSet))]
    public class LightSetDrawer : PropertyDrawer
    {
        private const string RELATIVE_TOOLTIP = "Tuning relative to the lights' default values";

        private SerializedProperty relativeTuning;
        private SerializedProperty lights;
        private SerializedProperty tunings;

        private float tuningsY;

        protected static float SpaceHeight { get { return 5.0f; } }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, EditorExtension.SingleLineHeight), property, label);
            if (property.isExpanded)
            {
                Rect indentedRect = EditorGUI.IndentedRect(position);
                Rect relativeTuningRect = new Rect(indentedRect.x, indentedRect.y + EditorExtension.SingleLineHeight, indentedRect.width, EditorExtension.SingleLineHeight);
                Rect lightsRect = new Rect(indentedRect.x, relativeTuningRect.y + relativeTuningRect.height, indentedRect.width, EditorExtension.SingleLineHeight);
                Rect tuningsRect = new Rect(indentedRect.x, lightsRect.y + lightsRect.height + this.tuningsY, indentedRect.width, EditorExtension.SingleLineHeight);

                this.relativeTuning.boolValue = EditorGUI.Toggle(relativeTuningRect, new GUIContent("Relative Tuning", RELATIVE_TOOLTIP), this.relativeTuning.boolValue);
                EditorExtension.DrawArraySafe(lightsRect, this.lights, nameof(this.lights), new GUIContent("Lights"));
                EditorExtension.DrawArraySafe(tuningsRect, this.tunings, nameof(this.tunings), new GUIContent("Tunings"), true, "Tuning ");
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            this.relativeTuning = property.FindPropertyRelative(nameof(this.relativeTuning));
            this.lights = property.FindPropertyRelative(nameof(this.lights));
            this.tunings = property.FindPropertyRelative(nameof(this.tunings));

            // lights and tunings MUST not be null!
            // No null checks, though; just let them fail and error out if they are null.

            float height = EditorExtension.SingleLineHeight;
            this.tuningsY = 0.0f;

            if (property.isExpanded)
            {
                height += EditorExtension.SingleLineHeight +    // relativeTuning
                          EditorExtension.SingleLineHeight +    // lights
                          EditorExtension.SingleLineHeight;     // tunings

                if (this.lights.isExpanded && this.lights.arraySize > 0)
                {
                    height += this.lights.arraySize * EditorExtension.SingleLineHeight + SpaceHeight;
                    this.tuningsY += this.lights.arraySize * EditorExtension.SingleLineHeight + SpaceHeight;
                }
                if (this.tunings.isExpanded)
                {
                    height += this.tunings.arraySize * EditorExtension.SingleLineHeight;
                    for (int i = 0; i < this.tunings.arraySize; ++i)
                    {
                        var tuningProperty = this.tunings.GetArrayElementAtIndex(i);
                        if (tuningProperty.isExpanded)
                            height += EditorGUI.GetPropertyHeight(tuningProperty);
                    }
                }
            }

            return height;
        }
    }
}

#endif