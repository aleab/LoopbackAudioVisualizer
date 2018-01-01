using Aleab.LoopbackAudioVisualizer.Code;
using Aleab.LoopbackAudioVisualizer.Events;
using System;

namespace Aleab.LoopbackAudioVisualizer.Scripts.Visualizers
{
    public interface IRangedSpectrumMeanAmplitudeProvider
    {
        event EventHandler<SpectrumRangeMeanAmplitudeEventArgs> SpectrumRangeMeanAmplitudeUpdated;

        MutableTuple<SpectrumRange, float, float>[] RangedSpectrumMeanAmplitudes { get; }
    }
}