using CommunityToolkit.Mvvm.ComponentModel;
using PBManager.Application.Interfaces;
using PBManager.Core.Entities;
using System.Security.Cryptography;

namespace PBManager.Application.Services
{

    public partial class AuthenticationService : ObservableObject, IAuthenticationService
    {
        private readonly IUserRepository _userRepository;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsLoggedIn))]
        private User? _currentUser;

        public bool IsLoggedIn => CurrentUser != null;

        public AuthenticationService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user != null && VerifyPassword(password, user.PasswordHash))
            {
                CurrentUser = user;
                return true;
            }
            return false;
        }

        public async Task<bool> RegisterAsync(string username, string password, bool isAdmin = false)
        {
            var existingUser = await _userRepository.GetByUsernameAsync(username);
            if (existingUser != null)
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
            return true;
        }

        public void Logout()
        {
            CurrentUser = null;
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