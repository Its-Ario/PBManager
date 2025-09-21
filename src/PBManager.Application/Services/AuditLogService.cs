using PBManager.Application.Interfaces;
using PBManager.Core.Entities;
using PBManager.Core.Enums;
using PBManager.Core.Interfaces;

namespace PBManager.Application.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _auditRepository;
        private readonly IUserSession _userSession;

        public AuditLogService(IAuditLogRepository auditRepository, IUserSession userSession)
        {
            _auditRepository = auditRepository;
            _userSession = userSession;
        }

        public async Task LogAsync(ActionType action, string entityType, int? entityId, string description)
        {
            var currentUser = _userSession.CurrentUser;
            var log = new AuditLog
            {
                Timestamp = DateTime.UtcNow,
                UserId = currentUser?.Id,
                Username = currentUser?.Username ?? "System",
                ActionType = action,
                EntityType = entityType,
                EntityId = entityId,
                Description = description
            };
            await _auditRepository.AddAsync(log);
            await _auditRepository.SaveChangesAsync();
        }
    }
}
