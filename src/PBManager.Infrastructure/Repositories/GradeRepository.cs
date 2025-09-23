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

        public void Update(GradeRecord grade)
        {
            dbContext.GradeRecords.Update(grade);
        }

        public void Delete(GradeRecord grade)
        {
            dbContext.GradeRecords.Remove(grade);
        }

        public Task<int> SaveChangesAsync()
        {
            return dbContext.SaveChangesAsync();
        }

        public Task<GradeRecord?> GetByIdAsync(int gradeId)
        {
            return dbContext.GradeRecords
                .Include(g => g.Student)
                .Include(g => g.Subject)
                .FirstOrDefaultAsync(g => g.Id == gradeId);
        }

        public Task<List<GradeRecord>> GetAllAsync()
        {
            return dbContext.GradeRecords
                .AsNoTracking()
                .Include(g => g.Student)
                .Include(g => g.Subject)
                .ToListAsync();
        }

        public Task<List<GradeRecord>> GetGradesForStudentAsync(int studentId)
        {
            return dbContext.GradeRecords
                .AsNoTracking()
                .Where(g => g.StudentId == studentId)
                .Include(g => g.Subject)
                .OrderByDescending(g => g.Date)
                .ToListAsync();
        }

        public Task<List<GradeRecord>> GetGradesForSubjectAsync(int subjectId)
        {
            return dbContext.GradeRecords
                .AsNoTracking()
                .Where(g => g.SubjectId == subjectId)
                .Include(g => g.Student)
                .OrderByDescending(g => g.Score)
                .ToListAsync();
        }
    }
}
