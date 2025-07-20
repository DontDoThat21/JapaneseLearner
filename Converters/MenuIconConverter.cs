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
}
