using Microsoft.EntityFrameworkCore;
using PBManager.Core.Entities;
using PBManager.Core.Interfaces;
using PBManager.Infrastructure.Data;

namespace PBManager.Infrastructure.Repositories;

public class SubjectRepository : ISubjectRepository
{
    private readonly DatabaseContext _db;

    public SubjectRepository(DatabaseContext db)
    {
        _db = db;
    }

    public Task<List<Subject>> GetAllAsync()
    {
        return _db.Subjects.AsNoTracking().ToListAsync();
    }

    public Task<int> GetCountAsync()
    {
        return _db.Subjects.AsNoTracking().CountAsync();
    }
}