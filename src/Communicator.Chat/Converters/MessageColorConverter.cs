// MessageColorConverter.cs

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Communicator.Chat
{
    /// <summary>
    /// Converts IsSentByMe boolean to message background color using theme colors.
    /// </summary>
    public class MessageColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSentByMe)
            {
                // Use theme colors from DynamicResource
                if (isSentByMe)
                {
                    // Sent by me - use primary color
                    return Application.Current?.TryFindResource("PrimaryBrush") 
                           ?? new SolidColorBrush(Color.FromRgb(0x60, 0xA5, 0xFA));
                }
                else
                {
                    // Received - use card background
                    return Application.Current?.TryFindResource("CardBackgroundBrush") 
                           ?? new SolidColorBrush(Color.FromRgb(0x1E, 0x29, 0x3B));
                }
            }
            
            return Application.Current?.TryFindResource("CardBackgroundBrush") 
                   ?? new SolidColorBrush(Color.FromRgb(0x1E, 0x29, 0x3B));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}