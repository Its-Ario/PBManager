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
using System.Windows;

namespace PBManager.UI.MVVM.ViewModel
{
    public partial class GradeOverviewViewModel(IGradeService gradeService, IServiceProvider serviceProvider) : ObservableObject
    {
        private readonly IGradeService _gradeRecordService = gradeService;
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        public Margin DrawMargin { get; set; } = new(50, 0, 50, 50);

        [ObservableProperty]
        private ICartesianAxis[]? _gradesOverTimeXAxes;
        [ObservableProperty]
        private ICartesianAxis[]? _gradesOverTimeYAxes;
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

                await LoadGradesOverTimeChartAsync(studentId);
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

        public async Task LoadGradesOverTimeChartAsync(int studentId)
        {
            var examScores = await _gradeRecordService.GetAllExamScoresForStudentAsync(studentId);

            if (examScores == null || examScores.Count == 0) return;

            examScores = [.. examScores.OrderBy(e => e.ExamDate)];

            var values = new List<double>();
            var labels = new List<string>();

            foreach (var score in examScores)
            {
                values.Add(score.AverageScore);

                labels.Add($"{score.ExamName}");
            }

            GradesOverTimeSeries = [
                new LineSeries<double>
                {
                    Values = values,
                    Stroke = new SolidColorPaint(new SKColor(92, 107, 192), 3),
                    Fill = null,
                    GeometrySize = 10
                }
            ];

            GradesOverTimeXAxes =
            [
                new Axis
                {
                    Labels = labels,
                    LabelsRotation = 45,
                    Padding = new LiveChartsCore.Drawing.Padding(0)
                }
            ];

            GradesOverTimeYAxes = 
            [
                new Axis
                {
                    MinLimit = 0,
                    MaxLimit = 100,
                    MinStep = 5
                }
            ];
        }


        [RelayCommand]
        private async Task ViewHistory()
        {
            if (Student == null) return;

            var historyView = _serviceProvider.GetRequiredService<GradeHistoryView>();
            if (historyView.DataContext is GradeHistoryViewModel viewModel)
            {
                await viewModel.InitializeAsync(Student);
            }

            historyView.Show();
        }

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
