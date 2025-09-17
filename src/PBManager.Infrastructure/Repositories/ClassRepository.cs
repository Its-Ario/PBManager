using Microsoft.EntityFrameworkCore;
using PBManager.Core.Entities;
using PBManager.Core.Interfaces;
using PBManager.Infrastructure.Data;
using System.Diagnostics;
using System.Xml.Linq;

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

    public async Task<Class?> GetByNameAsync(string name)
    {
        try
        {
            return await _db.Classes
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Name.Equals(name));
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }

        return null;
    }
    public async Task<Class?> GetByIdAsync(int id)
    {
        return await _db.Classes
        .AsNoTracking()
        .FirstOrDefaultAsync(c => c.Id == id);
    }
}