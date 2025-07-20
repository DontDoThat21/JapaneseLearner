using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace JapaneseTracker.Converters
{
    public class ViewVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Visibility.Collapsed;

            string selectedView = value.ToString() ?? "";
            string targetView = parameter.ToString() ?? "";

            return string.Equals(selectedView, targetView, StringComparison.OrdinalIgnoreCase)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}