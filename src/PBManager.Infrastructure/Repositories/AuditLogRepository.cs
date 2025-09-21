using Microsoft.EntityFrameworkCore;
using PBManager.Core.Entities;
using PBManager.Core.Interfaces;
using PBManager.Infrastructure.Data;

namespace PBManager.Infrastructure.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly DatabaseContext _db;
        public AuditLogRepository(DatabaseContext db) { _db = db; }

        public async Task AddAsync(AuditLog log) => await _db.AuditLogs.AddAsync(log);
        public async Task SaveChangesAsync() => await _db.SaveChangesAsync();

        public async Task<List<AuditLog>> GetAsync(string? entityTypeFilter, int? userIdFilter, int pageNumber, int pageSize)
        {
            var query = _db.AuditLogs.AsNoTracking();
            if (!string.IsNullOrEmpty(entityTypeFilter))
            {
                query = query.Where(l => l.EntityType == entityTypeFilter);
            }
            if (userIdFilter.HasValue)
            {
                query = query.Where(l => l.UserId == userIdFilter.Value);
            }
            return await query
                .OrderByDescending(l => l.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
