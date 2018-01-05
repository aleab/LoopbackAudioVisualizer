using MathNet.Numerics;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Aleab.LoopbackAudioVisualizer.LightTuning.TuningFunctions
{
    [Serializable]
    public sealed class OnOffThresholdTuningFloatParameters : InternalTuningParameters, IEquatable<OnOffThresholdTuningFloatParameters>, IComparable<OnOffThresholdTuningFloatParameters>
    {
        public const float XMIN_MIN = -5.0f;
        public const float XMAX_MAX = 5.0f;
        public const float E_MIN = 0.0f;
        public const float E_MAX = 100.0f;

        public const float YMIN_MIN = -5.0f;
        public const float YMAX_MAX = 5.0f;

        public float XMin { get; private set; }
        public float XMax { get; private set; } = 1.0f;
        public float Threshold { get; private set; } = 0.5f;
        public float E { get; private set; } = 50.0f;
        public float YMin { get; private set; }
        public float YMax { get; private set; } = 1.0f;

        public OnOffThresholdTuningFloatParameters()
        {
        }

        public OnOffThresholdTuningFloatParameters(InternalTuningParameters parameters) : base(parameters)
        {
            this.ReplaceParameters(parameters);
        }

        /// <inheritdoc />
        public override void ReplaceParameters(InternalTuningParameters parameters)
        {
            base.ReplaceParameters(parameters);
            if (ReferenceEquals(parameters, this))
                return;

            OnOffThresholdTuningFloatParameters thisParameters = parameters as OnOffThresholdTuningFloatParameters;
            if (thisParameters != null)
            {
                this.XMin = thisParameters.XMin;
                this.XMax = thisParameters.XMax;
                this.Threshold = thisParameters.Threshold;
                this.E = thisParameters.E;
                this.YMin = thisParameters.YMin;
                this.YMax = thisParameters.YMax;
            }
        }

        /// <inheritdoc />
        public override void SerializeParameters()
        {
            base.SerializeParameters();

            this.SetParameterValue(nameof(this.XMin), this.XMin);
            this.SetParameterValue(nameof(this.XMax), this.XMax);
            this.SetParameterValue(nameof(this.Threshold), this.Threshold);
            this.SetParameterValue(nameof(this.E), this.E);
            this.SetParameterValue(nameof(this.YMin), this.YMin);
            this.SetParameterValue(nameof(this.YMax), this.YMax);
        }

        /// <inheritdoc />
        public override void DeserializeParameters()
        {
            this.XMin = this.GetParameterValue<float>(nameof(this.XMin));
            this.XMax = this.GetParameterValue<float>(nameof(this.XMax));
            this.Threshold = this.GetParameterValue<float>(nameof(this.Threshold));
            this.E = this.GetParameterValue<float>(nameof(this.E));
            this.YMin = this.GetParameterValue<float>(nameof(this.YMin));
            this.YMax = this.GetParameterValue<float>(nameof(this.YMax));
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            OnOffThresholdTuningFloatParameters that = obj as OnOffThresholdTuningFloatParameters;
            return base.Equals(obj) && this.Equals(that);
        }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            if (!this.hashCode.HasValue)
            {
                long k = 347;
                long hash = base.GetHashCode() +
                            (this.XMin.AlmostEqual(0.0f) ? (k *= k) : this.XMin.GetHashCode()) +
                            (this.XMax.AlmostEqual(0.0f) ? (k *= k) : this.XMax.GetHashCode()) +
                            (this.Threshold.AlmostEqual(0.0f) ? (k *= k) : this.Threshold.GetHashCode()) +
                            (this.E.AlmostEqual(0.0f) ? (k *= k) : this.E.GetHashCode()) +
                            (this.YMin.AlmostEqual(0.0f) ? (k *= k) : this.YMin.GetHashCode()) +
                            (this.YMax.AlmostEqual(0.0f) ? (k *= k) : this.YMax.GetHashCode());

                this.hashCode = base.GetHashCode() + (int)(hash * k);
            }
            return this.hashCode.Value;
        }

        /// <inheritdoc />
        public bool Equals(OnOffThresholdTuningFloatParameters other)
        {
            if (other == null)
                return false;

            return this.XMin.AlmostEqual(other.XMin) &&
                   this.XMax.AlmostEqual(other.XMax) &&
                   this.Threshold.AlmostEqual(other.Threshold) &&
                   this.E.AlmostEqual(other.E) &&
                   this.YMin.AlmostEqual(other.YMin) &&
                   this.YMax.AlmostEqual(other.YMax);
        }

        /// <inheritdoc />
        public override int CompareTo(object obj)
        {
            OnOffThresholdTuningFloatParameters that = obj as OnOffThresholdTuningFloatParameters;
            int cmp = base.CompareTo(obj);
            if (cmp == 0)
                cmp = this.CompareTo(that);
            return cmp;
        }

        /// <inheritdoc />
        public int CompareTo(OnOffThresholdTuningFloatParameters other)
        {
            if (other == null)
                return int.MaxValue;

            // Xmin
            int cmp = this.XMin.CompareTo(other.XMin);
            if (cmp == 0)
            {
                // XMax
                cmp = this.XMax.CompareTo(other.XMax);
                if (cmp == 0)
                {
                    // Threshold
                    cmp = this.Threshold.CompareTo(other.Threshold);
                    if (cmp == 0)
                    {
                        // E
                        cmp = this.E.CompareTo(other.E);
                        if (cmp == 0)
                        {
                            // YMin
                            cmp = this.YMin.CompareTo(other.YMin);
                            if (cmp == 0)
                            {
                                // YMax
                                cmp = this.YMax.CompareTo(other.YMax);
                            }
                        }
                    }
                }
            }
            return cmp;
        }
    }
}