namespace Aleab.LoopbackAudioVisualizer.Scripts.Visualizers
{
    public interface ISpectrumAverageAmplitudeProvider
    {
        /// <summary>
        /// An average value of the spectrum's samples amplitude.
        /// </summary>
        float SpectrumAverageAmplitude { get; }
    }
}