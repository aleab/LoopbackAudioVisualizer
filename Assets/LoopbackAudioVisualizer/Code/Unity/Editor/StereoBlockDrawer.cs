#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Unity.UnityEditor
{
    [CustomPropertyDrawer(typeof(StereoBlock))]
    public class StereoBlockDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.Next(true);
            SerializedProperty leftProperty = property.Copy();
            property.Next(true);
            SerializedProperty rightProperty = property.Copy();

            Rect contentPosition = EditorGUI.PrefixLabel(position, label);

            // Check if there is enough space to put the name on the same line (to save space)
            if (position.height > 16f)
            {
                position.height = 16f;
                EditorGUI.indentLevel += 1;
                contentPosition = EditorGUI.IndentedRect(position);
                contentPosition.y += 18f;
            }

            float half = contentPosition.width / 2;
            GUI.skin.label.padding = new RectOffset(3, 3, 6, 6);

            // Show the values
            EditorGUIUtility.labelWidth = 42.0f;
            contentPosition.width = contentPosition.width * 0.5f - 5.0f;
            EditorGUI.indentLevel = 0;

            EditorGUI.BeginProperty(contentPosition, label, leftProperty);
            {
                EditorGUI.BeginChangeCheck();
                float newVal = EditorGUI.FloatField(contentPosition, new GUIContent("Left"), leftProperty.floatValue);
                if (EditorGUI.EndChangeCheck())
                    leftProperty.floatValue = newVal;
            }
            EditorGUI.EndProperty();

            contentPosition.x += half + 5.0f;

            EditorGUI.BeginProperty(contentPosition, label, rightProperty);
            {
                EditorGUI.BeginChangeCheck();
                float newVal = EditorGUI.FloatField(contentPosition, new GUIContent("Right"), rightProperty.floatValue);
                if (EditorGUI.EndChangeCheck())
                    rightProperty.floatValue = newVal;
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return Screen.width < 333 ? (16f + 18f) : 16f;
        }
    }
}

#endif