using CSCore.DSP;
using System.Linq;

namespace Aleab.LoopbackAudioVisualizer.Helpers
{
    public static class FftSizeExtensions
    {
        public static NumberOfFrequencyBands[] GetPossibleNumberOfFrequencyBands(this FftSize fftSize)
        {
            int maxFrequency = (int)fftSize / 4;
            var enumValues = typeof(NumberOfFrequencyBands).GetEnumValues().Cast<NumberOfFrequencyBands>();
            return enumValues.Where(bands => (int)bands <= maxFrequency).ToArray();
        }
    }
}