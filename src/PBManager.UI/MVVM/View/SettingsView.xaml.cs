using Microsoft.Win32;
using PBManager.UI.MVVM.ViewModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace PBManager.UI.MVVM.View
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        public SettingsViewModel ViewModel => (SettingsViewModel)this.DataContext!;

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

        private async void ImportDbButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select a file",
                Filter = "Sharifi Backup (*.sharifi)|*.sharifi|All Files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                string filePath = dialog.FileName;
                if (filePath == null) MessageBox.Show("فایل نامعتبر");
                else await ViewModel.ImportDatabaseAsync(filePath);
            }
        }

        private async void ExportDbButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Select output",
                Filter = "Sharifi Backup (*.sharifi)|*.sharifi|All Files (*.*)|*.*",
                FileName = $"PB_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.sharifi"
            };

            if (dialog.ShowDialog() == true)
            {
                string filePath = dialog.FileName;
                await ViewModel.ExportDatabaseAsync(filePath);
            }
        }

        private async void ExportStudentsButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Select output",
                Filter = "Excel file (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                FileName = $"PB_Students_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (dialog.ShowDialog() == true)
            {
                string filePath = dialog.FileName;
                await ViewModel.ExportStudentsAsync(filePath);
            }
        }
    }
}
