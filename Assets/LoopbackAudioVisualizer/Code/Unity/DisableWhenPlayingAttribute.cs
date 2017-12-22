using UnityEditor;

namespace Aleab.LoopbackAudioVisualizer.Unity
{
    public class DisableWhenPlayingAttribute : ReadOnlyAttribute
    {
#if UNITY_EDITOR
        protected override bool Enabled { get { return !EditorApplication.isPlayingOrWillChangePlaymode; } }
#endif
    }
}