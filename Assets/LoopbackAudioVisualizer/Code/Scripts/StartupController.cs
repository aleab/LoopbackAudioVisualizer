using Aleab.LoopbackAudioVisualizer.Helpers;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Aleab.LoopbackAudioVisualizer.Scripts
{
    public class StartupController : MonoBehaviour
    {
        #region Singleton

        private static StartupController _instance;

        public static StartupController Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject($"[C] {nameof(StartupController)}");
                    _instance = go.AddComponent<StartupController>();

                    Debug.LogWarning("StartupController object has been created automatically");
                }
                return _instance;
            }
        }

        #endregion Singleton

        public event EventHandler StartupCompleted;

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

        private void Start()
        {
            if (SceneManager.GetActiveScene().buildIndex != 0)
            {
                this.LoadScenesInTheRightOrder();
                return;
            }

            Preferences.Load();
            SceneManager.sceneLoaded += this.SceneManager_SceneLoaded;

#if DEBUG
            SceneManager.LoadScene(1, LoadSceneMode.Additive);
#endif

            this.OnStartupCompleted();
        }

        private void LoadScenesInTheRightOrder()
        {
            _instance = null;
            GameObject go = new GameObject(@"\__TEMP__/ [LoadScenesInTheRightOrder] \__TEMP__/");
            MonoBehaviour behaviour = go.AddComponent<NoBehaviour>();
            DontDestroyOnLoad(go);
            behaviour.Invoke(LoadScenesInTheRightOrder, SceneManager.GetActiveScene(), go, 0.1f);

            Destroy(this.gameObject);
        }

        private static void LoadScenesInTheRightOrder(Scene currentScene, GameObject gameObject)
        {
            var operation = SceneManager.LoadSceneAsync(0, LoadSceneMode.Single);
            operation.allowSceneActivation = true;
            operation.completed += asyncOperation =>
            {
                Destroy(gameObject);
#if UNITY_EDITOR
                Helpers.Helpers.ClearConsole();
#endif
            };
        }

        private void OnDestroy()
        {
            _instance = null;
            SceneManager.sceneLoaded -= this.SceneManager_SceneLoaded;
        }

        private void SceneManager_SceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            SceneManager.SetActiveScene(scene);
        }

        private void OnStartupCompleted()
        {
            this.StartupCompleted?.Invoke(this, EventArgs.Empty);
        }
    }
}