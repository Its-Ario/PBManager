using PBManager.MVVM.Model;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using PBManager.Services;

namespace PBManager.MVVM.ViewModel
{
    public class AddStudyRecordViewModel : ObservableObject
    {
        private StudyRecordService _studyRecordService;
        public ObservableCollection<SubjectEntry> WeeklySubjectEntries { get; set; }
        public IRelayCommand SubmitCommand { get; set; }
        private Student _student;
        private IEnumerable<StudyRecord> _existingRecords;
        private bool _isEditMode;

        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        public AddStudyRecordViewModel(Student student)
        {
            _student = student;
            _studyRecordService = new StudyRecordService();
            WeeklySubjectEntries = new ObservableCollection<SubjectEntry>();
            IsEditMode = false;
            LoadSubjects();
            SubmitCommand = new RelayCommand(async () => await SubmitAsync());
        }

        public AddStudyRecordViewModel(Student student, IEnumerable<StudyRecord> existingRecords)
        {
            _student = student;
            _existingRecords = existingRecords;
            _studyRecordService = new StudyRecordService();
            WeeklySubjectEntries = new ObservableCollection<SubjectEntry>();
            IsEditMode = true;
            LoadSubjectsWithExistingData();
            SubmitCommand = new RelayCommand(async () => await SubmitAsync());
        }

        private void LoadSubjects()
        {
            var subjectsFromDb = App.Db.Subjects.ToList();
            foreach (var subject in subjectsFromDb)
            {
                WeeklySubjectEntries.Add(new SubjectEntry { Subject = subject });
            }
        }

        private void LoadSubjectsWithExistingData()
        {
            var subjectsFromDb = App.Db.Subjects.ToList();
            var recordsBySubject = _existingRecords.GroupBy(r => r.Subject.Id).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var subject in subjectsFromDb)
            {
                var subjectEntry = new SubjectEntry { Subject = subject };

                if (recordsBySubject.ContainsKey(subject.Id))
                {
                    var subjectRecords = recordsBySubject[subject.Id];
                    PopulateSubjectEntry(subjectEntry, subjectRecords);
                }

                WeeklySubjectEntries.Add(subjectEntry);
            }
        }

        private void PopulateSubjectEntry(SubjectEntry entry, List<StudyRecord> records)
        {
            var startOfWeek = GetPersianStartOfWeekForRecords(records);

            foreach (var record in records)
            {
                var daysDifference = (record.Date - startOfWeek).Days;

                switch (daysDifference)
                {
                    case 0:
                        entry.MinutesSat = record.MinutesStudied.ToString();
                        break;
                    case 1:
                        entry.MinutesSun = record.MinutesStudied.ToString();
                        break;
                    case 2:
                        entry.MinutesMon = record.MinutesStudied.ToString();
                        break;
                    case 3:
                        entry.MinutesTue = record.MinutesStudied.ToString();
                        break;
                    case 4:
                        entry.MinutesWed = record.MinutesStudied.ToString();
                        break;
                    case 5:
                        entry.MinutesThu = record.MinutesStudied.ToString();
                        break;
                    case 6:
                        entry.MinutesFri = record.MinutesStudied.ToString();
                        break;
                }
            }
        }

        private DateTime GetPersianStartOfWeekForRecords(List<StudyRecord> records)
        {
            if (!records.Any()) return GetPersianStartOfCurrentWeek();

            var earliestDate = records.Min(r => r.Date);
            return GetPersianStartOfWeek(earliestDate);
        }

        private async Task SubmitAsync()
        {
            bool hasError = false;
            foreach (var entry in WeeklySubjectEntries)
            {
                entry.Validate();
                if (entry.HasErrors)
                    hasError = true;
            }

            if (hasError)
            {
                MessageBox.Show("بعضی داده‌ها نامعتبر هستند، لطفا دوباره بررسی کنید.", "خطای اعتبارسنجی",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var allRecords = new List<StudyRecord>();
            var startOfWeek = IsEditMode
                ? GetPersianStartOfWeekForRecords(_existingRecords.ToList())
                : GetPersianStartOfCurrentWeek();

            foreach (var entry in WeeklySubjectEntries)
            {
                var weeklyMinutes = entry.GetWeeklyMinutes();

                foreach (var dayMinutes in weeklyMinutes)
                {
                    if (dayMinutes.Value > 0)
                    {
                        var recordDate = CalculateDateForPersianWeekDay(startOfWeek, dayMinutes.Key);
                        allRecords.Add(new StudyRecord
                        {
                            Student = _student,
                            Subject = entry.Subject,
                            MinutesStudied = dayMinutes.Value,
                            Date = recordDate
                        });
                    }
                }
            }

            if (!allRecords.Any())
            {
                MessageBox.Show("هیچ داده‌ای برای ثبت وجود ندارد.", "اطلاعات",
                               MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                if (IsEditMode)
                {
                    await _studyRecordService.DeleteStudyRecordsForWeekAsync(_student, startOfWeek);
                }

                await _studyRecordService.AddStudyRecordsAsync(allRecords, startOfWeek);

                var message = IsEditMode
                    ? $"اطلاعات مطالعه با موفقیت بروزرسانی شد! ({allRecords.Count} رکورد)"
                    : $"اطلاعات مطالعه با موفقیت ثبت شد! ({allRecords.Count} رکورد)";

                MessageBox.Show(message, "موفقیت", MessageBoxButton.OK, MessageBoxImage.Information);

                if (!IsEditMode)
                {
                    foreach (var entry in WeeklySubjectEntries)
                    {
                        entry.ClearAllEntries();
                    }
                }
            }
            catch (Exception ex)
            {
                var errorMessage = IsEditMode
                    ? $"خطا در بروزرسانی اطلاعات: {ex.Message}"
                    : $"خطا در ثبت اطلاعات: {ex.Message}";

                MessageBox.Show(errorMessage, "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private DateTime GetPersianStartOfWeek(DateTime date)
        {
            int daysFromSaturday;
            switch (date.DayOfWeek)
            {
                case DayOfWeek.Saturday:
                    daysFromSaturday = 0;
                    break;
                case DayOfWeek.Sunday:
                    daysFromSaturday = 1;
                    break;
                case DayOfWeek.Monday:
                    daysFromSaturday = 2;
                    break;
                case DayOfWeek.Tuesday:
                    daysFromSaturday = 3;
                    break;
                case DayOfWeek.Wednesday:
                    daysFromSaturday = 4;
                    break;
                case DayOfWeek.Thursday:
                    daysFromSaturday = 5;
                    break;
                case DayOfWeek.Friday:
                    daysFromSaturday = 6;
                    break;
                default:
                    daysFromSaturday = 0;
                    break;
            }

            return date.Date.AddDays(-daysFromSaturday);
        }

        private DateTime GetPersianStartOfCurrentWeek()
        {
            return GetPersianStartOfWeek(DateTime.Today);
        }

        private DateTime CalculateDateForPersianWeekDay(DateTime startOfWeek, DayOfWeek dayOfWeek)
        {
            int daysToAdd;
            switch (dayOfWeek)
            {
                case DayOfWeek.Saturday:
                    daysToAdd = 0;
                    break;
                case DayOfWeek.Sunday:
                    daysToAdd = 1;
                    break;
                case DayOfWeek.Monday:
                    daysToAdd = 2;
                    break;
                case DayOfWeek.Tuesday:
                    daysToAdd = 3;
                    break;
                case DayOfWeek.Wednesday:
                    daysToAdd = 4;
                    break;
                case DayOfWeek.Thursday:
                    daysToAdd = 5;
                    break;
                case DayOfWeek.Friday:
                    daysToAdd = 6;
                    break;
                default:
                    daysToAdd = 0;
                    break;
            }

            return startOfWeek.AddDays(daysToAdd);
        }
    }
}