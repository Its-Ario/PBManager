using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.Globalization;
using Mohsen;
using Microsoft.Extensions.DependencyInjection;
using PBManager.Core.Entities;
using PBManager.Application.Services;
using PBManager.UI.MVVM.ViewModel.Helpers;
using PBManager.Core.Utils;
using PBManager.Application.Interfaces;

namespace PBManager.MVVM.ViewModel
{
    public partial class AddStudyRecordViewModel : ObservableObject
    {
        private readonly IStudyRecordService _studyRecordService;
        private readonly ISubjectService _subjectService;

        public ObservableCollection<SubjectEntry> WeeklySubjectEntries { get; set; } = [];

        [ObservableProperty]
        private Student _student;
        [ObservableProperty]
        private string _weekRangeText;
        [ObservableProperty]
        private bool _isEditMode;

        private PersianDate _selectedWeekStart;
        public PersianDate SelectedWeekStart
        {
            get => _selectedWeekStart;
            set
            {
                var adjustedDate = new PersianDate(DateUtils.GetPersianStartOfWeek(value.ToDateTime()));
                SetProperty(ref _selectedWeekStart, adjustedDate);
                UpdateWeekRangeText();
            }
        }

        public AddStudyRecordViewModel(IStudyRecordService studyRecordService, ISubjectService subjectService)
        {
            _studyRecordService = studyRecordService;
            _subjectService = subjectService;
        }
        public async Task Initialize(Student student, DateTime? weekStartDate = null)
        {
            _student = student;
            IsEditMode = false;
            SelectedWeekStart = new PersianDate(DateUtils.GetPersianStartOfWeek(weekStartDate ?? DateTime.Today));
            await LoadWeekAsync();
        }

        public async Task Initialize(Student student, IEnumerable<StudyRecord> existingRecords)
        {
            _student = student;
            IsEditMode = true;
            var recordsList = existingRecords.ToList();
            var earliestDate = recordsList.Any() ? recordsList.Min(r => r.Date) : DateTime.Today;
            SelectedWeekStart = new PersianDate(DateUtils.GetPersianStartOfWeek(earliestDate));
            await LoadSubjectsWithExistingData(recordsList);
        }

        private void UpdateWeekRangeText()
        {
            var endDate = _selectedWeekStart.AddDays(6);
            var culture = new CultureInfo("fa-IR");

            WeekRangeText = $"{_selectedWeekStart.ToDateTime().ToString("yyyy/MM/dd", culture)} تا {endDate.ToDateTime().ToString("yyyy/MM/dd", culture)}";
        }

        [RelayCommand]
        private async Task LoadWeekAsync()
        {
            try
            {
                var existingRecords = await _studyRecordService.GetStudyRecordsForWeekAsync(_student, _selectedWeekStart.ToDateTime());

                if (existingRecords.Any())
                {
                    IsEditMode = true;
                    await LoadSubjectsWithExistingData(existingRecords.ToList());
                }
                else
                {
                    IsEditMode = false;
                    await LoadSubjects();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بارگذاری داده‌ها: {ex.Message}",
                               "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadSubjects()
        {
            WeeklySubjectEntries.Clear();
            var subjectsFromDb = await _subjectService.GetSubjectsAsync();
            foreach (var subject in subjectsFromDb)
            {
                WeeklySubjectEntries.Add(new SubjectEntry { Subject = subject });
            }
        }

        private async Task LoadSubjectsWithExistingData(List<StudyRecord> existingRecords)
        {
            WeeklySubjectEntries.Clear();
            var subjectsFromDb = await _subjectService.GetSubjectsAsync();
            var recordsBySubject = existingRecords.GroupBy(r => r.Subject.Id).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var subject in subjectsFromDb)
            {
                var subjectEntry = new SubjectEntry { Subject = subject };

                if (recordsBySubject.TryGetValue(subject.Id, out List<StudyRecord>? subjectRecords))
                {
                    PopulateSubjectEntry(subjectEntry, subjectRecords);
                }

                WeeklySubjectEntries.Add(subjectEntry);
            }
        }

        private void PopulateSubjectEntry(SubjectEntry entry, List<StudyRecord> records)
        {
            var startOfWeek = _selectedWeekStart.ToDateTime();

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

        [RelayCommand]
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
            var startOfWeek = _selectedWeekStart;

            foreach (var entry in WeeklySubjectEntries)
            {
                var weeklyMinutes = entry.GetWeeklyMinutes();

                foreach (var dayMinutes in weeklyMinutes)
                {
                    if (dayMinutes.Value > 0)
                    {
                        var recordDate = CalculateDateForPersianWeekDay(startOfWeek.ToDateTime(), dayMinutes.Key);
                        allRecords.Add(new StudyRecord
                        {
                            StudentId = _student.Id,
                            SubjectId = entry.Subject.Id,
                            MinutesStudied = dayMinutes.Value,
                            Date = recordDate
                        });
                    }
                }
            }

            if (allRecords.Count == 0)
            {
                MessageBox.Show("هیچ داده‌ای برای ثبت وجود ندارد.", "اطلاعات",
                               MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                if (IsEditMode)
                {
                    await _studyRecordService.DeleteStudyRecordsForWeekAsync(_student, startOfWeek.ToDateTime());
                }

                await _studyRecordService.AddStudyRecordsAsync(allRecords, startOfWeek.ToDateTime());

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

        private DateTime CalculateDateForPersianWeekDay(DateTime startOfWeek, DayOfWeek dayOfWeek)
        {
            var daysToAdd = dayOfWeek switch
            {
                DayOfWeek.Saturday => 0,
                DayOfWeek.Sunday => 1,
                DayOfWeek.Monday => 2,
                DayOfWeek.Tuesday => 3,
                DayOfWeek.Wednesday => 4,
                DayOfWeek.Thursday => 5,
                DayOfWeek.Friday => 6,
                _ => 0,
            };
            return startOfWeek.AddDays(daysToAdd);
        }
    }
}