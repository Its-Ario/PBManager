using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using LiveChartsCore.Measure;
using PBManager.Core.Entities;
using PBManager.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using PBManager.UI.MVVM.View;

namespace PBManager.UI.MVVM.ViewModel
{
    public partial class StudyOverviewViewModel(IStudyRecordService studyRecordService, IServiceProvider serviceProvider) : ObservableObject
    {
        private readonly IStudyRecordService _studyRecordService = studyRecordService;
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        
        public Margin DrawMargin { get; set; } = new(50, 0, 50, 50);

        [ObservableProperty]
        private ICartesianAxis[]? _studyOverTimeXAxes;
        [ObservableProperty]
        private ISeries[]? _studyOverTimeSeries;
        [ObservableProperty]
        private Student? _student;
        [ObservableProperty]
        private double _avgWeeklyStudy;
        [ObservableProperty]
        private int _classRank;
        [ObservableProperty]
        private int _globalRank;

        public async Task InitializeAsync(Student student)
        {
            if (student == null) return;
            Student = student;
            await LoadAsync(student.Id);
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

                await LoadStudyOverTimeChartAsync(studentId);
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
            foreach (var (_, _, minutes) in weeklyData)
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

        [RelayCommand]
        private async Task ViewHistory()
        {
            if (Student == null) return;

            var historyView = _serviceProvider.GetRequiredService<StudyHistoryView>();
            if (historyView.DataContext is StudyHistoryViewModel viewModel)
            {
                 await viewModel.InitializeAsync(Student);
            }

            historyView.Show();
        }

        [RelayCommand]
        private async Task SubmitNewRecord()
        {
            if (Student == null) return;

            var recordView = _serviceProvider.GetRequiredService<AddStudyRecordView>();
            if (recordView.DataContext is AddStudyRecordViewModel viewModel)
            {
                await viewModel.InitializeAsync(Student);
            }

            recordView.Show();
        }
    }
}
