using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.DTO;

namespace STSL.SmartLocker.Utils.Data.Services.Contracts;

public interface ILockerLeaseService
{
    Task<LockerLeaseDTO?> GetLockerLeaseAsync(Guid tenantId, Guid lockerLeaseId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LockerLeaseDTO>> GetLockerLeaseHistoryAsync(Guid tenantId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LockerLeaseDTO>> GetLockerLeaseHistoryByCardHolderAsync(Guid tenantId, Guid cardHolderId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LockerLeaseDTO>> GetLockerLeaseHistoryByLockerAsync(Guid tenantId, Guid lockerId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default);

    Task<LockerLeaseDTO?> StartPermanentLockerLeaseAsync(Guid tenantId, CreatePermanentLockerLeaseDTO dto, CancellationToken cancellationToken = default);
    Task<LockerLeaseDTO?> EndPermanentLockerLeaseAsync(Guid tenantId, Guid lockerLeaseId, DateTimeOffset endedAt, CancellationToken cancellationToken = default);
}
