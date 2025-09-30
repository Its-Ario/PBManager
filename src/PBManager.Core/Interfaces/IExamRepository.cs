using PBManager.Core.Entities;

namespace PBManager.Core.Interfaces
{
    public interface IExamRepository
    {
        Task<Exam?> GetByIdAsync(int id);
        Task<List<Exam>> GetAllWithSubjectsAsync();
        Task AddAsync(Exam exam);
        void Update(Exam exam);
        void Delete(Exam exam);
        Task<int> SaveChangesAsync();
        Task<int> GetParticipantCountAsync(int examId);
    }
}
