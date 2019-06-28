namespace DotNet.Clipboard.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Controls;
    using System.Windows.Data;

    /// <summary>
    /// Represents a converter that converts a list-box item to it's corresponding index representation.
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class ListBoxIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var item = (ListBoxItem)value;
            var listBox = (ListBox)ItemsControl.ItemsControlFromItemContainer(item);
            var index = (listBox.ItemContainerGenerator.IndexFromContainer(item) + 10) % 10;

            return index.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
