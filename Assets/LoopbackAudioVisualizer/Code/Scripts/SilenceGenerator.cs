using CSCore;
using CSCore.SoundOut;
using System;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Scripts
{
    public class SilenceGenerator : MonoBehaviour, IDisposable
    {
        private IWaveSource soundSource;

        private ISoundOut soundOut;

        private void Init()
        {
            this.Dispose();

            this.soundSource = new SilenceSource();
            this.soundOut = new DirectSoundOut();
            this.soundOut.Initialize(this.soundSource);
        }

        private void Start()
        {
            this.Init();
            this.soundOut.Play();
        }

        private void OnEnable()
        {
            this.soundOut?.Resume();
        }

        private void OnDisable()
        {
            this.soundOut?.Pause();
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
                this.soundOut?.Pause();
            else
                this.soundOut?.Resume();
        }

        private void OnDestroy()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            if (this.soundOut != null)
            {
                this.soundOut.Stop();
                this.soundOut.Dispose();
                this.soundOut = null;
            }

            if (this.soundSource != null)
            {
                this.soundSource.Dispose();
                this.soundSource = null;
            }
        }
    }
}