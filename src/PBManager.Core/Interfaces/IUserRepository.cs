using PBManager.Core.Entities;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<List<User>> GetAllAsync();
    Task AddAsync(User user);
    Task SaveChangesAsync();
}