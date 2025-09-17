using CommunityToolkit.Mvvm.ComponentModel;
using PBManager.Core.Entities;
using System.ComponentModel.DataAnnotations;

public partial class SubjectEntry : ObservableValidator
{
    public required Subject Subject { get; set; }

    [ObservableProperty]
    [RegularExpression(@"^(\d+)?$", ErrorMessage = "Must be a number or empty")]
    private string _minutesSat = "";

    [ObservableProperty]
    [RegularExpression(@"^(\d+)?$", ErrorMessage = "Must be a number or empty")]
    private string _minutesSun = "";

    [ObservableProperty]
    [RegularExpression(@"^(\d+)?$", ErrorMessage = "Must be a number or empty")]
    private string _minutesMon = "";

    [ObservableProperty]
    [RegularExpression(@"^(\d+)?$", ErrorMessage = "Must be a number or empty")]
    private string _minutesTue = "";

    [ObservableProperty]
    [RegularExpression(@"^(\d+)?$", ErrorMessage = "Must be a number or empty")]
    private string _minutesWed = "";

    [ObservableProperty]
    [RegularExpression(@"^(\d+)?$", ErrorMessage = "Must be a number or empty")]
    private string _minutesThu = "";

    [ObservableProperty]
    [RegularExpression(@"^(\d+)?$", ErrorMessage = "Must be a number or empty")]
    private string _minutesFri = "";

    public void Validate() => ValidateAllProperties();

    public Dictionary<DayOfWeek, int> GetWeeklyMinutes()
    {
        return new Dictionary<DayOfWeek, int>
        {
            { DayOfWeek.Saturday, ParseMinutes(MinutesSat) },
            { DayOfWeek.Sunday,   ParseMinutes(MinutesSun) },
            { DayOfWeek.Monday,   ParseMinutes(MinutesMon) },
            { DayOfWeek.Tuesday,  ParseMinutes(MinutesTue) },
            { DayOfWeek.Wednesday,ParseMinutes(MinutesWed) },
            { DayOfWeek.Thursday, ParseMinutes(MinutesThu) },
            { DayOfWeek.Friday,   ParseMinutes(MinutesFri) }
        };
    }

    public void ClearAllEntries()
    {
        MinutesSat = "";
        MinutesSun = "";
        MinutesMon = "";
        MinutesTue = "";
        MinutesWed = "";
        MinutesThu = "";
        MinutesFri = "";
    }

    private int ParseMinutes(string value)
    {
        if (int.TryParse(value, out int minutes) && minutes >= 0)
        {
            return minutes;
        }
        return 0;
    }
}