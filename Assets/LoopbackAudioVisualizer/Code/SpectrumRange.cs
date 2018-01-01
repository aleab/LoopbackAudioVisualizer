using System;

namespace Aleab.LoopbackAudioVisualizer.Code
{
    [Serializable]
    public class SpectrumRange
    {
        public float lowerFrequency;
        public float higherFrequency;

        public SpectrumRange(float lowerFrequency, float higherFrequency)
        {
            if (lowerFrequency > higherFrequency)
                throw new ArgumentException($"{nameof(lowerFrequency)} must be larger than {nameof(higherFrequency)}!", $"{nameof(lowerFrequency)}");

            this.lowerFrequency = lowerFrequency;
            this.higherFrequency = higherFrequency;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{this.lowerFrequency}Hz - {this.higherFrequency}Hz";
        }
    }
}