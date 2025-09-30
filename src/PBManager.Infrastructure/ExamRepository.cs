using Microsoft.EntityFrameworkCore;
using PBManager.Core.Entities;
using PBManager.Core.Interfaces;
using PBManager.Infrastructure.Data;

namespace PBManager.Infrastructure
{
    public class ExamRepository : IExamRepository
    {
        private readonly DatabaseContext _db;

        public ExamRepository(DatabaseContext dbContext)
        {
            _db = dbContext;
        }

        public async Task<Exam?> GetByIdAsync(int id)
        {
            return await _db.Exams.FindAsync(id);
        }

        public async Task<List<Exam>> GetAllWithSubjectsAsync()
        {
            return await _db.Exams
                .Include(e => e.Subjects)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddAsync(Exam exam)
        {
            await _db.Exams.AddAsync(exam);
        }

        public void Update(Exam exam)
        {
            _db.Exams.Update(exam);
        }

        public void Delete(Exam exam)
        {
            _db.Exams.Remove(exam);
        }

        public async Task<int> GetParticipantCountAsync(int examId)
        {
            return await _db.GradeRecords
                .Where(gr => gr.ExamId == examId)
                .Select(gr => gr.StudentId)
                .Distinct()
                .CountAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _db.SaveChangesAsync();
        }
    }
}
