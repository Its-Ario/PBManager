using PBManager.Core.Entities;

namespace PBManager.Application.Interfaces
{
    public interface IExamService
    {
        Task<List<Exam>> GetAllExamsWithSubjectsAsync();
        Task AddExamAsync(Exam exam);
        Task<int> GetParticipantCountAsync(int examId);
        Task UpdateExamAsync(Exam exam);
    }
}
