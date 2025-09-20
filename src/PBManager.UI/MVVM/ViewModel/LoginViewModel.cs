using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PBManager.Application.Interfaces;

namespace PBManager.UI.MVVM.ViewModel
{
    [ObservableObject]
    public partial class LoginViewModel
    {
        private readonly IAuthenticationService _authService;

        [ObservableProperty] private string _username;
        [ObservableProperty] private string _password;
        [ObservableProperty] private string _errorMessage;

        public event EventHandler? LoginSuccess;

        public LoginViewModel(IAuthenticationService authService)
        {
            _authService = authService;
        }

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
