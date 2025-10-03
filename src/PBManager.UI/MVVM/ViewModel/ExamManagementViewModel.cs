using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using PBManager.Application.Interfaces;
using PBManager.Core.Entities;
using PBManager.UI.MVVM.View;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace PBManager.UI.MVVM.ViewModel
{
    public partial class ExamManagementViewModel : ObservableObject
    {
        private readonly IExamService _examService;
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private ExamOverviewViewModel? _examVM;

        private ObservableCollection<Exam> _exams = [];
        public ObservableCollection<Exam> Exams
        {
            get => _exams;
            set
            {
                _exams = value;
                FilteredExams = CollectionViewSource.GetDefaultView(_exams);
                FilteredExams.Filter = FilterExams;
                SetProperty(ref _exams, value);
            }
        }

        public ICollectionView? FilteredExams { get; private set; }
        private string _searchText = string.Empty;
        public string SearchText
        {
            get
            {
                return _searchText;
            }
            set
            {
                _searchText = value;
                SetProperty(ref _searchText, value);
                FilteredExams?.Refresh();
            }
        }

        [ObservableProperty]
        private Exam? _selectedExam;

        public bool HasSelection => SelectedExam != null;

        public ExamManagementViewModel(IExamService examService, IServiceProvider serviceProvider)
        {
            _examService = examService;
            _serviceProvider = serviceProvider;

            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                _ = LoadData();
            }
        }

        private bool FilterExams(object item)
        {
            if (string.IsNullOrEmpty(SearchText))
                return true;

            if (item is not Exam exam) return false;

            return (exam.Name?.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private async Task LoadData()
        {
            try
            {
                var examsFromDb = await _examService.GetAllExamsWithSubjectsAsync();

                Exams = new ObservableCollection<Exam>(examsFromDb);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}");
            }
        }

        async partial void OnSelectedExamChanged(Exam? value)
        {
            ExamVM = _serviceProvider.GetRequiredService<ExamOverviewViewModel>();
            await ExamVM.InitializeAsync(value);
        }

        [RelayCommand]
        private async Task AddNewExamAsync()
        {
            var view = _serviceProvider.GetRequiredService<AddExamView>();
            if(view.DataContext is AddExamViewModel viewModel)
            {
                await viewModel.InitializeAsync();
            }
            view.Show();
        }
    }
}
