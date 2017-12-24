using CSCore.DSP;
using System;
using System.Collections.Generic;

namespace Aleab.LoopbackAudioVisualizer
{
    public class SimpleSpectrumProvider : FftProvider, ISpectrumProvider
    {
        private readonly int _sampleRate;
        private readonly List<object> _contexts = new List<object>();

        public SimpleSpectrumProvider(int channels, int sampleRate, FftSize fftSize) : base(channels, fftSize)
        {
            if (sampleRate <= 0)
                throw new ArgumentOutOfRangeException(nameof(sampleRate));
            this._sampleRate = sampleRate;

            // TODO: Set WindowFunction to WindowFunctions.Hanning once CSCore version 1.3 (or >1.2.1.2) is out.
        }

        /// <summary>
        /// Calculates the Fast Fourier Transform and stores the result in fftBuffer.
        /// </summary>
        /// <param name="fftBuffer"> The output buffer. </param>
        /// <param name="context"> The object requesting the data. </param>
        /// <returns> Returns a value which indicates whether the Fast Fourier Transform got calculated. </returns>
        public bool GetFftData(float[] fftBuffer, object context)
        {
            if (this._contexts.Contains(context))
                return false;

            if (fftBuffer == null || fftBuffer.Length < (int)this.FftSize)
                return false;

            this._contexts.Add(context);
            try
            {
                this.GetFftData(fftBuffer);
            }
            catch (ArgumentException)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Get the index of the FFT buffer corresponding to the specified frequency.
        /// </summary>
        /// <param name="frequency"> Frequency. </param>
        /// <returns> Index. </returns>
        public int GetFftBandIndex(float frequency)
        {
            int fftSize = (int)this.FftSize;
            double f = this._sampleRate / 2.0;
            return (int)((frequency / f) * (fftSize / 2.0));
        }

        /// <summary>
        /// Get the frequency corresponding to the specified index of the FFT buffer.
        /// </summary>
        /// <param name="index"> Index. </param>
        /// <returns> Frequency. </returns>
        public float GetFrequency(int index)
        {
            int fftSize = (int)this.FftSize;
            double f = this._sampleRate / 2.0;
            return (float)((f * index) / (fftSize / 2.0));
        }

        public override void Add(float[] samples, int count)
        {
            base.Add(samples, count);
            if (count > 0)
                this._contexts.Clear();
        }

        public override void Add(float left, float right)
        {
            base.Add(left, right);
            this._contexts.Clear();
        }
    }
}