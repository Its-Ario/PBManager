using PBManager.MVVM.Model;

namespace PBManager.MVVM.ViewModel
{
    public class WeeklyStudyData
    {
        public DateTime StartOfWeek { get; set; }
        public DateTime EndOfWeek { get; set; }
        public double AverageMinutes { get; set; }
        public int TotalMinutes { get; set; }
        public List<StudyRecord>? Records { get; set; }
    }
}
