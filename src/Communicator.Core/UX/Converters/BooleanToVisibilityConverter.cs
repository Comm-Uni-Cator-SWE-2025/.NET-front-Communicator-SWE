using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Communicator.Core.UX.Converters;

/// <summary>
/// Converts boolean values to Visibility for binding scenarios, with optional inversion via converter parameter.
/// </summary>
public class BooleanToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Returns Visibility.Visible when the input evaluates to true; otherwise Visibility.Collapsed.
    /// A non-null parameter toggles the boolean before conversion so consumers can invert the result without additional converters.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool boolValue = (bool)value;
        if (parameter != null)
        {
            boolValue = !boolValue;
        }
        return boolValue ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    /// Conversion back is not supported because collapsing state does not map to a single boolean value.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
