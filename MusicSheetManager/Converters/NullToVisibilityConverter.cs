using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MusicSheetManager.Converters
{
    public class NullToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is string text
                ? string.IsNullOrWhiteSpace(text) ? Visibility.Collapsed : Visibility.Visible
                : value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
