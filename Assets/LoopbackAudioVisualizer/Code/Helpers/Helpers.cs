using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

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
        public static void ClearValues(this SerializedProperty property)
        {
            var logHandler = Debug.unityLogger.logHandler;
            Debug.unityLogger.logHandler = null;
            Debug.unityLogger.logEnabled = false;

            switch (property.propertyType)
            {
                case SerializedPropertyType.AnimationCurve:
                    property.animationCurveValue = default(AnimationCurve);
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
                    property.exposedReferenceValue = default(Object);
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
                    property.objectReferenceValue = default(Object);
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

#endif
    }
}