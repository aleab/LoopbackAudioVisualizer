using System;

namespace Aleab.LoopbackAudioVisualizer.Events
{
    public class BandValueCalculatedEventArgs : EventArgs
    {
        public int BandIndex { get; }
        public float Value { get; }

        public BandValueCalculatedEventArgs(int bandIndex, float value)
        {
            this.BandIndex = bandIndex;
            this.Value = value;
        }
    }
}