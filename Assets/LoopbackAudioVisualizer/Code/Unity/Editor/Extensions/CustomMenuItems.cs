#if UNITY_EDITOR

using UnityEditor;

namespace Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Extensions
{
    [InitializeOnLoad]
    public static class CustomMenuItems
    {
        static CustomMenuItems()
        {
        }

        [MenuItem("Edit/Play-Pause from Scene 0 _F5", false, 10000)]
        public static void PlayFromPrelaunchScene()
        {
            if (EditorApplication.isPlaying)
                EditorApplication.isPlaying = false;
            else
            {
                Scenes.Common.Load();
                EditorApplication.isPlaying = true;
            }
        }
    }
}

#endif