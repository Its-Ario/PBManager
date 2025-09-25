using PBManager.Core.Entities;

namespace PBManager.Core.Interfaces
{
    public interface IGradeRepository
    {
        Task AddAsync(GradeRecord grade);
        void Update(GradeRecord grade);
        void Delete(GradeRecord grade);
        Task<int> SaveChangesAsync();

        Task<GradeRecord?> GetByIdAsync(int gradeId);
        Task<List<GradeRecord>> GetAllAsync();
        Task<List<GradeRecord>> GetGradesForStudentAsync(int studentId);
        Task<List<GradeRecord>> GetGradesForSubjectAsync(int subjectId);
        Task<List<GradeRecord>> GetGradesForExamAsync(int examId);
        Task<List<GradeRecord>> GetAllExamGradesAsync();
        Task<List<GradeRecord>> GetAllExamGradesForClassAsync(int classId);
    }
}