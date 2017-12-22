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
            if (property.propertyType == SerializedPropertyType.Float)
                EditorGUI.Slider(position, property, this.min, this.max, label);
            else
                EditorGUI.LabelField(position, label.text, "Use Range with float or int.");
            return true;
        }

#endif
    }
}