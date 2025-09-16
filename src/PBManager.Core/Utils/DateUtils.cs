using System.Globalization;

namespace PBManager.Core.Utils;

public static class DateUtils
{
    private static readonly CultureInfo PersianCulture = new("fa-IR");

    public static DateTime GetPersianStartOfWeek(DateTime date)
    {
        date = date.Date;
        int diff = (int)date.DayOfWeek - (int)PersianCulture.DateTimeFormat.FirstDayOfWeek;
        if (diff < 0) diff += 7;
        return date.AddDays(-diff);
    }
}