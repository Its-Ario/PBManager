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

        public AddGradeRecordViewModel(IExamService examService, IGradeService gradeService)
        {
            _examService = examService;
            _gradeService = gradeService;
        }

        public async Task InitializeAsync(Student student)
        {
            _student = student;
            var exams = await _examService.GetAllExamsWithSubjectsAsync();
            AvailableExams.Clear();
            foreach (var exam in exams)
            {
                AvailableExams.Add(exam);
            }
        }

        partial void OnSelectedExamChanged(Exam? value)
        {
            GradeEntries.Clear();
            if (value?.Subjects != null)
            {
                foreach (var subject in value.Subjects)
                {
                    GradeEntries.Add(new GradeEntry(subject));
                }
            }
        }

        [RelayCommand]
        private async Task SaveGradesAsync()
        {
            if (SelectedExam == null) return;

            foreach (var entry in GradeEntries)
            {
                if (double.TryParse(entry.Score, out double score))
                {
                    var gradeRecord = new GradeRecord
                    {
                        StudentId = _student.Id,
                        SubjectId = entry.Subject.Id,
                        ExamId = SelectedExam.Id,
                        Score = score,
                        Date = SelectedExam.Date
                    };
                    await _gradeService.AddGradeAsync(gradeRecord);
                }
            }
            MessageBox.Show("Submitted");
        }
    }
}
