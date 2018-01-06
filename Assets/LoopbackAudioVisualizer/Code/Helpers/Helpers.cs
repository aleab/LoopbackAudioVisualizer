using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace Aleab.LoopbackAudioVisualizer.Helpers
{
    public static class Helpers
    {
        public static int ToNearestPowerOfTwo(int n)
        {
            return (int)Mathf.Pow(2, Mathf.Round(Mathf.Log(n) / Mathf.Log(2)));
        }

#if UNITY_EDITOR

        [SuppressMessage("ReSharper", "RedundantCaseLabel"), SuppressMessage("ReSharper", "EmptyGeneralCatchClause")]
        public static void ClearValue(this SerializedProperty property)
        {
            var logHandler = Debug.unityLogger.logHandler;
            Debug.unityLogger.logHandler = null;
            Debug.unityLogger.logEnabled = false;

            switch (property.propertyType)
            {
                case SerializedPropertyType.AnimationCurve:
                    property.animationCurveValue = null;
                    break;

                case SerializedPropertyType.ArraySize:
                    property.arraySize = default(int);
                    break;

                case SerializedPropertyType.Boolean:
                    property.boolValue = default(bool);
                    break;

                case SerializedPropertyType.Bounds:
                    property.boundsValue = default(Bounds);
                    break;

                case SerializedPropertyType.BoundsInt:
                    property.boundsIntValue = default(BoundsInt);
                    break;

                case SerializedPropertyType.Color:
                    property.colorValue = default(Color);
                    break;

                case SerializedPropertyType.Enum:
                    property.enumValueIndex = default(int);
                    break;

                case SerializedPropertyType.ExposedReference:
                    property.exposedReferenceValue = null;
                    break;

                case SerializedPropertyType.FixedBufferSize:
                    break;

                case SerializedPropertyType.Float:
                    property.floatValue = default(float);
                    break;

                case SerializedPropertyType.Integer:
                    property.intValue = default(int);
                    break;

                case SerializedPropertyType.ObjectReference:
                    property.objectReferenceValue = null;
                    break;

                case SerializedPropertyType.Quaternion:
                    property.quaternionValue = default(Quaternion);
                    break;

                case SerializedPropertyType.Rect:
                    property.rectValue = default(Rect);
                    break;

                case SerializedPropertyType.RectInt:
                    property.rectIntValue = default(RectInt);
                    break;

                case SerializedPropertyType.String:
                    property.stringValue = default(string);
                    break;

                case SerializedPropertyType.Vector2:
                    property.vector2Value = default(Vector2);
                    break;

                case SerializedPropertyType.Vector2Int:
                    property.vector2IntValue = default(Vector2Int);
                    break;

                case SerializedPropertyType.Vector3:
                    property.vector3Value = default(Vector3);
                    break;

                case SerializedPropertyType.Vector3Int:
                    property.vector3IntValue = default(Vector3Int);
                    break;

                case SerializedPropertyType.Vector4:
                    property.vector4Value = default(Vector4);
                    break;

                case SerializedPropertyType.Character:
                case SerializedPropertyType.Generic:
                case SerializedPropertyType.Gradient:
                case SerializedPropertyType.LayerMask:
                default:
                    break;
            }

            Debug.unityLogger.logEnabled = true;
            Debug.unityLogger.logHandler = logHandler;
        }

        public static void ClearConsole()
        {
            var logEntries = typeof(Editor).Assembly.GetType("UnityEditor.LogEntries");
            if (logEntries != null)
                logEntries.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public)?.Invoke(null, null);
            else
                Debug.LogWarning($"[{nameof(Helpers)}.{nameof(ClearConsole)}] Couldn't find LogEntries!");
        }

        // TODO: Change to extension methods and rename to "GetActualObject"

        #region GetActualObjectForSerializedProperty

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

        #endregion GetActualObjectForSerializedProperty

#endif

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
    }
}