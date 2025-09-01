using PBManager.MVVM.Model;

namespace PBManager.Services
{
    public interface IStudyRecordService
    {
        Task AddStudyRecordAsync(StudyRecord record);
        Task AddStudyRecordsAsync(List<StudyRecord> records);
        Task<List<StudyRecord>> GetStudyRecordsForStudentAsync(int studentId);
        Task<double> GetStudentWeeklyAverageAsync(int studentId);
        Task<double> GetWeeklyAverageAsync();
        Task<double> GetSubjectWeeklyAverageAsync(int subjectId);
        Task<int> GetGlobalWeeklyRankAsync(int studentId);
        Task<int> GetClassWeeklyRankAsync(int studentId);
        Task<Subject?> GetMostStudiedWeeklySubjectAsync(int studentId);
        Task<Subject?> GetMostStudiedWeeklySubjectAsync();
        Task<List<(DateTime StartOfWeek, DateTime EndOfWeek, int TotalMinutes)>> GetWeeklyStudyDataAsync();
    }
}
