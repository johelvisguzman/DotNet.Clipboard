namespace DotNet.Clipboard.Services
{
    using Infrastructure.Native;
    using System;
    using System.Windows.Forms;
    using System.Windows.Interop;

    /// <summary>
    /// Represents an object which can monitor any clipboard activity.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class ClipboardMonitor : IDisposable
    {
        #region Fields

        private readonly HwndSource _hwndSource = new HwndSource(0, 0, 0, 0, 0, 0, 0, null, new IntPtr(-3));
        private bool _isMonitoring;
        private bool _disposed;

        #endregion

        #region Events

        public event EventHandler ClipboardContentChanged;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ClipboardMonitor"/> class.
        /// </summary>
        public ClipboardMonitor()
        {
            _hwndSource.AddHook(WndProc);

            Start();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ClipboardMonitor"/> class.
        /// </summary>
        ~ClipboardMonitor()
        {
            ReleaseResources();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive
        {
            get { return _isMonitoring; }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Handles wind32 interop messages.
        /// </summary>
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == (int)NativeMethods.WM.CLIPBOARDUPDATE)
            {
                ClipboardContentChanged?.Invoke(this, EventArgs.Empty);
                handled = true;
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// Releases the resources.
        /// </summary>
        private void ReleaseResources()
        {
            if (_disposed)
                return;

            Stop();

            _hwndSource.RemoveHook(WndProc);
            _hwndSource.Dispose();
            _disposed = true;
        }

        /// <summary>
        /// Throws if this class has been disposed.
        /// </summary>
        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Starts monitoring the clipboard.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">If the class has been disposed.</exception>
        public void Start()
        {
            ThrowIfDisposed();

            if (_isMonitoring)
                return;

            NativeMethods.AddClipboardFormatListener(_hwndSource.Handle);

            _isMonitoring = true;
        }

        /// <summary>
        /// Stops monitoring the clipboard.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">If the class has been disposed.</exception>
        public void Stop()
        {
            ThrowIfDisposed();

            if (!_isMonitoring)
                return;

            NativeMethods.RemoveClipboardFormatListener(_hwndSource.Handle);

            _isMonitoring = false;
        }

        /// <summary>
        /// Gets the current data in the clipboard.
        /// </summary>
        public bool TryGetData(out object data, out string format)
        {
            var iData = System.Windows.Clipboard.GetDataObject();

            data = null;
            format = null;

            if (iData == null)
                return false;

            // To get around a 'OpenClipboard Failed (Exception from HRESULT: 0x800401D0 (CLIPBRD_E_CANT_OPEN))' exception,
            // we need to keep trying to get the data until there is no longer an exception
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    if (iData.GetDataPresent(DataFormats.Text))
                    {
                        data = iData.GetData(DataFormats.Text);
                        format = DataFormats.Text;
                        return true;
                    }
                    else if (iData.GetDataPresent(DataFormats.Bitmap))
                    {
                        data = iData.GetData(DataFormats.Bitmap);
                        format = DataFormats.Bitmap;
                        return true;
                    }
                }
                catch (System.Runtime.InteropServices.COMException)
                {
                    System.Threading.Thread.Sleep(10);
                }
            }

            return false;
        }

        #endregion

        #region Implementation of IDisposible

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            ReleaseResources();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
