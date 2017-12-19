using CSCore.DSP;
using CSCore.Streams;
using System.Collections;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Scripts.Visualizers
{
    public class BaseSpectrumVisualizer : MonoBehaviour
    {
        [SerializeField]
        private FftSize fftSize = FftSize.Fft512;

        [SerializeField]
        private float[] fftBuffer;

        protected SimpleSpectrumProvider spectrumProvider;

        private Coroutine updateFftDataCoroutine;

        protected virtual void Start()
        {
            if (AudioSourceController.LoopbackAudioSource != null)
            {
                AudioSourceController.LoopbackAudioSource.DeviceInitialized -= this.LoopbackAudioSource_DeviceInitialized;
                AudioSourceController.LoopbackAudioSource.DeviceInitialized += this.LoopbackAudioSource_DeviceInitialized;
            }
            else
                Debug.LogError($"{nameof(AudioSourceController)}.{nameof(AudioSourceController.LoopbackAudioSource)} is null!");
        }

        private IEnumerator UpdateFftData()
        {
            this.fftBuffer = new float[(int)this.fftSize];
            yield return null;

            while (this.updateFftDataCoroutine != null)
            {
                if (this.spectrumProvider.IsNewDataAvailable)
                    this.spectrumProvider.GetFftData(this.fftBuffer, this);

                yield return new WaitForSeconds(0.05f);
            }
        }

        private void OnDisable()
        {
            if (AudioSourceController.LoopbackAudioSource != null)
            {
                AudioSourceController.LoopbackAudioSource.SingleBlockRead -= this.LoopbackAudioSource_SingleBlockRead;
                AudioSourceController.LoopbackAudioSource.DeviceInitialized -= this.LoopbackAudioSource_DeviceInitialized;
            }
        }

        private void OnEnable()
        {
            if (AudioSourceController.LoopbackAudioSource != null)
            {
                AudioSourceController.LoopbackAudioSource.SingleBlockRead -= this.LoopbackAudioSource_SingleBlockRead;
                AudioSourceController.LoopbackAudioSource.SingleBlockRead += this.LoopbackAudioSource_SingleBlockRead;

                AudioSourceController.LoopbackAudioSource.DeviceInitialized -= this.LoopbackAudioSource_DeviceInitialized;
                AudioSourceController.LoopbackAudioSource.DeviceInitialized += this.LoopbackAudioSource_DeviceInitialized;
            }
        }

        private void OnDestroy()
        {
            if (AudioSourceController.LoopbackAudioSource != null)
            {
                AudioSourceController.LoopbackAudioSource.SingleBlockRead -= this.LoopbackAudioSource_SingleBlockRead;
                AudioSourceController.LoopbackAudioSource.DeviceInitialized -= this.LoopbackAudioSource_DeviceInitialized;
            }
        }

        private void LoopbackAudioSource_DeviceInitialized(object sender, Events.MMDeviceEventArgs e)
        {
            // If the device changes, we need to stop gathering FFT data and re-create the spectrum provider using the new device's format.
            AudioSourceController.LoopbackAudioSource.SingleBlockRead -= this.LoopbackAudioSource_SingleBlockRead;
            if (this.updateFftDataCoroutine != null)
            {
                this.StopCoroutine(this.updateFftDataCoroutine);
                this.updateFftDataCoroutine = null;
            }

            var deviceFormat = e.Device.DeviceFormat;
            this.spectrumProvider = new SimpleSpectrumProvider(deviceFormat.Channels, deviceFormat.SampleRate, this.fftSize);

            AudioSourceController.LoopbackAudioSource.SingleBlockRead += this.LoopbackAudioSource_SingleBlockRead;
            this.updateFftDataCoroutine = this.StartCoroutine(this.UpdateFftData());
        }

        private void LoopbackAudioSource_SingleBlockRead(object sender, SingleBlockReadEventArgs e)
        {
            this.spectrumProvider.Add(e.Left, e.Right);
        }

#if UNITY_EDITOR
        public bool fftBufferFoldout;
#endif
    }
}