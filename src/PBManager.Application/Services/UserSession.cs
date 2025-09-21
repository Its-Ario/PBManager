using PBManager.Core.Entities;
using PBManager.Application.Interfaces;

namespace PBManager.Application.Services
{
    public class UserSession : IUserSession
    {
        public User? CurrentUser { get; private set; }

        public void SetUser(User user) => CurrentUser = user;
        public void Clear() => CurrentUser = null;
    }
}
