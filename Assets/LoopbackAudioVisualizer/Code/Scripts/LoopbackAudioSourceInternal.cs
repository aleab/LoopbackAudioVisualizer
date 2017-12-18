using CSCore;
using CSCore.CoreAudioAPI;
using CSCore.SoundIn;
using CSCore.Streams;
using System;
using System.Linq;

namespace Aleab.LoopbackAudioVisualizer
{
    public partial class LoopbackAudioSource
    {
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
            this._currentAudioBlock.left = e.Left;
            this._currentAudioBlock.right = e.Right;
            this._currentAudioBlock.samples = e.Channels > 2 && e.Samples != null ? e.Samples : new[] { e.Left, e.Right };
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
    }
}