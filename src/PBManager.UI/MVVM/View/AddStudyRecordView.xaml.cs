using PBManager.Core.Entities;
using PBManager.MVVM.ViewModel;
using System.Windows;
using System.Windows.Input;

namespace PBManager.MVVM.View
{
    /// <summary>
    /// Interaction logic for AddStudyRecordView.xaml
    /// </summary>
    public partial class AddStudyRecordView : Window
    {
        public AddStudyRecordView(AddStudyRecordViewModel viewModel)
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

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            InfoPopup.IsOpen = !InfoPopup.IsOpen;
        }

        private void WeekStartDatePicker_SelectedDateChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is AddStudyRecordViewModel viewModel)
            {
                viewModel.SelectedWeekStart = WeekStartDatePicker.SelectedDate;
            }
        }

        private void submitBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}