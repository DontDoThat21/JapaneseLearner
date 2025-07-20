using System;
using System.Globalization;
using System.Windows.Data;
using MaterialDesignThemes.Wpf;

namespace JapaneseTracker.Converters
{
    public class MenuIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string menuItem)
            {
                return menuItem switch
                {
                    "Dashboard" => PackIconKind.ViewDashboard,
                    "Kanji" => PackIconKind.Translate,
                    "Vocabulary" => PackIconKind.BookOpenPageVariant,
                    "Grammar" => PackIconKind.Spellcheck,
                    "Practice" => PackIconKind.School,
                    "JLPT Progress" => PackIconKind.TrendingUp,
                    _ => PackIconKind.Circle
                };
            }
            return PackIconKind.Circle;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }    
    public class ViewVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string selectedView && parameter is string targetView)
            {
                bool isNegated = targetView.StartsWith("!");
                if (isNegated)
                {
                    targetView = targetView.Substring(1);
                    return selectedView != targetView ? Visibility.Visible : Visibility.Collapsed;
                }
                else
                {
                    return selectedView == targetView ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    public class ViewNotImplementedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string selectedView)
            {
                // Show "coming soon" message for views that are not yet implemented
                string[] implementedViews = { "Dashboard", "Kanji", "Practice", "JLPT Progress" };
                return !implementedViews.Contains(selectedView);
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    } main
}
