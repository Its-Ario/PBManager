using Microsoft.EntityFrameworkCore;
using PBManager.Data;
using PBManager.MVVM.Model;
namespace PBManager.Services
{
    public class SubjectService
    {
        private readonly DatabaseContext _db;
        public SubjectService(DatabaseContext db) {
            _db = db;
        }

        public async Task<List<Subject>> GetSubjectsAsync()
        {
            return await _db.Subjects.ToListAsync();
        }

        public async Task<int> GetSubjectCountAsync()
        {
            return await _db.Subjects.AsNoTracking().CountAsync();
        }
    }
}
