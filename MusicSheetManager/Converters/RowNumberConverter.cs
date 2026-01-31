using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace MusicSheetManager.Converters
{
    public class RowNumberConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ListViewItem item && ItemsControl.ItemsControlFromItemContainer(item) is ListView listView)
            {
                var index = listView.ItemContainerGenerator.IndexFromContainer(item);
                return (index + 1).ToString();
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
