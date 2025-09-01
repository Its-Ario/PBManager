using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveCharts.Wpf;
using LiveCharts;
using PBManager.MVVM.Model;
using PBManager.Services;
using System.Diagnostics;

namespace PBManager.MVVM.ViewModel
{
    internal class HomeViewModel : ObservableObject
    {
        private readonly StudyRecordService _studyRecordService;
        public SeriesCollection StudyOverTimeSeries { get; set; }
        public List<string> StudyOverTimeLabels { get; set; }

        private double _avgStudyTime;
        public double AvgStudyTime
        {
            get => _avgStudyTime;
            set { _avgStudyTime = value; OnPropertyChanged(); }
        }

        private string _mostStudiedSubject;
        public string MostStudiedSubject
        {
            get => _mostStudiedSubject;
            set { _mostStudiedSubject = value; OnPropertyChanged(); }
        }

        private int _classesCount;
        public int ClassesCount
        {
            get => _classesCount;
            set { _classesCount = value; OnPropertyChanged(); }
        }

        public HomeViewModel()
        {
            _studyRecordService = new();

            _ = LoadData();
        }

        private async Task LoadData()
        {
            AvgStudyTime = await _studyRecordService.GetWeeklyAverageAsync();
            Subject? subject = await _studyRecordService.GetMostStudiedWeeklySubjectAsync();
            MostStudiedSubject = subject?.Name;
            ClassesCount = App.Db.Classes.Count();

            await LoadStudyOverTimeChartAsync();
        }

        public async Task LoadStudyOverTimeChartAsync()
        {
            var weeklyData = await _studyRecordService.GetWeeklyStudyDataAsync();

            var values = new ChartValues<double>();
            var labels = new List<string>();

            int i = 1;
            foreach (var (start, end, minutes) in weeklyData)
            {
                values.Add(minutes);
                labels.Add($"هفته {i++}");
            }

            StudyOverTimeSeries = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "(زمان مطالعه (دقیقه",
                    Values = values,
                    Stroke = new BrushConverter().ConvertFrom("#5C6BC0") as SolidColorBrush,
                    Fill = Brushes.Transparent,
                    PointGeometrySize = 10
                }
            };

            StudyOverTimeLabels = labels;
        }
    }
}
