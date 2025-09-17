using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using PBManager.Application.Interfaces;
using PBManager.Core.Entities;
using PBManager.MVVM.View;
using PBManager.UI.MVVM.ViewModel.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace PBManager.MVVM.ViewModel
{
    public partial class StudyHistoryViewModel : ObservableObject
    {
        private readonly IStudyRecordService _studyRecordService;

        [ObservableProperty]
        private Student _student;
        [ObservableProperty]
        private ObservableCollection<WeeklyStudyData> _records;
        [ObservableProperty]
        private ObservableCollection<WeeklyStudyData> _filteredRecords;
        [ObservableProperty]
        private int _totalWeeksCount;
        [ObservableProperty]
        private int _presentWeeksCount;
        [ObservableProperty]
        private int _absentWeeksCount;
        [ObservableProperty]
        private bool _showAll = true;
        [ObservableProperty]
        private bool _showPresent;
        [ObservableProperty]
        private bool _showAbsent;
        [ObservableProperty]
        private string _emptyStateMessage;
        [ObservableProperty]
        private bool _isEmptyState;

        public StudyHistoryViewModel(Student student)
        {
            Student = student;
            _studyRecordService = App.ServiceProvider.GetRequiredService<IStudyRecordService>();

            _ = LoadData(Student.Id);
        }

        public async Task LoadData(int studentId)
        {
            try
            {
                List<StudyRecord> studyRecords = await _studyRecordService.GetStudyRecordsForStudentAsync(studentId);

                List<DateTime> absentDates = await _studyRecordService.GetStudentAbsentWeeksAsync(studentId);

                var allDates = new List<DateTime>();
                allDates.AddRange(studyRecords.Select(r => r.Date));
                allDates.AddRange(absentDates);

                if (allDates.Count == 0)
                {
                    Records = new ObservableCollection<WeeklyStudyData>();
                    UpdateStatistics();
                    ApplyFilter();
                    return;
                }

                var allWeekStarts = allDates
                    .Select(date => GetWeekStartDate(date))
                    .Distinct()
                    .OrderBy(date => date)
                    .ToList();

                var weeklyData = new List<WeeklyStudyData>();

                foreach (var weekStart in allWeekStarts)
                {
                    var weekEnd = weekStart.AddDays(6);
                    var weekRecords = studyRecords
                        .Where(r => GetWeekStartDate(r.Date) == weekStart)
                        .ToList();

                    var weekDays = Enumerable.Range(0, 7)
                        .Select(offset => weekStart.AddDays(offset).Date)
                        .ToList();

                    bool isAbsentWeek = weekDays.Any(day => absentDates.Any(absDate => absDate.Date == day));

                    WeeklyStudyData weeklyStudyData;

                    if (isAbsentWeek)
                    {
                        weeklyStudyData = new WeeklyStudyData
                        {
                            StartOfWeek = weekStart,
                            EndOfWeek = weekEnd,
                            TotalMinutes = 0,
                            AverageMinutes = 0,
                            Records = new List<StudyRecord>(),
                            IsAbsent = true
                        };
                    }
                    else
                    {
                        var dailyTotals = weekDays
                            .Select(day => weekRecords
                                .Where(r => r.Date.Date == day)
                                .Sum(r => r.MinutesStudied))
                            .ToList();

                        weeklyStudyData = new WeeklyStudyData
                        {
                            StartOfWeek = weekStart,
                            EndOfWeek = weekEnd,
                            TotalMinutes = dailyTotals.Sum(),
                            AverageMinutes = dailyTotals.Count > 0 ? dailyTotals.Average() : 0,
                            Records = weekRecords,
                            IsAbsent = false
                        };
                    }

                    weeklyData.Add(weeklyStudyData);
                }

                Records = new ObservableCollection<WeeklyStudyData>(weeklyData.OrderByDescending(w => w.StartOfWeek));
                UpdateStatistics();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بارگذاری اطلاعات: {ex.Message}", "خطا",
                               MessageBoxButton.OK, MessageBoxImage.Error);
                Records = new ObservableCollection<WeeklyStudyData>();
                UpdateStatistics();
                ApplyFilter();
            }
        }

        private void UpdateStatistics()
        {
            if (Records == null)
            {
                TotalWeeksCount = 0;
                PresentWeeksCount = 0;
                AbsentWeeksCount = 0;
                return;
            }

            TotalWeeksCount = Records.Count;
            PresentWeeksCount = Records.Count(r => !r.IsAbsent);
            AbsentWeeksCount = Records.Count(r => r.IsAbsent);
        }

        [RelayCommand]
        private void SetFilter(string filterType)
        {
            ShowAll = false;
            ShowPresent = false;
            ShowAbsent = false;

            switch (filterType)
            {
                case "All":
                    ShowAll = true;
                    break;
                case "Present":
                    ShowPresent = true;
                    break;
                case "Absent":
                    ShowAbsent = true;
                    break;
            }

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (Records == null || Records.Count == 0)
            {
                FilteredRecords = new ObservableCollection<WeeklyStudyData>();
                EmptyStateMessage = "هیچ داده‌ای برای نمایش وجود ندارد";
                IsEmptyState = true;
                return;
            }

            IEnumerable<WeeklyStudyData> filtered = Records;

            if (ShowPresent)
            {
                filtered = Records.Where(r => !r.IsAbsent);
                EmptyStateMessage = "هیچ هفته حضوری یافت نشد";
            }
            else if (ShowAbsent)
            {
                filtered = Records.Where(r => r.IsAbsent);
                EmptyStateMessage = "هیچ هفته غیبتی یافت نشد";
            }
            else
            {
                EmptyStateMessage = "هیچ داده‌ای برای نمایش وجود ندارد";
            }

            FilteredRecords = new ObservableCollection<WeeklyStudyData>(filtered);
            IsEmptyState = FilteredRecords.Count == 0;
        }

        [RelayCommand]
        private async Task EditRecordAsync(WeeklyStudyData weeklyRecord)
        {
            if (weeklyRecord == null) return;

            var view = App.ServiceProvider.GetRequiredService<AddStudyRecordView>();


            if (weeklyRecord.IsAbsent)
            {
                if (view.DataContext is AddStudyRecordViewModel vm)
                {
                    _ = vm.Initialize(Student, weeklyRecord.StartOfWeek);
                }
                var result = view.ShowDialog();
                return;
            }

            try
            {
                var studyRecords = await _studyRecordService.GetStudyRecordsForWeekAsync(Student, weeklyRecord.StartOfWeek);

                if (view.DataContext is AddStudyRecordViewModel vm)
                {
                    _ = vm.Initialize(Student, studyRecords);
                }
                var result = view.ShowDialog();

                if (result == true)
                {
                    await LoadData(Student.Id);
                }
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