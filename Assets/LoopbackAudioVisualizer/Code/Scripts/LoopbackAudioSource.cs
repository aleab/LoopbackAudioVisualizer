using Aleab.LoopbackAudioVisualizer.Events;
using Aleab.LoopbackAudioVisualizer.Settings;
using Aleab.LoopbackAudioVisualizer.Settings.UI;
using CSCore;
using CSCore.CoreAudioAPI;
using CSCore.SoundIn;
using CSCore.Streams;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Scripts
{
    public class LoopbackAudioSource : MonoBehaviour, IDisposable
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

        // ReSharper disable once NotAccessedField.Local
        [Space(10.0f)]
        [SerializeField]
        private string loopbackDeviceName;

        [Space(5.0f)]
        [SerializeField]
        private AudioEndpointVolumeLevels audioEndpointVolumeLevels = new AudioEndpointVolumeLevels();

        [SerializeField]
        private AudioBlock currentAudioBlock = AudioBlock.Zero;

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

        public AudioBlock CurrentAudioBlock { get { return this.currentAudioBlock.Copy(); } }

        #endregion Public properties

        public event EventHandler<MMDeviceChangedEventArgs> DeviceChanged;

        public event EventHandler<SingleBlockReadEventArgs> SingleBlockRead;

        private void Awake()
        {
            StartupController.Instance.StartupCompleted += this.StartupController_StartupCompleted;
        }

        private void Start()
        {
            UIController.SettingsMenu.FindSettingsPanel<CaptureSettingsPanel>().LoopbackDeviceSelected += this.SettingsPanel_LoopbackDeviceSelected;
            this.updateVolumeCoroutine = this.StartCoroutine(this.UpdateVolume());
        }

        public void Init(MMDevice loopbackDevice)
        {
            Debug.Log($"Initializing {nameof(LoopbackAudioSource)} ({this.gameObject.name})...");
            bool initialized = false;

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
                this.InitAudioEndpointVolume(loopbackDevice);

                this.InitWasapiLoopbackCapture(loopbackDevice);
                this.SetupSoundInSource();
                this.SetupSampleSource();

                Debug.Log($"Initialized {nameof(LoopbackAudioSource)} ({this.gameObject.name}).");
                initialized = true;
            }

            if (this.LoopbackDevice?.DeviceID != loopbackDevice?.DeviceID)
            {
                this.LoopbackDevice = loopbackDevice;
                this.DeviceChanged?.Invoke(this, new MMDeviceChangedEventArgs(loopbackDevice, initialized));
            }

            if (initialized)
                this.StartListening();
        }

        #region EnsureListening

        private MutableTuple<bool, bool> ensureListeningResult;

        /// <summary>
        /// Guarantees that the component is capturing audio samples.
        /// If it isn't, the component is restarted (<see cref="Init"/> will be called again).
        /// </summary>
        public void EnsureListening()
        {
            if (!this.IsListening)
                this.StartListening();

            this.StartCoroutine(this.EnsureListeningCoroutine(0.25f));
        }

        private IEnumerator EnsureListeningCoroutine(float timeoutSeconds)
        {
            this.ensureListeningResult = new MutableTuple<bool, bool>(false, false);
            Func<bool> isActuallyListening = () => this.ensureListeningResult.Item1 && this.ensureListeningResult.Item2;

            this.soundInSource.DataAvailable += this.EnsureListening_SoundInSource_DataAvailable;
            this.sampleSource.SingleBlockRead += this.EnsureListening_SampleSource_SingleBlockRead;

            while (timeoutSeconds >= 0.0f && !isActuallyListening())
            {
                yield return new WaitForSeconds(0.05f);
                timeoutSeconds -= 0.05f;
            }

            this.soundInSource.DataAvailable -= this.EnsureListening_SoundInSource_DataAvailable;
            this.sampleSource.SingleBlockRead -= this.EnsureListening_SampleSource_SingleBlockRead;

            if (!isActuallyListening())
            {
                Debug.LogWarning($"{nameof(LoopbackAudioSource)} ({this.gameObject.name}) is not listening! Restarting...");
                this.Init(this.LoopbackDevice);
            }

            this.ensureListeningResult = null;
        }

        private void EnsureListening_SoundInSource_DataAvailable(object sender, DataAvailableEventArgs e) => this.ensureListeningResult.Item1 = true;

        private void EnsureListening_SampleSource_SingleBlockRead(object sender, SingleBlockReadEventArgs e) => this.ensureListeningResult.Item2 = true;

        #endregion EnsureListening

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

        #region Internal

        #region Init

        private void InitAudioEndpointVolume(MMDevice loopbackDevice)
        {
            this.DisposeAudioEndpointVolume();
            this.audioEndpointVolume = AudioEndpointVolume.FromDevice(loopbackDevice);
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
            int bufferSize = (int)this.wasapiLoopbackCapture.WaveFormat.MillisecondsToBytes(Preferences.Instance.GameSettings.CaptureSettings.BufferSizeMilliseconds);
            this.soundInSource = new SoundInSource(this.wasapiLoopbackCapture, bufferSize);
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

        #region Event handlers

        private void StartupController_StartupCompleted(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Preferences.Instance.GameSettings.CaptureSettings.LoopbackDeviceID))
                ErrorController.Instance.AddErrorMessage();
            else
            {
                MMDeviceCollection deviceCollection = MMDeviceEnumerator.EnumerateDevices(DataFlow.Render, DeviceState.Active);
                MMDevice device = deviceCollection.FirstOrDefault(mmDevice => mmDevice.DeviceID == Preferences.Instance.GameSettings.CaptureSettings.LoopbackDeviceID);

                this.Init(device);
            }
        }

        private void SettingsPanel_LoopbackDeviceSelected(object sender, MMDeviceEventArgs e)
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
            this.currentAudioBlock.left = e.Left;
            this.currentAudioBlock.right = e.Right;
            this.currentAudioBlock.samples = e.Channels > 2 && e.Samples != null ? e.Samples : new[] { e.Left, e.Right };

            this.SingleBlockRead?.Invoke(this, e);
        }

        #endregion Event handlers

        #region Dispose

        public void Dispose()
        {
            this.StopListening();
            this.ReleaseAudioSources();
            this.DisposeAudioEndpointVolume();
        }

        private void DisposeAudioEndpointVolume()
        {
            if (this.audioEndpointVolume != null && !this.audioEndpointVolume.IsDisposed)
                this.audioEndpointVolume.Dispose();
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

        #endregion Internal
    }
}