using PBManager.UI.MVVM.ViewModel;
using System.Windows;
using System.Windows.Input;

namespace PBManager.UI.MVVM.View
{
    /// <summary>
    /// Interaction logic for AddGradeRecordView.xaml
    /// </summary>
    public partial class AddGradeRecordView : Window
    {
        public AddGradeRecordView(AddGradeRecordViewModel viewModel)
        {
            InitializeComponent();

            this.DataContext = viewModel;
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
