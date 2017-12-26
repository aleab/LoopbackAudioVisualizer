using System;

namespace Aleab.LoopbackAudioVisualizer.Scripts.Visualizers.Visualizer01
{
    public class SpectrumVisualizer : ScaledSpectrumVisualizer, ISpectrumMeanAmplitudeProvider
    {
        public float SpectrumMeanAmplitude { get; private set; }

        public float WeightFunction(float frequency)
        {
            const float minWeight = 0.05f;
            const float stdDeviation = 4.0f;
            const float mean = 2048.0f;
            return (float)((1.0f - minWeight) * Math.Exp(-Math.Pow((frequency - mean) / 1000.0, 2) / (2.0 * stdDeviation * stdDeviation)) + minWeight);
        }

        private void CalculateMean()
        {
            float weightedSum = 0.0f;
            float sumOfWeights = 0.0f;
            for (int i = 0; i < this.fftDataBuffer.Length; ++i)
            {
                float frequency = this.SpectrumProvider.GetFrequency(i);
                float weight = this.WeightFunction(frequency);
                sumOfWeights += weight;
                weightedSum += this.fftDataBuffer[i] * weight;
            }
            this.SpectrumMeanAmplitude = weightedSum / sumOfWeights;
        }

        protected override void OnFftDataBufferUpdated()
        {
            base.OnFftDataBufferUpdated();
            this.CalculateMean();
        }
    }
}