using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace LogisticsPro.UI.Converters
{
    public class EqualityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value?.ToString() == parameter?.ToString();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is bool boolValue && boolValue ? parameter : null;
        }
    }
}