using System;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.LightTuning.TuningFunctions
{
    public class OnOffThresholdTuningFloat : LightSetTuning<float, float>
    {
        /**
         *                1
         * F(x) = ——————————————————
         *             (m - xMin)ᴱ
         *         1 + ———————————
         *             (x - xMin)ᴱ
         *
         * F'(x) = (yMax - yMin) * F(x) + yMin
         *
         * m: threshold
         */

        private readonly float xMin, xMax;
        private readonly float threshold, e;

        private readonly float yMin, yMax;

        public OnOffThresholdTuningFloat(OnOffThresholdTuningFloatParameters tuningParameters) : base(tuningParameters)
        {
            this.xMin = tuningParameters.XMin;
            this.xMax = tuningParameters.XMax;
            this.threshold = tuningParameters.Threshold;
            this.e = tuningParameters.E;

            this.yMin = tuningParameters.YMin;
            this.yMax = tuningParameters.YMax;
        }

        /// <inheritdoc />
        protected override float ProcessInput(float x)
        {
            float F = 0.0f;
            float X = Mathf.Clamp(x, this.xMin, this.xMax);
            if (!(float.IsInfinity(this.e) && x <= this.threshold))
                F = (float)(1.0 / (1.0 + Math.Pow((double)(this.threshold - this.xMin) / (X - this.xMin), this.e)));
            return Mathf.Clamp((this.yMax - this.yMin) * F + this.yMin, this.yMin, this.yMax);
        }

        /// <inheritdoc />
        protected override float ProcessInput(float x, float defaultValue)
        {
            float F = this.ProcessInput(x);
            return F * defaultValue;
        }
    }
}