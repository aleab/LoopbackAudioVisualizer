using Aleab.LoopbackAudioVisualizer.Scripts;
using CSCore;
using CSCore.CoreAudioAPI;
using CSCore.SoundIn;
using CSCore.Streams;
using System;
using System.Collections;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer
{
    /*
     * - convert audio data to format required by FFT (e.g. int -> float, separate L/R channels)
     * - apply suitable window function (e.g. Hann aka Hanning window)
     * - apply FFT (NB: if using typical complex-to-complex FFT then set imaginary parts of input array to zero)
     * - calculate magnitude of first N/2 FFT output bins (sqrt(re*re + im*im))
     * - optionally convert magnitude to dB (log) scale (20 * log10(magnitude))
     * - plot N/2 (log) magnitude values
     */

    public partial class LoopbackAudioSource : MonoBehaviour, IDisposable
    {
        #region Private fields

        private MMDevice _loopbackDevice;

        private AudioEndpointVolume audioEndpointVolume;

        private Coroutine updateVolumeCoroutine;

        #region Capture

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

        #endregion Capture

        #endregion Private fields

        #region Inspector fields

#pragma warning disable 0414

        [Space(10.0f)]
        [SerializeField]
        private string loopbackDeviceName;

        [Space(5.0f)]
        [SerializeField]
        private AudioEndpointVolumeLevels audioEndpointVolumeLevels = new AudioEndpointVolumeLevels();

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

        public bool IsListening { get { return this.wasapiLoopbackCapture?.RecordingState == RecordingState.Recording; } }

        public AudioEndpointVolumeLevels VolumeLevels { get { return this.audioEndpointVolumeLevels.Copy(); } }

        public StereoBlock CurrentStereoBlock { get { return this.currentStereoBlock.Copy(); } }

        #endregion Public properties

        private void Awake()
        {
            StartupController.Instance.StartupCompleted += this.StartupController_StartupCompleted;
        }

        private void Start()
        {
            UIController.SettingsPanel.LoopbackDeviceSelected += this.SettingsPanel_LoopbackDeviceSelected;
            this.updateVolumeCoroutine = this.StartCoroutine(this.UpdateVolume());
        }

        public void Init(MMDevice loopbackDevice)
        {
            Debug.Log($"Initializing {nameof(LoopbackAudioSource)} ({this.gameObject.name})...");

            this.LoopbackDevice = loopbackDevice;
            this.InitAudioEndpointVolume(loopbackDevice);

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

                Debug.Log($"Initialized {nameof(LoopbackAudioSource)} ({this.gameObject.name}).");

                this.StartListening();
            }
        }

        public void StartListening()
        {
            if (this.wasapiLoopbackCapture != null && this.wasapiLoopbackCapture.RecordingState != RecordingState.Recording)
            {
                this.wasapiLoopbackCapture.Start();
                Debug.Log($"{nameof(LoopbackAudioSource)} ({this.gameObject.name}) started listening on device \"{this.LoopbackDevice.DeviceID}\" ({this.LoopbackDevice.FriendlyName}).");
            }
            else
                Debug.LogWarning($"{nameof(LoopbackAudioSource)} ({this.gameObject.name}) already listening!");
        }

        public void StopListening()
        {
            if (this.wasapiLoopbackCapture != null && this.wasapiLoopbackCapture.RecordingState == RecordingState.Stopped)
            {
                this.wasapiLoopbackCapture.Stop();
                Debug.Log($"{nameof(LoopbackAudioSource)} ({this.gameObject.name}) stopped listening.");
            }
        }

        private IEnumerator UpdateVolume()
        {
            yield return null;
            while (this.updateVolumeCoroutine != null)
            {
                if (this.audioEndpointVolume != null)
                    this.audioEndpointVolumeLevels?.Update(this.audioEndpointVolume);
                yield return new WaitForSeconds(0.5f);
            }
        }

        private void OnDestroy()
        {
            this.Dispose();
        }
    }
}