using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace JapaneseTracker.Converters
{
    public class JLPTLevelToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string jlptLevel)
            {
                var colorString = jlptLevel switch
                {
                    "N5" => "#4CAF50", // Green - Beginner
                    "N4" => "#8BC34A", // Light Green
                    "N3" => "#FFC107", // Amber
                    "N2" => "#FF9800", // Orange
                    "N1" => "#F44336", // Red - Advanced
                    _ => "#9E9E9E"     // Default Gray
                };

                try
                {
                    var color = (Color)ColorConverter.ConvertFromString(colorString);
                    return new SolidColorBrush(color);
                }
                catch
                {
                    return new SolidColorBrush(Colors.Gray);
                }
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
