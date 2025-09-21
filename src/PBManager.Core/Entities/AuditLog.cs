using PBManager.Core.Enums;

namespace PBManager.Core.Entities;

public class AuditLog
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public int? UserId { get; set; }
    public string Username { get; set; }
    public ActionType ActionType { get; set; }
    public string EntityType { get; set; }
    public int? EntityId { get; set; }
    public string Description { get; set; }
}