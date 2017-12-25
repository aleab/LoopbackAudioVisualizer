using System.Collections.Generic;

namespace Aleab.LoopbackAudioVisualizer.Scripts.Visualizers
{
    public interface IScaledSpectrumDataProvider
    {
        IReadOnlyCollection<float> ScaledFftDataBuffer { get; }

        float SpectrumScalingFunction(int fftBandIndex, float fftBandValue);
    }
}