using System.Globalization;
using System.Windows.Data;

namespace PBManager.Converters
{
    class DateConverter : IValueConverter
    {
        private readonly PersianCalendar _persianCalendar = new PersianCalendar();

        private static readonly string[] PersianMonthNames =
        {
        "فروردین", "اردیبهشت", "خرداد", "تیر",
        "مرداد", "شهریور", "مهر", "آبان",
        "آذر", "دی", "بهمن", "اسفند"
    };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime date)
            {
                int year = _persianCalendar.GetYear(date);
                int month = _persianCalendar.GetMonth(date);
                int day = _persianCalendar.GetDayOfMonth(date);

                string monthName = PersianMonthNames[month - 1];
                return $"{day} {monthName} {year}";
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
