using PBManager.UI.MVVM.ViewModel;
using System.Windows;
using System.Windows.Input;

namespace PBManager.UI.MVVM.View
{
    public partial class GradeHistoryView : Window
    {
        public GradeHistoryViewModel? ViewModel => this.DataContext as GradeHistoryViewModel;

        public GradeHistoryView(GradeHistoryViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ReloadButton.IsEnabled = false;

                var originalContent = ReloadButton.Content;
                ReloadButton.Content = "⏳";

                if (ViewModel?.Student != null)
                {
                    await ViewModel.LoadData(ViewModel.Student.Id);
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