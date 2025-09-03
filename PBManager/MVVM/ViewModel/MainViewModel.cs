using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PBManager.MVVM.ViewModel
{
    internal class MainViewModel : ObservableObject
    {
        public RelayCommand HomeViewCommand { get; set; }
        public RelayCommand StudyManagementViewCommand { get; set; }

        public HomeViewModel HomeVM { get; set; }
        public StudyManagementViewModel ManagementVM {  get; set; }

        private object _currentView;
        
        public object CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }
        public MainViewModel() {
            HomeVM = new HomeViewModel();
            ManagementVM = new StudyManagementViewModel();

            CurrentView = HomeVM;

            HomeViewCommand = new RelayCommand(() =>
            {
                CurrentView = HomeVM;
            });

            StudyManagementViewCommand = new RelayCommand(() =>
            {
                CurrentView = ManagementVM;
            });
        }
    }
}
