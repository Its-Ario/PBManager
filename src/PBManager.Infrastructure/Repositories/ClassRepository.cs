using Microsoft.EntityFrameworkCore;
using PBManager.Core.Entities;
using PBManager.Core.Interfaces;
using PBManager.Infrastructure.Data;

namespace PBManager.Infrastructure.Repositories;

public class ClassRepository : IClassRepository
{
    private readonly DatabaseContext _db;

    public ClassRepository(DatabaseContext db)
    {
        _db = db;
    }

    public async Task<List<Class>> GetAllAsync()
    {
        return await _db.Classes.AsNoTracking().ToListAsync();
    }

    public async Task<int> GetCountAsync()
    {
        return await _db.Classes.AsNoTracking().CountAsync();
    }
}