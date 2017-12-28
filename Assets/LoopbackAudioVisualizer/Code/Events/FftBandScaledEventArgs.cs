using System;

namespace Aleab.LoopbackAudioVisualizer.Events
{
    public class FftBandScaledEventArgs : EventArgs
    {
        public int Index { get; }
        public float UnscaledValue { get; }
        public float ScaledValue { get; }

        public FftBandScaledEventArgs(int index, float unscaledValue, float scaledValue)
        {
            this.Index = index;
            this.UnscaledValue = unscaledValue;
            this.ScaledValue = scaledValue;
        }
    }
}