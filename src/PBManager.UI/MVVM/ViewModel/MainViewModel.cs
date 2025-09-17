using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PBManager.MVVM.ViewModel
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly HomeViewModel _homeVM;
        private readonly StudyManagementViewModel _managementVM;
        private readonly SettingsViewModel _settingsVM;
        
        [ObservableProperty]
        private object _currentView;
        public MainViewModel(
        HomeViewModel homeVM,
        StudyManagementViewModel managementVM,
        SettingsViewModel settingsVM)
        {
            _homeVM = homeVM;
            _managementVM = managementVM;
            _settingsVM = settingsVM;

            CurrentView = _homeVM;
        }

        [RelayCommand]
        private void HomeView() => CurrentView = _homeVM;

        [RelayCommand]
        private void StudyManagementView() => CurrentView = _managementVM;

        [RelayCommand]
        private void SettingsView() => CurrentView = _settingsVM;
    }
}
