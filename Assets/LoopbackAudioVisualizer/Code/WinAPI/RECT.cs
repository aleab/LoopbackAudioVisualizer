using System.Runtime.InteropServices;

namespace Aleab.LoopbackAudioVisualizer.WinAPI
{
    // ReSharper disable once InconsistentNaming
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }
}