using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace JapaneseTracker.Converters
{
    public class ViewNotImplementedConverter : IValueConverter
    {
        private readonly string[] _implementedViews = { "Dashboard", "Kanji", "Vocabulary", "Grammar", "Practice", "JLPT Progress" };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;

            string selectedView = value.ToString() ?? "";

            // Show "not implemented" message for views not in the implemented list
            bool isImplemented = Array.Exists(_implementedViews, view => 
                string.Equals(view, selectedView, StringComparison.OrdinalIgnoreCase));

            return isImplemented ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}