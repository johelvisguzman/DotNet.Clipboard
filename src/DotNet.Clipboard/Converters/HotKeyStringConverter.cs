namespace DotNet.Clipboard.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    /// <summary>
    /// Represents a hotkey string converter.
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class HotKeyStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var hotkeyString = value as string;
            if (hotkeyString == null)
                return null;

            var hotkey = Infrastructure.Utils.ConvertStringToHotKey(hotkeyString);
            if (hotkey == null)
                return null;

            return new MahApps.Metro.Controls.HotKey(hotkey.Item1, hotkey.Item2);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var hotkey = value as MahApps.Metro.Controls.HotKey;
            if (hotkey == null)
                return string.Empty;

            return Infrastructure.Utils.ConvertHotKeyToString(hotkey.Key, hotkey.ModifierKeys);
        }
    }
}
