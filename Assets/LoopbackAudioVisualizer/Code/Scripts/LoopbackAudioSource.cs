using Aleab.LoopbackAudioVisualizer.Scripts;
using Aleab.LoopbackAudioVisualizer.Unity;
using CSCore.CoreAudioAPI;
using CSCore.SoundIn;
using System;
using System.Collections;
using System.Linq;
using CSCore.Streams;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer
{
    public class LoopbackAudioSource : MonoBehaviour, IDisposable
    {
        #region Private fields

        private MMDevice _loopbackDevice;

        private bool initialized;

        private Coroutine listeningCoroutine;

        /// <summary>
        /// The WASAPI loopback capture.
        /// </summary>
        private WasapiLoopbackCapture wasapiLoopbackCapture;

        /// <summary>
        /// The sound source.
        /// </summary>
        private SoundInSource soundIn;

        #endregion Private fields

        #region Inspector fields

#pragma warning disable 0414

        [Space(10.0f)]
        [ReadOnly]
        [SerializeField]
        private string loopbackDeviceName;

#pragma warning restore 0414

        #endregion Inspector fields

        #region Public properties

        /// <summary>
        /// The rendering device (speakers, headset, etc.) to listen
        /// </summary>
        public MMDevice LoopbackDevice
        {
            get { return this._loopbackDevice; }
            private set
            {
                this._loopbackDevice = value;
                this.loopbackDeviceName = this._loopbackDevice?.FriendlyName;
            }
        }

        public bool IsListening { get { return this.listeningCoroutine != null; } }

        #endregion Public properties

        private void Awake()
        {
            StartupController.Instance.StartupCompleted += this.StartupController_StartupCompleted;
        }

        private void Start()
        {
            UIController.SettingsPanel.LoopbackDeviceSelected += this.SettingsPanel_LoopbackDeviceSelected;
        }

        public void Init(MMDevice loopbackDevice)
        {
            Debug.Log($"Initializing {nameof(LoopbackAudioSource)} ({this.gameObject.name})...");

            this.LoopbackDevice = loopbackDevice;

            bool shouldRestartListening = this.IsListening;
            this.initialized = false;

            // Release the current wasapiLoopbackCapture
            if (this.wasapiLoopbackCapture != null)
            {
                this.wasapiLoopbackCapture.Stop();
                this.wasapiLoopbackCapture.Stopped -= this.WasapiLoopbackCapture_Stopped;
                this.wasapiLoopbackCapture.Dispose();
                this.wasapiLoopbackCapture = null;
            }

            // Release the current soundIn
            if (this.soundIn != null)
            {
                this.soundIn.DataAvailable -= this.SoundIn_DataAvailable;
                this.soundIn.Dispose();
                this.soundIn = null;
            }

            // Stop the current listening coroutine
            this.StopListening();

            if (loopbackDevice == null)
            {
                ErrorController.Instance.AddErrorMessage();
                Debug.LogWarning($"[{nameof(LoopbackAudioSource)}] ({this.gameObject.name}): No loopback device selected!");
            }
            else if (loopbackDevice.DataFlow != DataFlow.Render)
            {
                ErrorController.Instance.AddErrorMessage();
                Debug.LogError($"[{nameof(LoopbackAudioSource)}] ({this.gameObject.name}): The selected device ({loopbackDevice.FriendlyName}) is not a render device!");
            }
            else if (loopbackDevice.DeviceState != DeviceState.Active)
            {
                ErrorController.Instance.AddErrorMessage();
                Debug.LogWarning($"[{nameof(LoopbackAudioSource)}] ({this.gameObject.name}): The selected device ({loopbackDevice.FriendlyName}) is not enabled!");
            }
            else
            {
                this.wasapiLoopbackCapture = new WasapiLoopbackCapture { Device = loopbackDevice };
                this.wasapiLoopbackCapture.Stopped += this.WasapiLoopbackCapture_Stopped;
                this.wasapiLoopbackCapture.Initialize();

                this.soundIn = new SoundInSource(this.wasapiLoopbackCapture, (int)this.wasapiLoopbackCapture.WaveFormat.MillisecondsToBytes(Preferences.CaptureBufferMilliseconds));
                this.soundIn.DataAvailable += this.SoundIn_DataAvailable;

                this.initialized = true;
                Debug.Log($"Initialized {nameof(LoopbackAudioSource)} ({this.gameObject.name}).");

                if (shouldRestartListening)
                    this.StartListening();
            }
        }

        public void StartListening()
        {
            if (this.listeningCoroutine == null)
                this.listeningCoroutine = this.StartCoroutine(this.ListeningCoroutine());
            else
                Debug.LogWarning($"[{nameof(LoopbackAudioSource)}] Listening coroutine already started ({this.gameObject.name})!");
        }

        private IEnumerator ListeningCoroutine()
        {
            // Delay until the capture has been initialized
            while (!this.initialized)
                yield return new WaitForSeconds(0.1f);

            this.wasapiLoopbackCapture.Start();

            Debug.Log($"{nameof(LoopbackAudioSource)} ({this.gameObject.name}) started listening on device {this.LoopbackDevice.DeviceID} ({this.LoopbackDevice.FriendlyName}).");
        }

        public void StopListening()
        {
            if (this.listeningCoroutine != null)
            {
                this.wasapiLoopbackCapture?.Stop();
                this.StopCoroutine(this.listeningCoroutine);
                this.listeningCoroutine = null;

                Debug.Log($"{nameof(LoopbackAudioSource)} ({this.gameObject.name}) stopped listening.");
            }
        }

        #region Event handlers

        private void StartupController_StartupCompleted(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Preferences.LoopbackDeviceID))
                ErrorController.Instance.AddErrorMessage();
            else
            {
                MMDeviceCollection deviceCollection = MMDeviceEnumerator.EnumerateDevices(DataFlow.Render, DeviceState.Active);
                MMDevice device = deviceCollection.FirstOrDefault(mmDevice => mmDevice.DeviceID == Preferences.LoopbackDeviceID);

                this.Init(device);
            }
        }

        private void SettingsPanel_LoopbackDeviceSelected(object sender, Events.MMDeviceSelectedEventArgs e)
        {
            this.Init(e.Device);
        }

        private void SoundIn_DataAvailable(object sender, DataAvailableEventArgs e)
        {
            // TODO
        }

        private void WasapiLoopbackCapture_Stopped(object sender, RecordingStoppedEventArgs e)
        {
            // TODO
        }

        #endregion Event handlers

        public void Dispose()
        {
            this.wasapiLoopbackCapture?.Dispose();
        }

        public void OnDestroy()
        {
            this.Dispose();
        }
    }
}