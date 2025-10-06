using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using PBManager.Application.Interfaces;
using System.Windows;

namespace PBManager.UI.MVVM.ViewModel
{
    public partial class StartupViewModel(IServiceProvider serviceProvider, IAuthenticationService authService) : ObservableObject
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly IAuthenticationService _authService = authService;

        [ObservableProperty] private string? _username;
        [ObservableProperty] private string? _password;

        [RelayCommand]
        private async Task StartAppAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("Please fill out the form");
                return;
            }
            bool success = await _authService.RegisterAsync(Username, Password);

            if(!success)
            {
                MessageBox.Show("Error");
                return;
            }

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            foreach (var window in System.Windows.Application.Current.Windows)
            {
                if (window is Window w && w.DataContext == this)
                {
                    w.Close();
                    break;
                }
            }
        }
    }
}
