using CommunityToolkit.Mvvm.ComponentModel;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.IO;
using System;
using PBManager.MVVM.Model;
using PBManager.Services;

namespace PBManager.MVVM.ViewModel
{
    public class SettingsViewModel : ObservableObject
    {
        private readonly StudentService _studentService;

        private int _studyRecordCount;
        public int StudyRecordCount
        {
            get => _studyRecordCount;
            set => SetProperty(ref _studyRecordCount, value);
        }

        private int _gradeRecordCount;
        public int GradeRecordCount
        {
            get => _gradeRecordCount;
            set => SetProperty(ref _gradeRecordCount, value);
        }

        private int _studentsCount;
        public int StudentsCount
        {
            get => _studentsCount;
            set => SetProperty(ref _studentsCount, value);
        }

        private int _classesCount;
        public int ClassesCount
        {
            get => _classesCount;
            set => SetProperty(ref _classesCount, value);
        }

        private int _subjectsCount;
        public int SubjectsCount
        {
            get => _subjectsCount;
            set => SetProperty(ref _subjectsCount, value);
        }

        public SettingsViewModel()
        {
            _studentService = new StudentService();
            _ = LoadData();
        }

        private async Task LoadData()
        {
            StudyRecordCount = await App.Db.StudyRecords.AsNoTracking().CountAsync();
            GradeRecordCount = await App.Db.GradeRecords.AsNoTracking().CountAsync();
            StudentsCount = await App.Db.Students.AsNoTracking().CountAsync();
            ClassesCount = await App.Db.Classes.AsNoTracking().CountAsync();
            SubjectsCount = await App.Db.Subjects.AsNoTracking().CountAsync();

        }

        public async Task ImportCsvStudents(string filePath)
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = csv.GetRecords<Student>().ToList();

            await _studentService.AddStudentsAsync(records);
        }
    }
}
