namespace DotNet.Clipboard.Infrastructure.Native
{
    using System.Runtime.InteropServices;

    /// <summary>
    /// The MONITORINFOE structure contains information about a display monitor.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class MONITORINFO
    {
        /// <summary>
        /// The size of the structure, in bytes.
        /// </summary>
        public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));

        /// <summary>
        /// A RECT structure that specifies the display monitor rectangle, expressed in virtual-screen coordinates.
        /// </summary>
        public RECT rcMonitor = new RECT();

        /// <summary>
        /// A RECT structure that specifies the work area rectangle of the display monitor, expressed in virtual-screen coordinates.
        /// </summary>
        public RECT rcWork = new RECT();

        /// <summary>
        /// A set of flags that represent attributes of the display monitor.
        /// </summary>
        public int dwFlags = 0;

        /// <summary>
        /// The name of the device.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public char[] szDevice = new char[32];
    }
}
