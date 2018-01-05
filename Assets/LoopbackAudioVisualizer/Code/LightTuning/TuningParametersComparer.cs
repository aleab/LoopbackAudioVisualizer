using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;

namespace Aleab.LoopbackAudioVisualizer.LightTuning
{
    public class TuningParametersComparer : IComparer<TuningParameters>, IComparer, IEqualityComparer<TuningParameters>, IEqualityComparer
    {
        /// <inheritdoc />
        public int Compare(TuningParameters x, TuningParameters y)
        {
            return (x?.CompareTo(y) ?? 0) - (y?.CompareTo(x) ?? 0);
        }

        /// <inheritdoc />
        public int Compare(object x, object y)
        {
            TuningParameters xx = x as TuningParameters;
            TuningParameters yy = y as TuningParameters;
            return this.Compare(xx, yy);
        }

        /// <inheritdoc />
        public bool Equals(TuningParameters x, TuningParameters y)
        {
            return (x?.Equals(y) ?? y == null) &&
                   (y?.Equals(x) ?? x == null);
        }

        /// <inheritdoc />
        public int GetHashCode([CanBeNull] TuningParameters obj)
        {
            return obj?.GetHashCode() ?? 0;
        }

        /// <inheritdoc />
        public new bool Equals(object x, object y)
        {
            TuningParameters xx = x as TuningParameters;
            TuningParameters yy = y as TuningParameters;
            return this.Equals(xx, yy);
        }

        /// <inheritdoc />
        public int GetHashCode(object obj)
        {
            TuningParameters objTuningParameters = obj as TuningParameters;
            return this.GetHashCode(objTuningParameters);
        }
    }
}