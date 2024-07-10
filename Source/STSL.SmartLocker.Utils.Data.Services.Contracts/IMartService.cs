using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.DTO;

namespace STSL.SmartLocker.Utils.Data.Services.Contracts;

public interface IMartService
{
    // NOTE: Following 2 are unused, remove
    Task<List<LockerStatusDTO>> ListLockersWithStatusForLockerBankAsync(Guid tenantId, Guid lockerBankId, CancellationToken cancellationToken = default);
    Task<List<AuditRecordsForLockerDTO>> ListAuditRecordsForLockerAsync(Guid tenantId, Guid lockerId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LockerStatusDTO>> GetManyLockersWithStatusForLockerBankAsync(Guid tenantId, Guid lockerBankId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default);
    Task<IPagingResponse<AuditRecordsForLockerDTO>> GetPagedAuditRecordsForLockerAsync(Guid tenantId, Guid lockerId, IPagingRequest pagingRequest, CancellationToken cancellationToken = default);
}
