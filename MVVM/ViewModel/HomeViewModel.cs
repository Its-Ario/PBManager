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
        private int _avgStudyTime;
        public int AvgStudyTime
        {
            get => _avgStudyTime;
            set { _avgStudyTime = value; OnPropertyChanged(nameof(AvgStudyTime)); }
        }

        private string _mostStudiedSubject;
        public string MostStudiedSubject
        {
            get => _mostStudiedSubject;
            set { _mostStudiedSubject = value; OnPropertyChanged(nameof(MostStudiedSubject)); }
        }

        private int _classesCount;
        public int ClassesCount
        {
            get => _classesCount;
            set { _classesCount = value; OnPropertyChanged(nameof(ClassesCount)); }
        }

        public HomeViewModel()
        {
            AvgStudyTime = -5;
            MostStudiedSubject = "ادبیات";
            ClassesCount = 5;
        }
    }
}
