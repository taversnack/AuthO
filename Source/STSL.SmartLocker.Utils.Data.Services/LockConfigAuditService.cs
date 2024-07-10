using STSL.SmartLocker.Utils.Common.Data;
using STSL.SmartLocker.Utils.Data.Services.Contracts;
using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.Data.Services.Mappings;
using STSL.SmartLocker.Utils.Domain;
using STSL.SmartLocker.Utils.DTO;

namespace STSL.SmartLocker.Utils.Data.Services;

public sealed class LockConfigAuditService : ILockConfigAuditService
{
    private readonly IRepository<LockConfigEventAudit> _repository;

    public LockConfigAuditService(IRepository<LockConfigEventAudit> repository) => _repository = repository;

    public async Task<LockConfigEventAuditDTO?> LogConfigChangeAsync(Guid tenantId, CreateLockConfigEventAuditDTO dto, CancellationToken cancellationToken = default)
    {
        var entity = CreateLockConfigEventAuditMapper.ToEntity(dto);

        var newEntity = await _repository.CreateOneAsync(tenantId, entity, cancellationToken);

        return newEntity is null ? null : LockConfigEventAuditMapper.ToDTO(newEntity);
    }

    public async Task<LockConfigEventAuditDTO?> LogLockConfigChangeAsync(Guid tenantId, string userEmail, Guid entityId, string? description = null, CancellationToken cancellationToken = default)
        => await LogConfigChangeAsync(tenantId, new(LockConfigEventType.LockUpdated, userEmail, entityId, DateTimeOffset.UtcNow, description));

    public async Task<LockConfigEventAuditDTO?> LogLockerConfigChangeAsync(Guid tenantId, string userEmail, Guid entityId, string? description = null, CancellationToken cancellationToken = default)
        => await LogConfigChangeAsync(tenantId, new(LockConfigEventType.LockerUpdated, userEmail, entityId, DateTimeOffset.UtcNow, description));

    public async Task<LockConfigEventAuditDTO?> LogLockerBankConfigChangeAsync(Guid tenantId, string userEmail, Guid entityId, string? description = null, CancellationToken cancellationToken = default)
        => await LogConfigChangeAsync(tenantId, new(LockConfigEventType.LockerBankUpdated, userEmail, entityId, DateTimeOffset.UtcNow, description));
}
