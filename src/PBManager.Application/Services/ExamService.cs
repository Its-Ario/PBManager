using Microsoft.Extensions.Caching.Memory;
using PBManager.Application.Interfaces;
using PBManager.Core.Entities;
using PBManager.Core.Interfaces;

namespace PBManager.Application.Services
{
    public class ExamService : IExamService
    {
        private readonly IExamRepository _examRepository;
        private readonly IMemoryCache _cache;

        public ExamService(IExamRepository examRepository, IMemoryCache cache)
        {
            _examRepository = examRepository;
            _cache = cache;
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

        public async Task<int> GetParticipantCountAsync(int examId)
        {
            string cacheKey = $"Exam_{examId}_ParticipantCount";

            if (!_cache.TryGetValue(cacheKey, out int count))
            {
                count = await _examRepository.GetParticipantCountAsync(examId);
                _cache.Set(cacheKey, count);
            }

            return count;
        }

        public async Task UpdateExamAsync(Exam exam)
        {
            await _examRepository.UpdateExamAsync(exam);
        }
    }
}
