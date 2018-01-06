using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace Aleab.LoopbackAudioVisualizer.Unity
{
    public class ReadOnlyAttribute : MultiPropertyAttribute
    {
#if UNITY_EDITOR

        protected virtual bool Enabled { get { return false; } }

        public override bool OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = this.Enabled;
            return false;
        }

#endif
    }
}