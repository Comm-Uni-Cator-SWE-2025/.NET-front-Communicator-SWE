// MessageColorConverter.cs

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace chat_front
{
    public class MessageColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                // Sent by Me (Light Blue: #E1F5FE)
                return new SolidColorBrush(Color.FromRgb(0xE1, 0xF5, 0xFE));
            }
            else
            {
                // Received (Light Gray: #F1F1F1)
                return new SolidColorBrush(Color.FromRgb(0xF1, 0xF1, 0xF1));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}