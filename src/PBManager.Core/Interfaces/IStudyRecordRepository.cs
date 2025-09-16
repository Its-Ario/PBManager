using PBManager.Core.Entities;

namespace PBManager.Core.Interfaces
{
    public interface IStudyRecordRepository
    {
        Task AddAsync(StudyRecord record);
        Task AddRangeAsync(IEnumerable<StudyRecord> records);
        Task<bool> DoesRecordExistForWeekAsync(int studentId, DateTime startOfWeek);
        Task<List<StudyRecord>> GetRecordsForWeekAsync(int studentId, DateTime startOfWeek);
        void DeleteRange(IEnumerable<StudyRecord> records);
        Task<List<StudyRecord>> GetStudentRecords(int studentId);
        Task<List<StudyRecord>> GetStudyDataForAllAsync();
        Task<List<StudyRecord>> GetStudyDataForSubjectAsync(int subjectId);
        Task<List<(int StudentId, int TotalMinutes)>> GetWeeklyTotalsAsync(DateTime startOfWeek, DateTime endOfWeek);
        Task<List<(int StudentId, int TotalMinutes)>> GetClassWeeklyTotalsAsync(int classId, DateTime startOfWeek, DateTime endOfWeek);
        Task<Subject?> GetMostStudiedSubjectForStudentAsync(int studentId, DateTime startOfWeek, DateTime endOfWeek);
        Task<Subject?> GetMostStudiedSubjectOverallAsync(DateTime startOfWeek, DateTime endOfWeek);
        Task<List<int>> GetStudentsWithDataInWeekAsync(DateTime startOfWeek, DateTime endOfWeek);
        Task<List<StudyRecord>> GetRecordsForLastWeeksAsync(int? studentId, int weeks);
        Task<List<DateTime>> GetAllRecordDatesAsync();
        Task<HashSet<DateTime>> GetStudentRecordDatesAsync(int studentId);
        Task<int> GetCountAsync();
        Task<int> SaveChangesAsync();
    }
}
