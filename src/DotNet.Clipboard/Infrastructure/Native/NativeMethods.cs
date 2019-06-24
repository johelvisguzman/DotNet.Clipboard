namespace DotNet.Clipboard.Infrastructure.Native
{
    using System;
    using System.Runtime.InteropServices;

    internal static class NativeMethods
    {
        /// <summary>
        /// Represents Window message values.
        /// </summary>
        internal enum WM
        {
            CLIPBOARDUPDATE = 0x031D,
        }

        /// <summary>
        /// Represents MonitorFromWindow options.
        /// </summary>
        internal enum MFW
        {
            MONITOR_DEFAULTTOPRIMERTY = 0x00000001,
            MONITOR_DEFAULTTONEAREST = 0x00000002,
        }

        /// <summary>
        /// Represents SHAppBarMessage options.
        /// </summary>
        internal enum ABM
        {
            GETTASKBARPOS = 0x00000005,
        }

        /// <summary>
        /// Places the given window in the system-maintained clipboard format listener list.
        /// </summary>
        /// <devdoc>https://msdn.microsoft.com/en-us/library/windows/desktop/ms649033(v=vs.85).aspx</devdoc>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AddClipboardFormatListener(IntPtr windowHandle);

        /// <summary>
        /// Removes the given window from the system-maintained clipboard format listener list.
        /// </summary>
        /// <devdoc>https://msdn.microsoft.com/en-us/library/windows/desktop/ms649050(v=vs.85).aspx</devdoc>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RemoveClipboardFormatListener(IntPtr windowHandle);

        /// <summary>
        /// Retrieves the position of the mouse cursor, in screen coordinates.
        /// </summary>
        /// <devdoc>https://msdn.microsoft.com/en-us/library/windows/desktop/ms648390(v=vs.85).aspx</devdoc>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(out POINT lpPoint);

        /// <summary>
        /// The MonitorFromPoint function retrieves a handle to the display monitor that contains a specified point.
        /// </summary>
        /// <devdoc>https://msdn.microsoft.com/en-us/library/windows/desktop/dd145062(v=vs.85).aspx</devdoc>
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr MonitorFromPoint(POINT pt, int flags);

        /// <summary>
        /// The GetMonitorInfo function retrieves information about a display monitor.
        /// </summary>
        /// <devdoc>http://msdn.microsoft.com/en-us/library/dd145064%28v=VS.85%29.aspx</devdoc>
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetMonitorInfo(IntPtr hMonitor, [In, Out]MONITORINFO lpmi);

        /// <summary>
        /// Sends an appbar message to the system.
        /// </summary>
        /// <devdoc>https://msdn.microsoft.com/en-us/library/windows/desktop/bb762108(v=vs.85).aspx</devdoc>
        [DllImport("SHELL32", CallingConvention = CallingConvention.StdCall)]
        public static extern uint SHAppBarMessage(int dwMessage, ref APPBARDATA pData);
    }
}
