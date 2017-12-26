using Aleab.LoopbackAudioVisualizer.Events;
using System;
using System.Collections.Generic;

namespace Aleab.LoopbackAudioVisualizer.Scripts.Visualizers
{
    /// <summary>
    /// Provides a scaled version of the spectrum.
    /// </summary>
    public interface IScaledSpectrumDataProvider
    {
        event EventHandler<FftBandScaledEventArgs> FftBandScaled;

        IReadOnlyCollection<float> ScaledFftDataBuffer { get; }

        float SpectrumScalingFunction(int fftBandIndex, float fftBandValue);
    }
}