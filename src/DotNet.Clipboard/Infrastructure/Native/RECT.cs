namespace DotNet.Clipboard.Infrastructure.Native
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// The RECT structure defines the coordinates of the upper-left and lower-right corners of a rectangle.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        /// <summary>
        /// The x-coordinate of the upper-left corner of the rectangle.
        /// </summary>
        public int Left;

        /// <summary>
        /// The y-coordinate of the upper-left corner of the rectangle.
        /// </summary>
        public int Top;

        /// <summary>
        /// The x-coordinate of the lower-right corner of the rectangle.
        /// </summary>
        public int Right;

        /// <summary>
        /// The y-coordinate of the lower-right corner of the rectangle.
        /// </summary>
        public int Bottom;

        /// <summary>
        /// Gets the width of the rectangle.
        /// </summary>
        public int Width => Math.Abs(Right - Left);

        /// <summary>
        /// Gets the height of the rectangle.
        /// </summary>
        public int Height => Bottom - Top;
    }
}
