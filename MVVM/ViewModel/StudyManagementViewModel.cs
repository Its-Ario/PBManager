using Microsoft.EntityFrameworkCore;
using PBManager.Core;
using PBManager.MVVM.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Input;
using System;

namespace PBManager.MVVM.ViewModel
{
    internal class StudyManagementViewModel : ObservableObject
    {
        private ObservableCollection<Student> _students;
        public ObservableCollection<Student> Students
        {
            get => _students;
            set
            {
                _students = value;
                OnPropertyChanged(nameof(Students));
                FilteredStudents = CollectionViewSource.GetDefaultView(_students);
                FilteredStudents.Filter = FilterStudents;
                OnPropertyChanged(nameof(FilteredStudents));
            }
        }

        private ObservableCollection<Subject> _subjects;
        public ObservableCollection<Subject> Subjects
        {
            get => _subjects;
            set { _subjects = value; OnPropertyChanged(nameof(Subjects)); }
        }

        private ObservableCollection<StudyRecord> _studyRecords;
        public ObservableCollection<StudyRecord> StudyRecords
        {
            get => _studyRecords;
            set { _studyRecords = value; OnPropertyChanged(nameof(StudyRecords)); }
        }

        private StudyRecord _selectedRecord;
        public StudyRecord SelectedRecord
        {
            get => _selectedRecord;
            set { _selectedRecord = value; OnPropertyChanged(nameof(SelectedRecord)); }
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
                OnPropertyChanged(nameof(SearchText));
                FilteredStudents?.Refresh();
            }
        }

        private Student _selectedStudent;
        public Student SelectedStudent
        {
            get => _selectedStudent;
            set
            {
                _selectedStudent = value;
                OnPropertyChanged(nameof(SelectedStudent));
            }
        }

        public bool HasSelection => SelectedStudent != null;

        public ICommand SaveCommand { get; }

        public StudyManagementViewModel()
        {
            Students = new ObservableCollection<Student>();
            SaveCommand = new RelayCommand(async o => await SaveRecord());

            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                LoadData();
            }
        }

        private bool FilterStudents(object item)
        {
            if (string.IsNullOrEmpty(SearchText))
                return true;

            var student = item as Student;
            if (student == null) return false;

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

        private async Task SaveRecord()
        {
            if (SelectedRecord == null) return;
            App.Db.StudyRecords.Update(SelectedRecord);
            await App.Db.SaveChangesAsync();
        }
    }
}