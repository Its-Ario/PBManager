using LiveCharts.Wpf;
using LiveCharts;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PBManager.MVVM.View;
using PBManager.MVVM.ViewModel;

namespace PBManager.MVVM.View
{
    /// <summary>
    /// Interaction logic for StudentDetailView.xaml
    /// </summary>
    public partial class StudentDetailView : UserControl
    {
        public StudentDetailViewModel? ViewModel => this.DataContext as StudentDetailViewModel;
        public StudentDetailView()
        {
            InitializeComponent();

            var studyOverTimeSeries = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "نمره",
                    Values = new ChartValues<double> { 85, 88, 82, 90 },
                    Stroke = new BrushConverter().ConvertFrom("#5C6BC0") as SolidColorBrush,
                    Fill = Brushes.Transparent,
                    PointGeometrySize = 10
                }
            };

            string[] weeks = { "هفته 1", "هفته 2", "هفته 3", "هفته 4" };

            StudyOverTimeChart.Series = studyOverTimeSeries;

            StudyOverTimeChart.AxisX.Add(new Axis
            {
                Title = "هفته‌ها",
                Labels = weeks,
                Separator = new LiveCharts.Wpf.Separator { Step = 1, IsEnabled = true }
            });

            StudyOverTimeChart.AxisY.Add(new Axis
            {
                Title = "نمره",
                LabelFormatter = value => value.ToString()
            });
        }

        private void submitNewRecord_Click(object sender, RoutedEventArgs e)
        {
            AddStudyRecordView win = new(ViewModel?.Student);
            win.Show();
        }
    }
}
