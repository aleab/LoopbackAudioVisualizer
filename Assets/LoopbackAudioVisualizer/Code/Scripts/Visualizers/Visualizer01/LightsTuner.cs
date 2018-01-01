using Aleab.LoopbackAudioVisualizer.Helpers;
using Aleab.LoopbackAudioVisualizer.Unity;
using MathNet.Numerics;
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

        #region Intensity ⨯ Mean Spectrum Amplitude

        [SerializeField]
        [Range(-0.5f, 4.0f)]
        private float intMeanSpAMin = 0.1f;

        [SerializeField]
        [Range(1.0f, 10.0f)]
        private float intMeanSpAMax = 6.0f;

        [SerializeField]
        [Range(0.0f, 1.0f)]
        private float intMeanSpAThreshold = 0.05f;

        [SerializeField]
        [Range(0.01f, 1.0f)]
        private float intMeanSpASigma = 0.25f;

        #endregion Intensity ⨯ Mean Spectrum Amplitude

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
                if (set?.lights != null)
                {
                    if (this.intensities.Count <= i)
                        this.intensities.Add(new List<float>(set.lights.Length));
                    foreach (Light light in set.lights)
                        this.intensities[i].Add(light.intensity);
                }
            }

            this.lightSetsFunctions.Add(0, this.IntensityTuningOnSpectrumMeanAmplitudeChange);
            this.lightSetsFunctions.Add(1, this.OnOffThresholdOnMeanSpectrumAmplitude);

            if (!this.AutoUpdate)
                this.spectrumVisualizer.SpectrumMeanAmplitudeUpdated += this.SpectrumVisualizer_SpectrumMeanAmplitudeUpdated;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            this.spectrumVisualizer.SpectrumMeanAmplitudeUpdated -= this.SpectrumVisualizer_SpectrumMeanAmplitudeUpdated;
            if (!this.AutoUpdate)
                this.spectrumVisualizer.SpectrumMeanAmplitudeUpdated += this.SpectrumVisualizer_SpectrumMeanAmplitudeUpdated;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            this.spectrumVisualizer.SpectrumMeanAmplitudeUpdated -= this.SpectrumVisualizer_SpectrumMeanAmplitudeUpdated;
            this.ResetLightsIntensity();
        }

        #region LightSetsFunctions

        private readonly Dictionary<int, Action<Light, int, int>> lightSetsFunctions = new Dictionary<int, Action<Light, int, int>>();

        #region Intensity ⨯ Mean Spectrum Amplitude

        private float IntensityTuningOnSpectrumMeanAmplitudeChangeFunction(float value)
        {
            float f = -Mathf.Exp(-(value * value) / (2.0f * this.intMeanSpASigma * this.intMeanSpASigma)) + 1.0f;
            f = Mathf.Pow(f, 2.2f);
            return (this.intMeanSpAMax - this.intMeanSpAMin) * Math.Abs(f) + this.intMeanSpAMin;
        }

        private void IntensityTuningOnSpectrumMeanAmplitudeChange(Light light, int lightIndex, int setIndex)
        {
            float normalizedValue = Mathf.Clamp01(this.spectrumVisualizer.SpectrumMeanAmplitude / this.spectrumVisualizer.SpectrumMeanAmplitudePeak);
            if (normalizedValue.AlmostEqual(0.0f, 0.0001))
                light.intensity = 0.0f;
            else if (normalizedValue.IsLarger(this.intMeanSpAThreshold, 0.0000001))
                light.intensity = this.IntensityTuningOnSpectrumMeanAmplitudeChangeFunction(normalizedValue);
        }

        #endregion Intensity ⨯ Mean Spectrum Amplitude

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
                if (set?.lights == null)
                    continue;
                for (int j = 0; j < this.intensities[i].Count && j < set.lights.Length; ++j)
                {
                    if (set.lights[j] != null)
                        set.lights[j].intensity = this.intensities[i][j];
                }
            }
        }

        private void SpectrumVisualizer_SpectrumMeanAmplitudeUpdated(object sender, EventArgs e)
        {
            this.UpdateLights(0);
            this.UpdateLights(1);
        }
    }
}