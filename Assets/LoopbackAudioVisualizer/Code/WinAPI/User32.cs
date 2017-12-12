using System;
using System.Runtime.InteropServices;

namespace Aleab.LoopbackAudioVisualizer.WinAPI
{
    public static class User32
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
    }
}