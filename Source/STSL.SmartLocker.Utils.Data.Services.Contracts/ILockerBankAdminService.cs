using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.DTO;

namespace STSL.SmartLocker.Utils.Data.Services.Contracts;

public interface ILockerBankAdminService
{
    Task<LockerBankAdminDTO?> CreateLockerBankAdminAsync(Guid tenantId, CreateLockerBankAdminDTO dto, CancellationToken cancellationToken = default);
    Task<bool> CreateManyLockerBankAdminsAsync(Guid tenantId, IEnumerable<CreateLockerBankAdminDTO> dtoList, CancellationToken cancellationToken = default);
    Task ReplaceAdminsForLockerBankAsync(Guid tenantId, Guid lockerBankId, IReadOnlyList<Guid> cardHolderIds, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CardHolderDTO>> GetManyAdminsAsync(Guid tenantId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default);

    Task RemoveAdminFromLockerBankAsync(Guid tenantId, Guid lockerBankId, Guid cardHolderId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LockerBankDTO>> GetManyLockerBanksByAdminAsync(Guid tenantId, Guid adminId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LockerBankDTO>> GetManyLockerBanksByAdminAsync(Guid tenantId, string adminEmail, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LockerBankDTO>> GetManyLockerBanksByLocationByAdminAsync(Guid tenantId, Guid locationId, Guid adminId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LockerBankDTO>> GetManyLockerBanksByLocationByAdminAsync(Guid tenantId, Guid locationId, string adminEmail, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LocationDTO>> GetManyLocationsByAdminAsync(Guid tenantId, Guid adminId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LocationDTO>> GetManyLocationsByAdminAsync(Guid tenantId, string adminEmail, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CardHolderDTO>> GetManyAdminsByLockerBankAsync(Guid tenantId, Guid lockerBankId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default);

    Task<bool> IsCardHolderAdminForLockerBankAsync(Guid tenantId, Guid lockerBankId, Guid cardHolderId, CancellationToken cancellationToken = default);
    Task<bool> IsCardHolderAdminForLockerBankAsync(Guid tenantId, Guid lockerBankId, string cardHolderEmail, CancellationToken cancellationToken = default);

    
    Task<IReadOnlyList<LockerBankAdminSummaryDTO>> GetManyLockerBankSummariesAsync(Guid tenantId, string adminEmail, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default);

}
