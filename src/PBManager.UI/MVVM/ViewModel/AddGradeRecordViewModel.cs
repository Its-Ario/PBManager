using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PBManager.Application.Interfaces;
using PBManager.Core.Entities;
using PBManager.UI.MVVM.ViewModel.Helpers;
using System.Collections.ObjectModel;
using System.Windows;

namespace PBManager.UI.MVVM.ViewModel
{
    [ObservableObject]
    public partial class AddGradeRecordViewModel
    {
        private readonly IExamService _examService;
        private readonly IGradeService _gradeService;
        private Student _student;

        public ObservableCollection<Exam> AvailableExams { get; } = new();
        public ObservableCollection<GradeEntry> GradeEntries { get; } = new();

        [ObservableProperty]
        private Exam? _selectedExam;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SubmitButtonText))]
        private bool _isEditMode;

        public string SubmitButtonText => IsEditMode ? "Update Grades" : "Save Grades";

        public AddGradeRecordViewModel(IExamService examService, IGradeService gradeService)
        {
            _examService = examService;
            _gradeService = gradeService;
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

                IsEditMode = existingGrades.Any();

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
                MessageBox.Show($"An error occurred while loading grades: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                    ? $"Grades have been successfully updated for {SelectedExam.Name}."
                    : $"Grades have been successfully saved for {SelectedExam.Name}.";
                MessageBox.Show(message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                IsEditMode = true;
            }
            catch (Exception ex)
            {
                string errorMessage = IsEditMode
                    ? $"Failed to update grades: {ex.Message}"
                    : $"Failed to save grades: {ex.Message}";
                MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}