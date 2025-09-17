using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;
using PBManager.Core.Entities;
using PBManager.Application.Interfaces;
using System.Windows;
using PBManager.Infrastructure.Parsers;
using PBManager.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using PBManager.Infrastructure.Services.Parsers;

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

        private readonly IDatabasePorter _porter;

        public SettingsViewModel(IStudentService studentService, IStudyRecordService studyRecordService, ISubjectService subjectService, IClassService classService, IDatabasePorter porter)
        {
            _studentService = studentService;
            _studyRecordService = studyRecordService;
            _subjectService = subjectService;
            _classService = classService;
            _porter = porter;

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
                ".csv" => App.ServiceProvider.GetRequiredService<CsvStudentParser>(),
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

        public async Task ExportDatabaseAsync(string? filePath = null)
        {
            if (string.IsNullOrEmpty(filePath)) return;

            try
            {
                await _porter.ExportDatabaseAsync(filePath);
                MessageBox.Show("Export successful!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}");
            }
        }

        public async Task ImportDatabaseAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;

            try
            {
                var success = await _porter.ImportDatabaseAsync(filePath);
                if (success)
                {
                    if (_porter.IsPendingImportOnRestart())
                    {
                        var result = MessageBox.Show(
                            "Import successful! The application needs to restart to apply changes.\n\nRestart now?",
                            "Restart Required",
                            MessageBoxButton.YesNo);

                        if (result == MessageBoxResult.Yes)
                        {
                            System.Diagnostics.Process.Start(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                            System.Windows.Application.Current.Shutdown();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Import failed: {ex.Message}");
            }
        }

    }
}
