namespace DotNet.Clipboard.Infrastructure.Native
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Contains information about a system appbar message.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct APPBARDATA
    {
        /// <summary>
        /// The size of the structure, in bytes.
        /// </summary>
        public int cbSize;

        /// <summary>
        /// The handle to the appbar window. Not all messages use this member. See the individual message page to see if you need to provide an hWind value.
        /// </summary>
        public IntPtr hWnd;

        /// <summary>
        /// An application-defined message identifier.
        /// </summary>
        public int uCallbackMessage;

        /// <summary>
        /// A value that specifies an edge of the screen.
        /// </summary>
        public int uEdge;

        /// <summary>
        /// A RECT structure whose use varies depending on the message.
        /// </summary>
        public RECT rc;

        /// <summary>
        /// A message-dependent value. This member is used with these messages.
        /// </summary>
        public IntPtr lParam;
    }
}
