using System;
using System.Globalization;
using System.Windows.Data;
using MaterialDesignThemes.Wpf;

namespace JapaneseTracker.Converters
{
    public class SRSLevelToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int srsLevel)
            {
                return srsLevel switch
                {
                    0 => PackIconKind.School,        // New item
                    1 => PackIconKind.Seedling,      // Learning
                    2 => PackIconKind.Sprout,        // Apprentice 1
                    3 => PackIconKind.Leaf,          // Apprentice 2
                    4 => PackIconKind.Tree,          // Guru 1
                    5 => PackIconKind.PineTree,      // Guru 2
                    6 => PackIconKind.Crown,         // Master
                    7 => PackIconKind.LightbulbOn,   // Enlightened
                    8 => PackIconKind.Fire,          // Burned
                    _ => PackIconKind.Help
                };
            }
            return PackIconKind.Help;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
