using Aleab.LoopbackAudioVisualizer.Helpers;
using Aleab.LoopbackAudioVisualizer.Unity;
using System;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Scripts.Visualizers.Visualizer01
{
    public class LightsTuner : BaseLightsTuner
    {
        #region Inspector

#pragma warning disable 0414, 0649

        [SerializeField]
        [DisableWhenPlaying]
        private SpectrumVisualizer spectrumVisualizer;

#pragma warning restore 0414, 0649

        #endregion Inspector

        private int index_OnOffTh_MSA = -1;

        private void Awake()
        {
            this.RequireField(nameof(this.spectrumVisualizer), this.spectrumVisualizer);

            this.index_OnOffTh_MSA = this.GetLightSetIndex(nameof(this.index_OnOffTh_MSA));

            if (!this.AutoUpdate)
                this.SubscribeEventHandlers();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            this.index_OnOffTh_MSA = this.GetLightSetIndex(nameof(this.index_OnOffTh_MSA));

            this.UnSubscribeEventHandlers();
            if (!this.AutoUpdate)
                this.SubscribeEventHandlers();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            this.UnSubscribeEventHandlers();
            this.ResetIntensities();
        }

        #region Tuning functions' input providers

        /// <summary>
        /// Input provider for a tuning function.
        /// </summary>
        /// <param name="light"> The light being tuned. </param>
        /// <returns> The input for the <see cref="LightTuning.LightSetTuning{TIn,TTarget}.ProcessInput(TIn)"/> method of the tuning function. </returns>
        public float OnOffThresholdOnMeanSpectrumAmplitude_GetX(Light light)
        {
            return Mathf.Clamp01(this.spectrumVisualizer.SpectrumMeanAmplitude / this.spectrumVisualizer.SpectrumMeanAmplitudePeak);
        }

        #endregion Tuning functions' input providers

        private void ResetIntensities()
        {
            if (this.lightSets != null)
            {
                foreach (var lightSet in this.lightSets)
                    lightSet.ResetDefaultIntensities();
            }
        }

        #region Event Handlers

        private void SubscribeEventHandlers()
        {
            this.UnSubscribeEventHandlers();
            this.spectrumVisualizer.SpectrumMeanAmplitudeUpdated += this.SpectrumVisualizer_SpectrumMeanAmplitudeUpdated;
            this.spectrumVisualizer.SpectrumRangeMeanAmplitudeUpdated += this.SpectrumVisualizer_SpectrumRangeMeanAmplitudeUpdated;
        }

        private void UnSubscribeEventHandlers()
        {
            this.spectrumVisualizer.SpectrumMeanAmplitudeUpdated -= this.SpectrumVisualizer_SpectrumMeanAmplitudeUpdated;
            this.spectrumVisualizer.SpectrumRangeMeanAmplitudeUpdated -= this.SpectrumVisualizer_SpectrumRangeMeanAmplitudeUpdated;
        }

        private void SpectrumVisualizer_SpectrumMeanAmplitudeUpdated(object sender, EventArgs e)
        {
            if (this.index_OnOffTh_MSA >= 0)
                this.UpdateLights(this.index_OnOffTh_MSA);
        }

        private void SpectrumVisualizer_SpectrumRangeMeanAmplitudeUpdated(object sender, Events.SpectrumRangeMeanAmplitudeEventArgs e)
        {
        }

        #endregion Event Handlers

        #region LightSetMapping

#if UNITY_EDITOR

        /// <inheritdoc />
        protected override string[] GetLightSetMappingNames()
        {
            return new[]
            {
                nameof(this.index_OnOffTh_MSA),
            };
        }

#endif

        #endregion LightSetMapping
    }
}