namespace Aleab.LoopbackAudioVisualizer.Scripts.Visualizers
{
    public interface ISpectrumMeanAmplitudeProvider
    {
        /// <summary>
        /// An weighted mean of the values of the spectrum's samples.
        /// </summary>
        float SpectrumMeanAmplitude { get; }

        /// <summary>
        /// The function used to calculate the weight for each frequency band.
        /// </summary>
        /// <param name="frequency"> The frequency band. </param>
        /// <returns> The weight. </returns>
        float WeightFunction(float frequency);
    }
}