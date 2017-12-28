using Aleab.LoopbackAudioVisualizer.Events;
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Scripts.Visualizers.Visualizer01
{
    public class SpectrumVisualizer : ScaledSpectrumVisualizer, ISpectrumMeanAmplitudeProvider, IReducedBandsSpectrumProvider
    {
        #region Inspector

#pragma warning disable 0414, 0649

        [SerializeField]
        private NumberOfFrequencyBands numberOfBands;

#pragma warning restore 0414, 0649

        #endregion Inspector

        public event EventHandler SpectrumMeanAmplitudeUpdated;

        public event EventHandler<BandValueCalculatedEventArgs> BandValueCalculated;

        #region [ ISpectrumMeanAmplitudeProvider ]

        /// <inheritdoc />
        public float SpectrumMeanAmplitude { get; private set; }

        /// <inheritdoc />
        public float WeightFunction(float frequency)
        {
            const float minWeight = 0.05f;
            const float stdDeviation = 4.0f;
            const float mean = 2048.0f;
            return (float)((1.0f - minWeight) * Math.Exp(-Math.Pow((frequency - mean) / 1000.0, 2) / (2.0 * stdDeviation * stdDeviation)) + minWeight);
        }

        private void CalculateMean()
        {
            float weightedSum = 0.0f;
            float sumOfWeights = 0.0f;
            for (int i = 0; i < this.fftDataBuffer.Length; ++i)
            {
                float frequency = this.SpectrumProvider.GetFrequency(i);
                float weight = this.WeightFunction(frequency);
                sumOfWeights += weight;
                weightedSum += this.fftDataBuffer[i] * weight;
            }
            float mean = weightedSum / sumOfWeights;
            if (!this.SpectrumMeanAmplitude.AlmostEqual(mean))
            {
                this.SpectrumMeanAmplitude = mean;
                this.SpectrumMeanAmplitudeUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        #endregion [ ISpectrumMeanAmplitudeProvider ]

        #region [ IReducedBandsSpectrumProvider ]

        private float[] baseBandsHighestFrequencies;
        private float[] subBandsHighestFrequencies;

        private float[] bandsDataBuffer;

        /// <inheritdoc />
        public int NumberOfBands { get { return (int)this.numberOfBands; } }

        /// <inheritdoc />
        public IReadOnlyCollection<float> BandsDataBuffer { get { return this.bandsDataBuffer; } }

        private void PopulateBandsBuffer()
        {
            int S = this.SpectrumProvider.SampleRate;
            int M = this.fftDataBuffer.Length;
            float fm = S / (2.0f * M);

            // (Re-)Create the new bands buffer array (if necessary)
            if (this.NumberOfBands != this.bandsDataBuffer.Length)
            {
                this.bandsDataBuffer = new float[this.NumberOfBands];
                this.baseBandsHighestFrequencies = null;
            }

            // (Re-)Calculate the starting eight frequency bands (if necessary)
            if (this.baseBandsHighestFrequencies == null)
            {
                this.CalculateBaseBands();
                this.subBandsHighestFrequencies = null;
            }

            // (Re-)Calculate the necessary sub-divisions of the eight bands (if necessary)
            if (this.subBandsHighestFrequencies == null)
                this.CalculateSubBands();

            // Calculate bands' average values
            int i = 0;
            float f = 0.0f;
            for (int b = 0; b < this.NumberOfBands; ++b)
            {
                int samples = 0;
                float sum = 0.0f;
                for (; f < this.subBandsHighestFrequencies[b]; ++i, ++samples, f += fm)
                    sum += this.fftDataBuffer[i];
                this.bandsDataBuffer[b] = sum / samples;
                this.BandValueCalculated?.Invoke(this, new BandValueCalculatedEventArgs(b, this.bandsDataBuffer[b]));
            }
        }

        private void CalculateBaseBands()
        {
            #region Maths

            /*
             * Standard Frequency Bands:
             *
             * Sub-bass           20Hz - 60Hz       F₀
             * Bass               60Hz - 250Hz      F₁
             * Low midrange      250Hz - 500Hz      F₂
             * Midrange          500Hz - 2000Hz     F₃
             * Upper midrange   2000Hz - 4000Hz     F₄
             * Presence         4000Hz - 6000Hz     F₅
             * Brilliance       6000Hz - 20000Hz    F₆
             *
             * The FFT bands go from 0Hz to (SampleRate/2)Hz in (FftSize/2) samples.
             * Each sample has (SampleRate / FftSize)Hz.
             *
             * S: sample rate
             * M: number of original bands/samples, i.e. FftSize/2
             * fₘ: Hz in each original band
             * nᵢ: number of samples in the band i
             * fᵢ: highest frequency in band i
             *
             * fₘ = S / (2M)
             * nᵢ ≥ 1 always!
             * fᵢ = nᵢ * fₘ
             *
             * n₀ = ⌈(F₀ / fₘ)⌉
             * nᵢ = ⌈(U(i) / fₘ)⌉
             *        { (Fᵢ - ∑ⱼⁱ⁻¹fⱼ) / fₘ
             * U(i) = {
             *        { fₘ    if  ∑ⱼⁱ⁻¹fⱼ ≥ Fᵢ
             */

            #endregion Maths

            float[] F = { 60, 250, 500, 2000, 4000, 6000, 20000 };
            int S = this.SpectrumProvider.SampleRate;
            int M = this.fftDataBuffer.Length;
            float fm = S / (2.0f * M);

            this.baseBandsHighestFrequencies = new float[8];
            for (int i = 0; i < 8; ++i)
            {
                float prevFreq = i == 0 ? 0.0f : this.baseBandsHighestFrequencies[i - 1];
                this.baseBandsHighestFrequencies[i] = i == 7 ? S / 2.0f : prevFreq + fm * (prevFreq <= F[i] ? Mathf.Ceil((F[i] - prevFreq) / fm) : 1.0f);
            }
        }

        private void CalculateSubBands()
        {
            int S = this.SpectrumProvider.SampleRate;
            int M = this.fftDataBuffer.Length;
            float fm = S / (2.0f * M);

            this.subBandsHighestFrequencies = new float[this.NumberOfBands];
            if (this.NumberOfBands == 8)
                Array.Copy(this.baseBandsHighestFrequencies, this.subBandsHighestFrequencies, 8);
            else
            {
                // Assume NumberOfBands is a multiple of 8
                int subdivisionsPerBand = this.NumberOfBands / 8;
                int pendingSubdivisions = 0;
                float currentFrequency = 0.0f;
                for (int i = 0, j = 0; i < 8 && j < this.NumberOfBands; ++i)
                {
                    int samplesInBaseBand = (int)((this.baseBandsHighestFrequencies[i] - currentFrequency) / fm);

                    if (samplesInBaseBand == 1)
                    {
                        this.subBandsHighestFrequencies[j++] = this.baseBandsHighestFrequencies[i];
                        pendingSubdivisions += subdivisionsPerBand;
                    }
                    else
                    {
                        int subdivisionsToCreateInThisBand = subdivisionsPerBand + pendingSubdivisions;
                        int maxSubdivisionsThatCanBeCreated = Math.Min(samplesInBaseBand, subdivisionsToCreateInThisBand);
                        int samplesForEachSubdivision = Mathf.FloorToInt((float)samplesInBaseBand / maxSubdivisionsThatCanBeCreated);
                        for (int subD = 0; subD < maxSubdivisionsThatCanBeCreated - 1 && j < this.NumberOfBands - 1; ++subD)
                        {
                            float deltaF = fm * samplesForEachSubdivision * (subD + 1);
                            this.subBandsHighestFrequencies[j++] = currentFrequency + deltaF;
                        }
                        this.subBandsHighestFrequencies[j++] = this.baseBandsHighestFrequencies[i];
                        pendingSubdivisions = subdivisionsToCreateInThisBand - maxSubdivisionsThatCanBeCreated;
                    }
                    currentFrequency = this.subBandsHighestFrequencies[j - 1];
                }
            }
        }

        #endregion [ IReducedBandsSpectrumProvider ]

        #region Event Handlers

        protected override void OnFftDataBufferUpdated()
        {
            base.OnFftDataBufferUpdated();
            this.CalculateMean();
            this.PopulateBandsBuffer();
        }

        protected override void OnUpdateFftDataCoroutineStarted()
        {
            base.OnUpdateFftDataCoroutineStarted();
            this.bandsDataBuffer = new float[this.NumberOfBands];
        }

        protected override void OnUpdateFftDataCoroutineStopped()
        {
            base.OnUpdateFftDataCoroutineStopped();
            this.SpectrumMeanAmplitude = 0.0f;

            if (this.bandsDataBuffer != null)
                Array.Clear(this.bandsDataBuffer, 0, this.bandsDataBuffer.Length);
        }

        #endregion Event Handlers
    }
}