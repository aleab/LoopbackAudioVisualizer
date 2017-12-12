#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Unity.UnityEditor
{
    [CustomPropertyDrawer(typeof(LabelOverrideAttribute))]
    public class LabelOverrideAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var propertyAttribute = this.attribute as LabelOverrideAttribute;
            label.text = propertyAttribute?.label;
            EditorGUI.PropertyField(position, property, label);
        }
    }
}

#endif