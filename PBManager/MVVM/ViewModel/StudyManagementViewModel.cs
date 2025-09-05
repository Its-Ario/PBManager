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

namespace PBManager.MVVM.ViewModel
{
    public class StudyManagementViewModel : ObservableObject
    {
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

        private ObservableCollection<Subject> _subjects;
        public ObservableCollection<Subject> Subjects
        {
            get => _subjects;
            set { _subjects = value; OnPropertyChanged(); }
        }

        private ObservableCollection<StudyRecord> _studyRecords;
        public ObservableCollection<StudyRecord> StudyRecords
        {
            get => _studyRecords;
            set { _studyRecords = value; OnPropertyChanged(); }
        }

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

        public StudyManagementViewModel()
        {
            Students = new ObservableCollection<Student>();
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
                var studentsFromDb = await App.Db.Students
                    .Include(r => r.Class)
                    .ToListAsync();

                Students = new ObservableCollection<Student>(studentsFromDb);

                Subjects = new ObservableCollection<Subject>(await App.Db.Subjects.ToListAsync());
                StudyRecords = new ObservableCollection<StudyRecord>(
                     await App.Db.StudyRecords
                         .Include(r => r.Student)
                         .Include(r => r.Subject)
                         .OrderByDescending(r => r.Date)
                         .ToListAsync()
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}");
            }
        }
    }
}