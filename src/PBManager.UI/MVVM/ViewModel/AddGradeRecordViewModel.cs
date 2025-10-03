using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PBManager.Application.Interfaces;
using PBManager.Core.Entities;
using PBManager.UI.MVVM.ViewModel.Helpers;
using System.Collections.ObjectModel;
using System.Windows;

namespace PBManager.UI.MVVM.ViewModel
{
    public partial class AddGradeRecordViewModel(IExamService examService, IGradeService gradeService) : ObservableObject
    {
        private readonly IExamService _examService = examService;
        private readonly IGradeService _gradeService = gradeService;
        private Student _student;

        public ObservableCollection<Exam> AvailableExams { get; } = [];
        public ObservableCollection<GradeEntry> GradeEntries { get; } = [];

        [ObservableProperty]
        private Exam? _selectedExam;

        [ObservableProperty]
        private bool _isEditMode;

        public async Task InitializeAsync(Student student)
        {
            _student = student;
            await LoadAvailableExamsAsync();
        }

        private async Task LoadAvailableExamsAsync()
        {
            var exams = await _examService.GetAllExamsWithSubjectsAsync();
            AvailableExams.Clear();
            foreach (var exam in exams)
            {
                AvailableExams.Add(exam);
            }
        }

        partial void OnSelectedExamChanged(Exam? exam)
        {
            if (exam == null)
            {
                GradeEntries.Clear();
                return;
            }
            _ = LoadGradesForSelectedExamAsync(exam);
        }

        private async Task LoadGradesForSelectedExamAsync(Exam exam)
        {
            if (_student == null) return;

            GradeEntries.Clear();
            try
            {
                var existingGrades = await _gradeService.GetGradesForStudentAsync(_student.Id, exam.Id);

                IsEditMode = existingGrades.Count != 0;

                if (exam.Subjects != null)
                {
                    foreach (var subject in exam.Subjects)
                    {
                        var existingGrade = existingGrades.FirstOrDefault(g => g.SubjectId == subject.Id);
                        string score = existingGrade?.Score.ToString() ?? string.Empty;
                        GradeEntries.Add(new GradeEntry(subject, score));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بارگذاری داده‌ها: {ex.Message}",
                               "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task SaveGradesAsync()
        {
            if (SelectedExam == null || _student == null) return;

            var gradeRecordsToSave = new List<GradeRecord>();
            double maxScore = SelectedExam.MaxScore;
            
            foreach (var entry in GradeEntries)
            {
                if (double.TryParse(entry.Score, out double score))
                {
                    double normalizedScore = Math.Round((score / maxScore) * 100, 2);

                    gradeRecordsToSave.Add(new GradeRecord
                    {
                        StudentId = _student.Id,
                        SubjectId = entry.Subject.Id,
                        ExamId = SelectedExam.Id,
                        Score = normalizedScore,
                        Date = SelectedExam.Date
                    });
                }
            }

            try
            {
                if (IsEditMode)
                {
                    await _gradeService.DeleteRecords(_student.Id, SelectedExam.Id);
                }
                await _gradeService.SaveGradesForExamAsync(_student.Id, SelectedExam.Id, gradeRecordsToSave);

                string message = IsEditMode
                    ? $"نمرات آزمون با موفقیت بروزرسانی شد! ({gradeRecordsToSave.Count} رکورد)"
                    : $"نمرات آزمون با موفقیت ثبت شد! ({gradeRecordsToSave.Count} رکورد)";
                MessageBox.Show(message, "موفقیت", MessageBoxButton.OK, MessageBoxImage.Information);

                IsEditMode = true;
            }
            catch (Exception ex)
            {
                string errorMessage = IsEditMode
                    ? $"خطا در بروزرسانی اطلاعات: {ex.Message}"
                    : $"خطا در ثبت اطلاعات: {ex.Message}";
                MessageBox.Show(errorMessage, "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}