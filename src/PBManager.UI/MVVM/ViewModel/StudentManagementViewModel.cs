using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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

        private ObservableCollection<Student> _students = new();
        public ObservableCollection<Student> Students
        {
            get => _students;
            private set => SetProperty(ref _students, value);
        }

        public ICollectionView FilteredStudents { get; private set; }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilteredStudents?.Refresh();
                }
            }
        }

        private string _sortColumn = nameof(Student.LastName);
        private ListSortDirection _sortDirection = ListSortDirection.Ascending;

        public void SortBy(string propertyName, ListSortDirection direction)
        {
            if (FilteredStudents == null) return;

            _sortColumn = propertyName;
            _sortDirection = direction;

            FilteredStudents.SortDescriptions.Clear();
            FilteredStudents.SortDescriptions.Add(new SortDescription(_sortColumn, _sortDirection));
        }

        [ObservableProperty]
        private Student? _selectedStudent;

        [ObservableProperty]
        private StudentDetailViewModel? _detailVM;

        public bool HasSelection => SelectedStudent != null;
        public StudentManagementViewModel(IStudentService studentService, IServiceProvider serviceProvider)
        {
            _studentService = studentService;
            _serviceProvider = serviceProvider;

            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                _ = LoadDataAsync();
            }
        }
        private async Task LoadDataAsync()
        {
            try
            {
                var studentsFromDb = await _studentService.GetAllStudentsAsync();
                Students = new ObservableCollection<Student>(studentsFromDb);

                FilteredStudents = CollectionViewSource.GetDefaultView(Students);
                FilteredStudents.Filter = FilterStudents;

                ApplyDefaultSorting();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بارگذاری داده ها: {ex.Message}");
            }
        }
        private bool FilterStudents(object obj)
        {
            if (obj is not Student student) return false;
            if (string.IsNullOrWhiteSpace(SearchText)) return true;

            var words = SearchText.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in words)
            {
                var lowerWord = word.ToLower();

                if ((student.FirstName?.ToLower().Contains(lowerWord) ?? false) ||
                    (student.LastName?.ToLower().Contains(lowerWord) ?? false) ||
                    (student.Class?.Name?.ToLower().Contains(lowerWord) ?? false))
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        private void ApplyDefaultSorting()
        {
            if (FilteredStudents == null) return;

            FilteredStudents.SortDescriptions.Clear();
            FilteredStudents.SortDescriptions.Add(new SortDescription(nameof(Student.Class.Name), ListSortDirection.Ascending));
            FilteredStudents.SortDescriptions.Add(new SortDescription(nameof(Student.LastName), ListSortDirection.Ascending));
            FilteredStudents.SortDescriptions.Add(new SortDescription(nameof(Student.FirstName), ListSortDirection.Ascending));
        }
        partial void OnSelectedStudentChanged(Student? value)
        {
            if (value != null)
            {
                DetailVM = _serviceProvider.GetRequiredService<StudentDetailViewModel>();
                _ = DetailVM.InitializeAsync(value);
            }
            else
            {
                DetailVM = null;
            }
        }
    }
}