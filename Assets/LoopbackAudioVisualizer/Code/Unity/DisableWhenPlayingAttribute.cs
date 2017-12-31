#if UNITY_EDITOR

using UnityEditor;

#endif

namespace Aleab.LoopbackAudioVisualizer.Unity
{
    public class DisableWhenPlayingAttribute : ReadOnlyAttribute
    {
#if UNITY_EDITOR
        protected override bool Enabled { get { return !EditorApplication.isPlayingOrWillChangePlaymode; } }
#endif
    }
}