using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Scripts
{
    internal class ErrorController : MonoBehaviour
    {
        #region Singleton

        private static ErrorController _instance;

        public static ErrorController Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject($"[C] {nameof(ErrorController)}");
                    _instance = go.AddComponent<ErrorController>();

                    Debug.LogWarning("ErrorController object has been created automatically");
                }
                return _instance;
            }
        }

        #endregion Singleton

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
        }

        public void AddErrorMessage()
        {
        }
    }
}