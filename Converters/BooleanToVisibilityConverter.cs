using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace JapaneseTracker.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isVisible = true;
            bool invert = parameter?.ToString()?.Equals("Inverted", StringComparison.OrdinalIgnoreCase) == true;
            
            if (value is bool boolValue)
            {
                isVisible = boolValue;
            }
            else if (value != null)
            {
                // Non-null objects are considered "true"
                isVisible = true;
            }
            else
            {
                // Null objects are considered "false"
                isVisible = false;
            }
            
            if (invert)
            {
                isVisible = !isVisible;
            }
            
            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                bool invert = parameter?.ToString()?.Equals("Inverted", StringComparison.OrdinalIgnoreCase) == true;
                bool result = visibility == Visibility.Visible;
                return invert ? !result : result;
            }
            return false;
        }
    }
}