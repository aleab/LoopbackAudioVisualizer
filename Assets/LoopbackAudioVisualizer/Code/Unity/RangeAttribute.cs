using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace Aleab.LoopbackAudioVisualizer.Unity
{
    public class RangeAttribute : MultiPropertyAttribute
    {
        private readonly float min, max;

        public RangeAttribute(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

#if UNITY_EDITOR

        public override bool OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Float:
                    EditorGUI.Slider(position, property, this.min, this.max, label);
                    break;

                case SerializedPropertyType.Integer:
                    EditorGUI.IntSlider(position, property, (int)this.min, (int)this.max, label);
                    break;

                default:
                    EditorGUI.LabelField(position, label.text, "Use Range with float or int.");
                    break;
            }
            return true;
        }

#endif
    }
}