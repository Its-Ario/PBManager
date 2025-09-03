using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PBManager.Converters
{
    class MinuteToHourConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            int minutes;
            switch (value)
            {
                case int i:
                    minutes = i;
                    break;
                case double d:
                    minutes = (int)d;
                    break;
                case string s when int.TryParse(s, out var parsed):
                    minutes = parsed;
                    break;
                default:
                    return string.Empty;
            }

            if (minutes <= 0)
                return "0 دقیقه";

            int hours = minutes / 60;
            int mins = minutes % 60;

            if (hours > 0 && mins > 0)
                return $"{hours} ساعت {mins} دقیقه";

            if (hours > 0)
                return $"{hours} ساعت";

            return $"{mins} دقیقه";
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
