namespace DotNet.Clipboard.Services
{
    /// <summary>
    /// This interface is implemented by services that may need to store and load application based settings.
    /// </summary>
    public interface IAppSettingsService
    {
        /// <summary>
        /// Gets or sets a value indicating whether the clipboard is monitoring.
        /// </summary>
        /// <value>
        /// <c>true</c> if the clipboard is monitoring; otherwise, <c>false</c>.
        /// </value>
        bool IsClipboardMonitoring { get; set; }

        /// <summary>
        /// Gets or sets the hot key.
        /// </summary>
        /// <value>
        /// The hot key.
        /// </value>
        string HotKey { get; set; }

        /// <summary>
        /// Gets or sets the maximum saved copies count.
        /// </summary>
        /// <value>
        /// The maximum saved copies count.
        /// </value>
        int MaxSavedCopiesCount { get; set; }

        /// <summary>
        /// Gets or sets the culture.
        /// </summary>
        /// <value>
        /// The culture.
        /// </value>
        string Culture { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance has been modified.
        /// </summary>
        /// <value><c>true</c> if this instance has been modified; otherwise, <c>false</c>.</value>
        bool IsDirty { get; }

        /// <summary>
        /// Stores the current values of the application settings properties if this instance is <see cref="IsDirty"/>.
        /// </summary>
        void Save();
    }
}
