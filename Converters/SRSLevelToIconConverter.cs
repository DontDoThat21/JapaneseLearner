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
                    1 => PackIconKind.SeedlingIcon,  // Learning
                    2 => PackIconKind.Sprout,        // Apprentice 1
                    3 => PackIconKind.Leaf,          // Apprentice 2
                    4 => PackIconKind.Tree,          // Guru 1
                    5 => PackIconKind.Pine,          // Guru 2
                    6 => PackIconKind.Master,        // Master
                    7 => PackIconKind.Enlightened,   // Enlightened
                    8 => PackIconKind.Burned,        // Burned
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
