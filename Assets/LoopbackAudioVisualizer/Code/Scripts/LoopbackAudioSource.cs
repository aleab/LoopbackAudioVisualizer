using Aleab.LoopbackAudioVisualizer.Scripts;
using CSCore;
using CSCore.CoreAudioAPI;
using CSCore.SoundIn;
using CSCore.Streams;
using System;
using System.Collections;
using System.Linq;
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
        /// The sound source (IWaveSource) that provides raw bytes.
        /// </summary>
        private SoundInSource soundInSource;

        /// <summary>
        /// The sound source that provides audio samples.
        /// </summary>
        private SingleBlockNotificationStream sampleSource;

        /// <summary>
        /// A WaveSource converted from the current SampleSource
        /// </summary>
        private IWaveSource sampledWaveSource;

        #endregion Private fields

        #region Inspector fields

#pragma warning disable 0414

        [Space(10.0f)]
        [SerializeField]
        private string loopbackDeviceName;

        [Space(5.0f)]
        [SerializeField]
        private StereoBlock currentStereoBlock = StereoBlock.Zero;

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

        public StereoBlock CurrentStereoBlock
        {
            get { return this.currentStereoBlock.Copy(); }
        }

        #endregion Public properties

        private void Awake()
        {
            StartupController.Instance.StartupCompleted += this.StartupController_StartupCompleted;
        }

        private void Start()
        {
            UIController.SettingsPanel.LoopbackDeviceSelected += this.SettingsPanel_LoopbackDeviceSelected;
        }

        #region Init

        public void Init(MMDevice loopbackDevice)
        {
            Debug.Log($"Initializing {nameof(LoopbackAudioSource)} ({this.gameObject.name})...");

            this.LoopbackDevice = loopbackDevice;

            this.initialized = false;

            this.StopListening();
            this.ReleaseAudioSources();

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
                this.InitWasapiLoopbackCapture(loopbackDevice);
                this.SetupSoundInSource();
                this.SetupSampleSource();

                this.initialized = true;
                Debug.Log($"Initialized {nameof(LoopbackAudioSource)} ({this.gameObject.name}).");

                this.StartListening();
            }
        }

        private void InitWasapiLoopbackCapture(MMDevice loopbackDevice)
        {
            this.DisposeWasapiLoopbackCapture();
            this.wasapiLoopbackCapture = new WasapiLoopbackCapture { Device = loopbackDevice };
            this.wasapiLoopbackCapture.Stopped += this.WasapiLoopbackCapture_Stopped;
            this.wasapiLoopbackCapture.Initialize();
        }

        private void SetupSoundInSource()
        {
            this.DisposeSoundInSource();
            this.soundInSource = new SoundInSource(this.wasapiLoopbackCapture, (int)this.wasapiLoopbackCapture.WaveFormat.MillisecondsToBytes(Preferences.CaptureBufferMilliseconds));
            this.soundInSource.DataAvailable += this.SoundInSource_DataAvailable;
        }

        private void SetupSampleSource()
        {
            this.DisposeSampleSource();
            ISampleSource sampleSource = this.soundInSource.ToSampleSource();
            this.sampleSource = new SingleBlockNotificationStream(sampleSource);
            this.sampleSource.SingleBlockRead += this.SampleSource_SingleBlockRead;

            this.DisposeSampledWaveSource();
            this.sampledWaveSource = this.sampleSource.ToWaveSource(16);
        }

        #endregion Init

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

            Debug.Log($"{nameof(LoopbackAudioSource)} ({this.gameObject.name}) started listening on device \"{this.LoopbackDevice.DeviceID}\" ({this.LoopbackDevice.FriendlyName}).");
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

        private void WasapiLoopbackCapture_Stopped(object sender, RecordingStoppedEventArgs e)
        {
            // TODO: WasapiLoopbackCapture_Stopped
        }

        private void SoundInSource_DataAvailable(object sender, DataAvailableEventArgs e)
        {
            // We need to read from our SoundInSource, otherwise SingleBlockRead is never called
            byte[] buffer = new byte[this.sampledWaveSource.WaveFormat.BytesPerSecond / 2];
            while (this.sampledWaveSource.Read(buffer, 0, buffer.Length) > 0)
            {
            }
        }

        private void SampleSource_SingleBlockRead(object sender, SingleBlockReadEventArgs e)
        {
            if (e.Channels > 2 && e.Samples != null)
            {
                // TODO: Multi-channel (>2)
            }
            else
            {
                this.currentStereoBlock.left = e.Left;
                this.currentStereoBlock.right = e.Right;
            }
        }

        #endregion Event handlers

        #region Dispose

        public void Dispose()
        {
            this.StopListening();
            this.ReleaseAudioSources();
        }

        private void ReleaseAudioSources()
        {
            this.DisposeWasapiLoopbackCapture();
            this.DisposeSoundInSource();
            this.DisposeSampleSource();
            this.DisposeSampledWaveSource();
        }

        private void DisposeWasapiLoopbackCapture()
        {
            if (this.wasapiLoopbackCapture != null)
            {
                this.wasapiLoopbackCapture.Stop();
                this.wasapiLoopbackCapture.Stopped -= this.WasapiLoopbackCapture_Stopped;
                this.wasapiLoopbackCapture.Dispose();
                this.wasapiLoopbackCapture = null;
            }
        }

        private void DisposeSoundInSource()
        {
            if (this.soundInSource != null)
            {
                this.soundInSource.DataAvailable -= this.SoundInSource_DataAvailable;
                this.soundInSource.Dispose();
                this.soundInSource = null;
            }
        }

        private void DisposeSampleSource()
        {
            if (this.sampleSource != null)
            {
                this.sampleSource.SingleBlockRead -= this.SampleSource_SingleBlockRead;
                this.sampleSource.Dispose();
                this.sampleSource = null;
            }
        }

        private void DisposeSampledWaveSource()
        {
            if (this.sampledWaveSource != null)
            {
                this.sampledWaveSource.Dispose();
                this.sampledWaveSource = null;
            }
        }

        #endregion Dispose

        private void OnDestroy()
        {
            this.Dispose();
        }
    }
}