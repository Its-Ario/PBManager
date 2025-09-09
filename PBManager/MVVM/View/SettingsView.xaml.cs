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
                Filter = "CSV files (*.csv)|*.csv|Excel files (*.xlsx)|*.xlsx"
            };

            if (dialog.ShowDialog() == true)
            {
                string filePath = dialog.FileName;
                string extension = Path.GetExtension(filePath).ToLowerInvariant();

                switch (extension)
                {
                    case ".csv":
                        await ViewModel.ImportStudentsCsv(filePath);
                        break;
                    case ".xlsx":
                        await ViewModel.ImportStudentsXlsx(filePath);
                        break;
                    default:
                        MessageBox.Show("Unsupported Format");
                        break;
                }
             }
        }
    }
}
