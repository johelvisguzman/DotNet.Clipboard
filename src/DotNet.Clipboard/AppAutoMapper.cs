namespace DotNet.Clipboard
{
    using Infrastructure;
    using Models;
    using System;
    using System.Text;
    using System.Windows;
    using System.Windows.Media;
    using ViewModels;

    /// <summary>
    /// Represents a simple auto mapper for the application.
    /// </summary>
    public class AppAutoMapper
    {
        public static ClipViewModel Map(Clip source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            object data;

            if (source.Format == DataFormats.Bitmap)
                data = Utils.ByteArrayToImageSource(source.Data);
            else if (source.Format == DataFormats.Text)
                data = Encoding.UTF8.GetString(source.Data);
            else
                data = Utils.ByteArrayToObject(source.Data);

            var target = new ClipViewModel(data, source.Format);

            target.Id = source.Id;
            target.AddedDate = source.AddedDate;
            target.LastUsedDate = source.LastUsedDate;

            return target;
        }

        public static Clip Map(ClipViewModel source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var target = new Clip();

            Map(source, target);

            return target;
        }

        public static void Map(ClipViewModel source, Clip target)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (target == null)
                throw new ArgumentNullException(nameof(target));

            byte[] data;

            if (source.Format == DataFormats.Bitmap)
                data = Utils.ImageSourceToByteArray((ImageSource)source.Data);
            else if (source.Format == DataFormats.Text)
                data = Encoding.UTF8.GetBytes((string)source.Data);
            else
                data = Utils.ObjectToByteArray(source.Data);

            target.Id = source.Id;
            target.Data = data;
            target.Format = source.Format;
            target.AddedDate = source.AddedDate;
            target.LastUsedDate = source.LastUsedDate;
        }
    }
}
