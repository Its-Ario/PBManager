using LiveCharts.Wpf;
using LiveCharts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PBManager.MVVM.ViewModel;
using System.Windows.Ink;

namespace PBManager.MVVM.View
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {
        public SeriesCollection GradesOverTimeSeries { get; set; }
        public string[] Dates { get; set; }
        public HomeView()
        {
            InitializeComponent();
            this.DataContext = new HomeViewModel();

            SubjectBarChart.Series = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "میانگین",
                    Values = new ChartValues<double> { 85, 78, 92, 70, 95 },
                    Fill = new BrushConverter().ConvertFrom("#5C6BC0") as SolidColorBrush,

                }
            };

            SubjectBarChart.AxisX.Add(new Axis
            {
                Labels = new[] { "ریاضی", "زیست", "فیزیک", "شیمی", "ادبیات" },
                Separator = new LiveCharts.Wpf.Separator { Step = 1, IsEnabled = false }
            });

            SubjectBarChart.AxisY.Add(new Axis
            {
                Title = "نمره",
                LabelFormatter = value => value.ToString()
            });
        }
    }
}
