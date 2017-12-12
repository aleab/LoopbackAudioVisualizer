using System;
using System.Runtime.InteropServices;

namespace Aleab.LoopbackAudioVisualizer.WinAPI
{
    public static class Shell32
    {
        [DllImport("shell32.dll", SetLastError = true)]
        public static extern IntPtr SHAppBarMessage(AppBarMessage dwMessage, [In] ref APPBARDATA pData);
    }
}