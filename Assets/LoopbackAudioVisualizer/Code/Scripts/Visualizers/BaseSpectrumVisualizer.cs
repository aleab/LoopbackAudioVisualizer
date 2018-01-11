using Aleab.LoopbackAudioVisualizer.Events;
using Aleab.LoopbackAudioVisualizer.Helpers;
using Aleab.LoopbackAudioVisualizer.Unity;
using CSCore;
using CSCore.DSP;
using CSCore.Streams;
using System;
using System.Collections;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Scripts.Visualizers
{
    /// <summary>
    /// Base class of any audio visualizer that needs to use the spectrum data.<br/>
    ///
    /// This class can provide, at any moment after a device has been initialized, the samples of the FFT of the audio signal that is being played;
    /// the total number of samples is equal to half of the FFT size.<br/>
    ///
    /// The basic usage is to start polling the <see cref="fftDataBuffer"/> in the <see cref="OnUpdateFftDataCoroutineStarted"/> method to get the up-to-date spectrum data.
    /// </summary>
    public abstract class BaseSpectrumVisualizer : MonoBehaviour
    {
        public const float UPDATE_FFT_INTERVAL = 0.05f;

        #region Inspector

        [SerializeField]
        [DisableWhenPlaying]
        protected FftSize fftSize = FftSize.Fft512;

        [SerializeField]
        protected float[] rawFftDataBuffer;

        [SerializeField]
        protected float[] fftDataBuffer;

        #endregion Inspector

        protected SimpleSpectrumProvider spectrumProvider;

        private Coroutine updateFftDataCoroutine;

        public FftSize FftSize { get { return this.fftSize; } }

        public SimpleSpectrumProvider SpectrumProvider { get { return this.spectrumProvider ?? new SimpleSpectrumProvider(2, 48000, this.fftSize); } }

        #region Events

        public event EventHandler UpdateFftDataCoroutineStarted;

        public event EventHandler UpdateFftDataCoroutineStopped;

        public event EventHandler FftDataBufferUpdated;

        #endregion Events

        protected virtual void Start()
        {
            if (AudioSourceController.LoopbackAudioSource != null)
            {
                this.SubscribeToLoopbackAudioSourceEvents();
                if (AudioSourceController.LoopbackAudioSource.IsListening)
                {
                    AudioSourceController.LoopbackAudioSource.EnsureListening();
                    this.CreateSpectrumProvider(AudioSourceController.LoopbackAudioSource.LoopbackDevice.DeviceFormat);
                }
            }
            else
                Debug.LogError($"{nameof(AudioSourceController)}.{nameof(AudioSourceController.LoopbackAudioSource)} is null!");
        }

        private IEnumerator UpdateFftData()
        {
            this.rawFftDataBuffer = new float[(int)this.fftSize];
            this.fftDataBuffer = new float[(int)this.fftSize / 2];
            yield return null;

            this.OnUpdateFftDataCoroutineStarted();
            while (this.updateFftDataCoroutine != null)
            {
                if (this.spectrumProvider.IsNewDataAvailable)
                {
                    // Apply FFT with size N
                    this.spectrumProvider.GetFftData(this.rawFftDataBuffer, this);

                    // Take the first N/2 values
                    for (int i = 0; i < this.fftDataBuffer.Length; ++i)
                        this.fftDataBuffer[i] = this.ProcessRawFftValue(this.rawFftDataBuffer[i], i);
                    this.OnFftDataBufferUpdated();
                }

                yield return new WaitForSeconds(UPDATE_FFT_INTERVAL);
            }

            this.OnUpdateFftDataCoroutineStopped();
        }

        protected abstract float ProcessRawFftValue(float rawFftValue, int fftBandIndex);

        protected virtual void OnDisable()
        {
            this.UnsubscribeFromLoopbackAudioSourceEvents();
        }

        protected virtual void OnEnable()
        {
            this.SubscribeToLoopbackAudioSourceEvents();
        }

        protected virtual void OnDestroy()
        {
            this.UnsubscribeFromLoopbackAudioSourceEvents();
        }

        private void CreateSpectrumProvider(WaveFormat deviceFormat)
        {
            AudioSourceController.LoopbackAudioSource.SingleBlockRead -= this.LoopbackAudioSource_SingleBlockRead;
            this.spectrumProvider = new SimpleSpectrumProvider(deviceFormat.Channels, deviceFormat.SampleRate, this.fftSize);

            AudioSourceController.LoopbackAudioSource.SingleBlockRead += this.LoopbackAudioSource_SingleBlockRead;
            this.Invoke(() =>
            {
                this.updateFftDataCoroutine = this.StartCoroutine(this.UpdateFftData());
            }, UPDATE_FFT_INTERVAL * 1.2f);
        }

        private void SubscribeToLoopbackAudioSourceEvents()
        {
            if (AudioSourceController.LoopbackAudioSource != null)
            {
                this.UnsubscribeFromLoopbackAudioSourceEvents();
                AudioSourceController.LoopbackAudioSource.DeviceChanged += this.LoopbackAudioSource_DeviceChanged;
                AudioSourceController.LoopbackAudioSource.SingleBlockRead += this.LoopbackAudioSource_SingleBlockRead;
            }
        }

        private void UnsubscribeFromLoopbackAudioSourceEvents()
        {
            if (AudioSourceController.LoopbackAudioSource != null)
            {
                AudioSourceController.LoopbackAudioSource.DeviceChanged -= this.LoopbackAudioSource_DeviceChanged;
                AudioSourceController.LoopbackAudioSource.SingleBlockRead -= this.LoopbackAudioSource_SingleBlockRead;
            }
        }

        #region Event handlers

        protected virtual void LoopbackAudioSource_DeviceChanged(object sender, MMDeviceChangedEventArgs e)
        {
            // If the device changes, we need to stop gathering FFT data and re-create the spectrum provider using the new device's format.
            AudioSourceController.LoopbackAudioSource.SingleBlockRead -= this.LoopbackAudioSource_SingleBlockRead;
            this.updateFftDataCoroutine = null;
            if (e.Initialized)
                this.CreateSpectrumProvider(e.Device.DeviceFormat);
        }

        protected virtual void LoopbackAudioSource_SingleBlockRead(object sender, SingleBlockReadEventArgs e)
        {
            this.spectrumProvider.Add(e.Left, e.Right);
        }

        protected virtual void OnUpdateFftDataCoroutineStarted()
        {
            this.UpdateFftDataCoroutineStarted?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnUpdateFftDataCoroutineStopped()
        {
            Array.Clear(this.rawFftDataBuffer, 0, this.rawFftDataBuffer.Length);
            Array.Clear(this.fftDataBuffer, 0, this.fftDataBuffer.Length);
            this.UpdateFftDataCoroutineStopped?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnFftDataBufferUpdated()
        {
            this.FftDataBufferUpdated?.Invoke(this, EventArgs.Empty);
        }

        #endregion Event handlers
    }
}