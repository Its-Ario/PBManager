using PBManager.MVVM.Model;
using System.Threading.Tasks;

namespace PBManager.Services
{
    public interface IStudyRecordService
    {
        Task AddStudyRecordAsync(StudyRecord record);
        Task AddStudyRecordsAsync(List<StudyRecord> records, DateTime startOfWeek);
        Task<List<StudyRecord>> GetStudyRecordsForStudentAsync(int studentId);
        Task<double> GetStudentWeeklyAverageAsync(int studentId);
        Task<double> GetWeeklyAverageAsync();
        Task<double> GetSubjectWeeklyAverageAsync(int subjectId);
        Task<int> GetGlobalWeeklyRankAsync(int studentId);
        Task<int> GetClassWeeklyRankAsync(int studentId);
        Task<Subject?> GetMostStudiedWeeklySubjectAsync(int studentId);
        Task<Subject?> GetMostStudiedWeeklySubjectAsync();
        Task<List<(DateTime StartOfWeek, DateTime EndOfWeek, double AverageMinutes)>> GetWeeklyStudyDataAsync(int weeks = 8);
        Task<List<(DateTime StartOfWeek, DateTime EndOfWeek, double AverageMinutes)>> GetWeeklyStudyDataAsync(int studentId, int weeks = 8);
        Task<int> GetWeeklyAbsencesAsync();
        Task<int> GetStudentAbsencesAsync(int studentId);
    }
}
