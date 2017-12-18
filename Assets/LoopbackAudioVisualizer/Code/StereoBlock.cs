using System;

namespace Aleab.LoopbackAudioVisualizer
{
    [Serializable]
    public struct StereoBlock
    {
        public static readonly StereoBlock Zero = new StereoBlock(0.0f, 0.0f);

        public float left;
        public float right;

        public StereoBlock(float left, float right)
        {
            this.left = left;
            this.right = right;
        }

        public StereoBlock Copy()
        {
            return new StereoBlock(this.left, this.right);
        }
    }
}