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
    class StudyHistoryViewModel : ObservableObject
    {
        private readonly IStudyRecordService _studyRecordService;

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

        private ObservableCollection<WeeklyStudyData> _filteredRecords;
        public ObservableCollection<WeeklyStudyData> FilteredRecords
        {
            get => _filteredRecords;
            set => SetProperty(ref _filteredRecords, value);
        }

        private int _totalWeeksCount;
        public int TotalWeeksCount
        {
            get => _totalWeeksCount;
            set => SetProperty(ref _totalWeeksCount, value);
        }

        private int _presentWeeksCount;
        public int PresentWeeksCount
        {
            get => _presentWeeksCount;
            set => SetProperty(ref _presentWeeksCount, value);
        }

        private int _absentWeeksCount;
        public int AbsentWeeksCount
        {
            get => _absentWeeksCount;
            set => SetProperty(ref _absentWeeksCount, value);
        }

        private bool _showAll = true;
        public bool ShowAll
        {
            get => _showAll;
            set => SetProperty(ref _showAll, value);
        }

        private bool _showPresent;
        public bool ShowPresent
        {
            get => _showPresent;
            set => SetProperty(ref _showPresent, value);
        }

        private bool _showAbsent;
        public bool ShowAbsent
        {
            get => _showAbsent;
            set => SetProperty(ref _showAbsent, value);
        }

        private string _emptyStateMessage;
        public string EmptyStateMessage
        {
            get => _emptyStateMessage;
            set => SetProperty(ref _emptyStateMessage, value);
        }

        private bool _isEmptyState;
        public bool IsEmptyState
        {
            get => _isEmptyState;
            set => SetProperty(ref _isEmptyState, value);
        }

        public IRelayCommand<WeeklyStudyData> EditRecordCommand { get; }
        public IRelayCommand<string> SetFilterCommand { get; }

        public StudyHistoryViewModel(Student student)
        {
            Student = student;
            _studyRecordService = App.ServiceProvider.GetRequiredService<IStudyRecordService>();

            _ = LoadData(Student.Id);

            EditRecordCommand = new RelayCommand<WeeklyStudyData>(async (weeklyRecord) => await EditRecordAsync(weeklyRecord));
            SetFilterCommand = new RelayCommand<string>(SetFilter);
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