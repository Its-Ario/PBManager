using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using Microsoft.Extensions.DependencyInjection;
using PBManager.Application.Interfaces;
using PBManager.Core.Entities;
using PBManager.UI.MVVM.View;
using SkiaSharp;

namespace PBManager.UI.MVVM.ViewModel
{
    public partial class GradeOverviewViewModel : ObservableObject
    {
        private readonly IGradeService _gradeRecordService;
        private readonly IServiceProvider _serviceProvider;

        public Margin DrawMargin { get; set; } = new(50, 0, 50, 50);

        [ObservableProperty]
        private ICartesianAxis[]? _gradesOverTimeXAxes;
        [ObservableProperty]
        private ISeries[]? _gradesOverTimeSeries;
        [ObservableProperty]
        private Student? _student;
        [ObservableProperty]
        private double _avgGrade;
        [ObservableProperty]
        private int _classRank;
        [ObservableProperty]
        private int _globalRank;

        public GradeOverviewViewModel(IGradeService gradeService, IServiceProvider serviceProvider)
        {
            _gradeRecordService = gradeService;
            _serviceProvider = serviceProvider;
        }

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
                AvgGrade = await _gradeRecordService.GetAverageGradeForStudentAsync(studentId);
                ClassRank = await _gradeRecordService.GetClassExamRankAsync(studentId);
                GlobalRank = await _gradeRecordService.GetOverallExamRankAsync(studentId);

                //await LoadStudyOverTimeChartAsync(studentId);
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

        //public async Task LoadStudyOverTimeChartAsync(int studentId)
        //{
        //    var weeklyData = await _gradeRecordService.GetWeeklyStudyDataAsync(studentId, 4);

        //    var values = new List<double>();
        //    var labels = new List<string>();

        //    int i = 1;
        //    foreach (var (_, _, minutes) in weeklyData)
        //    {
        //        values.Add(minutes);
        //        labels.Add($"هفته {i++}");
        //    }

        //    StudyOverTimeSeries =
        //    [
        //        new LineSeries<double>
        //        {
        //            Values = values,
        //            Stroke = new SolidColorPaint(new SKColor(92, 107, 192), 3),
        //            Fill = null,
        //            GeometrySize = 10
        //        }
        //    ];

        //    StudyOverTimeXAxes =
        //       [
        //       new Axis
        //        {
        //            Labels = labels,
        //            LabelsRotation = 45,
        //            Padding = new LiveChartsCore.Drawing.Padding(0),
        //        }
        //   ];
        //}

        //[RelayCommand]
        //private async Task ViewHistory()
        //{
        //    if (Student == null) return;

        //    var historyView = _serviceProvider.GetRequiredService<StudyHistoryView>();
        //    if (historyView.DataContext is StudyHistoryViewModel viewModel)
        //    {
        //        await viewModel.InitializeAsync(Student);
        //    }

        //    historyView.Show();
        //}

        [RelayCommand]
        private async Task SubmitNewRecord()
        {
            if (Student == null) return;

            var recordView = _serviceProvider.GetRequiredService<AddGradeRecordView>();
            if (recordView.DataContext is AddGradeRecordViewModel viewModel)
            {
                await viewModel.InitializeAsync(Student);
            }

            recordView.Show();
        }
    }
}
