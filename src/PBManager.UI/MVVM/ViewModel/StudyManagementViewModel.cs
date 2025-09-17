using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using PBManager.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using PBManager.Core.Entities;
using PBManager.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace PBManager.MVVM.ViewModel
{
    public partial class StudyManagementViewModel : ObservableObject
    {
        private readonly IStudentService _studentService;

        private ObservableCollection<Student> _students;
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

        public StudentDetailViewModel DetailVM { get; }

        [ObservableProperty]
        private StudyRecord _selectedRecord;

        public ICollectionView FilteredStudents { get; private set; }

        private string _searchText;
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

        private Student? _selectedStudent;
        public Student? SelectedStudent
        {
            get => _selectedStudent;
            set
            {
                if (SetProperty(ref _selectedStudent, value))
                {
                    WeakReferenceMessenger.Default.Send(new StudentSelectedMessage(value));
                }
            }
        }
        public bool HasSelection => SelectedStudent != null;

        public StudyManagementViewModel(IStudentService studentService)
        {
            _studentService = studentService;

            DetailVM = App.ServiceProvider.GetRequiredService<StudentDetailViewModel>();

            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                LoadData();
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

        private async void LoadData()
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
    }
}