using Microsoft.EntityFrameworkCore;
using PBManager.Core.Entities;
using PBManager.Core.Interfaces;
using PBManager.Core.Utils;
using PBManager.Infrastructure.Data;
namespace PBManager.Infrastructure.Repositories;

public class StudyRecordRepository : IStudyRecordRepository
{
    private readonly DatabaseContext _db;
    public StudyRecordRepository(DatabaseContext db) { _db = db; }

    public async Task AddAsync(StudyRecord record) => await _db.StudyRecords.AddAsync(record);
    public async Task AddRangeAsync(IEnumerable<StudyRecord> records) => await _db.StudyRecords.AddRangeAsync(records);
    public void DeleteRange(IEnumerable<StudyRecord> records) => _db.StudyRecords.RemoveRange(records);
    public Task<int> GetCountAsync() => _db.StudyRecords.AsNoTracking().CountAsync();
    public Task<int> SaveChangesAsync() => _db.SaveChangesAsync();

    public Task<bool> DoesRecordExistForWeekAsync(int studentId, DateTime startOfWeek)
    {
        var weekEnd = startOfWeek.AddDays(6);
        return _db.StudyRecords.AsNoTracking().AnyAsync(r => r.StudentId == studentId && r.Date >= startOfWeek && r.Date <= weekEnd);
    }

    public Task<List<StudyRecord>> GetRecordsForWeekAsync(int studentId, DateTime startOfWeek)
    {
        var weekEnd = startOfWeek.AddDays(6);
        return _db.StudyRecords.AsNoTracking().Include(r => r.Subject)
            .Where(r => r.Student.Id == studentId && r.Date >= startOfWeek && r.Date <= weekEnd)
            .ToListAsync();
    }

    public Task<List<StudyRecord>> GetStudentRecords(int studentId)
    {
        return _db.StudyRecords.AsNoTracking()
            .Include(r => r.Subject).Include(r => r.Student)
            .Where(r => r.StudentId == studentId)
            .ToListAsync();
    }

    public Task<List<StudyRecord>> GetStudyDataForAllAsync()
    {
        return _db.StudyRecords.AsNoTracking().ToListAsync();
    }

    public Task<List<StudyRecord>> GetStudyDataForSubjectAsync(int subjectId)
    {
        return _db.StudyRecords.AsNoTracking()
            .Where(r => r.SubjectId == subjectId)
            .ToListAsync();
    }

    public async Task<List<(int StudentId, int TotalMinutes)>> GetWeeklyTotalsAsync(DateTime startOfWeek, DateTime endOfWeek)
    {
        var results = await _db.StudyRecords.AsNoTracking()
            .Where(r => r.Date >= startOfWeek && r.Date < endOfWeek)
            .GroupBy(r => r.StudentId)
            .Select(g => new { StudentId = g.Key, TotalMinutes = g.Sum(r => r.MinutesStudied) })
            .ToListAsync();
        return results.Select(r => (r.StudentId, r.TotalMinutes)).ToList();
    }

    public async Task<List<(int StudentId, int TotalMinutes)>> GetClassWeeklyTotalsAsync(int classId, DateTime startOfWeek, DateTime endOfWeek)
    {
        var results = await _db.StudyRecords.AsNoTracking()
            .Where(r => r.Student.ClassId == classId && r.Date >= startOfWeek && r.Date < endOfWeek)
            .GroupBy(r => r.StudentId)
            .Select(g => new { StudentId = g.Key, TotalMinutes = g.Sum(r => r.MinutesStudied) })
            .ToListAsync();
        return results.Select(r => (r.StudentId, r.TotalMinutes)).ToList();
    }

    public Task<Subject?> GetMostStudiedSubjectForStudentAsync(int studentId, DateTime startOfWeek, DateTime endOfWeek)
    {
        return _db.StudyRecords.AsNoTracking()
            .Where(r => r.StudentId == studentId && r.Date >= startOfWeek && r.Date < endOfWeek)
            .GroupBy(r => r.Subject)
            .OrderByDescending(g => g.Sum(r => r.MinutesStudied))
            .Select(g => g.Key)
            .FirstOrDefaultAsync();
    }

    public Task<Subject?> GetMostStudiedSubjectOverallAsync(DateTime startOfWeek, DateTime endOfWeek)
    {
        return _db.StudyRecords.AsNoTracking()
            .Where(r => r.Date >= startOfWeek && r.Date < endOfWeek)
            .GroupBy(r => r.Subject)
            .OrderByDescending(g => g.Sum(r => r.MinutesStudied))
            .Select(g => g.Key)
            .FirstOrDefaultAsync();
    }

    public Task<List<int>> GetStudentsWithDataInWeekAsync(DateTime startOfWeek, DateTime endOfWeek)
    {
        return _db.StudyRecords.AsNoTracking()
            .Where(r => r.Date >= startOfWeek && r.Date < endOfWeek)
            .Select(r => r.StudentId).Distinct().ToListAsync();
    }

    public async Task<List<StudyRecord>> GetRecordsForLastWeeksAsync(int? studentId, int weeks)
    {
        var lastSubmission = await _db.StudyRecords.AsNoTracking().OrderByDescending(r => r.Date).Select(r => (DateTime?)r.Date).FirstOrDefaultAsync();
        if (lastSubmission == null) return [];

        var lastWeekStart = DateUtils.GetPersianStartOfWeek(lastSubmission.Value);
        var startDate = lastWeekStart.AddDays(-7 * (weeks - 1));

        var query = _db.StudyRecords.AsNoTracking().Where(r => r.Date >= startDate);
        if (studentId.HasValue)
        {
            query = query.Where(r => r.StudentId == studentId.Value);
        }
        return await query.ToListAsync();
    }

    public Task<List<DateTime>> GetAllRecordDatesAsync()
    {
        return _db.StudyRecords.AsNoTracking().OrderBy(r => r.Date).Select(r => r.Date).ToListAsync();
    }

    public async Task<HashSet<DateTime>> GetStudentRecordDatesAsync(int studentId)
    {
        var dates = await _db.StudyRecords.AsNoTracking()
            .Where(r => r.StudentId == studentId)
            .Select(r => r.Date)
            .ToListAsync();
        return new HashSet<DateTime>(dates);
    }
}