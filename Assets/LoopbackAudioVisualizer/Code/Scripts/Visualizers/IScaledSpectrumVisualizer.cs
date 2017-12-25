using System.Collections.Generic;

namespace Aleab.LoopbackAudioVisualizer.Scripts.Visualizers
{
    public interface IScaledSpectrumVisualizer
    {
        IReadOnlyCollection<float> ScaledFftDataBuffer { get; }

        float SpectrumScalingFunction(int fftBandIndex, float fftBandValue);
    }
}