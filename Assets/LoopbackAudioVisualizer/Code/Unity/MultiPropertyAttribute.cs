using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace Aleab.LoopbackAudioVisualizer.Unity
{
    [AttributeUsage(AttributeTargets.Field)]
    public abstract class MultiPropertyAttribute : PropertyAttribute
    {
        public List<MultiPropertyAttribute> stored = new List<MultiPropertyAttribute>();

#if UNITY_EDITOR

        public virtual GUIContent BuildLabel(GUIContent label)
        {
            return label;
        }

        /// <summary>
        /// Override this method to make your own GUI for the property.
        /// </summary>
        /// <param name="position"> Rectangle on the screen to use for the property GUI. </param>
        /// <param name="property"> The SerializedProperty to make the custom GUI for. </param>
        /// <param name="label"> The label of this property. </param>
        /// <returns> True if the property has been drawn; false if this method just changed the GUI state. </returns>
        public abstract bool OnGUI(Rect position, SerializedProperty property, GUIContent label);


        /// <summary>
        /// Derived classes should override this method to define a new height for the property's label.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="label"> The label. </param>
        /// <returns> A Nullable height; null if the height is unchanged. </returns>
        public virtual float? GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return null;
        }

#endif
    }
}