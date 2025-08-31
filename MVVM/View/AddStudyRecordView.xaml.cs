using CommunityToolkit.Mvvm.Input;
using PBManager.MVVM.Model;
using PBManager.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
    }
}
