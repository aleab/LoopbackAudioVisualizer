using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.LightTuning
{
    public class LightValues
    {
        public Light Light { get; }
        public float Range { get; }
        public float Intensity { get; }
        public Color Color { get; }

        public LightValues(Light light)
        {
            this.Light = light;
            this.Range = light?.range ?? default(float);
            this.Intensity = light?.intensity ?? default(float);
            this.Color = light?.color ?? default(Color);
        }
    }
}