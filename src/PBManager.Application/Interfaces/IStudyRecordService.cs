using PBManager.Core.Entities;

namespace PBManager.Application.Interfaces
{
    public interface IStudyRecordService
    {
        Task AddStudyRecordAsync(StudyRecord record);
        Task AddStudyRecordsAsync(List<StudyRecord> records, DateTime startOfWeek);
        Task DeleteStudyRecordsForWeekAsync(Student student, DateTime startOfWeek);
        Task<List<StudyRecord>> GetStudyRecordsForStudentAsync(int studentId);
        Task<double> GetStudentWeeklyAverageAsync(int studentId);
        Task<double> GetWeeklyAverageAsync();
        Task<double> GetSubjectWeeklyAverageAsync(int subjectId);
        Task<int> GetGlobalWeeklyRankAsync(int studentId);
        Task<int> GetClassWeeklyRankAsync(int studentId);
        Task<Subject?> GetMostStudiedWeeklySubjectAsync(int studentId);
        Task<Subject?> GetMostStudiedWeeklySubjectAsync();
        Task<int> GetWeeklyAbsencesAsync();
        Task<List<(DateTime StartOfWeek, DateTime EndOfWeek, double AverageMinutes)>> GetWeeklyStudyDataAsync(int studentId, int weeks = 8);
        Task<List<(DateTime StartOfWeek, DateTime EndOfWeek, double AverageMinutes)>> GetWeeklyStudyDataAsync(int weeks = 8);
        Task<List<DateTime>> GetStudentAbsentWeeksAsync(int studentId);
        Task<List<StudyRecord>> GetStudyRecordsForWeekAsync(Student student, DateTime startOfWeek);
        Task<int> GetStudyRecordCountAsync();
    }
}