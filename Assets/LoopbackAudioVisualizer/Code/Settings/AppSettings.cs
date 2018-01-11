using Aleab.LoopbackAudioVisualizer.Helpers;
using Aleab.LoopbackAudioVisualizer.Unity;
using System;
using System.IO;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Settings
{
    public sealed class AppSettings : MonoBehaviour
    {
        #region Singleton

        private static AppSettings _instance;

        public static AppSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject($"[C] {nameof(AppSettings)}");
                    _instance = go.AddComponent<AppSettings>();

                    Debug.LogWarning("Settings object has been created automatically");
                }
                return _instance;
            }
        }

        #endregion Singleton

        #region Inspector

#pragma warning disable 0414, 0649

        [SerializeField]
        private GameSettings gameSettings;

        [Header("Default Settings")]
        [SerializeField]
        [DisableWhenPlaying]
        private GameSettings defaultGameSettings;

        [SerializeField]
        [DisableWhenPlaying]
        private CaptureSettings defaultCaptureSettings;

#pragma warning restore 0414, 0649

        #endregion Inspector

        public GameSettings GameSettings
        {
            get
            {
                if (this.gameSettings == null)
                    this.LoadSettings();
                return this.gameSettings;
            }
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

            this.RequireField(nameof(this.defaultGameSettings), this.defaultGameSettings);
            this.RequireField(nameof(this.defaultCaptureSettings), this.defaultCaptureSettings);

            this.LoadSettings();
        }

        private void LoadSettings()
        {
            // Load settings, either the saved ones or the default ones.
            this.LoadSettingsOrDefault();

            // Save the current settings, so that every type of settings get serialized to a file.
            this.gameSettings.Save(true);

            // Replace current settings with the non-preset ones.
            this.LoadSavedSettings();
        }

        private void LoadSettingsOrDefault()
        {
            this.gameSettings = BaseSettings.GetSavedSettings(this.defaultGameSettings);

            if (this.gameSettings.CaptureSettings == null)
                this.gameSettings.CaptureSettings = BaseSettings.GetSavedSettings(this.defaultCaptureSettings);
        }

        private void LoadSavedSettings()
        {
            if (this.gameSettings?.IsPreset ?? true)
                this.gameSettings = BaseSettings.GetSavedSettings<GameSettings>();

            if (this.gameSettings.CaptureSettings?.IsPreset ?? true)
                this.gameSettings.CaptureSettings = BaseSettings.GetSavedSettings<CaptureSettings>();
        }

        #region Static

        public static string AppData
        {
            get
            {
                return Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer
                    ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Application.companyName, Application.productName)
                    : Application.persistentDataPath;
            }
        }

        public static string LocalAppData
        {
            get
            {
                return Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer
                    ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Application.companyName, Application.productName)
                    : Application.persistentDataPath;
            }
        }

        #endregion Static
    }
}