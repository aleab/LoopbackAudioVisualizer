using Aleab.LoopbackAudioVisualizer.Common;
using System;
using System.Runtime.InteropServices;

namespace Aleab.LoopbackAudioVisualizer.WinAPI
{
    public sealed class Taskbar
    {
        private const string ClassName = "Shell_TrayWnd";

        public Rectangle Bounds { get; private set; }

        public TaskbarPosition Position { get; private set; }

        public Point Location { get { return this.Bounds.Location; } }

        public Size Size { get { return this.Bounds.Size; } }
        
        public bool AlwaysOnTop { get; private set; }

        public bool AutoHide { get; private set; }

        public Taskbar()
        {
            IntPtr taskbarHandle = User32.FindWindow(ClassName, null);

            APPBARDATA data = new APPBARDATA
            {
                cbSize = (uint)Marshal.SizeOf(typeof(APPBARDATA)),
                hWnd = taskbarHandle
            };
            IntPtr result = Shell32.SHAppBarMessage(AppBarMessage.GetTaskbarPos, ref data);
            if (result == IntPtr.Zero)
                throw new InvalidOperationException();

            this.Position = (TaskbarPosition)data.uEdge;
            this.Bounds = Rectangle.FromLTRB(data.rc.left, data.rc.top, data.rc.right, data.rc.bottom);

            data.cbSize = (uint)Marshal.SizeOf(typeof(APPBARDATA));
            result = Shell32.SHAppBarMessage(AppBarMessage.GetState, ref data);
            int state = result.ToInt32();
            this.AlwaysOnTop = (state & AppBarState.AlwaysOnTop) == AppBarState.AlwaysOnTop;
            this.AutoHide = (state & AppBarState.Autohide) == AppBarState.Autohide;
        }
    }
}