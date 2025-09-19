using PBManager.Messages;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using LiveChartsCore.Measure;
using PBManager.Core.Entities;
using PBManager.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace PBManager.UI.MVVM.ViewModel
{
    public partial class StudentDetailViewModel : ObservableObject, IRecipient<StudentSelectedMessage>
    {
        private readonly IStudyRecordService _studyRecordService;
        
        public Margin DrawMargin { get; set; } = new(50, 0, 50, 50);

        [ObservableProperty]
        private ICartesianAxis[] _studyOverTimeXAxes;
        [ObservableProperty]
        private ISeries[] _studyOverTimeSeries;
        [ObservableProperty]
        private Student? _student;
        [ObservableProperty]
        private double _avgWeeklyStudy;
        [ObservableProperty]
        private int _classRank;
        [ObservableProperty]
        private int _globalRank;

        public StudentDetailViewModel()
        {
            _studyRecordService = App.ServiceProvider.GetRequiredService<IStudyRecordService>();
            WeakReferenceMessenger.Default.Register(this);
        }

        public void Receive(StudentSelectedMessage message)
        {
            Student = message.Value;

            _ = LoadAsync(Student.Id);
        }

        public async Task LoadAsync(int studentId)
        {
            try
            {
                Task<double> avgStudyTask = _studyRecordService.GetStudentWeeklyAverageAsync(studentId);
                Task<int> classRankTask = _studyRecordService.GetClassWeeklyRankAsync(studentId);
                Task<int> globalRankTask = _studyRecordService.GetGlobalWeeklyRankAsync(studentId);

                await Task.WhenAll(avgStudyTask, classRankTask, globalRankTask);

                AvgWeeklyStudy = await avgStudyTask;
                ClassRank = await classRankTask;
                GlobalRank = await globalRankTask;

                await LoadStudyOverTimeChartAsync(Student.Id);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Data loading was cancelled.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load student data: {ex.Message}");
            }
        }

        public async Task LoadStudyOverTimeChartAsync(int studentId)
        {
            var weeklyData = await _studyRecordService.GetWeeklyStudyDataAsync(studentId, 4);

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
        }
    }
}
