using Microsoft.Extensions.Caching.Memory;
using PBManager.Application.DTOs;
using PBManager.Application.Interfaces;
using PBManager.Core.Entities;
using PBManager.Core.Enums;
using PBManager.Core.Interfaces;
using System.Diagnostics;

namespace PBManager.Application.Services
{
    public class GradeService(
        IGradeRepository repository,
        IStudentRepository studentRepository,
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

        public Task<List<GradeRecord>> GetGradesForStudentAsync(int studentId, int? examId = null)
        {
            string cacheKey = examId == null
                ? $"Grades_Student_{studentId}"
                : $"Grades_Student_{studentId}_Exam_{examId}";

            return cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.SetOptions(GetDefaultCacheOptions());

                var grades = await repository.GetGradesForStudentAsync(studentId);

                if (examId.HasValue)
                {
                    grades = grades.Where(g => g.ExamId == examId).ToList();
                }

                Debug.WriteLine(grades.Count);

                return grades;
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

        public async Task<List<StudentExamScore>> GetAllExamScoresForStudentAsync(int studentId)
        {
            string cacheKey = $"ExamScores_Student_{studentId}";
            if (!cache.TryGetValue(cacheKey, out List<StudentExamScore> examScores))
            {
                var grades = await repository.GetGradesForStudentAsync(studentId);

                examScores = grades
                    .Where(g => g.Exam != null)
                    .GroupBy(g => g.Exam)
                    .Select(group => new StudentExamScore
                    {
                        StudentId = studentId,
                        ExamId = group.Key.Id,
                        ExamName = group.Key.Name,
                        ExamDate = group.Key.Date,
                        AverageScore = group.Average(g => g.Score)
                    })
                    .OrderByDescending(e => e.ExamDate)
                    .ToList();

                cache.Set(cacheKey, examScores, GetDefaultCacheOptions());
            }
            return examScores;
        }

        public async Task<List<StudentExamScore>> GetRankedScoresForExamAsync(int examId)
        {
            string cacheKey = $"RankedScores_Exam_{examId}";
            if (!cache.TryGetValue(cacheKey, out List<StudentExamScore> rankedScores))
            {
                var grades = await repository.GetGradesForExamAsync(examId);

                rankedScores = grades
                    .GroupBy(g => g.Student)
                    .Select(group => new StudentExamScore
                    {
                        StudentId = group.Key.Id,
                        ExamId = examId,
                        ExamName = group.First().Exam?.Name ?? "Unknown",
                        ExamDate = group.First().Exam?.Date ?? DateTime.MinValue,
                        AverageScore = group.Average(g => g.Score)
                    })
                    .OrderByDescending(s => s.AverageScore)
                    .ToList();

                cache.Set(cacheKey, rankedScores, GetDefaultCacheOptions());
            }
            return rankedScores;
        }

        public async Task SaveGradesForExamAsync(int studentId, int examId, IEnumerable<GradeRecord> gradeRecords)
        {
            var records = gradeRecords.ToList();
            if (records.Any())
            {
                await repository.AddRangeAsync(records);
                await repository.SaveChangesAsync();
            }

            var cacheKey = $"Grades_Student_{studentId}_Exam_{examId}";
            cache.Remove(cacheKey);
            cache.Remove($"Grades_Student_{studentId}");
        }

        public async Task DeleteRecords(int studentId, int examId)
        {
            var records = await repository.GetGradesForExamAsync(examId);
            var studentRecords = records.Where(g => g.StudentId == studentId);

            repository.Delete(studentRecords);
            await repository.SaveChangesAsync();
        }

        public async Task<int> GetOverallExamRankAsync(int studentId)
        {
            string cacheKey = $"OverallExamRank_{studentId}";
            if (!cache.TryGetValue(cacheKey, out int rankInfo))
            {
                var allGrades = await repository.GetAllExamGradesAsync();
                rankInfo = CalculateRank(allGrades, studentId);
                cache.Set(cacheKey, rankInfo, GetDefaultCacheOptions());
            }
            return rankInfo;
        }

        public async Task<int> GetClassExamRankAsync(int studentId)
        {
            string cacheKey = $"ClassExamRank_{studentId}";
            if (!cache.TryGetValue(cacheKey, out int rankInfo))
            {
                var student = await studentRepository.FindByIdAsync(studentId);
                    if (student?.ClassId == null) return 0;

                var classGrades = await repository.GetAllExamGradesForClassAsync(student.ClassId);
                rankInfo = CalculateRank(classGrades, studentId);
                cache.Set(cacheKey, rankInfo, GetDefaultCacheOptions()) ;
            }
            return rankInfo;
        }

        private int CalculateRank(List<GradeRecord> grades, int studentId)
        {
            if (!grades.Any()) return 0;

            var studentAverages = grades
                .GroupBy(g => g.Student)
                .Select(group => new
                {
                    StudentId = group.Key.Id,
                    AverageScore = group.Average(g => g.Score)
                })
                .OrderByDescending(x => x.AverageScore)
                .ToList();

            var studentIndex = studentAverages.FindIndex(x => x.StudentId == studentId);

            if (studentIndex == -1)
            {
                return 0;
            }

            return studentIndex + 1;
        }
    }
}