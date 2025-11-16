// Converters.cs - Additional Converters for File Support
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace chat_front
{
    /// <summary>
    /// Converts file size in bytes to human-readable format
    /// </summary>
    public class FileSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is long bytes)
            {
                if (bytes < 1024)
                    return $"{bytes} B";

                int exp = (int)(Math.Log(bytes) / Math.Log(1024));
                string[] units = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };

                return string.Format("{0:F1} {1}",
                    bytes / Math.Pow(1024, exp),
                    units[exp]);
            }
            return "0 B";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts boolean to Visibility (inverted)
    /// </summary>
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts null or empty string to Visibility
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;

            if (value is string str && string.IsNullOrWhiteSpace(str))
                return Visibility.Collapsed;

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Selects the appropriate DataTemplate based on whether the message is a file or text
    /// </summary>
    public class MessageTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is MessageVM message)
            {
                var element = container as FrameworkElement;
                if (element != null)
                {
                    if (message.IsFileMessage)
                    {
                        return element.FindResource("FileMessageTemplate") as DataTemplate;
                    }
                    else
                    {
                        return element.FindResource("TextMessageTemplate") as DataTemplate;
                    }
                }
            }
            return null;
        }
    }
}