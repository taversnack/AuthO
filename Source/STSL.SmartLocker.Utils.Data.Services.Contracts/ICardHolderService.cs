using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.DTO;

namespace STSL.SmartLocker.Utils.Data.Services.Contracts;

public interface ICardHolderService
{
    Task<CardHolderDTO?> CreateCardHolderAsync(Guid tenantId, CreateCardHolderDTO dto, CancellationToken cancellationToken = default);
    Task<bool> CreateManyCardHoldersAsync(Guid tenantId, IEnumerable<CreateCardHolderDTO> dtoList, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CardHolderDTO>> CreateAndUseManyCardHoldersAsync(Guid tenantId, IEnumerable<CreateCardHolderDTO> dtoList, CancellationToken cancellationToken = default);

    Task AssignCardHoldersAsOwnersAndAddCardCredentialsToLockerAsync(Guid tenantId, Guid lockerId, UpdateManyCardCredentialAndCardHoldersDTO dto, CancellationToken cancellationToken = default);
    Task AssignCardHoldersAsLeaseUsersAndAddCardCredentialsToLockerBankAsync(Guid tenantId, Guid lockerBankId, UpdateManyCardCredentialAndCardHoldersDTO dto, CancellationToken cancellationToken = default);

    Task ReplaceCardHoldersAsOwnersAndReplaceCardCredentialsForLockerAsync(Guid tenantId, Guid lockerId, UpdateManyCardCredentialAndCardHoldersDTO dto, CancellationToken cancellationToken = default);
    Task ReplaceCardHoldersAsLeaseUsersAndReplaceCardCredentialsForLockerBankAsync(Guid tenantId, Guid lockerBankId, UpdateManyCardCredentialAndCardHoldersDTO dto, CancellationToken cancellationToken = default);

    Task<bool> RemoveCardHolderAndTheirCardCredentialsFromLocker(Guid tenantId, Guid lockerId, Guid cardHolderId, CancellationToken cancellationToken = default);

    //Task<IReadOnlyList<LockerBankDTO>> GetLockerBanksWhereCardHolderIsAssignedAsync(Guid tenantId, Guid cardHolderId, CancellationToken cancellationToken = default);
    //Task<IReadOnlyList<LockerDTO>> GetLockersWhereCardHolderIsAssignedAsync(Guid tenantId, Guid cardHolderId, CancellationToken cancellationToken = default);

    //Task<IReadOnlyList<CardHolderDTO>> GetManyCardHoldersByLockerAsync(Guid tenantId, Guid lockerId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default);
    //Task<IReadOnlyList<CardHolderDTO>> GetManyCardHoldersByLockerBankAsync(Guid tenantId, Guid lockerBankId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default);

    Task<CardHolderDTO?> GetCardHolderAsync(Guid tenantId, Guid cardHolderId, CancellationToken cancellationToken = default);
    Task<CardHolderDTO?> GetCardHolderByEmailAsync(Guid tenantId, string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LockerDTO>?> GetManyLockersForCardHolderAsync(Guid tenantId, Guid cardHolderId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CardHolderDTO>> GetManyCardHoldersAsync(Guid tenantId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CardHolderAndCardCredentialsDTO>> GetManyCardHoldersAndTheirUserCardCredentialsAsync(Guid tenantId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CardHolderAndCardCredentialsDTO>> GetManyCardHoldersAndTheirSpecialCardCredentialsAsync(Guid tenantId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<CardHolderAndCardCredentialsDTO>> GetManyLockerOwnersAndTheirUserCardCredentialsByLockerAsync(Guid tenantId, Guid lockerId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CardHolderAndCardCredentialsDTO>> GetManyCardHoldersAndTheirSpecialCardCredentialsByLockerBankAsync(Guid tenantId, Guid lockerBankId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CardHolderAndCardCredentialsDTO>> GetManyLeaseUsersAndTheirUserCardCredentialsByLockerBankAsync(Guid tenantId, Guid lockerBankId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default);

    Task UpdateCardHolderAsync(Guid tenantId, Guid cardHolderId, UpdateCardHolderDTO dto, CancellationToken cancellationToken = default);

    Task DeleteCardHolderAsync(Guid tenantId, Guid cardHolderId, CancellationToken cancellationToken = default);
}
