using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PBManager.MVVM.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        public RelayCommand HomeViewCommand { get; set; }
        public RelayCommand StudyManagementViewCommand { get; set; }
        public RelayCommand SettingsViewCommand { get; set; }

        public HomeViewModel HomeVM { get; set; }
        public StudyManagementViewModel ManagementVM {  get; set; }
        public SettingsViewModel SettingsVM { get; set; }

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
        public MainViewModel(
        HomeViewModel homeVM,
        StudyManagementViewModel managementVM,
        SettingsViewModel settingsVM)
        {
            HomeVM = homeVM;
            ManagementVM = managementVM;
            SettingsVM = settingsVM;

            CurrentView = HomeVM;

            HomeViewCommand = new RelayCommand(() => CurrentView = HomeVM);
            StudyManagementViewCommand = new RelayCommand(() => CurrentView = ManagementVM);
            SettingsViewCommand = new RelayCommand(() => CurrentView = SettingsVM);
        }
    }
}
