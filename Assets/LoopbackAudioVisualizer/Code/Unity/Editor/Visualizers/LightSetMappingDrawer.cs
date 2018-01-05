#if UNITY_EDITOR

using Aleab.LoopbackAudioVisualizer.Scripts.Visualizers;
using Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Extensions;
using UnityEditor;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Visualizers
{
    [CustomPropertyDrawer(typeof(BaseLightsTuner.LightSetMapping))]
    public sealed class LightSetMappingDrawer : PropertyDrawer
    {
        private SerializedProperty name;
        private SerializedProperty index;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect totalPosition = new Rect(position.x, position.y, position.width, EditorExtension.SingleLineHeight);
            Rect fieldNameRect = new Rect(totalPosition.x, totalPosition.y, totalPosition.width * 0.85f, totalPosition.height);
            Rect indexRect = new Rect(fieldNameRect.x + fieldNameRect.width + 5.0f, totalPosition.y, totalPosition.width - fieldNameRect.width - 5.0f, totalPosition.height);

            GUI.enabled = false;
            EditorGUI.PropertyField(fieldNameRect, this.name, GUIContent.none);
            GUI.enabled = true;
            EditorGUI.PropertyField(indexRect, this.index, GUIContent.none);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            this.name = property.FindPropertyRelative(nameof(this.name));
            this.index = property.FindPropertyRelative(nameof(this.index));
            return EditorExtension.SingleLineHeight;
        }
    }
}

#endif