#if UNITY_EDITOR

using System;
using System.Collections;
using System.Reflection;
using UnityEditor;

namespace Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Extensions
{
    public static class Helpers
    {
        public static T GetActualObjectForSerializedProperty<T>(SerializedProperty property) where T : class
        {
            object obj = GetActualObjectForSerializedProperty(property);
            return obj as T;
        }

        public static object GetActualObjectForSerializedProperty(SerializedProperty property, Type objectType)
        {
            object obj = GetActualObjectForSerializedProperty(property);
            return objectType.IsInstanceOfType(obj) ? obj : null;
        }

        private static object GetActualObjectForSerializedProperty(SerializedProperty property)
        {
            object obj = property.serializedObject.targetObject;

            string path = property.propertyPath.Replace(".Array.data[", "[");
            string[] pathElements = path.Split('.');
            foreach (var element in pathElements)
            {
                if (element.Contains("["))
                {
                    string elementName = element.Substring(0, element.IndexOf("[", StringComparison.Ordinal));
                    var index = Convert.ToInt32(element.Substring(element.IndexOf("[", StringComparison.Ordinal)).Replace("[", "").Replace("]", ""));
                    obj = GetValue(obj, elementName, index);
                }
                else
                    obj = GetValue(obj, element);
            }
            return obj;
        }

        private static object GetValue(object source, string name)
        {
            if (source == null)
                return null;
            Type type = source.GetType();

            while (type != null)
            {
                FieldInfo field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (field != null)
                    return field.GetValue(source);

                PropertyInfo property = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (property != null)
                    return property.GetValue(source, null);

                type = type.BaseType;
            }
            return null;
        }

        private static object GetValue(object source, string name, int index)
        {
            var enumerable = GetValue(source, name) as IEnumerable;
            if (enumerable == null)
                return null;

            var enumerator = enumerable.GetEnumerator();
            for (int i = 0; i <= index; i++)
            {
                if (!enumerator.MoveNext())
                    return null;
            }
            return enumerator.Current;
        }

        public static PropertyDrawer GetPropertyDrawerForType(Type type)
        {
            if (type == null)
                return null;

            PropertyDrawer drawer = null;

            Type ScriptAttributeUtility = typeof(PropertyDrawer).Assembly.GetType("UnityEditor.ScriptAttributeUtility");
            MethodInfo GetDrawerTypeForType = ScriptAttributeUtility?.GetMethod("GetDrawerTypeForType", BindingFlags.Static | BindingFlags.NonPublic);
            Type drawerType = (Type)GetDrawerTypeForType?.Invoke(null, new object[] { type });

            if (drawerType != null)
                drawer = (PropertyDrawer)Activator.CreateInstance(drawerType);

            return drawer;
        }
    }
}

#endif