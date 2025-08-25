using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBManager.MVVM.ViewModel
{
    using System.ComponentModel;
    using PBManager.Core;

    internal class HomeViewModel : ObservableObject
    {
        private int _avgGrade;
        public int AvgGrade
        {
            get => _avgGrade;
            set { _avgGrade = value; OnPropertyChanged(nameof(AvgGrade)); }
        }

        private int _totalStudents;
        public int TotalStudents
        {
            get => _totalStudents;
            set { _totalStudents = value; OnPropertyChanged(nameof(TotalStudents)); }
        }

        private int _classesCount;
        public int ClassesCount
        {
            get => _classesCount;
            set { _classesCount = value; OnPropertyChanged(nameof(ClassesCount)); }
        }

        public HomeViewModel()
        {
            AvgGrade = -5;
            TotalStudents = 30;
            ClassesCount = 5;
        }
    }
}
