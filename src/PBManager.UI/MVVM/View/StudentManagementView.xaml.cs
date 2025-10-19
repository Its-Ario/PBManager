using PBManager.UI.MVVM.ViewModel;
using System.ComponentModel;
using System.Windows.Controls;

namespace PBManager.UI.MVVM.View
{
    /// <summary>
    /// Interaction logic for StudentManagementView.xaml
    /// </summary>
    public partial class StudentManagementView : UserControl
    {
        public StudentManagementView()
        {
            InitializeComponent();
        }
        private void DataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true;

            var direction = e.Column.SortDirection != ListSortDirection.Ascending
                ? ListSortDirection.Ascending
                : ListSortDirection.Descending;

            if (DataContext is StudentManagementViewModel vm)
            {
                vm.SortBy(e.Column.SortMemberPath, direction);
            }

            e.Column.SortDirection = direction;
        }

    }
}
