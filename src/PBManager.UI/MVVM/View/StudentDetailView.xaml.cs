using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
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
        }

        private void submitNewRecord_Click(object sender, RoutedEventArgs e)
        {
            var view = App.ServiceProvider.GetRequiredService<AddStudyRecordView>();

            if (view.DataContext is AddStudyRecordViewModel vm)
            {
                _ = vm.Initialize(ViewModel?.Student);
            }
            view.ShowDialog();
        }

        private void viewHistory_Click(object sender, RoutedEventArgs e)
        {
            StudyHistoryView win = new(ViewModel?.Student);
            win.Show();
        }
    }
}
