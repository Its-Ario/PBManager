using LiveCharts.Wpf;
using LiveCharts;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PBManager.MVVM.View;
using PBManager.MVVM.ViewModel;
using PBManager.Services;

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
        }

        private void submitNewRecord_Click(object sender, RoutedEventArgs e)
        {
            AddStudyRecordView win = new(ViewModel?.Student);
            win.Show();
        }

        private void viewHistory_Click(object sender, RoutedEventArgs e)
        {
            StudyHistoryView win = new(ViewModel?.Student);
            win.Show();
        }
    }
}
