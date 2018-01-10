#if UNITY_EDITOR

using Aleab.LoopbackAudioVisualizer.Scripts;
using Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Extensions;
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Unity.UnityEditor
{
    public class ScriptableObjectWindow : EditorWindow
    {
        private static Type[] types;
        private static GUIContent[] displayedOptions;

        private int selectedIndex;

        public static void Init()
        {
            Assembly assembly = typeof(NoBehaviour).Assembly;
            types = assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(ScriptableObject)) && !t.IsAbstract).ToArray();
            displayedOptions = types.Select(t => new GUIContent(t.FullName)).ToArray();

            ScriptableObjectWindow window = GetWindow<ScriptableObjectWindow>(true, "Create a new ScriptableObject", true);
            window.minSize = new Vector2(500.0f, window.GetHeight());
            window.maxSize = new Vector2(800.0f, window.minSize.y);
            window.ShowPopup();
        }

        public void OnGUI()
        {
            GUI.enabled = types.Length > 0 && displayedOptions.Length > 0;

            GUILayout.Space(6.0f);
            this.selectedIndex = EditorGUILayout.Popup(new GUIContent("ScriptableObject Class"), this.selectedIndex, displayedOptions);
            if (GUILayout.Button("Create") && this.selectedIndex >= 0)
            {
                var asset = CreateInstance(types[this.selectedIndex]);
                ProjectWindowUtil.StartNameEditingIfProjectWindowExists(asset.GetInstanceID(), CreateInstance<EndNameEdit>(), $"{displayedOptions[this.selectedIndex].text}.asset", AssetPreview.GetMiniThumbnail(asset), null);
                this.Close();
            }
            GUILayout.Space(6.0f);

            GUI.enabled = true;
        }

        public float GetHeight()
        {
            return 2 * EditorExtension.SingleLineHeight + 12.0f;
        }

        internal class EndNameEdit : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                AssetDatabase.CreateAsset(EditorUtility.InstanceIDToObject(instanceId), AssetDatabase.GenerateUniqueAssetPath(pathName));
            }
        }
    }
}

#endif