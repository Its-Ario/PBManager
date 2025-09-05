using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using PBManager.MVVM.Model;
using PBManager.Services;
using Microsoft.EntityFrameworkCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using LiveChartsCore.Measure;

namespace PBManager.MVVM.ViewModel
{
    internal class HomeViewModel : ObservableObject
    {
        private readonly StudyRecordService _studyRecordService;
        public ISeries[] StudyOverTimeSeries { get; set; } = [];
        public ICartesianAxis[] StudyOverTimeXAxes { get; set; } = [];
        public ICartesianAxis[] StudyOverTimeYAxes { get; set; } = [];

        public ISeries[] StudyPerSubjectSeries { get; set; } = [];
        public ICartesianAxis[] StudyPerSubjectXAxes { get; set; } = [];
        public ICartesianAxis[] StudyPerSubjectYAxes { get; set; } = [];
        public Margin DrawMargin { get; set; } = new(50,0,50,50);

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

            var values = new List<double>();
            var labels = new List<string>();

            int i = 1;
            foreach (var (start, end, minutes) in weeklyData)
            {
                values.Add(minutes);
                labels.Add($"هفته {i++}");
            }

            StudyOverTimeSeries =
            [
            new LineSeries<double>
            {
                Name = "(زمان مطالعه (دقیقه",
                Values = values,
                Stroke = new SolidColorPaint(new SKColor(92, 107, 192), 3),
                Fill = null,
                GeometrySize = 10
                            }
            ];

            StudyOverTimeXAxes =
                [
                new Axis
                {
                    Labels = labels,
                    LabelsRotation = 45,
                    Padding = new LiveChartsCore.Drawing.Padding(0),
                }
            ];

            StudyOverTimeYAxes =
            [
                new Axis
                {
                    TextSize = 20,
                    LabelsDensity = 0,
                }
            ];
        }

        public async Task LoadStudyPerSubjectChartAsync()
        {
            List<Subject> subjects = await App.Db.Subjects.ToListAsync();
            List<double> values = [];

            foreach (var subject in subjects)
            {
                values.Add(await _studyRecordService.GetSubjectWeeklyAverageAsync(subject.Id));
            }

            StudyPerSubjectSeries =
            [
                new ColumnSeries<double>
                {
                    Name = "میانگین",
                    Values = values,
                    Fill = new SolidColorPaint(new SKColor(92, 107, 192)),
                }
            ];

            StudyPerSubjectXAxes =
            [
                new Axis
                {
                    Labels = subjects.Select(s => s.Name).ToList(),
                    LabelsRotation = 45,
                    CustomSeparators = Enumerable.Range(0, subjects.Count).Select(i => (double)i).ToList(),
                    Padding = new LiveChartsCore.Drawing.Padding(0),

                }
            ];
        }
    }
}
