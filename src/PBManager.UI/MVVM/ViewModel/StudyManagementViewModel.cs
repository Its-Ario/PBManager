using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using PBManager.Core.Entities;
using PBManager.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace PBManager.UI.MVVM.ViewModel
{
    public partial class StudentManagementViewModel : ObservableObject
    {
        private readonly IStudentService _studentService;
        private readonly IServiceProvider _serviceProvider;

        private ObservableCollection<Student> _students = [];
        public ObservableCollection<Student> Students
        {
            get => _students;
            set
            {
                _students = value;
                FilteredStudents = CollectionViewSource.GetDefaultView(_students);
                FilteredStudents.Filter = FilterStudents;
                SetProperty(ref _students, value);
            }
        }

        [ObservableProperty]
        private StudentDetailViewModel? _detailVM;

        public ICollectionView? FilteredStudents { get; private set; }

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
                FilteredStudents?.Refresh();
            }
        }

        [ObservableProperty]
        private Student? _selectedStudent;

        public bool HasSelection => SelectedStudent != null;

        public StudentManagementViewModel(IStudentService studentService, IServiceProvider serviceProvider)
        {
            _studentService = studentService;
            _serviceProvider = serviceProvider;

            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                _ = LoadData();
            }
        }

        private bool FilterStudents(object item)
        {
            if (string.IsNullOrEmpty(SearchText))
                return true;

            if (item is not Student student) return false;

            return (student.FirstName?.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                   (student.Class?.Name?.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private async Task LoadData()
        {
            try
            {
                var studentsFromDb = await _studentService.GetAllStudentsAsync();

                Students = new ObservableCollection<Student>(studentsFromDb);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}");
            }
        }

        async partial void OnSelectedStudentChanged(Student? value)
        {
            if (value != null)
            {
                DetailVM = _serviceProvider.GetRequiredService<StudentDetailViewModel>();

                await DetailVM.InitializeAsync(value);
            }
            else
            {
                DetailVM = null;
            }
        }
    }
}