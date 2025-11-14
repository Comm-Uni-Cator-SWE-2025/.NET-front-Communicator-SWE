// UsernameColorConverter.cs

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace chat_front
{
    public class UsernameColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                // Sent by Me (Darker Blue: #005A9E)
                return new SolidColorBrush(Color.FromRgb(0x00, 0x5A, 0x9E));
            }
            else
            {
                // Received (Standard Blue: #007BFF)
                return new SolidColorBrush(Color.FromRgb(0x00, 0x7B, 0xFF));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}