using System;
using System.Globalization;
using System.Windows.Data;

namespace MusicSheetManager.Converters
{
    public class TypeToBooleanConverter : IValueConverter
    {
        #region Properties

        public static TypeToBooleanConverter Instance { get; } = new TypeToBooleanConverter();

        #endregion


        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            return value.GetType() == (Type)parameter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
