using CommunityToolkit.Mvvm.ComponentModel;
using CsvHelper;
using System.Globalization;
using System.IO;
using System.Text;
using ExcelDataReader;
using PBManager.Core.Entities;
using PBManager.Application.Interfaces;
using System.Windows;
using PBManager.Infrastructure.Parsers;
using PBManager.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace PBManager.MVVM.ViewModel
{
    public class SettingsViewModel : ObservableObject
    {
        private readonly IStudentService _studentService;
        private readonly IStudyRecordService _studyRecordService;
        private readonly ISubjectService _subjectService;
        private readonly IClassService _classService;

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

        public SettingsViewModel(IStudentService studentService, IStudyRecordService studyRecordService, ISubjectService subjectService, IClassService classService)
        {
            _studentService = studentService;
            _studyRecordService = studyRecordService;
            _subjectService = subjectService;
            _classService = classService;

            _ = LoadData();
        }

        private async Task LoadData()
        {
            StudyRecordCount = await _studyRecordService.GetStudyRecordCountAsync();
            GradeRecordCount = 0; // SOON
            StudentsCount = await _studentService.GetStudentsCountAsync();
            ClassesCount = await _classService.GetClassCountAsync();
            SubjectsCount = await _subjectService.GetSubjectCountAsync();

        }

        public async Task ImportStudentsAsync(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;
            IFileParser<Student> parser = Path.GetExtension(filePath).ToLowerInvariant() switch
            {
                ".xlsx" => App.ServiceProvider.GetRequiredService<XlsxStudentParser>(),
                ".csv" => App.ServiceProvider.GetRequiredService<CsvParser<Student>>(),
                _ => throw new NotSupportedException("File type not supported.")
            };
            try
            {
                await using var fileStream = File.OpenRead(filePath);
                var result = await _studentService.ImportStudentsAsync(fileStream, parser);
                await LoadData();
                MessageBox.Show($"{result.ImportedCount} students imported.\n{result.SkippedCount} students skipped.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
    }
}
