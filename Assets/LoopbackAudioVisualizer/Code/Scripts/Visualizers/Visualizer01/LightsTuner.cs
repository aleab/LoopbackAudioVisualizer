using Aleab.LoopbackAudioVisualizer.Helpers;
using Aleab.LoopbackAudioVisualizer.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;
using Range = Aleab.LoopbackAudioVisualizer.Unity.RangeAttribute;

namespace Aleab.LoopbackAudioVisualizer.Scripts.Visualizers.Visualizer01
{
    public class LightsTuner : BaseLightsTuner
    {
        #region Inspector

#pragma warning disable 0414, 0649

        [SerializeField]
        [DisableWhenPlaying]
        private SpectrumVisualizer spectrumVisualizer;

        #region On/Off Threshold ⨯ Mean Spectrum Amplitude

        [SerializeField]
        [Range(0.0f, 1.0f)]
        private float onOffMeanSpAThreshold = 0.1f;

        #endregion On/Off Threshold ⨯ Mean Spectrum Amplitude

#pragma warning restore 0414, 0649

        #endregion Inspector

        private List<List<float>> intensities;

        /// <inheritdoc />
        protected override float UpdateInterval { get { return 0.075f; } }

        private void Awake()
        {
            this.RequireField(nameof(this.spectrumVisualizer), this.spectrumVisualizer);

            this.intensities = new List<List<float>>(this.lightSets.Length);
            for (int i = 0; i < this.lightSets.Length; ++i)
            {
                var set = this.lightSets[i];
                if (set?.Lights != null)
                {
                    if (this.intensities.Count <= i)
                        this.intensities.Add(new List<float>(set.Lights.Length));
                    foreach (Light light in set.Lights)
                        this.intensities[i].Add(light.intensity);
                }
            }

            this.lightSetsFunctions.Add(0, this.OnOffThresholdOnMeanSpectrumAmplitude);

            if (!this.AutoUpdate)
            {
                this.spectrumVisualizer.SpectrumMeanAmplitudeUpdated += this.SpectrumVisualizer_SpectrumMeanAmplitudeUpdated;
                this.spectrumVisualizer.SpectrumRangeMeanAmplitudeUpdated += this.SpectrumVisualizer_SpectrumRangeMeanAmplitudeUpdated;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            this.spectrumVisualizer.SpectrumMeanAmplitudeUpdated -= this.SpectrumVisualizer_SpectrumMeanAmplitudeUpdated;
            this.spectrumVisualizer.SpectrumRangeMeanAmplitudeUpdated -= this.SpectrumVisualizer_SpectrumRangeMeanAmplitudeUpdated;
            if (!this.AutoUpdate)
            {
                this.spectrumVisualizer.SpectrumMeanAmplitudeUpdated += this.SpectrumVisualizer_SpectrumMeanAmplitudeUpdated;
                this.spectrumVisualizer.SpectrumRangeMeanAmplitudeUpdated += this.SpectrumVisualizer_SpectrumRangeMeanAmplitudeUpdated;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            this.spectrumVisualizer.SpectrumMeanAmplitudeUpdated -= this.SpectrumVisualizer_SpectrumMeanAmplitudeUpdated;
            this.spectrumVisualizer.SpectrumRangeMeanAmplitudeUpdated -= this.SpectrumVisualizer_SpectrumRangeMeanAmplitudeUpdated;
            this.ResetLightsIntensity();
        }

        #region LightSetsFunctions

        private readonly Dictionary<int, Action<Light, int, int>> lightSetsFunctions = new Dictionary<int, Action<Light, int, int>>();

        #region On/Off Threshold ⨯ Mean Spectrum Amplitude

        private void OnOffThresholdOnMeanSpectrumAmplitude(Light light, int lightIndex, int setIndex)
        {
            float normalizedValue = Mathf.Clamp01(this.spectrumVisualizer.SpectrumMeanAmplitude / this.spectrumVisualizer.SpectrumMeanAmplitudePeak);
            float sigma = this.onOffMeanSpAThreshold / Mathf.Sqrt(2.0f * Mathf.Log(2.0f));
            float f = -Mathf.Exp(-(normalizedValue * normalizedValue) / (2.0f * sigma * sigma)) + 1.0f;
            f = Mathf.Pow(f, 2.2f);
            light.intensity = (this.intensities?[setIndex]?[lightIndex] ?? 0.0f) * f;
        }

        #endregion On/Off Threshold ⨯ Mean Spectrum Amplitude

        #endregion LightSetsFunctions

        /// <inheritdoc />
        protected override void TuneLight(int setIndex, Light light, int lightIndex)
        {
            if (this.lightSetsFunctions?.ContainsKey(setIndex) == true)
                this.lightSetsFunctions[setIndex]?.Invoke(light, lightIndex, setIndex);
        }

        private void ResetLightsIntensity()
        {
            for (int i = 0; i < this.intensities.Count; ++i)
            {
                var set = this.lightSets[i];
                if (set?.Lights == null)
                    continue;
                for (int j = 0; j < this.intensities[i].Count && j < set.Lights.Length; ++j)
                {
                    if (set.Lights[j] != null)
                        set.Lights[j].intensity = this.intensities[i][j];
                }
            }
        }

        private void SpectrumVisualizer_SpectrumMeanAmplitudeUpdated(object sender, EventArgs e)
        {
            this.UpdateLights(0);
        }

        private void SpectrumVisualizer_SpectrumRangeMeanAmplitudeUpdated(object sender, Events.SpectrumRangeMeanAmplitudeEventArgs e)
        {
        }
    }
}