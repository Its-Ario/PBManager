using Microsoft.EntityFrameworkCore;
using PBManager.MVVM.Model;
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

        public async Task AddStudyRecordsAsync(List<StudyRecord> records, DateTime startOfWeek)
        {
            try
            {
                startOfWeek = startOfWeek.Date;
                var endOfWeek = startOfWeek.AddDays(6);

                var existingRecords = App.Db.StudyRecords
                    .Where(r => r.Student.Id == records.First().Student.Id &&
                               r.Date >= startOfWeek && r.Date <= endOfWeek)
                    .ToList();

                App.Db.StudyRecords.AddRange(records);
                await App.Db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving study records: {ex.Message}", ex);
            }
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
                    r.Date.Year,
                    Week = CultureInfo.InvariantCulture.Calendar
                        .GetWeekOfYear(r.Date, CalendarWeekRule.FirstDay, DayOfWeek.Saturday)
                })
                .Select(g => g.Sum(r => r.MinutesStudied))
                .ToList();

            return weeklyTotals.Count != 0 ? weeklyTotals.Average() : 0;
        }

        public async Task<double> GetWeeklyAverageAsync()
        {
            var records = await App.Db.StudyRecords
                .Select(r => new { r.Date, r.MinutesStudied })
                .ToListAsync();

            var weeklyTotals = records
                .GroupBy(r => new
                {
                    r.Date.Year,
                    Week = CultureInfo.InvariantCulture.Calendar
                        .GetWeekOfYear(r.Date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday)
                })
                .Select(g => g.Sum(r => r.MinutesStudied))
                .ToList();

            return weeklyTotals.Count != 0 ? weeklyTotals.Average() : 0;
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
                    r.Date.Year,
                    Week = CultureInfo.InvariantCulture.Calendar
                        .GetWeekOfYear(r.Date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday)
                })
                .Select(g => g.Sum(r => r.MinutesStudied))
                .ToList();

            return weeklyTotals.Count != 0 ? weeklyTotals.Average() : 0;
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
            int? studentClassId = await App.Db.Students
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

        public async Task<List<(DateTime StartOfWeek, DateTime EndOfWeek, double AverageMinutes)>> GetWeeklyStudyDataAsync(int weeks = 8)
        {
            var records = await GetRecordsForLastWeeksAsync(weeks: weeks);
            return CalculateWeeklyAverages(records, weeks: weeks);
        }

        public async Task<List<(DateTime StartOfWeek, DateTime EndOfWeek, double AverageMinutes)>> GetWeeklyStudyDataAsync(int studentId, int weeks = 8)
        {
            var records = await GetRecordsForLastWeeksAsync(studentId, weeks);
            return CalculateWeeklyAverages(records, singleStudent: true, weeks);
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

        public async Task<int> GetWeeklyAbsencesAsync()
        {
            var lastRecord = await App.Db.StudyRecords
                .OrderByDescending(r => r.Date)
                .FirstOrDefaultAsync();

            if (lastRecord == null)
            {
                return await App.Db.Students.CountAsync();
            }

            DateTime endOfWeek = lastRecord.Date.Date;
            DateTime startOfWeek = endOfWeek.AddDays(-7);

            var studentsWithData = await App.Db.StudyRecords
                .Where(r => r.Date >= startOfWeek && r.Date <= endOfWeek)
                .Select(r => r.StudentId)
                .Distinct()
                .ToListAsync();

            var studentsWithoutDataCount = await App.Db.Students
                .CountAsync(s => !studentsWithData.Contains(s.Id));

            return studentsWithoutDataCount;
        }

        public async Task<List<DateTime>> GetStudentAbsentWeeksAsync(int studentId)
        {
            var absentWeeks = new List<DateTime>();

            var lastRecordDate = await App.Db.StudyRecords.MaxAsync(r => (DateTime?)r.Date);

            if (lastRecordDate == null)
            {
                return absentWeeks;
            }
            DateTime lastDate = lastRecordDate.Value.Date;

            var studentRecordDates = await App.Db.StudyRecords
                .Where(r => r.StudentId == studentId)
                .Select(r => r.Date.Date)
                .ToListAsync();

            DateTime startDate;

            if (!studentRecordDates.Any())
            {
                var firstEverDate = await App.Db.StudyRecords.MinAsync(r => (DateTime?)r.Date);
                startDate = firstEverDate.Value.Date;
            }
            else
            {
                startDate = studentRecordDates.Min();
            }

            var dateSet = studentRecordDates.ToHashSet();

            DateTime weekStart = startDate;
            while (weekStart <= lastDate)
            {
                DateTime weekEnd = weekStart.AddDays(7);

                bool hasRecord = dateSet.Any(d => d >= weekStart && d < weekEnd);

                if (!hasRecord)
                {
                    absentWeeks.Add(weekStart);
                }

                weekStart = weekEnd;
            }

            return absentWeeks;
        }

        public async Task DeleteStudyRecordsForWeekAsync(Student student, DateTime startOfWeek)
        {
            var weekStart = startOfWeek.Date;
            var weekEnd = weekStart.AddDays(6);

            var recordsToDelete = await Task.Run(() =>
                App.Db.StudyRecords
                    .Where(r => r.Student.Id == student.Id &&
                               r.Date.Date >= weekStart &&
                               r.Date.Date <= weekEnd)
                    .ToList());

            if (recordsToDelete.Count != 0)
            {
                App.Db.StudyRecords.RemoveRange(recordsToDelete);
                await App.Db.SaveChangesAsync();
            }
        }

        public async Task<List<StudyRecord>> GetStudyRecordsForWeekAsync(Student student, DateTime startOfWeek)
        {
            var weekStart = startOfWeek.Date;
            var weekEnd = weekStart.AddDays(6);

            var records = await App.Db.StudyRecords
                    .Include(r => r.Subject)
                    .Where(r => r.Student.Id == student.Id &&
                               r.Date.Date >= weekStart &&
                               r.Date.Date <= weekEnd)
                    .ToListAsync();

            return records;
        }
        public static DateTime GetPersianStartOfWeek(DateTime date)
        {
            var dateOnly = date.Date;
            var daysFromSaturday = dateOnly.DayOfWeek switch
            {
                DayOfWeek.Saturday => 0,
                DayOfWeek.Sunday => 1,
                DayOfWeek.Monday => 2,
                DayOfWeek.Tuesday => 3,
                DayOfWeek.Wednesday => 4,
                DayOfWeek.Thursday => 5,
                DayOfWeek.Friday => 6,
                _ => 0,
            };
            return dateOnly.AddDays(-daysFromSaturday);
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

        private async Task<List<StudyRecord>> GetRecordsForLastWeeksAsync(int? studentId = null, int weeks = 8)
        {
            var lastSubmission = await App.Db.StudyRecords
                .OrderByDescending(r => r.Date)
                .Select(r => r.Date)
                .FirstOrDefaultAsync();

            if (lastSubmission == default)
                return [];

            var startDate = lastSubmission.AddDays((-7 * weeks) + 1);
            var endDate = lastSubmission.AddDays(1);

            var query = App.Db.StudyRecords
                .Where(r => r.Date >= startDate && r.Date <= endDate);

            if (studentId.HasValue)
                query = query.Where(r => r.StudentId == studentId.Value);

            return await query.OrderBy(r => r.Date).ToListAsync();
        }

        private List<(DateTime StartOfWeek, DateTime EndOfWeek, double AverageMinutes)>
CalculateWeeklyAverages(List<StudyRecord> records, bool singleStudent = false, int weeks = 8)
        {
            var results = new List<(DateTime, DateTime, double)>();

            if (records.Count == 0)
                return results;

            DateTime lastSubmission = records.Max(r => r.Date);
            DateTime startDate = lastSubmission.AddDays((-7 * weeks) + 1);
            DateTime endDate = lastSubmission.AddDays(1);

            DateTime cursor = endDate;
            while (cursor > startDate)
            {
                DateTime weekEnd = cursor.Date;
                DateTime weekStart = cursor.AddDays(-6).Date;

                var weekRecords = records
                    .Where(r => r.Date >= weekStart && r.Date <= weekEnd)
                    .ToList();

                int totalMinutes = weekRecords.Sum(r => r.MinutesStudied);

                double averageMinutes = singleStudent
                    ? totalMinutes
                    : weekRecords.Select(r => r.StudentId).Distinct().Any()
                        ? (double)totalMinutes / weekRecords.Select(r => r.StudentId).Distinct().Count()
                        : 0;

                results.Insert(0, (weekStart, weekEnd, averageMinutes));
                cursor = weekStart.AddDays(-1);
            }

            return results;
        }
    }
}