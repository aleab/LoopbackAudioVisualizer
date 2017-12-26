using Aleab.LoopbackAudioVisualizer.Events;
using System;
using System.Collections.Generic;

namespace Aleab.LoopbackAudioVisualizer.Scripts.Visualizers
{
    /// <summary>
    /// Provides a version of the spectrum mapped to a reduced number of bands.
    /// </summary>
    public interface IReducedBandsSpectrumProvider
    {
        event EventHandler<BandValueCalculatedEventArgs> BandValueCalculated;

        /// <summary>
        /// The number of frequency bands.
        /// </summary>
        int NumberOfBands { get; }

        /// <summary>
        /// A buffer containing the calculated values of the reduced frequency bands.
        /// </summary>
        IReadOnlyCollection<float> BandsDataBuffer { get; }
    }
}