#if UNITY_EDITOR

using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Unity.UnityEditor
{
    [CustomPropertyDrawer(typeof(AudioEndpointVolumeLevels))]
    public class AudioEndpointVolumeLevelsDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.Next(true);
            SerializedProperty decibelsProperty = property.Copy();
            property.Next(true);
            SerializedProperty scalarProperty = property.Copy();

            Rect contentPosition = EditorGUI.PrefixLabel(position, label);

            EditorGUI.BeginDisabledGroup(true);
            string linear = $"{(float.IsInfinity(scalarProperty.floatValue) || float.IsNaN(scalarProperty.floatValue) ? 0 : scalarProperty.floatValue * 100):N0}";
            string decibels = $"{(float.IsInfinity(decibelsProperty.floatValue) || float.IsNaN(decibelsProperty.floatValue) ? "-∞" : decibelsProperty.floatValue.ToString(CultureInfo.InvariantCulture))}";
            EditorGUI.LabelField(contentPosition, $"{linear}% ({decibels} db)");
            EditorGUI.EndDisabledGroup();
        }
    }
}

#endif