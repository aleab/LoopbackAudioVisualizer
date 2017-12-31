using Aleab.LoopbackAudioVisualizer.Helpers;
using Aleab.LoopbackAudioVisualizer.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Scripts.Visualizers.Visualizer01
{
    public class LightsTuner : BaseLightsTuner
    {
        #region Inspector

        [SerializeField]
        [DisableWhenPlaying]
        private SpectrumVisualizer spectrumVisualizer;

        #endregion Inspector

        /// <inheritdoc />
        protected override float UpdateInterval { get { return 0.075f; } }

        private void Awake()
        {
            this.RequireField(nameof(this.spectrumVisualizer), this.spectrumVisualizer);

            this.lightSetsFunctions.Add(0, this.IntensityTuningOnSpectrumMeanAmplitudeChange);

            this.spectrumVisualizer.SpectrumMeanAmplitudeUpdated += this.SpectrumVisualizer_SpectrumMeanAmplitudeUpdated;
        }

        #region LightSetsFunctions

        private readonly Dictionary<int, Action<Light>> lightSetsFunctions = new Dictionary<int, Action<Light>>();

        private void IntensityTuningOnSpectrumMeanAmplitudeChange(Light light)
        {
            float normalizedValue = Mathf.Clamp01(this.spectrumVisualizer.SpectrumMeanAmplitude / this.spectrumVisualizer.SpectrumMeanAmplitudePeak);

            const float min = -0.2f;
            const float max = 6.0f;
            const float stdDev = 0.4f;
            float f2 = Mathf.Exp(-(normalizedValue - 1.0f) * (normalizedValue - 1.0f) / (2.0f * stdDev * stdDev));
            light.intensity = (max - min) * Mathf.Clamp(f2, 0.0f, float.PositiveInfinity) + min;
        }

        #endregion LightSetsFunctions

        /// <inheritdoc />
        protected override void TuneLight(int setIndex, Light light)
        {
            if (this.lightSetsFunctions?.ContainsKey(setIndex) == true)
                this.lightSetsFunctions[setIndex]?.Invoke(light);
        }

        private void SpectrumVisualizer_SpectrumMeanAmplitudeUpdated(object sender, EventArgs e)
        {
            this.UpdateLights(0);
        }
    }
}