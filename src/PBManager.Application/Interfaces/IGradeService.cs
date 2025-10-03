using PBManager.Application.DTOs;
using PBManager.Core.Entities;

namespace PBManager.Application.Interfaces
{
    public interface IGradeService
    {
        Task AddGradeAsync(GradeRecord grade);
        Task UpdateGradeAsync(GradeRecord grade);
        Task<bool> DeleteGradeAsync(int gradeId);

        Task<List<GradeRecord>> GetGradesForStudentAsync(int studentId, int? examId = null);
        Task<double> GetAverageGradeForStudentAsync(int studentId);
        Task<double> GetAverageGradeForSubjectAsync(int subjectId);
        Task<double> GetOverallAverageGradeAsync();
        Task<(int Rank, int Total)> GetStudentRankBySubjectAsync(int studentId, int subjectId);
        Task<Subject?> GetTopPerformingSubjectAsync(int studentId);
        Task<List<StudentExamScore>> GetAllExamScoresForStudentAsync(int studentId);
        Task<List<StudentExamScore>> GetRankedScoresForExamAsync(int examId);
        Task<List<GradeRecord>> GetExamRecords(int examId);
        Task SaveGradesForExamAsync(int studentId, int examId, IEnumerable<GradeRecord> gradeRecords);
        Task DeleteRecords(int studentId, int examId);
        Task<int> GetOverallExamRankAsync(int studentId);
        Task<int> GetClassExamRankAsync(int studentId);
    }
}