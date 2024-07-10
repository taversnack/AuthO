using STSL.SmartLocker.Utils.Common.Data;

namespace STSL.SmartLocker.Utils.DTO;

public sealed record CreateLockConfigEventAuditDTO(
    LockConfigEventType EventType,
    string UpdatedByUserEmail,
    Guid EntityId,
    DateTimeOffset? CreatedAtUTC,
    string? AdditionalDescription = null);

public sealed record LockConfigEventAuditDTO(
    Guid Id,
    LockConfigEventType EventType,
    string UpdatedByUserEmail,
    Guid EntityId,
    DateTimeOffset CreatedAtUTC,
    string? AdditionalDescription = null);
