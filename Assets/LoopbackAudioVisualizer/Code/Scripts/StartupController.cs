﻿using System;
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
            SceneManager.sceneLoaded += this.SceneManager_SceneLoaded;

            Preferences.Load();

#if DEBUG && UNITY_EDITOR
            Scenes.AudioVisualizer01.Load(LoadSceneMode.Additive);
#endif

            this.OnStartupCompleted();
        }

        private void OnDestroy()
        {
            _instance = null;
            SceneManager.sceneLoaded -= this.SceneManager_SceneLoaded;
        }

        private void SceneManager_SceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
        }

        private void OnStartupCompleted()
        {
            this.StartupCompleted?.Invoke(this, EventArgs.Empty);
        }
    }
}