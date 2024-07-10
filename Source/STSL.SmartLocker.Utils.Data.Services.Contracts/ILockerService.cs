using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.DTO;

namespace STSL.SmartLocker.Utils.Data.Services.Contracts;

public interface ILockerService
{
    Task<LockerDTO?> CreateLockerAsync(Guid tenantId, CreateLockerDTO dto, CancellationToken cancellationToken = default);
    Task<bool> CreateManyLockersAsync(Guid tenantId, IEnumerable<CreateLockerDTO> dtoList, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LockerDTO>> CreateAndUseManyLockersAsync(Guid tenantId, IEnumerable<CreateLockerDTO> dtoList, CancellationToken cancellationToken = default);

    Task<LockerDTO?> GetLockerAsync(Guid tenantId, Guid lockerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LockerDTO>> GetManyLockersAsync(Guid tenantId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LockerDTO>> GetManyLockersByLockerBankAsync(Guid tenantId, Guid lockerBankId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LockerAndLockDTO>> GetManyLockersAndLocksByLockerBankAsync(Guid tenantId, Guid lockerBankId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CardHolderDTO>> GetManyCardHoldersByLockerAsync(Guid tenantId, Guid lockerId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default);

    Task<GlobalLockerSearchResultDTO> GetManyLockersWithLockAndLocationAndLockerBankDetailsAsync(Guid tenantId, IPagingRequest? page = null, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default);

    Task UpdateLockerAsync(Guid tenantId, Guid lockerId, UpdateLockerDTO dto, CancellationToken cancellationToken = default);
    Task<bool> UpdateManyLockersAsync(Guid tenantId, UpdateManyLockersDTO dto, CancellationToken cancellationToken = default);

    Task DeleteLockerAsync(Guid tenantId, Guid lockerId, CancellationToken cancellationToken = default);

    // Could these go into a lease facade / service
    // TODO: Implement these and throw away old code that assigns cards only, not suited to dumb locks
    Task<IReadOnlyList<CardHolderAndCardCredentialsDTO>> GetOwnersWithCardsAssignedToLockerAsync(Guid tenantId, Guid lockerId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default);
    Task ReplaceOwnersForLockerByCardCredentialsAsync(Guid tenantId, Guid lockerId, IReadOnlyList<Guid> cardCredentialIds, CancellationToken cancellationToken = default);
    Task ReplaceOwnersForLockerByCardHoldersAsync(Guid tenantId, Guid lockerId, IReadOnlyList<Guid> cardHolderIds, CancellationToken cancellationToken = default);

}

