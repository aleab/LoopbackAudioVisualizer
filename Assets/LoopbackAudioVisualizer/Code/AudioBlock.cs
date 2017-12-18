using System;

namespace Aleab.LoopbackAudioVisualizer
{
    [Serializable]
    public struct AudioBlock
    {
        public static readonly AudioBlock Zero = new AudioBlock(0.0f, 0.0f, new float[] { });

        public float left;
        public float right;
        public float[] samples;

        public AudioBlock(float left, float right, float[] samples)
        {
            this.left = left;
            this.right = right;
            this.samples = samples;
        }

        /// <summary>
        /// Returns this instance of <see cref="AudioBlock"/> after changing its fields to their absolute values.
        /// </summary>
        /// <returns> This instance. </returns>
        public AudioBlock Abs()
        {
            this.left = Math.Abs(this.left);
            this.right = Math.Abs(this.right);
            for (var i = 0; i < this.samples.Length; ++i)
                this.samples[i] = Math.Abs(this.samples[i]);
            return this;
        }

        public AudioBlock Copy()
        {
            return new AudioBlock(this.left, this.right, this.samples);
        }
    }
}