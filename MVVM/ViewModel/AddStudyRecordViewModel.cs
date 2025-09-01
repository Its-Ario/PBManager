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

        public AddStudyRecordViewModel(Student student)
        {
            _student = student;
            _studyRecordService = new StudyRecordService();
            WeeklySubjectEntries = new ObservableCollection<SubjectEntry>();
            LoadSubjects();

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
            var startOfWeek = GetStartOfCurrentWeek();

            foreach (var entry in WeeklySubjectEntries)
            {
                var weeklyMinutes = entry.GetWeeklyMinutes();

                foreach (var dayMinutes in weeklyMinutes)
                {
                    if (dayMinutes.Value > 0)
                    {
                        var recordDate = startOfWeek.AddDays((int)dayMinutes.Key);
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
                await _studyRecordService.AddStudyRecordsAsync(allRecords);
                MessageBox.Show($"اطلاعات مطالعه با موفقیت ثبت شد! ({allRecords.Count} رکورد)", "موفقیت",
                               MessageBoxButton.OK, MessageBoxImage.Information);

                foreach (var entry in WeeklySubjectEntries)
                {
                    entry.ClearAllEntries();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در ثبت اطلاعات: {ex.Message}", "خطا",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private DateTime GetStartOfCurrentWeek()
        {
            var today = DateTime.Today;
            var daysFromSaturday = ((int)today.DayOfWeek + 1) % 7;
            return today.AddDays(-daysFromSaturday);
        }
    }
}