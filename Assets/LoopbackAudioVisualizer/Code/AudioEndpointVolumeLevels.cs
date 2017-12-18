using CSCore.CoreAudioAPI;
using System;

namespace Aleab.LoopbackAudioVisualizer
{
    [Serializable]
    public class AudioEndpointVolumeLevels
    {
        public float decibels = float.NaN;

        public float scalar = float.NaN;

        public AudioEndpointVolumeLevels()
        {
        }

        protected AudioEndpointVolumeLevels(float decibels, float scalar)
        {
            this.decibels = decibels;
            this.scalar = scalar;
        }

        public void Update(AudioEndpointVolume audioEndpointVolume)
        {
            this.decibels = audioEndpointVolume.GetMasterVolumeLevel();
            this.scalar = audioEndpointVolume.GetMasterVolumeLevelScalar();
        }

        public AudioEndpointVolumeLevels Copy()
        {
            return new AudioEndpointVolumeLevels(this.decibels, this.scalar);
        }
    }
}