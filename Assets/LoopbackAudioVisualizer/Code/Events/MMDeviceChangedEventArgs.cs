using CSCore.CoreAudioAPI;

namespace Aleab.LoopbackAudioVisualizer.Events
{
    public class MMDeviceChangedEventArgs : MMDeviceEventArgs
    {
        public bool Initialized { get; }

        public MMDeviceChangedEventArgs(MMDevice device, bool initialized) : base(device)
        {
            this.Initialized = initialized;
        }
    }
}