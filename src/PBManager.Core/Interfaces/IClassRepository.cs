using PBManager.Core.Entities;

namespace PBManager.Core.Interfaces
{
    public interface IClassRepository
    {
        Task<List<Class>> GetAllAsync();
        Task<int> GetCountAsync();
        Task<Class?> GetByNameAsync(string name);
        Task<Class?> GetByIdAsync(int id);
        Task AddAsync(Class c);
        void Update(Class c);
        void Delete(Class c);
        Task<int> SaveChangesAsync();
    }
}
