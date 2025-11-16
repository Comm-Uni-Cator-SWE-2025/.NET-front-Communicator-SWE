using System;
using System.Globalization;
using System.Windows.Data;

namespace Communicator.Core.UX.Converters;

/// <summary>
/// Inverts a boolean value for binding scenarios.
/// </summary>
public class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool boolValue && !boolValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool boolValue && !boolValue;
    }
}
