using Microsoft.Extensions.Caching.Memory;
using PBManager.Application.Interfaces;
using PBManager.Core.Entities;
using PBManager.Core.Interfaces;
using PBManager.Core.Utils;
using System.Globalization;

namespace PBManager.Application.Services;

public class StudyRecordService : IStudyRecordService
{
    private readonly IStudyRecordRepository _repository;
    private readonly IStudentRepository _studentRepository;
    private readonly IMemoryCache _cache;
    private static readonly CultureInfo PersianCulture = new("fa-IR");

    public StudyRecordService(IStudyRecordRepository repository, IStudentRepository studentRepository, IMemoryCache cache)
    {
        _repository = repository;
        _studentRepository = studentRepository;
        _cache = cache;
    }

    private MemoryCacheEntryOptions GetDefaultCacheOptions(TimeSpan? absoluteExpiration = null) =>
        new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(10))
            .SetAbsoluteExpiration(absoluteExpiration ?? TimeSpan.FromHours(1));

    public async Task AddStudyRecordAsync(StudyRecord record)
    {
        await _repository.AddAsync(record);
        await _repository.SaveChangesAsync();
        InvalidateCachesForRecord(record);
    }

    public async Task AddStudyRecordsAsync(List<StudyRecord> records, DateTime startOfWeek)
    {
        if (records == null || !records.Any())
            throw new ArgumentException("No study records provided.");

        var studentId = records.First().StudentId;
        var weekStart = DateUtils.GetPersianStartOfWeek(startOfWeek);

        if (await _repository.DoesRecordExistForWeekAsync(studentId, weekStart))
            throw new InvalidOperationException("Records already exist for this student in this week.");

        await _repository.AddRangeAsync(records);
        await _repository.SaveChangesAsync();
        records.ForEach(InvalidateCachesForRecord);
    }

    public async Task DeleteStudyRecordsForWeekAsync(Student student, DateTime startOfWeek)
    {
        var weekStart = DateUtils.GetPersianStartOfWeek(startOfWeek);
        var recordsToDelete = await _repository.GetRecordsForWeekAsync(student.Id, weekStart);

        if (recordsToDelete.Any())
        {
            _repository.DeleteRange(recordsToDelete);
            await _repository.SaveChangesAsync();
            recordsToDelete.ForEach(InvalidateCachesForRecord);
        }
    }

    private void InvalidateCachesForRecord(StudyRecord record)
    {
        _cache.Remove($"StudyRecords_Student_{record.StudentId}");
        _cache.Remove($"WeeklyAverage_Student_{record.StudentId}");
        _cache.Remove($"WeeklyAverage_Subject_{record.SubjectId}");
        _cache.Remove($"MostStudied_Student_{record.StudentId}");
        _cache.Remove($"WeeklyStudyData_Student_{record.StudentId}");
        _cache.Remove($"GlobalRank_Student_{record.StudentId}");
        _cache.Remove($"ClassRank_Student_{record.StudentId}");
        _cache.Remove("WeeklyAverage_All");
        _cache.Remove("MostStudied_All");
        _cache.Remove("WeeklyAbsences_All");
        _cache.Remove("WeeklyStudyData_All");
    }

    public async Task<List<StudyRecord>> GetStudyRecordsForStudentAsync(int studentId)
    {
        string cacheKey = $"StudyRecords_Student_{studentId}";
        if (!_cache.TryGetValue(cacheKey, out List<StudyRecord> studyRecords))
        {
            studyRecords = await _repository.GetStudentRecords(studentId);
            _cache.Set(cacheKey, studyRecords, GetDefaultCacheOptions());
        }
        return studyRecords;
    }

    public async Task<double> GetStudentWeeklyAverageAsync(int studentId)
    {
        string cacheKey = $"WeeklyAverage_Student_{studentId}";
        if (!_cache.TryGetValue(cacheKey, out double weeklyAverage))
        {
            var records = await _repository.GetStudentRecords(studentId);
            weeklyAverage = CalculateAverageFromRecords(records);
            _cache.Set(cacheKey, weeklyAverage, GetDefaultCacheOptions());
        }
        return weeklyAverage;
    }

    public async Task<double> GetWeeklyAverageAsync()
    {
        const string cacheKey = "WeeklyAverage_All";
        if (!_cache.TryGetValue(cacheKey, out double weeklyAverage))
        {
            var records = await _repository.GetStudyDataForAllAsync();
            weeklyAverage = CalculateAverageFromRecords(records);
            _cache.Set(cacheKey, weeklyAverage, GetDefaultCacheOptions());
        }
        return weeklyAverage;
    }

    public async Task<double> GetSubjectWeeklyAverageAsync(int subjectId)
    {
        string cacheKey = $"WeeklyAverage_Subject_{subjectId}";
        if (!_cache.TryGetValue(cacheKey, out double weeklyAverage))
        {
            var records = await _repository.GetStudyDataForSubjectAsync(subjectId);
            weeklyAverage = CalculateAverageFromRecords(records);
            _cache.Set(cacheKey, weeklyAverage, GetDefaultCacheOptions());
        }
        return weeklyAverage;
    }

    public async Task<int> GetGlobalWeeklyRankAsync(int studentId)
    {
        string cacheKey = $"GlobalRank_Student_{studentId}";
        if (!_cache.TryGetValue(cacheKey, out int rank))
        {
            var (startOfWeek, endOfWeek) = GetCurrentPersianWeekRange();
            List<(int StudentId, int TotalMinutes)> weeklyTotals = await _repository.GetWeeklyTotalsAsync(startOfWeek, endOfWeek);
            int studentTotalMinutes = weeklyTotals.FirstOrDefault(t => t.StudentId == studentId).TotalMinutes;
            int higherRankedCount = weeklyTotals.Count(t => t.TotalMinutes > studentTotalMinutes);
            rank = higherRankedCount + 1;
            _cache.Set(cacheKey, rank, GetDefaultCacheOptions(TimeSpan.FromMinutes(5)));
        }
        return rank;
    }

    public async Task<int> GetClassWeeklyRankAsync(int studentId)
    {
        string cacheKey = $"ClassRank_Student_{studentId}";
        if (!_cache.TryGetValue(cacheKey, out int rank))
        {
            int? studentClassId = (await _studentRepository.FindByIdAsync(studentId))?.ClassId;
            if (studentClassId == null) return 0;

            var (startOfWeek, endOfWeek) = GetCurrentPersianWeekRange();
            var weeklyTotals = await _repository.GetClassWeeklyTotalsAsync(studentClassId.Value, startOfWeek, endOfWeek);
            var studentTotalMinutes = weeklyTotals.FirstOrDefault(t => t.StudentId == studentId).TotalMinutes;
            var higherRankedCount = weeklyTotals.Count(t => t.TotalMinutes > studentTotalMinutes);
            rank = higherRankedCount + 1;
            _cache.Set(cacheKey, rank, GetDefaultCacheOptions(TimeSpan.FromMinutes(5)));
        }
        return rank;
    }

    public async Task<Subject?> GetMostStudiedWeeklySubjectAsync(int studentId)
    {
        string cacheKey = $"MostStudied_Student_{studentId}";
        if (!_cache.TryGetValue(cacheKey, out Subject? mostStudied))
        {
            var (startOfWeek, endOfWeek) = GetCurrentPersianWeekRange();
            mostStudied = await _repository.GetMostStudiedSubjectForStudentAsync(studentId, startOfWeek, endOfWeek);
            _cache.Set(cacheKey, mostStudied, GetDefaultCacheOptions());
        }
        return mostStudied;
    }

    public async Task<Subject?> GetMostStudiedWeeklySubjectAsync()
    {
        const string cacheKey = "MostStudied_All";
        if (!_cache.TryGetValue(cacheKey, out Subject? mostStudied))
        {
            var (startOfWeek, endOfWeek) = GetCurrentPersianWeekRange();
            mostStudied = await _repository.GetMostStudiedSubjectOverallAsync(startOfWeek, endOfWeek);
            _cache.Set(cacheKey, mostStudied, GetDefaultCacheOptions());
        }
        return mostStudied;
    }

    public async Task<int> GetWeeklyAbsencesAsync()
    {
        const string cacheKey = "WeeklyAbsences_All";
        if (!_cache.TryGetValue(cacheKey, out int absenceCount))
        {
            var (startOfWeek, endOfWeek) = GetCurrentPersianWeekRange();
            var studentsWithData = await _repository.GetStudentsWithDataInWeekAsync(startOfWeek, endOfWeek);
            var totalStudents = await _studentRepository.GetCountAsync();
            absenceCount = totalStudents - studentsWithData.Count;
            _cache.Set(cacheKey, absenceCount, GetDefaultCacheOptions());
        }
        return absenceCount;
    }

    public async Task<List<(DateTime StartOfWeek, DateTime EndOfWeek, double AverageMinutes)>> GetWeeklyStudyDataAsync(int studentId, int weeks = 8)
    {
        string cacheKey = $"WeeklyStudyData_Student_{studentId}";
        if (!_cache.TryGetValue(cacheKey, out List<(DateTime, DateTime, double)> weeklyData))
        {
            var records = await _repository.GetRecordsForLastWeeksAsync(studentId, weeks);
            weeklyData = CalculateWeeklyAveragesForChart(records, true, weeks);
            _cache.Set(cacheKey, weeklyData, GetDefaultCacheOptions());
        }
        return weeklyData;
    }

    public async Task<List<(DateTime StartOfWeek, DateTime EndOfWeek, double AverageMinutes)>> GetWeeklyStudyDataAsync(int weeks = 8)
    {
        const string cacheKey = "WeeklyStudyData_All";
        if (!_cache.TryGetValue(cacheKey, out List<(DateTime, DateTime, double)> weeklyData))
        {
            var records = await _repository.GetRecordsForLastWeeksAsync(null, weeks);
            weeklyData = CalculateWeeklyAveragesForChart(records, false, weeks);
            _cache.Set(cacheKey, weeklyData, GetDefaultCacheOptions());
        }
        return weeklyData;
    }

    public async Task<List<DateTime>> GetStudentAbsentWeeksAsync(int studentId)
    {
        var absentWeeks = new List<DateTime>();
        var allRecordDates = await _repository.GetAllRecordDatesAsync();
        if (!allRecordDates.Any()) return absentWeeks;

        var studentRecordDates = await _repository.GetStudentRecordDatesAsync(studentId);

        var firstWeekStart = DateUtils.GetPersianStartOfWeek(allRecordDates.First());
        var lastWeekStart = DateUtils.GetPersianStartOfWeek(allRecordDates.Last());

        for (var week = firstWeekStart; week <= lastWeekStart; week = week.AddDays(7))
        {
            var weekEnd = week.AddDays(7);
            if (!studentRecordDates.Any(d => d >= week && d < weekEnd))
            {
                absentWeeks.Add(week);
            }
        }
        return absentWeeks;
    }

    public Task<List<StudyRecord>> GetStudyRecordsForWeekAsync(Student student, DateTime startOfWeek)
    {
        var weekStart = DateUtils.GetPersianStartOfWeek(startOfWeek);
        return _repository.GetRecordsForWeekAsync(student.Id, weekStart);
    }

    public Task<int> GetStudyRecordCountAsync() => _repository.GetCountAsync();

    private double CalculateAverageFromRecords(List<StudyRecord> records)
    {
        if (!records.Any()) return 0;
        var weeklyTotals = records
            .GroupBy(r => DateUtils.GetPersianStartOfWeek(r.Date))
            .Select(g => g.Sum(r => r.MinutesStudied))
            .ToList();
        return weeklyTotals.Any() ? weeklyTotals.Average() : 0;
    }

    private List<(DateTime StartOfWeek, DateTime EndOfWeek, double AverageMinutes)> CalculateWeeklyAveragesForChart(List<StudyRecord> records, bool singleStudent, int weeks)
    {
        var results = new List<(DateTime, DateTime, double)>();
        if (!records.Any()) return results;

        var lastDate = records.Max(r => r.Date);
        var lastWeekStart = DateUtils.GetPersianStartOfWeek(lastDate);

        for (int i = 0; i < weeks; i++)
        {
            var weekStart = lastWeekStart.AddDays(-7 * i);
            var weekEnd = weekStart.AddDays(6);
            var weekRecords = records.Where(r => r.Date >= weekStart && r.Date <= weekEnd).ToList();
            double totalMinutes = weekRecords.Sum(r => r.MinutesStudied);
            double averageMinutes = 0;

            if (singleStudent)
            {
                averageMinutes = totalMinutes;
            }
            else
            {
                int distinctStudents = weekRecords.Select(r => r.StudentId).Distinct().Count();
                if (distinctStudents > 0)
                {
                    averageMinutes = totalMinutes / distinctStudents;
                }
            }
            results.Add((weekStart, weekEnd, averageMinutes));
        }
        return results.OrderBy(r => r.Item1).ToList();
    }

    private (DateTime startOfWeek, DateTime endOfWeek) GetCurrentPersianWeekRange()
    {
        DateTime today = DateTime.UtcNow.Date;
        DateTime startOfWeek = DateUtils.GetPersianStartOfWeek(today);
        DateTime endOfWeek = startOfWeek.AddDays(7);
        return (startOfWeek, endOfWeek);
    }
}