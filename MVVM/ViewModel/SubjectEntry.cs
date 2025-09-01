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
            return new Dictionary<DayOfWeek, int>
        {
            { DayOfWeek.Saturday, GetMinutesForDay(MinutesSat) },
            { DayOfWeek.Sunday, GetMinutesForDay(MinutesSun) },
            { DayOfWeek.Monday, GetMinutesForDay(MinutesMon) },
            { DayOfWeek.Tuesday, GetMinutesForDay(MinutesTue) },
            { DayOfWeek.Wednesday, GetMinutesForDay(MinutesWed) },
            { DayOfWeek.Thursday, GetMinutesForDay(MinutesThu) },
            { DayOfWeek.Friday, GetMinutesForDay(MinutesFri) }
        };
        }

        private int GetMinutesForDay(string minutesString)
        {
            return int.TryParse(minutesString, out var minutes) ? minutes : 0;
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
