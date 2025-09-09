using Microsoft.EntityFrameworkCore;
using PBManager.Data;
using PBManager.MVVM.Model;

namespace PBManager.Services
{
    public class ClassService
    {
        private readonly DatabaseContext _db;
        public ClassService(DatabaseContext db)
        {
            _db = db;
        }

        public async Task<List<Class>> GetClassesAsync()
        {
            return await _db.Classes.ToListAsync();
        }

        public async Task<int> GetClassCountAsync()
        {
            return await _db.Classes.AsNoTracking().CountAsync();
        }
    }
}
