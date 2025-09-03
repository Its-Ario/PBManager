using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PBManager.MVVM.Model;
using PBManager.MVVM.View;
using PBManager.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PBManager.MVVM.ViewModel
{
    class StudyHistoryViewModel : ObservableObject
    {
        private readonly StudyRecordService _studyRecordService;

        private Student _student;
        public Student Student
        {
            get => _student;
            set => SetProperty(ref _student, value);
        }

        private ObservableCollection<WeeklyStudyData> _records;
        public ObservableCollection<WeeklyStudyData> Records
        {
            get => _records;
            set => SetProperty(ref _records, value);
        }

        public IRelayCommand<WeeklyStudyData> EditRecordCommand { get; }

        public StudyHistoryViewModel(Student student)
        {
            Student = student;
            _studyRecordService = new StudyRecordService();

            _ = LoadData(Student.Id);

            EditRecordCommand = new RelayCommand<WeeklyStudyData>(async (weeklyRecord) => await EditRecordAsync(weeklyRecord));
        }

        public async Task LoadData(int studentId)
        {
            List<StudyRecord> records = await _studyRecordService.GetStudyRecordsForStudentAsync(studentId);

            var groupedByWeek = records
                .GroupBy(r => GetWeekStartDate(r.Date))
                .Select(g =>
                {
                    var weekStart = g.Key;
                    var weekEnd = weekStart.AddDays(6);

                    var allDays = Enumerable.Range(0, 7)
                        .Select(offset => weekStart.AddDays(offset).Date)
                        .ToList();

                    var dailyTotals = allDays
                        .Select(day => g
                            .Where(r => r.Date.Date == day)
                            .Sum(r => r.MinutesStudied))
                        .ToList();

                    return new WeeklyStudyData
                    {
                        StartOfWeek = weekStart,
                        EndOfWeek = weekEnd,
                        TotalMinutes = dailyTotals.Sum(),
                        AverageMinutes = dailyTotals.Average(),
                        Records = g.ToList()
                    };
                })
                .OrderBy(w => w.StartOfWeek)
                .ToList();


            Records = new ObservableCollection<WeeklyStudyData>(groupedByWeek);
        }

        private async Task EditRecordAsync(WeeklyStudyData weeklyRecord)
        {
            if (weeklyRecord == null) return;

            try
            {
                var studyRecords = await _studyRecordService.GetStudyRecordsForWeekAsync(Student, weeklyRecord.StartOfWeek);

                if (!studyRecords.Any())
                {
                    MessageBox.Show("هیچ رکورد مطالعه‌ای برای این هفته یافت نشد.", "اطلاعات",
                                   MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var editView = new AddStudyRecordView(Student, studyRecords);
                var result = editView.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بارگذاری اطلاعات: {ex.Message}", "خطا",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public static DateTime GetWeekStartDate(DateTime date)
        {
            int diff = date.DayOfWeek - DayOfWeek.Saturday;

            if (diff < 0)
            {
                diff += 7;
            }

            return date.AddDays(-1 * diff).Date;
        }
    }

    
}
