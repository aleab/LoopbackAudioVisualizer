using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace Aleab.LoopbackAudioVisualizer.Unity
{
    public class LabelOverrideAttribute : MultiPropertyAttribute
    {
        private readonly string label;

        public LabelOverrideAttribute(string label)
        {
            this.label = label;
        }

#if UNITY_EDITOR

        public override GUIContent BuildLabel(GUIContent label)
        {
            label.text = this.label;
            return base.BuildLabel(label);
        }

        public override bool OnGUI(Rect position, SerializedProperty property, GUIContent label) => false;

#endif
    }
}