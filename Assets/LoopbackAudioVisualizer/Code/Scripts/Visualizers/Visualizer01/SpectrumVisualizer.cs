using System.Linq;

namespace Aleab.LoopbackAudioVisualizer.Scripts.Visualizers.Visualizer01
{
    public class SpectrumVisualizer : ScaledSpectrumVisualizer, ISpectrumAverageAmplitudeProvider
    {
        public float SpectrumAverageAmplitude { get; private set; }

        protected override void OnFftDataBufferUpdated()
        {
            base.OnFftDataBufferUpdated();
            this.SpectrumAverageAmplitude = this.fftDataBuffer.Average();
        }
    }
}