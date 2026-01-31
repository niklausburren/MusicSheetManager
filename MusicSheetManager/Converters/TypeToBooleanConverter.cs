using System;
using System.Globalization;
using System.Windows.Data;

namespace MusicSheetManager.Converters
{
    public class TypeToBooleanConverter : IValueConverter
    {
        #region Properties

        public static TypeToBooleanConverter Instance { get; } = new();

        #endregion


        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
            {
                return false;
            }

            var valueType = value.GetType();
            var expectedType = parameter as Type;

            return expectedType != null && expectedType.IsAssignableFrom(valueType);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
