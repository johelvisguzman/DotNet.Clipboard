namespace DotNet.Clipboard.ViewModels
{
    using DotNetToolkit.Wpf.Mvvm;
    using System;
    using System.Windows;

    /// <summary>
    /// This class contains properties that the view can data bind to.
    /// </summary>
    /// <seealso cref="ViewModelBase" />
    public class ClipViewModel : ViewModelBase
    {
        #region Fields

        private int _id;
        private object _data;
        private string _format;
        private DateTime _addedDate;
        private DateTime _lastUsedDate;

        #endregion

        #region Constructors

        public ClipViewModel(string data) : this(data, DataFormats.Text) { }

        public ClipViewModel(object data, string format)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (format == null)
                throw new ArgumentNullException(nameof(format));

            Data = data;
            Format = format; // System.Windows.DataFormats

            var now = DateTime.Now;
            AddedDate = now;
            LastUsedDate = now;
        }

        #endregion

        #region Properties

        public int Id
        {
            get { return _id; }
            set { SetProperty(ref _id, value); }
        }

        public object Data
        {
            get { return _data; }
            private set { SetProperty(ref _data, value); }
        }

        public string Format
        {
            get { return _format; }
            private set { SetProperty(ref _format, value); }
        }

        public DateTime AddedDate
        {
            get { return _addedDate; }
            set { SetProperty(ref _addedDate, value); }
        }

        public DateTime LastUsedDate
        {
            get { return _lastUsedDate; }
            set { SetProperty(ref _lastUsedDate, value); }
        }

        #endregion
    }
}
