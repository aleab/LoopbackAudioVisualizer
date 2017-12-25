using Aleab.LoopbackAudioVisualizer.Events;
using Aleab.LoopbackAudioVisualizer.Maths;
using Aleab.LoopbackAudioVisualizer.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;
using Range = Aleab.LoopbackAudioVisualizer.Unity.RangeAttribute;

namespace Aleab.LoopbackAudioVisualizer.Scripts.Visualizers
{
    /// <summary>
    /// Implementation of <see cref="BaseSpectrumVisualizer"/>, which uses a parametric scaling function to get a scaled version of the spectrum.
    /// </summary>
    /// <see cref="BaseSpectrumVisualizer"/>
    /// <see cref="IScaledSpectrumVisualizer"/>
    public class ScaledSpectrumVisualizer : BaseSpectrumVisualizer, IScaledSpectrumVisualizer
    {
        #region Inspector

#pragma warning disable 0414, 0649

        [SerializeField]
        private FunctionType scalingFunctionType = FunctionType.Gaussian;

        #region Gaussian

        [SerializeField]
        [Range(1.0f, 25.0f)]
        private float gaussStdDeviation = 1.8f;

        [SerializeField]
        [Range(0.1f, 10.0f)]
        private float gaussLowFreqGain = 4.0f;

        [SerializeField]
        [Range(20.0f, 75.0f)]
        private float gaussHighFreqGain = 45.0f;

        #endregion Gaussian

        #region Logarithmic

        [SerializeField]
        [Range(25.0f, 75.0f)]
        private float logSteepness = 45.0f;

        [SerializeField]
        [Range(2.048f, 48.0f)]
        private float logHighFreq = 4.096f;

        [SerializeField]
        [Range(0.1f, 10.0f)]
        private float logLowFreqGain = 1.5f;

        [SerializeField]
        [Range(20.0f, 75.0f)]
        private float logHighFreqGain = 40.0f;

        #endregion Logarithmic

#pragma warning restore 0414, 0649

        #endregion Inspector

        public IReadOnlyCollection<float> ScaledFftDataBuffer { get { return this.fftDataBuffer; } }

        public event EventHandler<FftBandScaledEventArgs> FftBandScaled;

        protected override float ProcessRawFftValue(float rawFftValue, int fftBandIndex)
        {
            float scaledFftValue = this.SpectrumScalingFunction(fftBandIndex, rawFftValue);
            this.OnFftBandScaled(fftBandIndex, rawFftValue, scaledFftValue);
            return scaledFftValue;
        }

        public virtual float SpectrumScalingFunction(int fftBandIndex, float fftBandValue)
        {
            float f = this.SpectrumProvider.GetFrequency(fftBandIndex);

            float gain = 1.0f;
            const float k = 100.0f; // scale

            switch (this.scalingFunctionType)
            {
                case FunctionType.Gaussian:
                    float gaussLowPeak = k * (this.gaussLowFreqGain - this.gaussHighFreqGain);
                    double gaussVariance = Math.Pow(this.gaussStdDeviation, 2);
                    gain = (float)(gaussLowPeak * Math.Exp(-Math.Pow(f / 1000.0, 2) / (2.0 * gaussVariance)) + k * this.gaussHighFreqGain);
                    break;

                case FunctionType.Logarithm:
                    const double logBase = 10.0;
                    float logScale = k * this.logSteepness;
                    double lowFreqPow = Math.Pow(logBase, (this.logLowFreqGain * k) / logScale);
                    double highFreqPow = Math.Pow(logBase, (this.logHighFreqGain * k) / logScale);
                    gain = (float)(logScale * Math.Log((highFreqPow - lowFreqPow) * (f / (this.logHighFreq * 1000.0)) + lowFreqPow, logBase));
                    break;
            }
            return fftBandValue * gain;
        }

        #region Event Handlers

        /// <summary>
        /// Called every time a frequency band's scaled value is calculated and added to <see cref="ScaledFftDataBuffer"/>.
        /// </summary>
        /// <param name="fftBandIndex"> The index of the band. </param>
        /// <param name="fftBandValue"> The unscaled value of the band. </param>
        /// <param name="fftBandScaledValue"> The scaled value of the band. </param>
        protected virtual void OnFftBandScaled(int fftBandIndex, float fftBandValue, float fftBandScaledValue)
        {
            this.FftBandScaled?.Invoke(this, new FftBandScaledEventArgs(fftBandIndex, fftBandValue, fftBandScaledValue));
        }

        #endregion Event Handlers
    }
}