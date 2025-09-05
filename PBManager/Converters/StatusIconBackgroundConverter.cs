using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PBManager.Converters
{
    public class StatusIconBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isAbsent && isAbsent)
            {
                return new SolidColorBrush(Color.FromRgb(0xff, 0x66, 0x66)); // Red for absent
            }
            return new SolidColorBrush(Color.FromRgb(0x44, 0xaa, 0x44)); // Green for present
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
