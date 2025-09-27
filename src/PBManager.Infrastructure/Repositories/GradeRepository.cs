using Microsoft.EntityFrameworkCore;
using PBManager.Core.Entities;
using PBManager.Core.Interfaces;
using PBManager.Infrastructure.Data;

namespace PBManager.Infrastructure.Repositories
{
    public class GradeRepository(DatabaseContext dbContext) : IGradeRepository
    {
        public async Task AddAsync(GradeRecord grade)
        {
            await dbContext.GradeRecords.AddAsync(grade);
        }

        public async Task AddRangeAsync(IEnumerable<GradeRecord> grades)
        {
            await dbContext.GradeRecords.AddRangeAsync(grades);
        }

        public void Update(GradeRecord grade)
        {
            dbContext.GradeRecords.Update(grade);
        }

        public void Delete(GradeRecord grade)
        {
            dbContext.GradeRecords.Remove(grade);
        }

        public void Delete(IEnumerable<GradeRecord> grades)
        {
            dbContext.GradeRecords.RemoveRange(grades);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await dbContext.SaveChangesAsync();
        }

        public async Task<GradeRecord?> GetByIdAsync(int gradeId)
        {
            return await dbContext.GradeRecords
                .Include(g => g.Student)
                .Include(g => g.Subject)
                .FirstOrDefaultAsync(g => g.Id == gradeId);
        }

        public async Task<List<GradeRecord>> GetAllAsync()
        {
            return await dbContext.GradeRecords
                .AsNoTracking()
                .Include(g => g.Student)
                .Include(g => g.Subject)
                .ToListAsync();
        }

        public async Task<List<GradeRecord>> GetGradesForStudentAsync(int studentId)
        {
            return await dbContext.GradeRecords
                .AsNoTracking()
                .Where(g => g.StudentId == studentId)
                .Include(g => g.Subject)
                .OrderByDescending(g => g.Date)
                .ToListAsync();
        }

        public async Task<List<GradeRecord>> GetGradesForSubjectAsync(int subjectId)
        {
            return await dbContext.GradeRecords
                .AsNoTracking()
                .Where(g => g.SubjectId == subjectId)
                .Include(g => g.Student)
                .OrderByDescending(g => g.Score)
                .ToListAsync();
        }

        public async Task<List<GradeRecord>> GetGradesForExamAsync(int examId)
        {
            return await dbContext.GradeRecords
                .Include(g => g.Student)
                .Where(g => g.ExamId == examId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<GradeRecord>> GetAllExamGradesAsync()
        {
            return await dbContext.GradeRecords
                .Where(g => g.ExamId != null)
                .Include(g => g.Student)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<GradeRecord>> GetAllExamGradesForClassAsync(int classId)
        {
            return await dbContext.GradeRecords
                .Where(g => g.ExamId != null && g.Student.ClassId == classId)
                .Include(g => g.Student)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
