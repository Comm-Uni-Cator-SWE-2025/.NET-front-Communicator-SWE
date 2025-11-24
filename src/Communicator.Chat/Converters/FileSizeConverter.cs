using System;
using System.Globalization;
using System.Windows.Data;

namespace Communicator.Chat
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
}
