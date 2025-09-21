namespace PBManager.Application.Interfaces
{
    public interface IAuthenticationService
    {
        Task<bool> LoginAsync(string username, string password);
        Task<bool> RegisterAsync(string username, string password, bool isAdmin = false);
        Task LogoutAsync();
    }
}