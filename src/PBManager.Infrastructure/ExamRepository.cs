using DocumentFormat.OpenXml.InkML;
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

        public async Task UpdateExamAsync(Exam exam)
        {
            var existingExam = await _db.Exams
                .Include(e => e.Subjects)
                .FirstOrDefaultAsync(e => e.Id == exam.Id);

            if (existingExam == null)
                throw new InvalidOperationException("آزمون یافت نشد");

            existingExam.Name = exam.Name;
            existingExam.Date = exam.Date;
            existingExam.MaxScore = exam.MaxScore;

            existingExam.Subjects.Clear();
            foreach (var subject in exam.Subjects)
            {
                var attachedSubject = await _db.Subjects.FindAsync(subject.Id);
                if (attachedSubject != null)
                    existingExam.Subjects.Add(attachedSubject);
            }

            await _db.SaveChangesAsync();
        }
    }
}
