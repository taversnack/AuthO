using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.DTO;

namespace STSL.SmartLocker.Utils.Data.Services.Contracts;

public interface ILockerBankService
{
    Task<LockerBankDTO?> CreateLockerBankAsync(Guid tenantId, CreateLockerBankDTO dto, CancellationToken cancellationToken = default);
    Task<bool> CreateManyLockerBanksAsync(Guid tenantId, IEnumerable<CreateLockerBankDTO> dtoList, CancellationToken cancellationToken = default);

    Task<LockerBankDTO?> GetLockerBankAsync(Guid tenantId, Guid lockerBankId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LockerBankDTO>> GetManyLockerBanksAsync(Guid tenantId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LockerBankDTO>> GetManyLockerBanksByLocationAsync(Guid tenantId, Guid locationId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default);

    Task UpdateLockerBankAsync(Guid tenantId, Guid lockerBankId, UpdateLockerBankDTO dto, CancellationToken cancellationToken = default);

    Task UpdateAllLockersInLockerBankAsync(Guid tenantId, Guid lockerBankId, UpdateLockerBankLockersDTO dto, CancellationToken cancellationToken = default);

    Task DeleteLockerBankAsync(Guid tenantId, Guid lockerBankId, CancellationToken cancellationToken = default);


    // TODO: Massively refactor other services (card holder / card credential), make things simpler
    // Depending on the type of the locker bank, this will add lease users by joining the cardCredentialIds
    Task ReplaceLeaseUsersForLockerBankAsync(Guid tenantId, Guid lockerBankId, IReadOnlyList<Guid> cardCredentialIds, CancellationToken cancellationToken = default);
    Task ReplaceSpecialCardsForLockerBankAsync(Guid tenantId, Guid lockerBankId, IReadOnlyList<Guid> cardCredentialIds, CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<CardHolderAndCardCredentialsDTO>> GetLeaseUsersWithUserCardsAssignedToLockerBankAsync(Guid tenantId, Guid lockerBankId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CardHolderAndCardCredentialsDTO>> GetCardHoldersWithSpecialCardsAssignedToLockerBankAsync(Guid tenantId, Guid lockerBankId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default);
    Task MoveManyLockersToAnotherBankAsync(Guid tenantId, Guid origin, Guid destination, List<Guid> lockers, CancellationToken cancellationToken = default);

    /*
    //Task AssignUserCardCredentialsToLockerBankAsync(Guid tenantId, Guid lockerBankId, IReadOnlyList<Guid> cardCredentials, CancellationToken cancellationToken = default);
    //Task AssignSpecialCardCredentialsToLockerBankAsync(Guid tenantId, Guid lockerBankId, IReadOnlyList<Guid> cardCredentials, CancellationToken cancellationToken = default);

    //Task RemoveUserCardCredentialsFromLockerBankAsync(Guid tenantId, Guid lockerBankId, IReadOnlyList<Guid> cardCredentials, CancellationToken cancellationToken = default);
    //Task RemoveSpecialCardCredentialsFromLockerBankAsync(Guid tenantId, Guid lockerBankId, IReadOnlyList<Guid> cardCredentials, CancellationToken cancellationToken = default);

    */
}
