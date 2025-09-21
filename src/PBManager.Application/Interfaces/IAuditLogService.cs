using PBManager.Core.Enums;

namespace PBManager.Application.Interfaces
{
    public interface IAuditLogService
    {
        Task LogAsync(ActionType action, string entityType, int? entityId, string description);
    }
}
