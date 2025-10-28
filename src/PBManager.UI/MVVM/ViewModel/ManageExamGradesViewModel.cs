using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PBManager.Application.Interfaces;
using PBManager.Core.Entities;
using System.Collections.ObjectModel;
using System.Windows;

namespace PBManager.UI.MVVM.ViewModel
{
    public partial class ManageExamGradesViewModel(
        IStudentService studentService,
        IGradeService gradeService,
        IClassService classService) : ObservableObject
    {
        private readonly IStudentService _studentService = studentService;
        private readonly IGradeService _gradeService = gradeService;
        private readonly IClassService _classService = classService;
        private Exam _exam;

        public ObservableCollection<ClassGradeTab> ClassTabs { get; } = [];

        [ObservableProperty]
        private ClassGradeTab? _selectedTab;

        [ObservableProperty]
        private string _examTitle = string.Empty;

        public async Task InitializeAsync(Exam exam)
        {
            _exam = exam;
            ExamTitle = $"مدیریت نمرات آزمون: {exam.Name}";

            await LoadClassTabsAsync();
        }

        private async Task LoadClassTabsAsync()
        {
            try
            {
                var classes = await _classService.GetClassesAsync();
                var allStudents = await _studentService.GetAllStudentsAsync();

                ClassTabs.Clear();

                foreach (var cls in classes)
                {
                    var studentsInClass = allStudents.Where(s => s.ClassId == cls.Id).ToList();

                    if (studentsInClass.Count > 0)
                    {
                        var tab = new ClassGradeTab(cls, _exam);
                        await LoadStudentGradesAsync(tab, studentsInClass);
                        ClassTabs.Add(tab);
                    }
                }

                SelectedTab = ClassTabs.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بارگذاری کلاس ها: {ex.Message}",
                               "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadStudentGradesAsync(ClassGradeTab tab, List<Student> students)
        {
            foreach (var student in students)
            {
                var studentGradeRow = new StudentGradeRow
                {
                    Student = student,
                    StudentName = student.FullName,
                    NationalCode = student.NationalCode
                };

                var existingGrades = await _gradeService.GetGradesForStudentAsync(student.Id, _exam.Id);

                if (_exam.Subjects != null)
                {
                    foreach (var subject in _exam.Subjects)
                    {
                        var existingGrade = existingGrades.FirstOrDefault(g => g.SubjectId == subject.Id);

                        var gradeCell = new SubjectGradeCell
                        {
                            Subject = subject,
                            Score = existingGrade?.Score.ToString() ?? string.Empty,
                            HasExistingGrade = existingGrade != null
                        };

                        studentGradeRow.SubjectGrades.Add(gradeCell);
                    }
                }

                studentGradeRow.UpdateHasGrades();
                tab.StudentRows.Add(studentGradeRow);
            }
        }

        [RelayCommand]
        private async Task SaveAllGradesAsync()
        {
            if (_exam == null) return;

            try
            {
                int totalSaved = 0;
                int totalErrors = 0;

                foreach (var tab in ClassTabs)
                {
                    foreach (var row in tab.StudentRows)
                    {
                        var gradeRecordsToSave = new List<GradeRecord>();
                        bool hasAnyGrade = false;

                        foreach (var gradeCell in row.SubjectGrades)
                        {
                            if (!string.IsNullOrWhiteSpace(gradeCell.Score))
                            {
                                if (double.TryParse(gradeCell.Score, out double score))
                                {
                                    if (score < 0 || score > _exam.MaxScore)
                                    {
                                        totalErrors++;
                                        MessageBox.Show(
                                            $"نمره نامعتبر برای {row.StudentName} در درس {gradeCell.Subject.Name}: نمره باید بین 0 تا {_exam.MaxScore} باشد",
                                            "خطای اعتبارسنجی",
                                            MessageBoxButton.OK,
                                            MessageBoxImage.Warning);
                                        return;
                                    }

                                    gradeRecordsToSave.Add(new GradeRecord
                                    {
                                        StudentId = row.Student.Id,
                                        SubjectId = gradeCell.Subject.Id,
                                        ExamId = _exam.Id,
                                        Score = score,
                                        Date = _exam.Date
                                    });
                                    hasAnyGrade = true;
                                }
                                else
                                {
                                    totalErrors++;
                                    MessageBox.Show(
                                        $"فرمت نمره نامعتبر برای {row.StudentName} در درس {gradeCell.Subject.Name}",
                                        "خطای اعتبارسنجی",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Warning);
                                    return;
                                }
                            }
                        }

                        if (hasAnyGrade)
                        {
                            try
                            {
                                await _gradeService.DeleteRecords(row.Student.Id, _exam.Id);

                                await _gradeService.SaveGradesForExamAsync(
                                    row.Student.Id,
                                    _exam.Id,
                                    gradeRecordsToSave);

                                totalSaved++;
                                row.UpdateHasGrades();
                            }
                            catch (Exception ex)
                            {
                                totalErrors++;
                                MessageBox.Show(
                                    $"خطا در ذخیره نمرات {row.StudentName}: {ex.Message}",
                                    "خطا",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                            }
                        }
                    }
                }

                if (totalErrors == 0)
                {
                    MessageBox.Show(
                        $"نمرات با موفقیت ذخیره شد!\nتعداد دانش آموزان ثبت شده: {totalSaved}",
                        "موفقیت",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(
                        $"ذخیره سازی با خطا مواجه شد.\nموفق: {totalSaved}\nناموفق: {totalErrors}",
                        "هشدار",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در ذخیره سازی: {ex.Message}",
                               "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public partial class ClassGradeTab(Class cls, Exam exam) : ObservableObject
    {
        public Class Class { get; } = cls;
        public Exam Exam { get; } = exam;
        public string TabHeader { get; } = cls.Name;
        public ObservableCollection<StudentGradeRow> StudentRows { get; } = [];
    }

    public partial class StudentGradeRow : ObservableObject
    {
        public Student Student { get; set; }
        public string StudentName { get; set; }
        public string NationalCode { get; set; }
        public ObservableCollection<SubjectGradeCell> SubjectGrades { get; } = [];

        [ObservableProperty]
        private bool _hasGrades;

        public void UpdateHasGrades()
        {
            HasGrades = SubjectGrades.Any(g => !string.IsNullOrWhiteSpace(g.Score));
        }
    }

    public partial class SubjectGradeCell : ObservableObject
    {
        public Subject Subject { get; set; }

        [ObservableProperty]
        private string _score;

        public bool HasExistingGrade { get; set; }

        partial void OnScoreChanged(string value)
        {
            // Trigger parent row update when score changes
            // This will be handled by the row itself
        }
    }
}