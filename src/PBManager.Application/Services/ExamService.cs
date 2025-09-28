using PBManager.Application.Interfaces;
using PBManager.Core.Entities;
using PBManager.Core.Interfaces;

namespace PBManager.Application.Services
{
    public class ExamService : IExamService
    {
        private readonly IExamRepository _examRepository;

        public ExamService(IExamRepository examRepository)
        {
            _examRepository = examRepository;
        }

        public async Task AddExamAsync(Exam exam)
        {
            await _examRepository.AddAsync(exam);
            await _examRepository.SaveChangesAsync();
        }

        public Task<List<Exam>> GetAllExamsWithSubjectsAsync()
        {
            return _examRepository.GetAllWithSubjectsAsync();
        }
    }
}
