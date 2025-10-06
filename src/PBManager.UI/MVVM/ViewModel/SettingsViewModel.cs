using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;
using PBManager.Core.Entities;
using PBManager.Application.Interfaces;
using System.Windows;
using PBManager.Infrastructure.Parsers;
using PBManager.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using PBManager.Infrastructure.Services.Parsers;
using CommunityToolkit.Mvvm.Input;
using PBManager.Infrastructure.Exporters;

namespace PBManager.UI.MVVM.ViewModel
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly IStudentService _studentService;
        private readonly IStudyRecordService _studyRecordService;
        private readonly ISubjectService _subjectService;
        private readonly IClassService _classService;
        private readonly IDialogService _dialogService;
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private int _studyRecordCount;
        [ObservableProperty]
        private int _gradeRecordCount;
        [ObservableProperty]
        private int _studentsCount;
        [ObservableProperty]
        private int _classesCount;
        [ObservableProperty]
        private int _subjectsCount;
        
        private readonly IDatabasePorter _porter;
        private readonly IDatabaseManagementService _dbManagementService;

        public SettingsViewModel(IStudentService studentService, IStudyRecordService studyRecordService, ISubjectService subjectService, IClassService classService, IDatabasePorter porter, IDatabaseManagementService dbManagementServices, IDialogService dialogService, IServiceProvider serviceProvider)
        {
            _studentService = studentService;
            _studyRecordService = studyRecordService;
            _subjectService = subjectService;
            _classService = classService;
            _porter = porter;
            _dbManagementService = dbManagementServices;
            _dialogService = dialogService;
            _serviceProvider = serviceProvider;

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
                ".xlsx" => _serviceProvider.GetRequiredService<XlsxStudentParser>(),
                ".csv" => _serviceProvider.GetRequiredService<CsvStudentParser>(),
                _ => throw new NotSupportedException("File type not supported.")
            };
            try
            {
                await using var fileStream = File.OpenRead(filePath);
                var result = await _studentService.ImportStudentsAsync(fileStream, parser);
                await LoadData();
                MessageBox.Show($"{result.ImportedCount} دانش آموز وارد شد.\n{result.SkippedCount} دانش آموز نامعتبر.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا: {ex.Message}");
            }
        }

        public async Task ExportStudentsAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;

            var exporter = _serviceProvider.GetRequiredService<XlsxStudentExporter>();

            try
            {
                await using var stream = File.Create(filePath);
                await _studentService.ExportAllStudentsAsync(stream, exporter);
                MessageBox.Show("خروجی با موفقیت انجام شد!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public async Task ExportDatabaseAsync(string? filePath = null)
        {
            if (string.IsNullOrEmpty(filePath)) return;

            try
            {
                await _porter.ExportDatabaseAsync(filePath);
                MessageBox.Show("خروجی با موفقیت انجام شد!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    var result = MessageBox.Show(
                        "عملیات موفقیت آمیز بود! برای اعمال تغییرات برنامه باید ریستارت شود.\n\nالان ریستارت شود؟",
                        "Restart Required",
                        MessageBoxButton.YesNo, MessageBoxImage.Information);

                    if (result == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(Environment.ProcessPath ?? string.Empty);
                        System.Windows.Application.Current.Shutdown();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        [RelayCommand]
        private async Task WipeDatabaseAsync()
        {
            var result = MessageBox.Show(
                "آیا مطمئن هستید؟\n\nاین عملیات تمام داده های برنامه را حذف میکند. برنامه بسته خواهد شد.",
                "Confirm Data Wipe",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                var backupDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "PBManager_Backups");
                Directory.CreateDirectory(backupDir);

                var fileName = $"pre-wipe-backup_{DateTime.Now:yyyyMMdd_HHmmss}.sharifi";
                var backupPath = Path.Combine(backupDir, fileName);

                await _porter.ExportDatabaseAsync(backupPath);

                _dbManagementService.WipeDatabase();

                MessageBox.Show(
                    $"یک بکاپ در:\n{backupPath} ذخیره شد\n\nبرنامه هم اکنون بسته میشود.",
                    "Wipe Complete",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                System.Windows.Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        [RelayCommand]
        private void ManageSubjects()
        {
            _dialogService.ShowManagementWindow<Subject>("مدیریت دروس");
        }

        [RelayCommand]
        private void ManageClasses()
        {
            _dialogService.ShowManagementWindow<Class>("مدیریت کلاس ها");
        }
    }
}
