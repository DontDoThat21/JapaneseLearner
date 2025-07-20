using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace JapaneseTracker.Converters
{
    public class StringVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Visibility.Collapsed;
            
            var stringValue = value.ToString();
            var parameterString = parameter.ToString();
            
            // Handle multiple parameter values separated by |
            if (parameterString.Contains("|"))
            {
                var validValues = parameterString.Split('|');
                foreach (var validValue in validValues)
                {
                    if (string.Equals(stringValue, validValue.Trim(), StringComparison.OrdinalIgnoreCase))
                        return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
            
            return string.Equals(stringValue, parameterString, StringComparison.OrdinalIgnoreCase) 
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            
            // For null or non-boolean values, show the element
            return value == null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility != Visibility.Visible;
            }
            return false;
        }
    }
    
    public class SRSLevelToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int level)
            {
                return level switch
                {
                    0 => "#F44336", // Red - Learning
                    1 => "#FF9800", // Orange - Beginner
                    2 => "#FF9800", // Orange - Beginner
                    3 => "#FFC107", // Amber - Intermediate
                    4 => "#FFC107", // Amber - Intermediate
                    5 => "#8BC34A", // Light Green - Advanced
                    6 => "#4CAF50", // Green - Advanced
                    7 => "#4CAF50", // Green - Master
                    8 => "#4CAF50", // Green - Master
                    _ => "#9E9E9E"  // Grey - Unknown
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