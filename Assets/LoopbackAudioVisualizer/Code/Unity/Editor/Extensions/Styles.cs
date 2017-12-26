#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Extensions
{
    internal static class Styles
    {
        public static GUIStyle FoldoutNoArrow { get; } = new GUIStyle(EditorStyles.label)
        {
            border = new RectOffset(EditorStyles.foldout.border.left, EditorStyles.foldout.border.right, EditorStyles.foldout.border.top, EditorStyles.foldout.border.bottom),
            margin = new RectOffset(EditorStyles.foldout.margin.left, EditorStyles.foldout.margin.right, EditorStyles.foldout.margin.top, EditorStyles.foldout.margin.bottom),
            padding = new RectOffset(EditorStyles.foldout.padding.left, EditorStyles.foldout.padding.right, EditorStyles.foldout.padding.top, EditorStyles.foldout.padding.bottom)
        };

        public static GUIStyle RightAlignedLabel { get; } = new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleRight
        };

        public static GUIStyle IndentedMiniLabel { get; } = new GUIStyle(EditorStyles.miniLabel)
        {
            padding = { left = 12 }
        };

        public static GUIStyle IndentedRightAlignedMiniLabel { get; } = new GUIStyle(IndentedMiniLabel)
        {
            alignment = TextAnchor.MiddleRight
        };

        public static GUIStyle ItalicsBoldLabel { get; } = new GUIStyle(EditorStyles.boldLabel)
        {
            fontStyle = FontStyle.BoldAndItalic
        };
    }
}

#endif