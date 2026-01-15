using System;
using System.Globalization;
using System.Windows.Data;
using MusicSheetManager.Models;

namespace MusicSheetManager.Converters;

public class AddToPlaylistParameterConverter : IMultiValueConverter
{
    #region IMultiValueConverter Members

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length == 2 && values[0] is Playlist playlist && values[1] is MusicSheetFolder folder)
        {
            return (playlist, folder);
        }

        return null;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    #endregion
}
