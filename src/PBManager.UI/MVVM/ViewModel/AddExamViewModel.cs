using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mohsen;
using PBManager.Application.Interfaces;
using PBManager.Core.Entities;
using System.Collections.ObjectModel;
using System.Windows;

namespace PBManager.UI.MVVM.ViewModel
{
    public partial class AddExamViewModel(ISubjectService subjectService, IExamService examService) : ObservableObject
    {
        private readonly ISubjectService _subjectService = subjectService;
        private readonly IExamService _examService = examService;

        public ObservableCollection<Subject> AvailableSubjects { get; } = [];

        [ObservableProperty]
        private string? _name;

        [ObservableProperty]
        private string? _maxScore = "20";

        [ObservableProperty]
        private PersianDate _selectedDate = PersianDate.Today;

        [ObservableProperty]
        private Subject? _selectedSubject;

        [ObservableProperty]
        private bool _isEditMode;

        private Exam? _existingExam;

        public async Task InitializeAsync()
        {
            IsEditMode = false;
            await LoadAvailableSubjectsAsync();
        }

        public async Task InitializeAsync(Exam exam)
        {
            IsEditMode = true;
            _existingExam = exam;

            await LoadAvailableSubjectsAsync();

            Name = exam.Name;
            MaxScore = exam.MaxScore.ToString();
            SelectedDate = new PersianDate(exam.Date);

            if (exam.Subjects != null && exam.Subjects.Count > 0)
                SelectedSubject = AvailableSubjects.FirstOrDefault(s => s.Id == exam.Subjects.First().Id);
        }

        private async Task LoadAvailableSubjectsAsync()
        {
            var subjects = await _subjectService.GetSubjectsAsync(tracking: true);
            AvailableSubjects.Clear();
            foreach (var subject in subjects)
            {
                AvailableSubjects.Add(subject);
            }
        }

        [RelayCommand]
        private async Task SubmitAsync()
        {
            if (Name == null || MaxScore == null || SelectedDate == null || SelectedSubject == null)
            {
                MessageBox.Show(".لطفا همه فیلد هارا پر کنید", "خطا", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(MaxScore, out int maxScoreInt))
            {
                MessageBox.Show(".حداکثر نمره عددی وارد کنید", "خطا", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (IsEditMode && _existingExam != null)
                {
                    _existingExam.Name = Name;
                    _existingExam.Date = SelectedDate.ToDateTime();
                    _existingExam.MaxScore = maxScoreInt;
                    _existingExam.Subjects = [SelectedSubject];

                    await _examService.UpdateExamAsync(_existingExam);

                    MessageBox.Show($"آزمون {Name} با موفقیت ویرایش شد.", "موفقیت", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var newExam = new Exam
                    {
                        Name = Name,
                        Date = SelectedDate.ToDateTime(),
                        Subjects = [SelectedSubject],
                        MaxScore = maxScoreInt
                    };

                    await _examService.AddExamAsync(newExam);

                    MessageBox.Show($"آزمون {Name} در تاریخ {SelectedDate} ثبت شد.", "موفقیت", MessageBoxButton.OK, MessageBoxImage.Information);

                    Name = null;
                    MaxScore = "20";
                    SelectedDate = PersianDate.Today;
                    SelectedSubject = null;
                }
            }
            catch (Exception ex)
            {
                var errorMessage = IsEditMode
                    ? $"خطا در بروزرسانی آزمون: {ex.Message}"
                    : $"خطا در ثبت آزمون: {ex.Message}";

                MessageBox.Show(errorMessage, "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
