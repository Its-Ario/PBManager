using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PBManager.Converters
{
    public class AbsentBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isAbsent && isAbsent)
            {
                return new SolidColorBrush(Color.FromRgb(0x40, 0x30, 0x30));
            }
            return new SolidColorBrush(Color.FromRgb(0x32, 0x30, 0x40));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
