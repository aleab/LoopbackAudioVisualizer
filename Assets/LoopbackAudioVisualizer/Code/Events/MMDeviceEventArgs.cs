using CSCore.CoreAudioAPI;
using System;

namespace Aleab.LoopbackAudioVisualizer.Events
{
    public class MMDeviceEventArgs : EventArgs
    {
        public MMDevice Device { get; }

        public MMDeviceEventArgs(MMDevice device)
        {
            this.Device = device;
        }
    }
}