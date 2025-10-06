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
        public async Task InitializeAsync(Student student, int examId)
        {
            _student = student;
            await LoadAvailableExamsAsync();

            SelectedExam = AvailableExams.FirstOrDefault(e => e.Id == examId);
        }
        public async Task InitializeAsync(Student student, List<GradeRecord> existingGrades)
        {
            _student = student;
            IsEditMode = true;

            await LoadAvailableExamsAsync();

            if (existingGrades.Count > 0)
            {
                var examId = existingGrades[0].ExamId;
                SelectedExam = AvailableExams.FirstOrDefault(e => e.Id == examId);

                if (SelectedExam != null)
                {
                    await LoadExistingGradesAsync(SelectedExam, existingGrades);
                }
            }
        }
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

        partial void OnSelectedExamChanged(Exam? value)
        {
            if (value == null)
            {
                GradeEntries.Clear();
                return;
            }

            if (!IsEditMode)
            {
                _ = LoadGradesForSelectedExamAsync(value);
            }
        }

        private async Task LoadGradesForSelectedExamAsync(Exam exam)
        {
            if (_student == null) return;

            GradeEntries.Clear();
            try
            {
                var existingGrades = await _gradeService.GetGradesForStudentAsync(_student.Id, exam.Id);

                if (existingGrades.Count != 0)
                {
                    IsEditMode = true;
                    await LoadExistingGradesAsync(exam, existingGrades);
                }
                else
                {
                    if (exam.Subjects != null)
                    {
                        foreach (var subject in exam.Subjects)
                        {
                            GradeEntries.Add(new GradeEntry(subject, string.Empty));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بارگذاری داده ها: {ex.Message}",
                               "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadExistingGradesAsync(Exam exam, List<GradeRecord> existingGrades)
        {
            GradeEntries.Clear();

            if (exam.Subjects != null)
            {
                foreach (var subject in exam.Subjects)
                {
                    var existingGrade = existingGrades.FirstOrDefault(g => g.SubjectId == subject.Id);

                    GradeEntries.Add(new GradeEntry(subject, existingGrade.Score.ToString()));
                }
            }

            await Task.CompletedTask;
        }

        [RelayCommand]
        private async Task SaveGradesAsync()
        {
            if (SelectedExam == null || _student == null) return;

            var gradeRecordsToSave = new List<GradeRecord>();

            foreach (var entry in GradeEntries)
            {
                if (double.TryParse(entry.Score, out double score))
                {
                    gradeRecordsToSave.Add(new GradeRecord
                    {
                        StudentId = _student.Id,
                        SubjectId = entry.Subject.Id,
                        ExamId = SelectedExam.Id,
                        Score = score,
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