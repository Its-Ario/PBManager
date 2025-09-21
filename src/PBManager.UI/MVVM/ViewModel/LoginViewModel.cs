using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PBManager.Application.Interfaces;

namespace PBManager.UI.MVVM.ViewModel
{
    public partial class LoginViewModel(IAuthenticationService authService) : ObservableObject
    {
        private readonly IAuthenticationService _authService = authService;

        [ObservableProperty] private string? _username;
        [ObservableProperty] private string? _password;
        [ObservableProperty] private string? _errorMessage;

        public event EventHandler? LoginSuccess;

        [RelayCommand]
        private async Task LoginAsync()
        {
            ErrorMessage = string.Empty;
            bool success = await _authService.LoginAsync(Username, Password);
            if (success)
            {
                LoginSuccess?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                ErrorMessage = "ورود نامعتبر";
            }
        }
    }
}
