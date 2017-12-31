﻿using Aleab.LoopbackAudioVisualizer.Helpers;
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

        [SerializeField]
        [DisableWhenPlaying]
        private SpectrumVisualizer spectrumVisualizer;

        #region Intensity ⨯ Mean Spectrum Amplitude

        [SerializeField]
        [Range(-0.5f, 4.0f)]
        private float intMeanSpAMin = 0.25f;

        [SerializeField]
        [Range(1.0f, 10.0f)]
        private float intMeanSpAMax = 6.0f;

        [SerializeField]
        [Range(0.0f, 1.0f)]
        private float intMeanSpAThreshold = 0.05f;

        [SerializeField]
        [Range(0.01f, 1.0f)]
        private float intMeanSpASigma = 0.4f;

        #endregion Intensity ⨯ Mean Spectrum Amplitude

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

        private readonly Dictionary<int, Action<Light>> lightSetsFunctions = new Dictionary<int, Action<Light>>();

        private void IntensityTuningOnSpectrumMeanAmplitudeChange(Light light)
        {
            float normalizedValue = Mathf.Clamp01(this.spectrumVisualizer.SpectrumMeanAmplitude / this.spectrumVisualizer.SpectrumMeanAmplitudePeak);
            if (normalizedValue.AlmostEqual(0.0f, 0.0001))
                light.intensity = 0.0f;
            else if (normalizedValue.IsLarger(this.intMeanSpAThreshold, 0.0000001))
            {
                float f = Mathf.Exp(-(normalizedValue - 1.0f) * (normalizedValue - 1.0f) / (2.0f * this.intMeanSpASigma * this.intMeanSpASigma));
                light.intensity = (this.intMeanSpAMax - this.intMeanSpAMin) * Mathf.Clamp(f, 0.0f, float.PositiveInfinity) + this.intMeanSpAMin;
            }
        }

        #endregion LightSetsFunctions

        /// <inheritdoc />
        protected override void TuneLight(int setIndex, Light light)
        {
            if (this.lightSetsFunctions?.ContainsKey(setIndex) == true)
                this.lightSetsFunctions[setIndex]?.Invoke(light);
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
        }
    }
}