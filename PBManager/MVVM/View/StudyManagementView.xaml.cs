using PBManager.MVVM.ViewModel;
using System.Windows.Controls;

namespace PBManager.MVVM.View
{
    /// <summary>
    /// Interaction logic for StudyManagementView.xaml
    /// </summary>
    public partial class StudyManagementView : UserControl
    {
        public StudyManagementView()
        {
            InitializeComponent();
            this.DataContext = new StudyManagementViewModel();
        }
    }
}
