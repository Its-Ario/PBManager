using Microsoft.Extensions.Caching.Memory;
using PBManager.Application.Interfaces;
using PBManager.Core.Entities;
using PBManager.Core.Enums;
using PBManager.Core.Interfaces;

namespace PBManager.Application.Services
{

    public class GradeService(
        IGradeRepository repository,
        IMemoryCache cache,
        IAuditLogService auditLogService) : IGradeService
    {
        private static MemoryCacheEntryOptions GetDefaultCacheOptions(TimeSpan? absoluteExpiration = null) =>
            new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(10))
                .SetAbsoluteExpiration(absoluteExpiration ?? TimeSpan.FromHours(1));

        public async Task AddGradeAsync(GradeRecord grade)
        {
            await repository.AddAsync(grade);
            await repository.SaveChangesAsync();
            InvalidateGradeCaches(grade);

            await auditLogService.LogAsync(ActionType.Create, nameof(GradeRecord), grade.Id,
                $"ثبت نمره {grade.Score} برای دانش آموز با کد {grade.StudentId} در درس با کد {grade.SubjectId}");
        }

        public async Task UpdateGradeAsync(GradeRecord grade)
        {
            repository.Update(grade);
            await repository.SaveChangesAsync();
            InvalidateGradeCaches(grade);

            await auditLogService.LogAsync(ActionType.Update, nameof(GradeRecord), grade.Id,
                $"ویرایش نمره برای دانش آموز با کد {grade.StudentId} در درس با کد {grade.SubjectId} به {grade.Score}");
        }

        public async Task<bool> DeleteGradeAsync(int gradeId)
        {
            var grade = await repository.GetByIdAsync(gradeId);
            if (grade == null) return false;

            repository.Delete(grade);
            await repository.SaveChangesAsync();
            InvalidateGradeCaches(grade);

            await auditLogService.LogAsync(ActionType.Delete, nameof(GradeRecord), gradeId,
                $"حذف نمره {grade.Score} برای دانش آموز با کد {grade.StudentId} در درس با کد {grade.SubjectId}");

            return true;
        }

        private void InvalidateGradeCaches(GradeRecord grade)
        {
            cache.Remove($"Grades_Student_{grade.StudentId}");
            cache.Remove($"AverageGrade_Student_{grade.StudentId}");
            cache.Remove($"AverageGrade_Subject_{grade.SubjectId}");
            cache.Remove("OverallAverageGrade");
            cache.Remove($"Rank_Student_{grade.StudentId}_Subject_{grade.SubjectId}");
            cache.Remove($"TopSubject_Student_{grade.StudentId}");
        }

        public Task<List<GradeRecord>> GetGradesForStudentAsync(int studentId)
        {
            string cacheKey = $"Grades_Student_{studentId}";
            return cache.GetOrCreateAsync(cacheKey, entry =>
            {
                entry.SetOptions(GetDefaultCacheOptions());
                return repository.GetGradesForStudentAsync(studentId);
            })!;
        }

        public async Task<double> GetAverageGradeForStudentAsync(int studentId)
        {
            string cacheKey = $"AverageGrade_Student_{studentId}";
            if (!cache.TryGetValue(cacheKey, out double average))
            {
                var grades = await repository.GetGradesForStudentAsync(studentId);
                average = grades.Any() ? grades.Average(g => g.Score) : 0;
                cache.Set(cacheKey, average, GetDefaultCacheOptions());
            }
            return average;
        }

        public async Task<double> GetAverageGradeForSubjectAsync(int subjectId)
        {
            string cacheKey = $"AverageGrade_Subject_{subjectId}";
            if (!cache.TryGetValue(cacheKey, out double average))
            {
                var grades = await repository.GetGradesForSubjectAsync(subjectId);
                average = grades.Any() ? grades.Average(g => g.Score) : 0;
                cache.Set(cacheKey, average, GetDefaultCacheOptions());
            }
            return average;
        }

        public async Task<double> GetOverallAverageGradeAsync()
        {
            const string cacheKey = "OverallAverageGrade";
            if (!cache.TryGetValue(cacheKey, out double average))
            {
                var grades = await repository.GetAllAsync();
                average = grades.Any() ? grades.Average(g => g.Score) : 0;
                cache.Set(cacheKey, average, GetDefaultCacheOptions());
            }
            return average;
        }

        public async Task<(int Rank, int Total)> GetStudentRankBySubjectAsync(int studentId, int subjectId)
        {
            string cacheKey = $"Rank_Student_{studentId}_Subject_{subjectId}";
            if (!cache.TryGetValue(cacheKey, out (int Rank, int Total) rankInfo))
            {
                var subjectGrades = await repository.GetGradesForSubjectAsync(subjectId);
                var studentAverages = subjectGrades
                    .GroupBy(g => g.StudentId)
                    .Select(group => new { StudentId = group.Key, Average = group.Average(g => g.Score) })
                    .OrderByDescending(s => s.Average)
                    .ToList();

                var studentScore = studentAverages.FirstOrDefault(s => s.StudentId == studentId);
                if (studentScore == null)
                {
                    rankInfo = (0, studentAverages.Count);
                }
                else
                {
                    int rank = studentAverages.FindIndex(s => s.StudentId == studentId) + 1;
                    rankInfo = (rank, studentAverages.Count);
                }
                cache.Set(cacheKey, rankInfo, GetDefaultCacheOptions());
            }
            return rankInfo;
        }

        public async Task<Subject?> GetTopPerformingSubjectAsync(int studentId)
        {
            string cacheKey = $"TopSubject_Student_{studentId}";
            if (!cache.TryGetValue(cacheKey, out Subject? topSubject))
            {
                var studentGrades = await repository.GetGradesForStudentAsync(studentId);
                if (!studentGrades.Any())
                {
                    return null;
                }

                topSubject = studentGrades
                    .GroupBy(g => g.Subject)
                    .Select(group => new { Subject = group.Key, Average = group.Average(g => g.Score) })
                    .OrderByDescending(s => s.Average)
                    .FirstOrDefault()?
                    .Subject;

                cache.Set(cacheKey, topSubject, GetDefaultCacheOptions());
            }
            return topSubject;
        }
    }
}