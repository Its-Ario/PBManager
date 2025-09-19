using PBManager.Core.Interfaces;

namespace PBManager.Application.Interfaces
{
    public interface IManagementService<T> where T : class, IManagedEntity
    {
        Task<List<T>> GetAllAsync();
        Task<T> AddAsync(string name);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
    }
}
