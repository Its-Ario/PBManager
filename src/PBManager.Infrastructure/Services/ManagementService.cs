using Microsoft.EntityFrameworkCore;
using PBManager.Application.Interfaces;
using PBManager.Core.Interfaces;
using PBManager.Infrastructure.Data;

namespace PBManager.Infrastructure.Services
{
    public class ManagementService<T> : IManagementService<T> where T : class, IManagedEntity, new()
    {
        private readonly DatabaseContext _db;
        private readonly DbSet<T> _dbSet;

        public ManagementService(DatabaseContext dbContext)
        {
            _db = dbContext;
            _dbSet = _db.Set<T>();
        }

        public Task<List<T>> GetAllAsync() => _dbSet.AsNoTracking().ToListAsync();

        public async Task<T> AddAsync(string name)
        {
            var entity = new T { Name = name };
            _dbSet.Add(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _db.SaveChangesAsync();
            }
        }
    }
}