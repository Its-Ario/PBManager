using CommunityToolkit.Mvvm.ComponentModel;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.IO;
using PBManager.MVVM.Model;
using PBManager.Services;
using System.Text;
using ExcelDataReader;

namespace PBManager.MVVM.ViewModel
{
    public class SettingsViewModel : ObservableObject
    {
        private readonly StudentService _studentService;
        private readonly StudyRecordService _studyRecordService;
        private readonly SubjectService _subjectService;
        private readonly ClassService _classService;

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

        public SettingsViewModel(StudentService studentService, StudyRecordService studyRecordService, SubjectService subjectService, ClassService classService)
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

        public async Task ImportStudentsCsv(string filePath)
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = csv.GetRecords<Student>().ToList();

            await _studentService.AddStudentsAsync(records);
        }

        public async Task ImportStudentsXlsx(string filePath)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var result = reader.AsDataSet();

            var table = result.Tables[0];

            var students = new List<Student>(table.Rows.Count - 1);

            for (int i = 1; i < table.Rows.Count; i++)
            {
                var row = table.Rows[i];
                if (row[0] == null || row[1] == null) continue;

                students.Add(new Student
                {
                    FirstName = row[0]?.ToString() ?? string.Empty,
                    LastName = row[1]?.ToString() ?? string.Empty,
                    NationalCode = row[2]?.ToString() ?? string.Empty,
                });
            }

            await _studentService.AddStudentsAsync(students);
        }
    }
}
