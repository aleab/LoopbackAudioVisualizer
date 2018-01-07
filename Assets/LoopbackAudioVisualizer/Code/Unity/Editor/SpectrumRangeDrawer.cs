#if UNITY_EDITOR

using Aleab.LoopbackAudioVisualizer.Code;
using Aleab.LoopbackAudioVisualizer.Helpers;
using UnityEditor;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Unity.UnityEditor
{
    [CustomPropertyDrawer(typeof(SpectrumRange))]
    public class SpectrumRangeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SpectrumRange spectrumRange = property.GetActualObject<SpectrumRange>();

            float[] values = { spectrumRange.lowerFrequency, spectrumRange.higherFrequency };
            EditorGUI.MultiFloatField(position, label, new[] { new GUIContent("L", "Lower frequency (Hz)"), new GUIContent("H", "Higher frequency (Hz)") }, values);
            spectrumRange.lowerFrequency = values[0];
            spectrumRange.higherFrequency = values[1];
        }
    }
}

#endif