using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PBManager.UI.MVVM.View;

namespace PBManager.UI.MVVM.ViewModel
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly HomeViewModel _homeVM;
        private readonly StudyManagementViewModel _managementVM;
        private readonly SettingsViewModel _settingsVM;
        private readonly HistoryViewModel _historyVM;
        private readonly ExamManagementViewModel _examManagementVM;
        
        [ObservableProperty]
        private object _currentView;
        public MainViewModel(
        HomeViewModel homeVM,
        StudyManagementViewModel managementVM,
        SettingsViewModel settingsVM,
        HistoryViewModel historyVM,
        ExamManagementViewModel examVM)
        {
            _homeVM = homeVM;
            _managementVM = managementVM;
            _settingsVM = settingsVM;
            _historyVM = historyVM;
            _examManagementVM = examVM;

            CurrentView = _homeVM;
        }

        [RelayCommand]
        private void HomeView() => CurrentView = _homeVM;

        [RelayCommand]
        private void StudyManagementView() => CurrentView = _managementVM;

        [RelayCommand]
        private void SettingsView() => CurrentView = _settingsVM;

        [RelayCommand]
        private void HistoryView() => CurrentView = _historyVM;

        [RelayCommand]
        private void ExamManagementView() => CurrentView = _examManagementVM;
    }
}
