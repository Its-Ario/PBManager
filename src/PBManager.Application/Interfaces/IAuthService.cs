using PBManager.Core.Entities;

namespace PBManager.Application.Interfaces
{
    public interface IAuthenticationService
    {
        User? CurrentUser { get; }
        bool IsLoggedIn { get; }

        Task<bool> LoginAsync(string username, string password);
        Task<bool> RegisterAsync(string username, string password, bool isAdmin = false);
        void Logout();
    }
}