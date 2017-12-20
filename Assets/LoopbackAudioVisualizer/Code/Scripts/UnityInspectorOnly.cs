using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Scripts
{
    public class UnityInspectorOnly : MonoBehaviour
    {
        private void Awake()
        {
#if UNITY_EDITOR
            this.gameObject.SetActive(false);
#else
            Destroy(this.gameObject);
#endif
        }
    }
}