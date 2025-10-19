using PBManager.UI.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PBManager.UI.MVVM.View
{
    /// <summary>
    /// Interaction logic for ExamManagementView.xaml
    /// </summary>
    public partial class ExamManagementView : UserControl
    {
        public ExamManagementView()
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
