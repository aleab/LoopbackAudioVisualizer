using Aleab.LoopbackAudioVisualizer.Code;
using System;

namespace Aleab.LoopbackAudioVisualizer.Events
{
    public class SpectrumRangeMeanAmplitudeEventArgs : EventArgs
    {
        public SpectrumRange SpectrumRange { get; }
        public float MeanAmplitude { get; }
        public float PeakAmplitude { get; }

        public SpectrumRangeMeanAmplitudeEventArgs(SpectrumRange spectrumRange, float meanAmplitude, float peakAmplitude)
        {
            this.SpectrumRange = spectrumRange;
            this.MeanAmplitude = meanAmplitude;
            this.PeakAmplitude = peakAmplitude;
        }
    }
}