namespace DotNet.Clipboard.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// Converts the clip data to it's corresponding content control, which will be used to display the data.
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class ClipDataToContentControlConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue)
                return null;

            var data = values[0];
            var format = (string)values[1];

            if (data == null || string.IsNullOrEmpty(format))
                return null;

            if (format == DataFormats.Bitmap)
            {
                var image = new Image
                {
                    Margin = new Thickness(2),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Source = (BitmapSource)data
                };

                var text = new TextBlock { Text = DataFormats.Bitmap };
                var dockPanel = new DockPanel();

                dockPanel.Children.Add(image);
                dockPanel.Children.Add(text);

                return dockPanel;
            }

            return data;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
