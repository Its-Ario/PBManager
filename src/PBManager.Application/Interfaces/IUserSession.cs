using PBManager.Application.Services;
using PBManager.Core.Entities;

namespace PBManager.Application.Interfaces
{
    public interface IUserSession
    {
        User? CurrentUser { get; }
        bool IsLoggedIn => CurrentUser != null;
        void SetUser(User user);
        void Clear();
    }
}
