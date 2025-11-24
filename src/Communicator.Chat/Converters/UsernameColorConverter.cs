// UsernameColorConverter.cs

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Communicator.Chat
{
    /// <summary>
    /// Converts IsSentByMe boolean to username text color using theme colors.
    /// </summary>
    public class UsernameColorConverter : IValueConverter
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
                    // Received - use secondary text color
                    return Application.Current?.TryFindResource("TextSecondaryBrush") 
                           ?? new SolidColorBrush(Color.FromRgb(0xCB, 0xD5, 0xE1));
                }
            }
            
            return Application.Current?.TryFindResource("TextPrimaryBrush") 
                   ?? new SolidColorBrush(Color.FromRgb(0xF1, 0xF5, 0xF9));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}