using CommunityToolkit.Mvvm.ComponentModel;
using PBManager.MVVM.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PBManager.MVVM.ViewModel
{
    public class SubjectEntry : ObservableValidator
    {
        public Subject Subject { get; set; }

        private string _minutesSat = "";
        private string _minutesSun = "";
        private string _minutesMon = "";
        private string _minutesTue = "";
        private string _minutesWed = "";
        private string _minutesThu = "";
        private string _minutesFri = "";

        [RegularExpression(@"^(\d+)?$", ErrorMessage = "Must be a number or empty")]
        public string MinutesSat
        {
            get => _minutesSat;
            set => SetProperty(ref _minutesSat, value, true);
        }

        [RegularExpression(@"^(\d+)?$", ErrorMessage = "Must be a number or empty")]
        public string MinutesSun
        {
            get => _minutesSun;
            set => SetProperty(ref _minutesSun, value, true);
        }

        [RegularExpression(@"^(\d+)?$", ErrorMessage = "Must be a number or empty")]
        public string MinutesMon
        {
            get => _minutesMon;
            set => SetProperty(ref _minutesMon, value, true);
        }

        [RegularExpression(@"^(\d+)?$", ErrorMessage = "Must be a number or empty")]
        public string MinutesTue
        {
            get => _minutesTue;
            set => SetProperty(ref _minutesTue, value, true);
        }

        [RegularExpression(@"^(\d+)?$", ErrorMessage = "Must be a number or empty")]
        public string MinutesWed
        {
            get => _minutesWed;
            set => SetProperty(ref _minutesWed, value, true);
        }

        [RegularExpression(@"^(\d+)?$", ErrorMessage = "Must be a number or empty")]
        public string MinutesThu
        {
            get => _minutesThu;
            set => SetProperty(ref _minutesThu, value, true);
        }

        [RegularExpression(@"^(\d+)?$", ErrorMessage = "Must be a number or empty")]
        public string MinutesFri
        {
            get => _minutesFri;
            set => SetProperty(ref _minutesFri, value, true);
        }

        public void Validate() => base.ValidateAllProperties();

        public Dictionary<DayOfWeek, int> GetWeeklyMinutes()
        {
            var weeklyMinutes = new Dictionary<DayOfWeek, int>();

            // Parse and validate each day's input
            if (int.TryParse(MinutesSat, out int satMinutes) && satMinutes >= 0)
                weeklyMinutes[DayOfWeek.Saturday] = satMinutes;
            else
                weeklyMinutes[DayOfWeek.Saturday] = 0;

            if (int.TryParse(MinutesSun, out int sunMinutes) && sunMinutes >= 0)
                weeklyMinutes[DayOfWeek.Sunday] = sunMinutes;
            else
                weeklyMinutes[DayOfWeek.Sunday] = 0;

            if (int.TryParse(MinutesMon, out int monMinutes) && monMinutes >= 0)
                weeklyMinutes[DayOfWeek.Monday] = monMinutes;
            else
                weeklyMinutes[DayOfWeek.Monday] = 0;

            if (int.TryParse(MinutesTue, out int tueMinutes) && tueMinutes >= 0)
                weeklyMinutes[DayOfWeek.Tuesday] = tueMinutes;
            else
                weeklyMinutes[DayOfWeek.Tuesday] = 0;

            if (int.TryParse(MinutesWed, out int wedMinutes) && wedMinutes >= 0)
                weeklyMinutes[DayOfWeek.Wednesday] = wedMinutes;
            else
                weeklyMinutes[DayOfWeek.Wednesday] = 0;

            if (int.TryParse(MinutesThu, out int thuMinutes) && thuMinutes >= 0)
                weeklyMinutes[DayOfWeek.Thursday] = thuMinutes;
            else
                weeklyMinutes[DayOfWeek.Thursday] = 0;

            if (int.TryParse(MinutesFri, out int friMinutes) && friMinutes >= 0)
                weeklyMinutes[DayOfWeek.Friday] = friMinutes;
            else
                weeklyMinutes[DayOfWeek.Friday] = 0;

            return weeklyMinutes;
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
    }
}
