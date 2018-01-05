using System;

namespace Aleab.LoopbackAudioVisualizer.LightTuning
{
    public class InternalTuningParametersChangedEventArgs : EventArgs
    {
        public TuningParameters TuningParameters { get; }

        public InternalTuningParametersChangedEventArgs(TuningParameters tuningParameters)
        {
            this.TuningParameters = tuningParameters;
        }
    }
}