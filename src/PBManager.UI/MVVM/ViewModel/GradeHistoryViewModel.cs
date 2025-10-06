using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using PBManager.Application.Interfaces;
using PBManager.Core.Entities;
using PBManager.UI.MVVM.View;
using System.Collections.ObjectModel;
using System.Windows;

namespace PBManager.UI.MVVM.ViewModel
{
    public partial class GradeHistoryViewModel(IGradeService gradeService, IExamService examService, IServiceProvider serviceProvider) : ObservableObject
    {
        private readonly IGradeService _gradeService = gradeService;
        private readonly IExamService _examService = examService;
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        [ObservableProperty]
        private Student _student;
        [ObservableProperty]
        private ObservableCollection<ExamGradeData> _records = [];
        [ObservableProperty]
        private ObservableCollection<ExamGradeData> _filteredRecords = [];
        [ObservableProperty]
        private int _totalExamsCount;
        [ObservableProperty]
        private int _presentExamsCount;
        [ObservableProperty]
        private int _absentExamsCount;
        [ObservableProperty]
        private bool _showAll = true;
        [ObservableProperty]
        private bool _showPresent;
        [ObservableProperty]
        private bool _showAbsent;
        [ObservableProperty]
        private string _emptyStateMessage = string.Empty;
        [ObservableProperty]
        private bool _isEmptyState;

        public async Task InitializeAsync(Student student)
        {
            Student = student;
            await LoadData(Student.Id);
        }

        public async Task LoadData(int studentId)
        {
            try
            {
                var allExams = await _examService.GetAllExamsWithSubjectsAsync();
                var studentGrades = await _gradeService.GetGradesForStudentAsync(studentId);

                if (allExams == null || allExams.Count == 0)
                {
                    Records = [];
                    UpdateStatistics();
                    ApplyFilter();
                    return;
                }

                var examGradeDataList = new List<ExamGradeData>();

                foreach (var exam in allExams)
                {
                    var examGrades = studentGrades.Where(g => g.ExamId == exam.Id).ToList();

                    ExamGradeData examData;

                    if (examGrades.Count == 0)
                    {
                        examData = new ExamGradeData
                        {
                            ExamId = exam.Id,
                            ExamName = exam.Name,
                            ExamDate = exam.Date,
                            MaxScore = exam.MaxScore,
                            TotalScore = 0,
                            AverageScore = 0,
                            SubjectCount = 0,
                            SubjectScores = [],
                            GradeRecords = [],
                            IsAbsent = true
                        };
                    }
                    else
                    {
                        var subjectScores = examGrades
                            .GroupBy(g => new { g.SubjectId, g.Subject.Name })
                            .Select(group => new SubjectScore
                            {
                                SubjectId = group.Key.SubjectId,
                                SubjectName = group.Key.Name,
                                Score = group.Sum(g => g.Score)
                            })
                            .ToList();

                        examData = new ExamGradeData
                        {
                            ExamId = exam.Id,
                            ExamName = exam.Name,
                            ExamDate = exam.Date,
                            MaxScore = exam.MaxScore,
                            TotalScore = examGrades.Sum(g => g.Score),
                            AverageScore = examGrades.Average(g => g.Score),
                            SubjectCount = subjectScores.Count,
                            SubjectScores = new ObservableCollection<SubjectScore>(subjectScores),
                            GradeRecords = examGrades,
                            IsAbsent = false
                        };
                    }

                    examGradeDataList.Add(examData);
                }

                Records = new ObservableCollection<ExamGradeData>(examGradeDataList.OrderByDescending(e => e.ExamDate));
                UpdateStatistics();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بارگذاری داده ها: {ex.Message}", "خطا",
                               MessageBoxButton.OK, MessageBoxImage.Error);
                Records = [];
                UpdateStatistics();
                ApplyFilter();
            }
        }

        private void UpdateStatistics()
        {
            if (Records == null)
            {
                TotalExamsCount = 0;
                PresentExamsCount = 0;
                AbsentExamsCount = 0;
                return;
            }

            TotalExamsCount = Records.Count;
            PresentExamsCount = Records.Count(r => !r.IsAbsent);
            AbsentExamsCount = Records.Count(r => r.IsAbsent);
        }

        [RelayCommand]
        private void SetFilter(string filterType)
        {
            ShowAll = false;
            ShowPresent = false;
            ShowAbsent = false;

            switch (filterType)
            {
                case "All":
                    ShowAll = true;
                    break;
                case "Present":
                    ShowPresent = true;
                    break;
                case "Absent":
                    ShowAbsent = true;
                    break;
            }

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (Records == null || Records.Count == 0)
            {
                FilteredRecords = [];
                EmptyStateMessage = "هیچ آزمونی برای نمایش وجود ندارد";
                IsEmptyState = true;
                return;
            }

            IEnumerable<ExamGradeData> filtered = Records;

            if (ShowPresent)
            {
                filtered = Records.Where(r => !r.IsAbsent);
                EmptyStateMessage = "هیچ آزمون حضوری یافت نشد";
            }
            else if (ShowAbsent)
            {
                filtered = Records.Where(r => r.IsAbsent);
                EmptyStateMessage = "هیچ آزمون غیبتی یافت نشد";
            }
            else
            {
                EmptyStateMessage = "هیچ آزمونی برای نمایش وجود ندارد";
            }

            FilteredRecords = new ObservableCollection<ExamGradeData>(filtered);
            IsEmptyState = FilteredRecords.Count == 0;
        }

        [RelayCommand]
        private async Task EditRecordAsync(ExamGradeData examRecord)
        {
            if (examRecord == null) return;

            var view = _serviceProvider.GetRequiredService<AddGradeRecordView>();

            if (examRecord.IsAbsent)
            {
                if (view.DataContext is AddGradeRecordViewModel vm)
                {
                    _ = vm.InitializeAsync(Student, examRecord.ExamId);
                }
                view.ShowDialog();
                return;
            }

            try
            {
                if (view.DataContext is AddGradeRecordViewModel vm)
                {
                    _ = vm.InitializeAsync(Student, examRecord.GradeRecords);
                }
                var result = view.ShowDialog();

                if (result == true)
                {
                    await LoadData(Student.Id);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بارگذاری داده ها: {ex.Message}", "خطا",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class ExamGradeData
    {
        public int ExamId { get; set; }
        public string ExamName { get; set; }
        public DateTime ExamDate { get; set; }
        public int MaxScore { get; set; }
        public double TotalScore { get; set; }
        public double AverageScore { get; set; }
        public int SubjectCount { get; set; }
        public ObservableCollection<SubjectScore> SubjectScores { get; set; }
        public List<GradeRecord> GradeRecords { get; set; }
        public bool IsAbsent { get; set; }
    }

    public class SubjectScore
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }
        public double Score { get; set; }
    }
}