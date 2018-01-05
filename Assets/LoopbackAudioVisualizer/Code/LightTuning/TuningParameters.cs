using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.LightTuning
{
    /// <summary>
    /// Parameters used to build an appropriate <see cref="LightSetTuning{TIn,TTarget}"/>.
    /// </summary>
    [SuppressMessage("ReSharper", "ConvertToConstant.Local")]
    [Serializable]
    public class TuningParameters : IEquatable<TuningParameters>, IComparable, IComparable<TuningParameters>
    {
        /// <summary>
        /// The hash code of this object when it was first calculated.
        /// </summary>
        /// <remarks>
        /// This class has no readonly field that can be used for hash code calculations. This problem causes objects inserted in a hash-based collection
        /// to be un-retrievable if the their state changes after having been inserted in the collection.
        /// The solution is to calculate the hash code only once, that is when the object is added to the collection.
        /// </remarks>
        protected int? hashCode;

        #region Serialized Fields

        // ReSharper disable FieldCanBeMadeReadOnly.Local

        [SerializeField]
        private string tuningName = string.Empty;

        [SerializeField]
        private TuningTarget tuningTarget = TuningTarget.Intensity;

        [SerializeField]
        private TuningType tuningType = TuningType.OnOffThreshold;

        [SerializeField]
        private InternalTuningParameters internalTuningParameters;

        // ReSharper restore FieldCanBeMadeReadOnly.Local

        #endregion Serialized Fields

        public event EventHandler<InternalTuningParametersChangedEventArgs> InternalTuningParametersChanged;

        public string TuningName { get { return this.tuningName; } }

        public TuningTarget TuningTarget { get { return this.tuningTarget; } }

        public TuningType TuningType { get { return this.tuningType; } }

        public InternalTuningParameters InternalTuningParameters
        {
            get { return this.internalTuningParameters; }
            set { this.internalTuningParameters = value; }
        }

        public void ListenForInternalTuningParametersChanges()
        {
            if (this.InternalTuningParameters != null)
            {
                this.InternalTuningParameters.ParameterChanged -= this.InternalTuningParameters_ParameterChanged;
                this.InternalTuningParameters.ParameterChanged += this.InternalTuningParameters_ParameterChanged;
            }
        }

        public void StopListenForInternalTuningParametersChanges()
        {
            if (this.InternalTuningParameters != null)
                this.InternalTuningParameters.ParameterChanged -= this.InternalTuningParameters_ParameterChanged;
        }

        private void InternalTuningParameters_ParameterChanged(object sender, EventArgs e)
        {
            this.InternalTuningParametersChanged?.Invoke(sender, new InternalTuningParametersChangedEventArgs(this));
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            TuningParameters that = obj as TuningParameters;
            return this.Equals(that);
        }

        /// <inheritdoc />
        /// <remarks>
        /// Derived classes are required to use the following template when overriding this method.
        /// <code>
        /// public override int GetHashCode()
        /// {
        ///     if (!this.hashCode.HasValue)
        ///     {
        ///         int hash;  // Calculated hash code
        ///         this.hashCode = base.GetHashCode() + hash;
        ///     }
        ///     return this.hashCode.Value;
        /// }
        /// </code>
        /// </remarks>
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            if (!this.hashCode.HasValue)
            {
                long k = 347;
                long hash = (this.tuningTarget.GetHashCode() == 0 ? (k *= k) : this.tuningTarget.GetHashCode()) +
                            (this.tuningType.GetHashCode() == 0 ? (k *= k) : this.tuningType.GetHashCode()) +
                            (this.internalTuningParameters?.GetHashCode() ?? (k *= k));
                this.hashCode = (int)(hash * k);
            }
            return this.hashCode.Value;
        }

        /// <inheritdoc />
        public bool Equals(TuningParameters other)
        {
            if (other == null)
                return false;

            return this.tuningTarget == other.tuningTarget &&
                   this.tuningType == other.tuningType &&
                   (this.internalTuningParameters?.Equals(other.internalTuningParameters) ?? other.internalTuningParameters == null);
        }

        /// <inheritdoc />
        public int CompareTo(object obj)
        {
            TuningParameters that = obj as TuningParameters;
            return this.CompareTo(that);
        }

        /// <inheritdoc />
        public int CompareTo(TuningParameters other)
        {
            if (other == null)
                return int.MaxValue;

            if (this.tuningTarget == other.tuningTarget)
            {
                if (this.tuningType == other.tuningType)
                    return this.internalTuningParameters.CompareTo(other.internalTuningParameters);
                return (int)this.tuningType - (int)other.tuningType;
            }
            return (int)this.tuningTarget - (int)other.tuningTarget;
        }
    }
}