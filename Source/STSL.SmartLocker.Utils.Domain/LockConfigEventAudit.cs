using STSL.SmartLocker.Utils.Common.Data;

namespace STSL.SmartLocker.Utils.Domain;

public sealed class LockConfigEventAudit : EntityBaseWithTenant, IUsesGuidId
{
    public Guid Id { get; set; }
    public LockConfigEventType EventType { get; set; }
    public required string UpdatedByUserEmail { get; set; }
    public DateTimeOffset CreatedAtUTC { get; set; } = DateTimeOffset.UtcNow;
    public string? AdditionalDescription { get; set; }

    public Guid EntityId { get; set; }
}
