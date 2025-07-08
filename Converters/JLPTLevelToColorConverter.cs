using System.Globalization;
using System.Windows.Data;

namespace JapaneseTracker.Converters
{
    public class JLPTLevelToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string jlptLevel)
            {
                return jlptLevel switch
                {
                    "N5" => "#4CAF50", // Green - Beginner
                    "N4" => "#8BC34A", // Light Green
                    "N3" => "#FFC107", // Amber
                    "N2" => "#FF9800", // Orange
                    "N1" => "#F44336", // Red - Advanced
                    _ => "#9E9E9E"     // Default Gray
                };
            }
            return "#9E9E9E";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
