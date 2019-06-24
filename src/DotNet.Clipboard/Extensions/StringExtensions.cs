namespace DotNet.Clipboard.Extensions
{
    using System;

    /// <summary>
    /// This represents the extension entity for the <see cref="System.String" /> class.
    /// </summary>
    public static class StringExtensions
    {
        public static bool Contains(this string source, string target, StringComparison comp)
        {
            return source != null && target != null && source.IndexOf(target, comp) >= 0;
        }
    }
}
