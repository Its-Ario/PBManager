using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mohsen;
using PBManager.Application.Interfaces;
using PBManager.Core.Entities;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls.Ribbon;

namespace PBManager.UI.MVVM.ViewModel
{
    public partial class AddExamViewModel : ObservableObject
    {
        private readonly ISubjectService _subjectService;
        private readonly IExamService _examService;

        public ObservableCollection<Subject> AvailableSubjects { get; } = [];

        [ObservableProperty]
        private string? _name;

        [ObservableProperty]
        private string? _maxScore = "20";

        [ObservableProperty]
        private PersianDate _selectedDate = PersianDate.Today;

        [ObservableProperty]
        private Subject? _selectedSubject;

        public AddExamViewModel(ISubjectService subjectService, IExamService examService)
        {
            _subjectService = subjectService;
            _examService = examService;

            LoadAvailableSubjectsAsync();
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

            if (!int.TryParse(MaxScore, out int MaxScoreInt)) {
                MessageBox.Show(".حداکثر نمره عددی وارد کنید", "خطا", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await _examService.AddExamAsync(new Exam
            {
                Name = Name,
                Date = SelectedDate.ToDateTime(),
                Subjects = new List<Subject> { SelectedSubject },
                MaxScore = MaxScoreInt
            });

            MessageBox.Show($"آزمون {Name} در تاریخ {SelectedDate} ثبت شد.");
        }
    }
}
