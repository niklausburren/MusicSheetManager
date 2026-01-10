using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace MusicSheetManager.Converters;

public class FirstLetterConverter : IValueConverter
{
    #region Fields

    private static readonly string[] _ignoredPrefixes = ["A ", "An ", "The "];

    #endregion


    #region Public Methods

    public static string GetSortKey(string? text)
    {
        var normalizedText = NormalizeText(text);
        
        if (string.IsNullOrEmpty(normalizedText) || !char.IsLetter(normalizedText[0]))
        {
            return $"#{normalizedText}"; // # am Anfang, dann Original für Sortierung innerhalb der Gruppe
        }

        return normalizedText;
    }

    #endregion


    #region Private Methods

    private static string NormalizeText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var trimmedText = text.Trim();
        var prefix = _ignoredPrefixes.FirstOrDefault(p => trimmedText.StartsWith(p, StringComparison.OrdinalIgnoreCase));

        return prefix != null 
            ? trimmedText[prefix.Length..].Trim() 
            : trimmedText;
    }

    #endregion


    #region IValueConverter Members

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var normalizedText = NormalizeText(value as string);
        
        return string.IsNullOrEmpty(normalizedText) || !char.IsLetter(normalizedText[0])
            ? "#"
            : char.ToUpper(normalizedText[0]).ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    #endregion
}