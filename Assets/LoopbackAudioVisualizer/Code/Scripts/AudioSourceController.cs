using Aleab.LoopbackAudioVisualizer.Helpers;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Scripts
{
    public class AudioSourceController : MonoBehaviour
    {
        #region Singleton

        private static AudioSourceController _instance;

        public static AudioSourceController Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject($"[C] {nameof(AudioSourceController)}");
                    _instance = go.AddComponent<AudioSourceController>();

                    Debug.LogWarning($"{nameof(AudioSourceController)} object has been created automatically");
                }
                return _instance;
            }
        }

        #endregion Singleton

        #region Inspector

#pragma warning disable 0649

        [Space(10.0f)]
        [SerializeField]
        private LoopbackAudioSource loopbackAudioSource;

#pragma warning restore 0649

        #endregion Inspector

        public static LoopbackAudioSource LoopbackAudioSource
        {
            get { return _instance?.loopbackAudioSource; }
        }

        private void Awake()
        {
            if (_instance == null)
                _instance = this;
            else if (_instance != this)
            {
                Destroy(this.gameObject);
                return;
            }

            DontDestroyOnLoad(this.gameObject);

            this.RequireField(nameof(this.loopbackAudioSource), this.loopbackAudioSource);
        }
    }
}