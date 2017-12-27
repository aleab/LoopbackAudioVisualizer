using System;
using System.Collections.Generic;

namespace Aleab.LoopbackAudioVisualizer.Helpers
{
    public class AnonymousComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> comparer;

        public AnonymousComparer(Func<T, T, bool> comparer)
        {
            this.comparer = comparer;
        }

        /// <inheritdoc />
        public bool Equals(T x, T y)
        {
            return this.comparer(x, y);
        }

        /// <inheritdoc />
        public int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }
    }
}