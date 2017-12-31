using System;

namespace Aleab.LoopbackAudioVisualizer.Scripts.Visualizers
{
    /// <summary>
    /// Provides a weighted mean of the spectrum's values.
    /// </summary>
    public interface ISpectrumMeanAmplitudeProvider
    {
        event EventHandler SpectrumMeanAmplitudeUpdated;

        /// <summary>
        /// An weighted mean of the values of the spectrum's samples.
        /// </summary>
        float SpectrumMeanAmplitude { get; }

        /// <summary>
        /// The highest mean amplitude reached.
        /// </summary>
        float SpectrumMeanAmplitudePeak { get; }

        /// <summary>
        /// The function used to calculate the weight for each frequency band.
        /// </summary>
        /// <param name="frequency"> The frequency band. </param>
        /// <returns> The weight. </returns>
        float WeightFunction(float frequency);
    }
}