#if UNITY_EDITOR

using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Unity.UnityEditor
{
    [CustomPropertyDrawer(typeof(MultiPropertyAttribute), true)]
    public class MultiPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            MultiPropertyAttribute thisAttribute = this.attribute as MultiPropertyAttribute;

            float height = base.GetPropertyHeight(property, label);
            if (thisAttribute != null && thisAttribute.stored.Count > 0)
                height = thisAttribute.stored.Max(attr => attr.GetPropertyHeight(property, label) ?? height);
            return height;
        }

        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            MultiPropertyAttribute thisAttribute = this.attribute as MultiPropertyAttribute;

            if (thisAttribute == null)
            {
                base.OnGUI(position, property, label);
                return;
            }

            // First get the attribute since it contains the range for the slider
            if (thisAttribute.stored == null || thisAttribute.stored.Count == 0)
                thisAttribute.stored = this.fieldInfo.GetCustomAttributes(typeof(MultiPropertyAttribute), false)
                                                     .Cast<MultiPropertyAttribute>()
                                                     .OrderBy(a => a.order).ToList();

            // Store current GUI state
            string origTooltip = GUI.tooltip;
            Color origBackgroundColor = GUI.backgroundColor;
            Color origColor = GUI.color;
            Color origContentColor = GUI.contentColor;
            bool origEnabled = GUI.enabled;
            int origIndentLevel = EditorGUI.indentLevel;

            bool propertyDrawn = false;
            foreach (var attr in thisAttribute.stored)
            {
                label = attr.BuildLabel(label);
                propertyDrawn |= attr.OnGUI(position, property, label);
            }
            if (!propertyDrawn)
                EditorGUI.PropertyField(position, property, label);

            // Restore GUI state
            GUI.tooltip = origTooltip;
            GUI.backgroundColor = origBackgroundColor;
            GUI.color = origColor;
            GUI.contentColor = origContentColor;
            GUI.enabled = origEnabled;
            EditorGUI.indentLevel = origIndentLevel;
        }
    }
}

#endif