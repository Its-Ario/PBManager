using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using Microsoft.Extensions.DependencyInjection;
using PBManager.Application.Interfaces;
using PBManager.Core.Entities;
using PBManager.UI.MVVM.View;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using PBManager.Application.DTOs;

namespace PBManager.UI.MVVM.ViewModel
{
    public partial class ExamOverviewViewModel : ObservableObject
    {
        private readonly IExamService _examService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IGradeService _gradeService;

        [ObservableProperty]
        private Exam? _exam;

        [ObservableProperty]
        private Subject _subject;

        [ObservableProperty]
        private int _participants;

        [ObservableProperty]
        private ISeries[] _scoreDistributionSeries;

        [ObservableProperty]
        private ICartesianAxis[] _xAxes;

        [ObservableProperty]
        private ICartesianAxis[] _yAxes;

        public ExamOverviewViewModel(IExamService examService, IServiceProvider serviceProvider, IGradeService gradeService)
        {
            _examService = examService;
            _serviceProvider = serviceProvider;
            _gradeService = gradeService;
        }

        public async Task InitializeAsync(Exam exam)
        {
            if (exam == null) return;
            Exam = exam;
            Participants = await _examService.GetParticipantCountAsync(exam.Id);
            Subject = exam.Subjects.First();

            var grades = await _gradeService.GetExamRecords(exam.Id);
            LoadChart(grades.Select(g => g.Score));
        }

        private void LoadChart(IEnumerable<double> scores)
        {
            if(Exam == null) return;

            int maxScore = Exam.MaxScore;
            int binSize = maxScore / 5;

            var ranges = new List<(int Start, int End)>();
            int start = 0;
            while (start <= maxScore)
            {
                int end = Math.Min(start + binSize - 1, maxScore);
                int remainingAfterThis = maxScore - end;

                if (remainingAfterThis > 0 && remainingAfterThis < binSize)
                {
                    end = maxScore;
                }

                ranges.Add((Start: start, End: end));
                start = end + 1;
            }

            if (ranges.Count == 0) return;

            var counts = new int[ranges.Count];
            foreach (var score in scores)
            {
                if (score < 0 || score > maxScore) continue;

                int index = (int)Math.Min(score / binSize, ranges.Count - 1);
                counts[index]++;
            }

            ScoreDistributionSeries =
            [
                new ColumnSeries<int>
                {
                    Values = counts,
                    Name = "تعداد",
                    Stroke = null,
                    Fill = new SolidColorPaint(new SKColor(92, 107, 192))
                }
            ];

            XAxes = [
                new Axis
                {
                    Labels = ranges
                        .Select(r => r.Start == r.End ? $"{r.Start}" : $"{r.Start} - {r.End}")
                        .ToArray(),
                    LabelsRotation = 0,
                }
            ];

            YAxes =
            [
                new Axis
                {
                    MinLimit = 0,
                    MinStep = 1
                }
            ];
        }


        [RelayCommand]
        private async Task UpdateExamAsync()
        {
            if (Exam == null) return;
            
            var view = _serviceProvider.GetRequiredService<AddExamView>();
            if (view.DataContext is AddExamViewModel viewModel)
            {
                await viewModel.InitializeAsync(Exam);
            }
            view.Show();
        }
    }
}
