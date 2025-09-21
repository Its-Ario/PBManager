using PBManager.Application.Interfaces;
using PBManager.Core.Entities;
using PBManager.Core.Enums;
using System.Security.Cryptography;

namespace PBManager.Application.Services
{
    public partial class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuditLogService _auditLogService;
        private readonly IUserSession _userSession;

        public AuthenticationService(IUserRepository userRepository, IAuditLogService auditLogService, IUserSession userSession)
        {
            _userRepository = userRepository;
            _auditLogService = auditLogService;
            _userSession = userSession;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user != null && VerifyPassword(password, user.PasswordHash))
            {
                _userSession.SetUser(user);
                await _auditLogService.LogAsync(ActionType.Login, nameof(User), user.Id, $"کاربر '{username}' با موفقیت وارد شد.");
                return true;
            }

            await _auditLogService.LogAsync(ActionType.Login, nameof(User), null, $"تلاش ناموفق برای ورود با نام کاربری: '{username}'.");
            return false;
        }

        public async Task<bool> RegisterAsync(string username, string password, bool isAdmin = false)
        {
            if (await _userRepository.GetByUsernameAsync(username) != null)
            {
                return false;
            }

            var newUser = new User
            {
                Username = username,
                PasswordHash = HashPassword(password),
                IsAdmin = isAdmin
            };

            await _userRepository.AddAsync(newUser);
            await _userRepository.SaveChangesAsync();

            await _auditLogService.LogAsync(ActionType.Create, nameof(User), newUser.Id, $"کاربر جدید '{username}' ثبت نام کرد.");
            return true;
        }

        public async Task LogoutAsync()
        {
            var userToLogOut = _userSession.CurrentUser;
            if (userToLogOut != null)
            {
                await _auditLogService.LogAsync(ActionType.Logout, nameof(User), userToLogOut.Id, $"کاربر '{userToLogOut.Username}' خارج شد.");
            }
            _userSession.Clear();
        }

        private string HashPassword(string password)
        {
            const int saltSize = 16;
            const int hashSize = 20;
            const int iterations = 10000;

            byte[] salt = RandomNumberGenerator.GetBytes(saltSize);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(hashSize);

            byte[] hashBytes = new byte[saltSize + hashSize];
            Array.Copy(salt, 0, hashBytes, 0, saltSize);
            Array.Copy(hash, 0, hashBytes, saltSize, hashSize);

            return Convert.ToBase64String(hashBytes);
        }

        private bool VerifyPassword(string password, string base64Hash)
        {
            byte[] hashBytes = Convert.FromBase64String(base64Hash);
            const int saltSize = 16;
            const int hashSize = 20;
            const int iterations = 10000;

            if (hashBytes.Length < saltSize + hashSize)
            {
                return false;
            }

            byte[] salt = new byte[saltSize];
            Array.Copy(hashBytes, 0, salt, 0, saltSize);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(hashSize);

            for (int i = 0; i < hashSize; i++)
            {
                if (hashBytes[i + saltSize] != hash[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}