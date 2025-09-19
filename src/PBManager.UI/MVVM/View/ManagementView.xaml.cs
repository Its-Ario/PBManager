using System.Windows;
using System.Windows.Input;

namespace PBManager.UI.MVVM.View
{
    /// <summary>
    /// Interaction logic for ManagementVieww.xaml
    /// </summary>
    public partial class ManagementView : Window
    {
        public ManagementView()
        {
            InitializeComponent();
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
