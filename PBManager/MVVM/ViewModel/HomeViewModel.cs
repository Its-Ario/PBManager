using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveCharts.Wpf;
using LiveCharts;
using PBManager.MVVM.Model;
using PBManager.Services;
using Microsoft.EntityFrameworkCore;

namespace PBManager.MVVM.ViewModel
{
    internal class HomeViewModel : ObservableObject
    {
        private readonly StudyRecordService _studyRecordService;
        public SeriesCollection StudyOverTimeSeries { get; set; }
        public SeriesCollection StudyPerSubjectSeries { get; set; }
        public List<string> StudyOverTimeLabels { get; set; }
        public List<string> StudyPerSubjectLabels { get; set; }

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

        private int _absentees;
        public int Absentees
        {
            get => _absentees;
            set { _absentees = value; OnPropertyChanged(); }
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
            Absentees = await _studyRecordService.GetWeeklyAbsencesAsync();

            await LoadStudyOverTimeChartAsync();
            await LoadStudyPerSubjectChartAsync();
        }

        public async Task LoadStudyOverTimeChartAsync()
        {
            var weeklyData = await _studyRecordService.GetWeeklyStudyDataAsync(weeks: 8);

            var values = new ChartValues<double>();
            var labels = new List<string>();

            int i = 1;
            foreach (var (start, end, minutes) in weeklyData)
            {
                values.Add(minutes);
                labels.Add($"هفته {i++}");
            }

            StudyOverTimeSeries =
            [
                new LineSeries
                {
                    Title = "(زمان مطالعه (دقیقه",
                    Values = values,
                    Stroke = new BrushConverter().ConvertFrom("#5C6BC0") as SolidColorBrush,
                    Fill = Brushes.Transparent,
                    PointGeometrySize = 10
                }
            ];

            StudyOverTimeLabels = labels;
        }

        public async Task LoadStudyPerSubjectChartAsync()
        {
            List<Subject> subjects = await App.Db.Subjects.ToListAsync();
            ChartValues<double> values = [];

            foreach (var subject in subjects)
            {
                values.Add(await _studyRecordService.GetSubjectWeeklyAverageAsync(subject.Id));
            }

            StudyPerSubjectSeries =
            [
                new ColumnSeries
                {
                    Title = "میانگین",
                    Values = values,
                    Fill = new BrushConverter().ConvertFrom("#5C6BC0") as SolidColorBrush,

                }
            ];

            StudyPerSubjectLabels = subjects.Select(s => s.Name).ToList();
        }
    }
}
