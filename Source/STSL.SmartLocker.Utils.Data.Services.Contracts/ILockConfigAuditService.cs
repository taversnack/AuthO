using STSL.SmartLocker.Utils.DTO;

namespace STSL.SmartLocker.Utils.Data.Services.Contracts;

public interface ILockConfigAuditService
{
    Task<LockConfigEventAuditDTO?> LogConfigChangeAsync(Guid tenantId, CreateLockConfigEventAuditDTO dto, CancellationToken cancellationToken = default);
    Task<LockConfigEventAuditDTO?> LogLockConfigChangeAsync(Guid tenantId, string userEmail, Guid entityId, string? description = null, CancellationToken cancellationToken = default);
    Task<LockConfigEventAuditDTO?> LogLockerConfigChangeAsync(Guid tenantId, string userEmail, Guid entityId, string? description = null, CancellationToken cancellationToken = default);
    Task<LockConfigEventAuditDTO?> LogLockerBankConfigChangeAsync(Guid tenantId, string userEmail, Guid entityId, string? description = null, CancellationToken cancellationToken = default);
}
