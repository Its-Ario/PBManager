using PBManager.MVVM.Model;
using PBManager.MVVM.ViewModel;
using System.Windows;
using System.Windows.Input;

namespace PBManager.MVVM.View
{
    /// <summary>
    /// Interaction logic for StudyHistoryView.xaml
    /// </summary>
    public partial class StudyHistoryView : Window
    {
        private StudyHistoryViewModel _viewModel;

        public StudyHistoryView(Student student)
        {
            InitializeComponent();
            this.DataContext = new StudyHistoryViewModel(student);
            _viewModel = this.DataContext as StudyHistoryViewModel;
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ReloadButton.IsEnabled = false;

                var originalContent = ReloadButton.Content;
                ReloadButton.Content = "⏳";

                if (_viewModel?.Student != null)
                {
                    await _viewModel.LoadData(_viewModel.Student.Id);
                }

                ReloadButton.Content = "✅";
                await Task.Delay(1000);

                ReloadButton.Content = originalContent;
                ReloadButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                ReloadButton.Content = "🔄";
                ReloadButton.IsEnabled = true;

                System.Diagnostics.Debug.WriteLine($"Error in ReloadButton_Click: {ex.Message}");
            }
        }
    }
}
