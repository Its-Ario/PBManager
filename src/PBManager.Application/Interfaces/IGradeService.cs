using PBManager.Core.Entities;

namespace PBManager.Application.Interfaces
{
    public interface IGradeService
    {
        Task AddGradeAsync(GradeRecord grade);
        Task UpdateGradeAsync(GradeRecord grade);
        Task<bool> DeleteGradeAsync(int gradeId);

        Task<List<GradeRecord>> GetGradesForStudentAsync(int studentId);
        Task<double> GetAverageGradeForStudentAsync(int studentId);
        Task<double> GetAverageGradeForSubjectAsync(int subjectId);
        Task<double> GetOverallAverageGradeAsync();
        Task<(int Rank, int Total)> GetStudentRankBySubjectAsync(int studentId, int subjectId);
        Task<Subject?> GetTopPerformingSubjectAsync(int studentId);
    }
}