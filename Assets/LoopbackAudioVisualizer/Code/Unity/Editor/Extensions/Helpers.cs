#if UNITY_EDITOR

using System;
using System.Reflection;
using UnityEditor;

namespace Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Extensions
{
    public static class Helpers
    {
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