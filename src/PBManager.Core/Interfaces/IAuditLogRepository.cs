using PBManager.Core.Entities;

namespace PBManager.Core.Interfaces
{
    public interface IAuditLogRepository
    {
        Task AddAsync(AuditLog log);
        Task<List<AuditLog>> GetAsync(string? entityTypeFilter, int? userIdFilter, int pageNumber, int pageSize);
        Task SaveChangesAsync();
    }
}
