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
        public StudyHistoryView(Student student)
        {
            InitializeComponent();
            this.DataContext = new StudyHistoryViewModel(student);
        }
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
    }
}
