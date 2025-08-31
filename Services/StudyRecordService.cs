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
                        .Where(r => r.Student.Id == studentId)
                        .ToListAsync();
        }

        public async Task<double> GetWeeklyAverageAsync(int studentId)
        {
            var records = await App.Db.StudyRecords
                .Where(r => r.StudentId == studentId)
                .ToListAsync();

            var weeklyAverages = records
                .GroupBy(r => new
                {
                    Year = r.Date.Year,
                    Week = CultureInfo.CurrentCulture.Calendar
                        .GetWeekOfYear(r.Date, CalendarWeekRule.FirstDay, DayOfWeek.Monday)
                })
                .Select(g => g.Sum(r => r.MinutesStudied))
                .ToList();


            return weeklyAverages.Any() ? weeklyAverages.Average() : 0;
        }

        public async Task<double> GetWeeklyAverageAsync()
        {
            var records = await App.Db.StudyRecords
                .ToListAsync();

            var weeklyAverages = records
                .GroupBy(r => new
                {
                    Year = r.Date.Year,
                    Week = CultureInfo.CurrentCulture.Calendar
                        .GetWeekOfYear(r.Date, CalendarWeekRule.FirstDay, DayOfWeek.Monday)
                })
                .Select(g => g.Sum(r => r.MinutesStudied))
                .ToList();

            return weeklyAverages.Any() ? weeklyAverages.Average() : 0;
        }
        public async Task<int> GetGlobalRankAsync(int studentId)
        {
            var totals = await App.Db.StudyRecords
                .GroupBy(r => r.Student.Id)
                .Select(g => new
                {
                    StudentId = g.Key,
                    TotalMinutes = g.Sum(r => r.MinutesStudied)
                })
                .OrderByDescending(t => t.TotalMinutes)
                .ToListAsync();

            var rank = totals.FindIndex(t => t.StudentId == studentId);
            return rank >= 0 ? rank + 1 : 0;
        }
        public async Task<int> GetClassRankAsync(int studentId)
        {
            var student = await App.Db.Students
                .Where(s => s.Id == studentId)
                .Select(s => new { s.Id, s.ClassId })
                .FirstOrDefaultAsync();

            if (student == null) return 0;

            var totals = await App.Db.StudyRecords
                .Where(r => r.Student.ClassId == student.ClassId)
                .GroupBy(r => r.StudentId)
                .Select(g => new
                {
                    StudentId = g.Key,
                    TotalMinutes = g.Sum(r => r.MinutesStudied)
                })
                .OrderByDescending(t => t.TotalMinutes)
                .ToListAsync();

            var target = totals.FindIndex(t => t.StudentId == studentId);
            var result = target >= 0 ? target + 1 : 0;

            Debug.WriteLine("Target Is " + result);
            return result;
        }

    }
}
