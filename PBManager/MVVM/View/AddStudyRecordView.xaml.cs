using PBManager.MVVM.Model;
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
        public AddStudyRecordView(Student student)
        {
            InitializeComponent();
            this.DataContext = new AddStudyRecordViewModel(student);
            this.Title = "ثبت اطلاعات مطالعه";
        }

        public AddStudyRecordView(Student student, IEnumerable<StudyRecord> existingRecords)
        {
            InitializeComponent();
            this.DataContext = new AddStudyRecordViewModel(student, existingRecords);
            this.Title = "ویرایش اطلاعات مطالعه";
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
            if (DataContext is AddStudyRecordViewModel viewModel && WeekStartDatePicker.SelectedDate != null)
            {
                viewModel.SelectedWeekStart = WeekStartDatePicker.SelectedDate;
            }
        }
    }
}