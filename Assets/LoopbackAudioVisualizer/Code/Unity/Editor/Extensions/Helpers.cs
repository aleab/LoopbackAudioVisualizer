using System;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Extensions
{
    public static class Helpers
    {
        public static T GetActualObjectForSerializedProperty<T>(SerializedProperty property, FieldInfo fieldInfo) where T : class
        {
            var obj = fieldInfo.GetValue(property.serializedObject.targetObject);
            if (obj == null)
                return null;

            T actualObject;
            if (obj.GetType().IsArray)
            {
                var index = Convert.ToInt32(new string(property.propertyPath.Where(char.IsDigit).ToArray()));
                actualObject = ((T[])obj)[index];
            }
            else
                actualObject = obj as T;
            return actualObject;
        }
    }
}