using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Aleab.LoopbackAudioVisualizer.LightTuning
{
    /// <remarks>
    /// Derived classes should override the default <see cref="SerializeParameters"/> and <see cref="DeserializeParameters"/> methods
    /// to handle the serialization of the internal parameters; specifically, derived classes must use the <see cref="SetParameterValue{T}"/>
    /// and <see cref="GetParameterValue{T}"/> methods to get/add a parameter from/to the collection that is going to be serialized by Unity.
    /// </remarks>
    [Serializable]
    public class InternalTuningParameters : IEquatable<InternalTuningParameters>, IComparable<InternalTuningParameters>, IComparable
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

        #region Input Value Provider Method

        // ReSharper disable FieldCanBeMadeReadOnly.Local

        [SerializeField]
        private Object ipComponent;

        [SerializeField]
        private string ipName;

        [SerializeField]
        private string ipReturnType;

        // ReSharper restore FieldCanBeMadeReadOnly.Local

        #endregion Input Value Provider Method

        [SerializeField]
        [HideInInspector]
        protected List<string> parametersNames = new List<string>();

        [SerializeField]
        [HideInInspector]
        protected List<string> parametersValues = new List<string>();

        #endregion Serialized Fields

        public event EventHandler ParameterChanged;

        public Object IPComponent { get { return this.ipComponent; } }

        public string IPName { get { return this.ipName; } }

        public string IPReturnType
        {
            get { return this.ipReturnType; }
            set { this.ipReturnType = value; }
        }

        public InternalTuningParameters()
        {
        }

        public InternalTuningParameters(InternalTuningParameters parameters)
        {
            this.parametersNames.Clear();
            this.parametersValues.Clear();
            this.parametersNames.AddRange(parameters.parametersNames);
            this.parametersValues.AddRange(parameters.parametersValues);

            this.ipComponent = parameters.ipComponent;
            this.ipName = parameters.ipName;
            this.ipReturnType = parameters.ipReturnType;
        }

        public virtual void ReplaceParameters(InternalTuningParameters parameters)
        {
            if (ReferenceEquals(parameters, this))
                return;

            this.parametersNames.Clear();
            this.parametersValues.Clear();
            this.parametersNames.AddRange(parameters.parametersNames);
            this.parametersValues.AddRange(parameters.parametersValues);

            this.ipComponent = parameters.ipComponent;
            this.ipName = parameters.ipName;
            this.ipReturnType = parameters.ipReturnType;
        }

        #region Parameters

        public void SetParameterValue<T>(string parameterName, T value) where T : IConvertible
        {
            string valueAsString = value.ToString(CultureInfo.InvariantCulture);
            int index = this.parametersNames.FindIndex(name => name == parameterName);
            if (index < 0)
            {
                this.parametersNames.Add(parameterName);
                this.parametersValues.Add(valueAsString);
            }
            else
                this.parametersValues[index] = valueAsString;

            this.ParameterChanged?.Invoke(this, EventArgs.Empty);
        }

        public T GetParameterValue<T>(string parameterName) where T : IConvertible
        {
            T value = default(T);
            int index = this.parametersNames.FindIndex(name => name == parameterName);
            if (index >= 0)
            {
                string valueAsString = this.parametersValues[index];
                value = (T)Convert.ChangeType(valueAsString, typeof(T), CultureInfo.InvariantCulture);
            }
            return value;
        }

        /// <summary>
        /// Clear the Unity-serializable parameters collection and populate it with the current values.
        /// </summary>
        public virtual void SerializeParameters()
        {
            this.parametersNames.Clear();
            this.parametersValues.Clear();
        }

        /// <summary>
        /// Fetch the serialized values from the Unity-serializable parameters collection.
        /// </summary>
        public virtual void DeserializeParameters()
        {
        }

        #endregion Parameters

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            InternalTuningParameters that = obj as InternalTuningParameters;
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
                long hash = (this.ipComponent?.GetHashCode() ?? (k *= k)) +
                            (this.ipName?.GetHashCode() ?? (k *= k)) +
                            (this.ipReturnType?.GetHashCode() ?? (k *= k)) +
                            (this.parametersNames?.Sum(s => s?.GetHashCode() ?? 0L) ?? (k *= k)) +
                            (this.parametersValues?.Sum(s => s?.GetHashCode() ?? 0L) ?? (k *= k));
                this.hashCode = (int)(hash * k);
            }
            return this.hashCode.Value;
        }

        /// <inheritdoc />
        public bool Equals(InternalTuningParameters other)
        {
            if (other == null)
                return false;

            return (this.ipComponent?.Equals(other.ipComponent) ?? other.ipComponent == null) &&
                   this.ipName == other.ipName &&
                   this.ipReturnType == other.ipReturnType &&
                   this.parametersNames?.Count == other.parametersNames?.Count && (this.parametersNames?.All(name => other.parametersNames?.Contains(name) ?? false) ?? other.parametersNames == null) &&
                   this.parametersValues?.Count == other.parametersValues?.Count && (this.parametersValues?.All(value => other.parametersValues?.Contains(value) ?? false) ?? other.parametersValues == null);
        }

        /// <inheritdoc />
        public int CompareTo(InternalTuningParameters other)
        {
            if (other == null)
                return int.MaxValue;

            // ipComponent
            int cmp = string.CompareOrdinal(this.ipComponent?.name, other.ipComponent?.name);
            if (cmp == 0)
            {
                // ipName
                cmp = string.CompareOrdinal(this.ipName, other.ipName);
                if (cmp == 0)
                {
                    // ipReturnType
                    cmp = string.CompareOrdinal(this.ipReturnType, other.ipReturnType);
                }
            }
            return cmp;
        }

        /// <inheritdoc />
        public virtual int CompareTo(object obj)
        {
            InternalTuningParameters that = obj as InternalTuningParameters;
            return this.CompareTo(that);
        }
    }
}