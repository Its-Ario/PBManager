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

        public ObservableCollection<Subject> AvailableSubjects { get; } = [];

        [ObservableProperty]
        private string? _name;

        [ObservableProperty]
        private string? _maxScore;

        [ObservableProperty]
        private PersianDate _selectedDate = PersianDate.Today;

        public AddExamViewModel(ISubjectService subjectService)
        {
            _subjectService = subjectService;

            LoadAvailableSubjectsAsync();
        }

        private async Task LoadAvailableSubjectsAsync()
        {
            var subjects = await _subjectService.GetSubjectsAsync();
            AvailableSubjects.Clear();
            foreach (var subject in subjects)
            {
                AvailableSubjects.Add(subject);
            }
        }

        [RelayCommand]
        private async Task SubmitAsync()
        {
            if (Name == null || MaxScore == null || SelectedDate == null)
            {
                MessageBox.Show("Please fill out all fields");
                return;
            }
            MessageBox.Show($"Submitted {Name}");
        }
    }
}
