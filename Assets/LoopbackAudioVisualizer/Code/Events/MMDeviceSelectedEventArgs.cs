using CSCore.CoreAudioAPI;
using System;

namespace Aleab.LoopbackAudioVisualizer.Events
{
    public class MMDeviceSelectedEventArgs : EventArgs
    {
        public MMDevice Device { get; }

        public MMDeviceSelectedEventArgs(MMDevice device)
        {
            this.Device = device;
        }
    }
}