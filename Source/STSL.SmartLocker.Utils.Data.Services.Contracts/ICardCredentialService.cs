using STSL.SmartLocker.Utils.Common.Data;
using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.DTO;

namespace STSL.SmartLocker.Utils.Data.Services.Contracts;

public interface ICardCredentialService
{
    Task<CardCredentialDTO?> CreateCardCredentialAsync(Guid tenantId, CreateCardCredentialDTO dto, CancellationToken cancellationToken = default);
    Task<bool> CreateManyCardCredentialsAsync(Guid tenantId, IEnumerable<CreateCardCredentialDTO> dtoList, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CardCredentialDTO>> CreateAndUseManyCardCredentialsAsync(Guid tenantId, IEnumerable<CreateCardCredentialDTO> dtoList, CancellationToken cancellationToken = default);

    // TODO: [10] REMOVE ASSIGN FUNCTIONS. MOVE GET & REPLACE BY LOCKER / BANK TO LOCKER & BANK SERVICES & USE USER / SPECIAL CARD DISTINCTIONS FOR BANKS
    //Task<bool> AssignCardCredentialsToLockerAsync(Guid tenantId, Guid lockerId, IEnumerable<Guid> cardCredentialIds, CancellationToken cancellationToken = default);
    //Task<bool> AssignCardCredentialsToLockersAsync(Guid tenantId, IEnumerable<CreateLockerCardCredentialDTO> dtoList, CancellationToken cancellationToken = default);

    //Task<bool> AssignSpecialCardCredentialsToLockerBankAsync(Guid tenantId, Guid lockerBankId, IEnumerable<Guid> cardCredentialIds, CancellationToken cancellationToken = default);
    //Task<bool> AssignUserCardCredentialsToLockerBankAsync(Guid tenantId, Guid lockerBankId, IEnumerable<Guid> cardCredentialIds, CancellationToken cancellationToken = default);

    //Task<bool> ReplaceCardCredentialsForLockerAsync(Guid tenantId, Guid lockerId, IEnumerable<Guid> cardCredentialIds, CancellationToken cancellationToken = default);
    //Task<bool> ReplaceCardCredentialsForLockerBankAsync(Guid tenantId, Guid lockerBankId, IEnumerable<Guid> cardCredentialIds, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LockerBankDTO>> GetLockerBanksWhereCardCredentialIsAssignedAsync(Guid tenantId, Guid cardCredentialId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LockerDTO>> GetLockersWhereCardCredentialIsAssignedAsync(Guid tenantId, Guid cardCredentialId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LockerBankDTO>> GetLockerBanksWhereCardHolderHasCardCredentialsAssignedAsync(Guid tenantId, Guid cardHolderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LockerDTO>> GetLockersWhereCardHolderHasCardCredentialsAssignedAsync(Guid tenantId, Guid cardHolderId, CancellationToken cancellationToken = default);

    Task<CardCredentialDTO?> GetCardCredentialFromCCureAsync(Guid tenantId, Guid cardHolderId, string uniqueIdentifier, CancellationToken cancellationToken = default);
    Task<CardCredentialDTO?> GetCardCredentialAsync(Guid tenantId, Guid cardCredentialId, CancellationToken cancellationToken = default);
    Task<CardCredentialDTO?> GetCardCredentialByHidNumberAsync(Guid tenantId, string hidNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CardCredentialDTO>> GetManyCardCredentialsAsync(Guid tenantId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CardType? cardTypeFilter = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CardCredentialDTO>> GetManyCardCredentialsByCardHolderAsync(Guid tenantId, Guid cardHolderId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CardType? cardTypeFilter = null, CancellationToken cancellationToken = default);
    //Task<IReadOnlyList<CardCredentialDTO>> GetManyCardCredentialsByLockerAsync(Guid tenantId, Guid lockerId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default);
    //Task<IReadOnlyList<CardCredentialDTO>> GetManyCardCredentialsByLockerBankAsync(Guid tenantId, Guid lockerBankId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CardType? cardTypeFilter = null, CancellationToken cancellationToken = default);

    //Task<IReadOnlyList<CardCredentialAndCardHolderDTO>> GetManyCardCredentialWithCardHoldersByLockerBankAsync(Guid tenantId, Guid lockerBankId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default);
    //Task<IReadOnlyList<CardCredentialAndCardHolderDTO>> GetManyCardCredentialWithCardHoldersByLockerAsync(Guid tenantId, Guid lockerId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default);

    Task<CardCredentialDTO> AssignExistingTemporaryCardAsync(Guid tenantId, Guid cardCredentialId, Guid cardHolderId, CancellationToken cancellation = default);
    Task UpdateCardCredentialAsync(Guid tenantId, Guid cardCredentialId, UpdateCardCredentialDTO dto, CancellationToken cancellationToken = default);

    Task DeleteCardCredentialAsync(Guid tenantId, Guid cardCredentialId, CancellationToken cancellationToken = default);
}
