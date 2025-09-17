using Microsoft.Win32;
using PBManager.MVVM.ViewModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace PBManager.MVVM.View
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        public SettingsViewModel? ViewModel => this.DataContext as SettingsViewModel;

        public SettingsView()
        {
            InitializeComponent();

            versionLabel.Text = Assembly
                .GetExecutingAssembly()
                .GetCustomAttribute<AssemblyFileVersionAttribute>()?
                .Version ?? "----";

        }

        private async void ImportStudentsButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select a file",
                Filter =
                    "Supported files (*.csv;*.xlsx)|*.csv;*.xlsx|" +
                    "CSV files (*.csv)|*.csv|" +
                    "Excel files (*.xlsx)|*.xlsx"
            };

            if (dialog.ShowDialog() == true)
            {
                string filePath = dialog.FileName;
                string extension = Path.GetExtension(filePath).ToLowerInvariant();

                try
                {
                    await ViewModel.ImportStudentsAsync(filePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطا: {ex.Message}", "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
