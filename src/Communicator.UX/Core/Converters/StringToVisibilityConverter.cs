/*
 * -----------------------------------------------------------------------------
 *  File: StringToVisibilityConverter.cs
 *  Owner: Dhruvadeep
 *  Roll Number : 142201026
 *  Module : UX
 *
 * -----------------------------------------------------------------------------
 */
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Communicator.UX.Core.Converters;

/// <summary>
/// Converts a string to Visibility. Returns Visible if string is not null or empty, otherwise Collapsed.
/// </summary>
public class StringToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Returns Visibility.Visible when the string is not null or empty; otherwise Visibility.Collapsed.
    /// A non-null parameter inverts the result.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool hasValue = !string.IsNullOrEmpty(value as string);

        if (parameter != null)
        {
            hasValue = !hasValue;
        }

        return hasValue ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    /// Conversion back is not supported.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

