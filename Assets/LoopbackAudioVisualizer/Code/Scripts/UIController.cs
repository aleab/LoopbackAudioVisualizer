using Aleab.LoopbackAudioVisualizer.Helpers;
using Aleab.LoopbackAudioVisualizer.Settings.UI;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Scripts
{
    public class UIController : MonoBehaviour
    {
        #region Singleton

        private static UIController _instance;

        public static UIController Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject($"[C] {nameof(UIController)}");
                    _instance = go.AddComponent<UIController>();

                    Debug.LogWarning($"{nameof(UIController)} object has been created automatically");
                }
                return _instance;
            }
        }

        #endregion Singleton

        #region Inspector

#pragma warning disable 0649

        [Space(10.0f)]
        [SerializeField]
        private Canvas canvas;

        [SerializeField]
        private SettingsMenu settingsMenu;

#pragma warning restore 0649

        #endregion Inspector

        public static Canvas Canvas
        {
            get { return _instance?.canvas; }
        }

        public static SettingsMenu SettingsMenu
        {
            get { return _instance?.settingsMenu; }
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

            this.RequireField(nameof(this.canvas), this.canvas);
            this.RequireField(nameof(this.settingsMenu), this.settingsMenu);
        }
    }
}