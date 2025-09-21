using PBManager.Core.Entities;

namespace PBManager.UI.MVVM.ViewModel.Helpers
{
    public class WeeklyStudyData
    {
        public DateTime StartOfWeek { get; set; }
        public DateTime EndOfWeek { get; set; }
        public int TotalMinutes { get; set; }
        public double AverageMinutes { get; set; }
        public List<StudyRecord> Records { get; set; } = [];
        public bool IsAbsent { get; set; } = false;
    }
}