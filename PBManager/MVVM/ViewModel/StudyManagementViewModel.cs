using Microsoft.EntityFrameworkCore;
using PBManager.MVVM.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using PBManager.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PBManager.Services;

namespace PBManager.MVVM.ViewModel
{
    public class StudyManagementViewModel : ObservableObject
    {
        private readonly StudentService _studentService;

        private ObservableCollection<Student> _students;
        public ObservableCollection<Student> Students
        {
            get => _students;
            set
            {
                _students = value;
                OnPropertyChanged();
                FilteredStudents = CollectionViewSource.GetDefaultView(_students);
                FilteredStudents.Filter = FilterStudents;
                OnPropertyChanged();
            }
        }

        public StudentDetailViewModel DetailVM { get; }

        private StudyRecord _selectedRecord;
        public StudyRecord SelectedRecord
        {
            get => _selectedRecord;
            set { _selectedRecord = value; OnPropertyChanged(); }
        }

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
                OnPropertyChanged();
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

        public RelayCommand SaveCommand { get; }

        public StudyManagementViewModel(StudentService studentService)
        {
            _studentService = studentService;

            DetailVM = new StudentDetailViewModel();

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