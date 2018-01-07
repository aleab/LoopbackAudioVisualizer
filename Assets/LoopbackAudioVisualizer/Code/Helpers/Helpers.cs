using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
            // TODO: Clear Generic properties

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
                case SerializedPropertyType.FixedBufferSize:
                case SerializedPropertyType.Gradient:
                case SerializedPropertyType.LayerMask:
                default:
                    property.SetActualObjectToDefault();
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

        #region GetActualObject / SetActualObject

        public static T GetActualObject<T>(this SerializedProperty property) where T : class
        {
            object obj = property.GetActualObject();
            return obj as T;
        }

        public static object GetActualObject(this SerializedProperty property, Type objectType)
        {
            object obj = property.GetActualObject();
            return objectType.IsInstanceOfType(obj) ? obj : null;
        }

        private static object GetActualObject(this SerializedProperty property)
        {
            object obj = property.serializedObject.targetObject;

            string path = property.propertyPath.Replace(".Array.data[", "[");
            string[] pathElements = path.Split('.');
            foreach (var element in pathElements)
            {
                if (element.Contains("["))
                {
                    string arrayName = element.Substring(0, element.IndexOf("[", StringComparison.Ordinal));
                    var index = Convert.ToInt32(element.Substring(element.IndexOf("[", StringComparison.Ordinal)).Replace("[", "").Replace("]", ""));
                    obj = GetValue(obj, arrayName, index);
                }
                else
                    obj = GetValue(obj, element);
            }
            return obj;
        }

        public static bool SetActualObjectToDefault(this SerializedProperty property)
        {
            object source = property.serializedObject.targetObject;

            string path = property.propertyPath.Replace(".Array.data[", "[");
            string[] pathElements = path.Split('.');
            for (int i = 0; i < pathElements.Length - 1; ++i)
            {
                string element = pathElements[i];
                if (element.Contains("["))
                {
                    string arrayName = element.Substring(0, element.IndexOf("[", StringComparison.Ordinal));
                    var index = Convert.ToInt32(element.Substring(element.IndexOf("[", StringComparison.Ordinal)).Replace("[", "").Replace("]", ""));
                    source = GetValue(source, arrayName, index);
                }
                else
                    source = GetValue(source, element);
            }

            string name = pathElements.Last();
            if (name.Contains("["))
            {
                string arrayName = name.Substring(0, name.IndexOf("[", StringComparison.Ordinal));
                var index = Convert.ToInt32(name.Substring(name.IndexOf("[", StringComparison.Ordinal)).Replace("[", "").Replace("]", ""));
                return SetDefaultValue(source, arrayName, index);
            }
            return SetDefaultValue(source, name);
        }

        #endregion GetActualObject / SetActualObject

        public static void InsertNewArrayElement(this SerializedProperty property)
        {
            if (!property.isArray)
                return;

            object source = property.serializedObject.targetObject;

            string path = property.propertyPath.Replace(".Array.data[", "[");
            string[] pathElements = path.Split('.');
            for (int i = 0; i < pathElements.Length - 1; ++i)
            {
                string element = pathElements[i];
                if (element.Contains("["))
                {
                    string arrayName = element.Substring(0, element.IndexOf("[", StringComparison.Ordinal));
                    var index = Convert.ToInt32(element.Substring(element.IndexOf("[", StringComparison.Ordinal)).Replace("[", "").Replace("]", ""));
                    source = GetValue(source, arrayName, index);
                }
                else
                    source = GetValue(source, element);
            }

            string name = pathElements.Last();
            SetDefaultValue(source, name, property.arraySize);
        }

#endif

        #region GetValue / SetValue

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

        private static bool SetValue(object source, string name, object value)
        {
            if (source == null)
                return false;
            Type type = source.GetType();

            while (type != null)
            {
                FieldInfo field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(source, value);
                    return true;
                }

                PropertyInfo property = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (property != null)
                {
                    property.SetValue(source, value);
                    return true;
                }

                type = type.BaseType;
            }
            return false;
        }

        private static object GetValue(object source, string name, int index)
        {
            var arr = GetValue(source, name) as IList;
            return index < (arr?.Count ?? 0) ? arr?[index] : null;
        }

        private static bool SetValue(object source, string name, int index, object value)
        {
            var arr = GetValue(source, name);
            Type type = arr.GetType();
            if (type.IsArray)
            {
                Type elementType = type.GetElementType();
                var list = arr as IList;
                if (list != null)
                {
                    if (index < list.Count)
                    {
                        list[index] = value;
                        return true;
                    }

                    // Create a larger array if the new element's index is out of bounds
                    if (elementType != null)
                    {
                        var newArrList = new ArrayList(list) { value };
                        var newArr = newArrList.ToArray(elementType);
                        return SetValue(source, name, newArr);
                    }
                }
            }
            return false;
        }

        private static object GetDefault(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        private static bool SetDefaultValue(object source, string name)
        {
            object currentValue = GetValue(source, name);
            if (currentValue != null)
            {
                Type type = currentValue.GetType();
                object value = GetDefault(type);
                return SetValue(source, name, value);
            }
            return false;
        }

        private static bool SetDefaultValue(object source, string name, int index)
        {
            object arr = GetValue(source, name);
            if (arr != null)
            {
                Type type = arr.GetType();
                if (type.IsArray)
                {
                    Type arrayType = type.GetElementType();
                    object value = GetDefault(arrayType);
                    return SetValue(source, name, index, value);
                }
                return false;
            }
            return false;
        }

        #endregion GetValue / SetValue
    }
}