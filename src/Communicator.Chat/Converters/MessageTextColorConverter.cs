// MessageTextColorConverter.cs

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Communicator.Chat
{
    /// <summary>
    /// Converts IsSentByMe boolean to message text color.
    /// Uses white text for sent messages (on primary background) and primary text for received messages.
    /// </summary>
    public class MessageTextColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSentByMe)
            {
                if (isSentByMe)
                {
                    // Sent by me - use white/light text on primary background
                    return Application.Current?.TryFindResource("TextOnPrimaryBrush") 
                           ?? new SolidColorBrush(Colors.White);
                }
                else
                {
                    // Received - use primary text color
                    return Application.Current?.TryFindResource("TextPrimaryBrush") 
                           ?? new SolidColorBrush(Color.FromRgb(0xF1, 0xF5, 0xF9));
                }
            }
            
            return Application.Current?.TryFindResource("TextPrimaryBrush") 
                   ?? new SolidColorBrush(Color.FromRgb(0xF1, 0xF5, 0xF9));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) 
            => throw new NotImplementedException();
    }
}
