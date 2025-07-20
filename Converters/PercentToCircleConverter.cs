using System;
using System.Globalization;
using System.Windows.Data;

namespace JapaneseTracker.Converters
{
    public class PercentToCircleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double percent)
            {
                // Calculate the circumference for a circle with radius (default 32, based on 80x80 size)
                double radius = 32; // Default radius
                if (parameter is string radiusParam && double.TryParse(radiusParam, out double parsedRadius))
                {
                    radius = parsedRadius;
                }
                double circumference = 2 * Math.PI * radius;
                
                // Calculate the dash length based on percentage
                double dashLength = (percent / 100.0) * circumference;
                double gapLength = circumference - dashLength;
                
                return $"{dashLength},{gapLength}";
            }
            return "0,251"; // Default for 0%
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}