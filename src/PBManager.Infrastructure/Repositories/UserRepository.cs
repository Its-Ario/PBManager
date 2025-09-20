using Microsoft.EntityFrameworkCore;
using PBManager.Core.Entities;
using PBManager.Infrastructure.Data;

namespace PBManager.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DatabaseContext _db;

        public UserRepository(DatabaseContext dbContext)
        {
            _db = dbContext;
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username.Equals(username));
        }

        public async Task AddAsync(User user)
        {
            await _db.Users.AddAsync(user);
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}