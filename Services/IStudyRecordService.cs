using PBManager.MVVM.Model;

namespace PBManager.Services
{
    public interface IStudyRecordService
    {
        Task AddStudyRecordAsync(StudyRecord record);
        Task AddStudyRecordsAsync(List<StudyRecord> records);
        Task<List<StudyRecord>> GetStudyRecordsForStudentAsync(int studentId);
        Task<double> GetWeeklyAverageAsync(int studentId);
        Task<double> GetWeeklyAverageAsync();
    }
}
