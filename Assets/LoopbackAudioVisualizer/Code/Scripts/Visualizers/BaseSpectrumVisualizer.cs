using Aleab.LoopbackAudioVisualizer.Events;
using Aleab.LoopbackAudioVisualizer.Helpers;
using Aleab.LoopbackAudioVisualizer.Unity;
using CSCore.DSP;
using CSCore.Streams;
using System;
using System.Collections;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Scripts.Visualizers
{
    public class BaseSpectrumVisualizer : MonoBehaviour
    {
        protected const float UPDATE_FFT_INTERVAL = 0.05f;

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

        protected virtual void Start()
        {
            if (AudioSourceController.LoopbackAudioSource != null)
            {
                AudioSourceController.LoopbackAudioSource.DeviceChanged -= this.LoopbackAudioSource_DeviceChanged;
                AudioSourceController.LoopbackAudioSource.DeviceChanged += this.LoopbackAudioSource_DeviceChanged;
            }
            else
                Debug.LogError($"{nameof(AudioSourceController)}.{nameof(AudioSourceController.LoopbackAudioSource)} is null!");
        }

        private IEnumerator UpdateFftData()
        {
            this.rawFftDataBuffer = new float[(int)this.fftSize];
            this.fftDataBuffer = new float[(int)this.fftSize / 2];
            yield return null;

            while (this.updateFftDataCoroutine != null)
            {
                if (this.spectrumProvider.IsNewDataAvailable)
                {
                    // Apply FFT with size N
                    this.spectrumProvider.GetFftData(this.rawFftDataBuffer, this);

                    // Take the first N/2 values
                    for (int i = 0; i < this.fftDataBuffer.Length; ++i)
                        this.fftDataBuffer[i] = this.rawFftDataBuffer[i];
                }

                yield return new WaitForSeconds(UPDATE_FFT_INTERVAL);
            }

            Array.Clear(this.rawFftDataBuffer, 0, this.rawFftDataBuffer.Length);
            Array.Clear(this.fftDataBuffer, 0, this.fftDataBuffer.Length);
        }

        protected virtual void OnDisable()
        {
            if (AudioSourceController.LoopbackAudioSource != null)
            {
                AudioSourceController.LoopbackAudioSource.SingleBlockRead -= this.LoopbackAudioSource_SingleBlockRead;
                AudioSourceController.LoopbackAudioSource.DeviceChanged -= this.LoopbackAudioSource_DeviceChanged;
            }
        }

        protected virtual void OnEnable()
        {
            if (AudioSourceController.LoopbackAudioSource != null)
            {
                AudioSourceController.LoopbackAudioSource.SingleBlockRead -= this.LoopbackAudioSource_SingleBlockRead;
                AudioSourceController.LoopbackAudioSource.SingleBlockRead += this.LoopbackAudioSource_SingleBlockRead;

                AudioSourceController.LoopbackAudioSource.DeviceChanged -= this.LoopbackAudioSource_DeviceChanged;
                AudioSourceController.LoopbackAudioSource.DeviceChanged += this.LoopbackAudioSource_DeviceChanged;
            }
        }

        protected virtual void OnDestroy()
        {
            if (AudioSourceController.LoopbackAudioSource != null)
            {
                AudioSourceController.LoopbackAudioSource.SingleBlockRead -= this.LoopbackAudioSource_SingleBlockRead;
                AudioSourceController.LoopbackAudioSource.DeviceChanged -= this.LoopbackAudioSource_DeviceChanged;
            }
        }

        protected virtual void LoopbackAudioSource_DeviceChanged(object sender, MMDeviceChangedEventArgs e)
        {
            // If the device changes, we need to stop gathering FFT data and re-create the spectrum provider using the new device's format.
            AudioSourceController.LoopbackAudioSource.SingleBlockRead -= this.LoopbackAudioSource_SingleBlockRead;
            this.updateFftDataCoroutine = null;

            if (e.Initialized)
            {
                var deviceFormat = e.Device.DeviceFormat;
                this.spectrumProvider = new SimpleSpectrumProvider(deviceFormat.Channels, deviceFormat.SampleRate, this.fftSize);

                AudioSourceController.LoopbackAudioSource.SingleBlockRead += this.LoopbackAudioSource_SingleBlockRead;
                this.Invoke(() =>
                {
                    this.updateFftDataCoroutine = this.StartCoroutine(this.UpdateFftData());
                }, UPDATE_FFT_INTERVAL * 1.2f);
            }
        }

        protected virtual void LoopbackAudioSource_SingleBlockRead(object sender, SingleBlockReadEventArgs e)
        {
            this.spectrumProvider.Add(e.Left, e.Right);
        }
    }
}