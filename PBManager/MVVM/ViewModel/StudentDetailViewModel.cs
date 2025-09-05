using PBManager.MVVM.Model;
using PBManager.Services;
using PBManager.Messages;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveCharts.Wpf;
using LiveCharts;
using System.Windows.Media;

namespace PBManager.MVVM.ViewModel
{
    public class StudentDetailViewModel : ObservableObject, IRecipient<StudentSelectedMessage>
    {
        private readonly StudyRecordService _studyRecordService;

        private SeriesCollection _studyOverTimeSeries;
        public SeriesCollection StudyOverTimeSeries
        {
            get => _studyOverTimeSeries;
            set => SetProperty(ref _studyOverTimeSeries, value);
        }

        private List<string> _studyOverTimeLabels;
        public List<string> StudyOverTimeLabels
        {
            get => _studyOverTimeLabels;
            set => SetProperty(ref _studyOverTimeLabels, value);
        }


        private Student? _student;
        public Student? Student
        {
            get => _student;
            private set => SetProperty(ref _student, value);
        }

        private double _avgWeeklyStudy;
        public double AvgWeeklyStudy {
            get => _avgWeeklyStudy;
            set => SetProperty(ref _avgWeeklyStudy, value);
        }

        private int _classRank;
        public int ClassRank
        {
            get => _classRank;
            set => SetProperty(ref _classRank, value);
        }

        private int _globalRank;
        public int GlobalRank
        {
            get => _globalRank;
            set => SetProperty(ref _globalRank, value);
        }

        public StudentDetailViewModel()
        {
            _studyRecordService = new();
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
    }
}
