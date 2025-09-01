using Microsoft.EntityFrameworkCore;
using PBManager.MVVM.Model;
using System;
using System.Diagnostics;
using System.Globalization;

namespace PBManager.Services
{
    internal class StudyRecordService : IStudyRecordService
    {
        public async Task AddStudyRecordAsync(StudyRecord record)
        {
            await App.Db.StudyRecords.AddAsync(record);
            await App.Db.SaveChangesAsync();
        }

        public async Task AddStudyRecordsAsync(List<StudyRecord> records)
        {
            await App.Db.StudyRecords.AddRangeAsync(records);
            await App.Db.SaveChangesAsync();
        }

        public async Task<List<StudyRecord>> GetStudyRecordsForStudentAsync(int studentId)
        {
            return await App.Db.StudyRecords
                        .Include(r => r.Subject)
                        .Include(r => r.Student)
                        .Where(r => r.StudentId == studentId)
                        .ToListAsync();
        }

        public async Task<double> GetStudentWeeklyAverageAsync(int studentId)
        {
            var records = await App.Db.StudyRecords
                .Where(r => r.StudentId == studentId)
                .Select(r => new { r.Date, r.MinutesStudied })
                .ToListAsync();

            var weeklyTotals = records
                .GroupBy(r => new
                {
                    Year = r.Date.Year,
                    Week = CultureInfo.InvariantCulture.Calendar
                        .GetWeekOfYear(r.Date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday)
                })
                .Select(g => g.Sum(r => r.MinutesStudied))
                .ToList();

            return weeklyTotals.Any() ? weeklyTotals.Average() : 0;
        }

        public async Task<double> GetWeeklyAverageAsync()
        {
            var records = await App.Db.StudyRecords
                .Select(r => new { r.Date, r.MinutesStudied })
                .ToListAsync();

            var weeklyTotals = records
                .GroupBy(r => new
                {
                    Year = r.Date.Year,
                    Week = CultureInfo.InvariantCulture.Calendar
                        .GetWeekOfYear(r.Date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday)
                })
                .Select(g => g.Sum(r => r.MinutesStudied))
                .ToList();

            return weeklyTotals.Any() ? weeklyTotals.Average() : 0;
        }

        public async Task<double> GetSubjectWeeklyAverageAsync(int subjectId)
        {
            var records = await App.Db.StudyRecords
                .Where(r => r.SubjectId == subjectId)
                .Select(r => new { r.Date, r.MinutesStudied })
                .ToListAsync();

            var weeklyTotals = records
                .GroupBy(r => new
                {
                    Year = r.Date.Year,
                    Week = CultureInfo.InvariantCulture.Calendar
                        .GetWeekOfYear(r.Date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday)
                })
                .Select(g => g.Sum(r => r.MinutesStudied))
                .ToList();

            return weeklyTotals.Any() ? weeklyTotals.Average() : 0;
        }

        public async Task<int> GetGlobalWeeklyRankAsync(int studentId)
        {
            var (startDate, endDate) = await GetCurrentWeekRangeAsync();
            if (!startDate.HasValue || !endDate.HasValue)
                return 0;

            var studentTotalMinutes = await App.Db.StudyRecords
                .Where(r => r.StudentId == studentId &&
                            r.Date >= startDate && r.Date < endDate)
                .SumAsync(r => r.MinutesStudied);

            var higherRankedCount = await App.Db.StudyRecords
                .Where(r => r.Date >= startDate && r.Date < endDate)
                .GroupBy(r => r.StudentId)
                .CountAsync(g => g.Sum(r => r.MinutesStudied) > studentTotalMinutes);

            return higherRankedCount + 1;
        }

        public async Task<int> GetClassWeeklyRankAsync(int studentId)
        {
            var studentClassId = await App.Db.Students
                .Where(s => s.Id == studentId)
                .Select(s => s.ClassId)
                .FirstOrDefaultAsync();

            if (studentClassId == null)
                return 0;

            var (startDate, endDate) = await GetCurrentWeekRangeForClassAsync(studentClassId);
            if (!startDate.HasValue || !endDate.HasValue)
                return 0;

            var studentTotalMinutes = await App.Db.StudyRecords
                .Where(r => r.Student.ClassId == studentClassId &&
                            r.StudentId == studentId &&
                            r.Date >= startDate && r.Date < endDate)
                .SumAsync(r => r.MinutesStudied);

            var higherRankedCount = await App.Db.StudyRecords
                .Where(r => r.Student.ClassId == studentClassId &&
                            r.Date >= startDate && r.Date < endDate)
                .GroupBy(r => r.StudentId)
                .CountAsync(g => g.Sum(r => r.MinutesStudied) > studentTotalMinutes);

            return higherRankedCount + 1;
        }

        public async Task<List<(DateTime StartOfWeek, DateTime EndOfWeek, int TotalMinutes)>>
    GetWeeklyStudyDataAsync()
        {
            var lastSubmission = await App.Db.StudyRecords
                .OrderByDescending(r => r.Date)
                .Select(r => r.Date)
                .FirstOrDefaultAsync();

            if (lastSubmission == default)
                return new List<(DateTime, DateTime, int)>();

            var startDate = lastSubmission.AddDays((-7 * 8) + 1);
            var endDate = lastSubmission.AddDays(1);

            var records = await App.Db.StudyRecords
                .Where(r => r.Date >= startDate && r.Date <= endDate)
                .OrderBy(r => r.Date)
                .ToListAsync();

            var results = new List<(DateTime, DateTime, int)>();

            DateTime cursor = endDate;
            while (cursor > startDate)
            {
                DateTime weekEnd = cursor.Date;
                DateTime weekStart = cursor.AddDays(-6).Date;

                int totalMinutes = records
                    .Where(r => r.Date >= weekStart && r.Date <= weekEnd)
                    .Sum(r => r.MinutesStudied);

                results.Insert(0, (weekStart, weekEnd, totalMinutes));
                cursor = weekStart.AddDays(-1);
            }

            return results;
        }
        public async Task<Subject?> GetMostStudiedWeeklySubjectAsync(int studentId)
        {
            var (startDate, endDate) = await GetCurrentWeekRangeForStudentAsync(studentId);
            if (!startDate.HasValue || !endDate.HasValue)
                return null;

            return await App.Db.StudyRecords
                .Where(r => r.StudentId == studentId &&
                            r.Date >= startDate && r.Date < endDate)
                .GroupBy(r => r.Subject)
                .OrderByDescending(g => g.Sum(r => r.MinutesStudied))
                .Select(g => g.Key)
                .FirstOrDefaultAsync();
        }

        public async Task<Subject?> GetMostStudiedWeeklySubjectAsync()
        {
            var (startDate, endDate) = await GetCurrentWeekRangeAsync();
            if (!startDate.HasValue || !endDate.HasValue)
                return null;

            return await App.Db.StudyRecords
                .Where(r => r.Date >= startDate && r.Date < endDate)
                .GroupBy(r => r.Subject)
                .OrderByDescending(g => g.Sum(r => r.MinutesStudied))
                .Select(g => g.Key)
                .FirstOrDefaultAsync();
        }

        private async Task<(DateTime? startDate, DateTime? endDate)> GetCurrentWeekRangeAsync()
        {
            var lastRecord = await App.Db.StudyRecords
                .OrderByDescending(r => r.Date)
                .Select(r => r.Date)
                .FirstOrDefaultAsync();

            if (lastRecord == default)
                return (null, null);

            var endDate = lastRecord.Date.AddDays(1);
            var startDate = lastRecord.AddDays(-6);

            return (startDate, endDate);
        }

        private async Task<(DateTime? startDate, DateTime? endDate)> GetCurrentWeekRangeForStudentAsync(int studentId)
        {
            var lastRecord = await App.Db.StudyRecords
                .Where(r => r.StudentId == studentId)
                .OrderByDescending(r => r.Date)
                .Select(r => r.Date)
                .FirstOrDefaultAsync();

            if (lastRecord == default)
                return (null, null);

            var endDate = lastRecord.Date.AddDays(1);
            var startDate = lastRecord.AddDays(-6);

            return (startDate, endDate);
        }

        private async Task<(DateTime? startDate, DateTime? endDate)> GetCurrentWeekRangeForClassAsync(int? classId)
        {
            var lastRecord = await App.Db.StudyRecords
                .Where(r => r.Student.ClassId == classId)
                .OrderByDescending(r => r.Date)
                .Select(r => r.Date)
                .FirstOrDefaultAsync();

            if (lastRecord == default)
                return (null, null);

            var endDate = lastRecord.Date.AddDays(1);
            var startDate = lastRecord.AddDays(-6);

            return (startDate, endDate);
        }
    }
}