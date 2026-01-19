using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows.Data;

namespace MusicSheetManager.Converters
{
    public class EnumDisplayNameConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is null)
            {
                return string.Empty;
            }

            var type = value.GetType();

            if (!type.IsEnum)
            {
                return value.ToString();
            }

            var name = Enum.GetName(type, value);

            if (name is null)
            {
                return value.ToString();
            }

            var field = type.GetField(name);

            var display = field?.GetCustomAttributes(typeof(DisplayAttribute), false)
               .Cast<DisplayAttribute>()
               .FirstOrDefault();

            return display?.Name ?? name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}