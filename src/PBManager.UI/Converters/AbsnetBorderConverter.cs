using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace PBManager.UI.Converters
{
    public class AbsentBorderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isAbsent && isAbsent)
            {
                return new SolidColorBrush(Color.FromRgb(0x77, 0x44, 0x44)); // Dark red border
            }
            return new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0x44)); // Normal border
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
