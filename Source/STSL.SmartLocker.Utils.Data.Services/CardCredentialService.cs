using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using STSL.SmartLocker.Utils.AzureServiceBus.Contracts;
using STSL.SmartLocker.Utils.Common.Data;
using STSL.SmartLocker.Utils.Common.Exceptions;
using STSL.SmartLocker.Utils.Common.Helpers;
using STSL.SmartLocker.Utils.Data.Contexts;
using STSL.SmartLocker.Utils.Data.Extensions;
using STSL.SmartLocker.Utils.Data.Services.Configuration;
using STSL.SmartLocker.Utils.Data.Services.Contracts;
using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.Data.Services.Mappings;
using STSL.SmartLocker.Utils.Domain;
using STSL.SmartLocker.Utils.DTO;
using STSL.SmartLocker.Utils.DTO.AzureServiceBus;
using System.Threading;

namespace STSL.SmartLocker.Utils.Data.Services;

public class CardCredentialService : ICardCredentialService
{
    private readonly IRepository<CardCredential> _repository;
    private readonly IValidator<CardCredential> _validator;
    private readonly SmartLockerDbContext _dbContext;
    private readonly GlobalServiceOptions _options;
    private readonly IAzureServiceBusService _azureServiceBusService;

    public CardCredentialService(IRepository<CardCredential> repository, IValidator<CardCredential> validator, SmartLockerDbContext dbContext, IOptions<GlobalServiceOptions> options, IAzureServiceBusService azureServiceBusService)
        => (_repository, _validator, _dbContext, _options, _azureServiceBusService) = (repository, validator, dbContext, options.Value, azureServiceBusService);

    public async Task<CardCredentialDTO?> CreateCardCredentialAsync(Guid tenantId, CreateCardCredentialDTO dto, CancellationToken cancellationToken = default)
    {
        var entity = CreateCardCredentialMapper.ToEntity(dto);

        _repository.ValidateOne(entity, _validator);

        var newEntity = await _repository.CreateOneAsync(tenantId, entity, cancellationToken);

        return newEntity is null ? null : CardCredentialMapper.ToDTO(newEntity);
    }

    public async Task<bool> CreateManyCardCredentialsAsync(Guid tenantId, IEnumerable<CreateCardCredentialDTO> dtoList, CancellationToken cancellationToken = default)
    {
        var dtoCount = dtoList.TryGetNonEnumeratedCount(out var count) ? count : dtoList.Count();

        return (await CreateAndUseManyCardCredentialsAsync(tenantId, dtoList, cancellationToken)).Count == dtoCount;
    }

    public async Task<IReadOnlyList<CardCredentialDTO>> CreateAndUseManyCardCredentialsAsync(Guid tenantId, IEnumerable<CreateCardCredentialDTO> dtoList, CancellationToken cancellationToken = default)
    {
        if (!dtoList.Any())
        {
            return _options.EmptyBulkOperationIsError ? throw new BadRequestException("No data was passed") : new List<CardCredentialDTO>();
        }

        var entities = dtoList.Select(CreateCardCredentialMapper.ToEntity).ToList();

        _repository.ValidateMany(entities, _validator, _options.ThrowOnFirstValidationErrorForBulkOperations);

        await _repository.CreateManyAsync(tenantId, entities, cancellationToken);

        return entities.ConvertAll(CardCredentialMapper.ToDTO);
    }

    //public async Task<bool> AssignCardCredentialsToLockerAsync(Guid tenantId, Guid lockerId, IEnumerable<Guid> cardCredentialIds, CancellationToken cancellationToken = default)
    //    => await AssignCardCredentialsToLockersAsync(tenantId, cardCredentialIds.Select(CardCredentialId => new CreateLockerCardCredentialDTO(LockerId: lockerId, CardCredentialId)), cancellationToken);

    //public async Task<bool> AssignCardCredentialsToLockersAsync(Guid tenantId, IEnumerable<CreateLockerCardCredentialDTO> dtoList, CancellationToken cancellationToken = default)
    //{
    //    if (!dtoList.Any())
    //    {
    //        return _options.EmptyBulkOperationIsError ? throw new BadRequestException("No data was passed") : true;
    //    }

    //    var entities = dtoList.Select(x => new LockerCardCredential
    //    {
    //        TenantId = tenantId,
    //        LockerId = x.LockerId,
    //        CardCredentialId = x.CardCredentialId,
    //    }).ToList();

    //    _dbContext.LockerCardCredentials.AddRange(entities);

    //    var count = await _dbContext.SaveChangesAsync();

    //    return count == entities.Count;
    //}

    //public async Task<bool> AssignSpecialCardCredentialsToLockerBankAsync(Guid tenantId, Guid lockerBankId, IEnumerable<Guid> cardCredentialIds, CancellationToken cancellationToken = default)
    //    => await AssignCardCredentialsToLockerBankAsync<LockerBankSpecialCardCredential>(tenantId, lockerBankId, cardCredentialIds, cancellationToken);

    //public async Task<bool> AssignUserCardCredentialsToLockerBankAsync(Guid tenantId, Guid lockerBankId, IEnumerable<Guid> cardCredentialIds, CancellationToken cancellationToken = default)
    //    => await AssignCardCredentialsToLockerBankAsync<LockerBankUserCardCredential>(tenantId, lockerBankId, cardCredentialIds, cancellationToken);

    //private async Task<bool> AssignCardCredentialsToLockerBankAsync<T>(Guid tenantId, Guid lockerBankId, IEnumerable<Guid> cardCredentialIds, CancellationToken cancellationToken = default) where T : EntityBase, ILockerBankCardCredential, new()
    //{
    //    if (!cardCredentialIds.Any())
    //    {
    //        return _options.EmptyBulkOperationIsError ? throw new BadRequestException("No data was passed") : true;
    //    }

    //    var entities = cardCredentialIds.Select(cardCredentialId => new T()
    //    {
    //        TenantId = tenantId,
    //        LockerBankId = lockerBankId,
    //        CardCredentialId = cardCredentialId,
    //    }).ToList();

    //    _dbContext.Set<T>().AddRange(entities);

    //    var count = await _dbContext.SaveChangesAsync(cancellationToken);

    //    return count == entities.Count;
    //}

    //public async Task<bool> ReplaceCardCredentialsForLockerAsync(Guid tenantId, Guid lockerId, IEnumerable<Guid> cardCredentialIds, CancellationToken cancellationToken = default)
    //{
    //    var lockerCardCredentials = await _dbContext.LockerCardCredentials
    //        .ByTenant(tenantId)
    //        .Where(x => x.LockerId == lockerId)
    //        .ToListAsync(cancellationToken);

    //    _dbContext.RemoveRange(lockerCardCredentials);

    //    //await _dbContext.SaveChangesAsync(cancellationToken);

    //    return await AssignCardCredentialsToLockerAsync(tenantId, lockerId, cardCredentialIds, cancellationToken);
    //}

    //public async Task<bool> ReplaceCardCredentialsForLockerBankAsync(Guid tenantId, Guid lockerBankId, IEnumerable<Guid> cardCredentialIds, CancellationToken cancellationToken = default)
    //{
    //    var lockerBankCardCredentials = await _dbContext.LockerBankUserCardCredentials
    //        .ByTenant(tenantId)
    //        .Where(x => x.LockerBankId == lockerBankId)
    //        .ToListAsync(cancellationToken);

    //    _dbContext.RemoveRange(lockerBankCardCredentials);

    //    //await _dbContext.SaveChangesAsync(cancellationToken);

    //    return await AssignSpecialCardCredentialsToLockerBankAsync(tenantId, lockerBankId, cardCredentialIds, cancellationToken);
    //}

    public async Task<IReadOnlyList<LockerBankDTO>> GetLockerBanksWhereCardCredentialIsAssignedAsync(Guid tenantId, Guid cardCredentialId, CancellationToken cancellationToken = default)
    {
        var lockerBanksWhereUserCardsAreAssigned = await _dbContext.LockerBankUserCardCredentials
            .ByTenant(tenantId)
            .Include(x => x.LockerBank)
            .Where(x => x.CardCredentialId == cardCredentialId)
            .Select(x => LockerBankMapper.ToDTO(x.LockerBank!))
            .ToListAsync(cancellationToken);
        
        var lockerBanksWhereSpecialCardsAreAssigned = await _dbContext.LockerBankSpecialCardCredentials
            .ByTenant(tenantId)
            .Include(x => x.LockerBank)
            .Where(x => x.CardCredentialId == cardCredentialId)
            .Select(x => LockerBankMapper.ToDTO(x.LockerBank!))
            .ToListAsync(cancellationToken);

        return lockerBanksWhereUserCardsAreAssigned.UnionBy(lockerBanksWhereSpecialCardsAreAssigned, x => x.Id).ToList();
    }

    public async Task<IReadOnlyList<LockerDTO>> GetLockersWhereCardCredentialIsAssignedAsync(Guid tenantId, Guid cardCredentialId, CancellationToken cancellationToken = default)
        => await _dbContext.LockerCardCredentials
            .ByTenant(tenantId)
            .Include(x => x.Locker)
            .Where(x => x.CardCredentialId == cardCredentialId)
            .Select(x => LockerMapper.ToDTO(x.Locker!))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<LockerBankDTO>> GetLockerBanksWhereCardHolderHasCardCredentialsAssignedAsync(Guid tenantId, Guid cardHolderId, CancellationToken cancellationToken = default)
    {
        var lockerBanksWhereUserCardsAreAssigned = await _dbContext.LockerBankLeaseUsers
            .ByTenant(tenantId)
            .Include(x => x.LockerBank)
            .Where(x => x.CardHolderId == cardHolderId)
            .Select(x => LockerBankMapper.ToDTO(x.LockerBank!))
            .ToListAsync(cancellationToken);

        var lockerBanksWhereSpecialCardsAreAssigned = await _dbContext.LockerBankSpecialCardCredentials
            .ByTenant(tenantId)
            .Include(x => x.CardCredential)
            .ThenInclude(x => x!.CardHolder)
            .Include(x => x.LockerBank)
            .Where(x => x.CardCredential!.CardHolderId == cardHolderId)
            .Select(x => LockerBankMapper.ToDTO(x.LockerBank!))
            .ToListAsync(cancellationToken);

        return lockerBanksWhereUserCardsAreAssigned.UnionBy(lockerBanksWhereSpecialCardsAreAssigned, x => x.Id).ToList();
    }

    public async Task<IReadOnlyList<LockerDTO>> GetLockersWhereCardHolderHasCardCredentialsAssignedAsync(Guid tenantId, Guid cardHolderId, CancellationToken cancellationToken = default)
        => await _dbContext.LockerOwners
            .ByTenant(tenantId)
            .Include(x => x.Locker)
            .Where(x => x.CardHolderId == cardHolderId)
            .Select(x => LockerMapper.ToDTO(x.Locker!))
            .ToListAsync(cancellationToken);

    public async Task<CardCredentialDTO?> GetCardCredentialAsync(Guid tenantId, Guid cardCredentialId, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetOneAsync(tenantId, cardCredentialId, cancellationToken);

        return entity is null ? null : CardCredentialMapper.ToDTO(entity);
    }

    public async Task<CardCredentialDTO?> GetCardCredentialFromCCureAsync(Guid tenantId, Guid cardHolderId, string uniqueIdentifier, CancellationToken cancellationToken = default)
    {
        var azureGetCardCredentialsMessage = new AzureServiceBusGetCardCredentialDTO
        { 
            CardHolderId = uniqueIdentifier // ObjectID
        };

        var cardCredential = await _azureServiceBusService.PublishMessageAndGetDataResponseAsync<AzureServiceBusGetCardCredentialDTO, CreateCardCredentialDTO>(azureGetCardCredentialsMessage, cancellationToken);
        
        if (cardCredential is null)
        {
            return null;
        }

        var newCardCredential = cardCredential with { CardHolderId = cardHolderId };

        return await CreateCardCredentialAsync(tenantId, newCardCredential, cancellationToken);
    }

    public async Task<CardCredentialDTO?> GetCardCredentialByHidNumberAsync(Guid tenantId, string hidNumber, CancellationToken cancellationToken = default)
    {
        var entity = await _repository
           .QueryingAll(tenantId)
           .Where(x => x.HidNumber == hidNumber)
           .FirstOrDefaultAsync(cancellationToken);

        return entity is null ? null : CardCredentialMapper.ToDTO(entity); ;
    }

    public async Task<IReadOnlyList<CardCredentialDTO>> GetManyCardCredentialsAsync(Guid tenantId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CardType? cardTypeFilter = null, CancellationToken cancellationToken = default)
        => await FilterAndSortCardCredentialsAsync(_repository.QueryingAll(tenantId), filter, sort, cardTypeFilter, cancellationToken);

    public async Task<IReadOnlyList<CardCredentialDTO>> GetManyCardCredentialsByCardHolderAsync(Guid tenantId, Guid cardHolderId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CardType? cardTypeFilter = null, CancellationToken cancellationToken = default)
        => await FilterAndSortCardCredentialsAsync(_repository.QueryingAll(tenantId).Where(x => x.CardHolderId == cardHolderId), filter, sort, cardTypeFilter, cancellationToken);

    //public async Task<IReadOnlyList<CardCredentialDTO>> GetManyCardCredentialsByLockerAsync(Guid tenantId, Guid lockerId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default)
    //    => await FilterAndSortCardCredentialsAsync(
    //        _dbContext.LockerCardCredentials
    //            .ByTenant(tenantId)
    //            .Where(x => x.LockerId == lockerId)
    //            .Include(x => x.CardCredential)
    //            .Select(x => x.CardCredential)
    //            .OfType<CardCredential>(),
    //        filter, sort, null, cancellationToken);

    //public async Task<IReadOnlyList<CardCredentialDTO>> GetManyCardCredentialsByLockerBankAsync(Guid tenantId, Guid lockerBankId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CardType? cardTypeFilter = null, CancellationToken cancellationToken = default)
    //    => await FilterAndSortCardCredentialsAsync(
    //        _dbContext.LockerBankUserCardCredentials
    //            .ByTenant(tenantId)
    //            .Where(x => x.LockerBankId == lockerBankId)
    //            .Include(x => x.CardCredential)
    //            .Select(x => x.CardCredential)
    //            .OfType<CardCredential>(),
    //        filter, sort, cardTypeFilter, cancellationToken);

    // TODO: Add filtering and sorting if necessary
    //public async Task<IReadOnlyList<CardCredentialAndCardHolderDTO>> GetManyCardCredentialWithCardHoldersByLockerBankAsync(Guid tenantId, Guid lockerBankId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default)
    //    => await _dbContext.LockerBankUserCardCredentials
    //            .ByTenant(tenantId)
    //            .Where(x => x.LockerBankId == lockerBankId)
    //            .Include(x => x.CardCredential)
    //            .ThenInclude(x => x!.CardHolder)
    //            .Select(x => new CardCredentialAndCardHolderDTO
    //            {
    //                CardCredential = CardCredentialMapper.ToDTO(x.CardCredential!),
    //                CardHolder = x.CardCredential!.CardHolder == null ? null : CardHolderMapper.ToDTO(x.CardCredential!.CardHolder!)
    //            })
    //            .ToListAsync(cancellationToken);

    //public async Task<IReadOnlyList<CardCredentialAndCardHolderDTO>> GetManyCardCredentialWithCardHoldersByLockerAsync(Guid tenantId, Guid lockerId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default)
    //    => await _dbContext.LockerCardCredentials
    //            .ByTenant(tenantId)
    //            .Where(x => x.LockerId == lockerId)
    //            .Include(x => x.CardCredential)
    //            .ThenInclude(x => x!.CardHolder)
    //            .Select(x => new CardCredentialAndCardHolderDTO
    //            {
    //                CardCredential = CardCredentialMapper.ToDTO(x.CardCredential!),
    //                CardHolder = x.CardCredential!.CardHolder == null ? null : CardHolderMapper.ToDTO(x.CardCredential!.CardHolder!)
    //            })
    //            .ToListAsync(cancellationToken);

    public async Task<CardCredentialDTO> AssignExistingTemporaryCardAsync(Guid tenantId, Guid cardCredentialId, Guid cardHolderId, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetOneAsync(tenantId, cardCredentialId, cancellationToken)
            ?? throw new NotFoundException("Card Credential does not exist"); // It should never not exist at this point

        entity.CardHolderId = cardHolderId;

        await _repository.UpdateOneAsync(tenantId, cardCredentialId, entity, cancellationToken);

        return CardCredentialMapper.ToDTO(entity);
    }

    public async Task UpdateCardCredentialAsync(Guid tenantId, Guid cardCredentialId, UpdateCardCredentialDTO dto, CancellationToken cancellationToken = default)
    {
        var entity = UpdateCardCredentialMapper.ToEntity(dto);

        _repository.ValidateOne(entity, _validator);

        await _repository.UpdateOneAsync(tenantId, cardCredentialId, entity, cancellationToken);
    }

    // TODO: [10] Configure logic for removing locker owners / lease-users where
    // the to-be-deleted is their only card assigned to the locker / locker bank.
    // Are they removed or do they stay assigned and when they get a new card it is
    // automatically added on card create (will also need to modify card creation logic in such case).
    public async Task DeleteCardCredentialAsync(Guid tenantId, Guid cardCredentialId, CancellationToken cancellationToken = default)
        => await _repository.DeleteOneAsync(tenantId, cardCredentialId, _options.ThrowNotFoundWhenDeletingNonExistantEntity, cancellationToken);

    private static async Task<IReadOnlyList<CardCredentialDTO>> FilterAndSortCardCredentialsAsync(IQueryable<CardCredential> all, IFilteredRequest? filter = null, ISortedRequest? sort = null, CardType? cardTypeFilter = null, CancellationToken cancellationToken = default)
    {
        const string SerialNumber = "serialnumber";
        const string HidNumber = "hidnumber";
        const string CardType = "cardtype";
        const string CardLabel = "cardlabel";

        var filterCardType = cardTypeFilter.HasValue;

        if (filterCardType)
        {
            all = all.Where(x => x.CardType == cardTypeFilter);
        }

        if (filter is not null && !string.IsNullOrWhiteSpace(filter.FilterValue))
        {

            var filterSerialNumber = filter.FilterProperties.IsNullOrEmpty() || filter.FilterProperties.Contains(SerialNumber, StringComparer.OrdinalIgnoreCase);
            var filterHidNumber = filter.FilterProperties.IsNullOrEmpty() || filter.FilterProperties.Contains(HidNumber, StringComparer.OrdinalIgnoreCase);
            var filterCardLabel = filter.FilterProperties.IsNullOrEmpty() || filter.FilterProperties.Contains(CardLabel, StringComparer.OrdinalIgnoreCase);

            all = all.Where(x =>
                (filterSerialNumber && x.SerialNumber != null && x.SerialNumber.Contains(filter.FilterValue)) ||
                (filterHidNumber && x.HidNumber.Contains(filter.FilterValue)) ||
                (filterCardLabel && x.CardLabel != null && x.CardLabel.Contains(filter.FilterValue))
            );
        }

        if (sort is not null && !string.IsNullOrWhiteSpace(sort.SortBy))
        {
            all = sort.SortBy.ToLowerInvariant() switch
            {
                SerialNumber => sort.SortOrder == SortOrder.Ascending ? all.OrderBy(x => x.SerialNumber) : all.OrderByDescending(x => x.SerialNumber),
                HidNumber => sort.SortOrder == SortOrder.Ascending ? all.OrderBy(x => x.HidNumber) : all.OrderByDescending(x => x.HidNumber),
                CardType => sort.SortOrder == SortOrder.Ascending ? all.OrderBy(x => x.CardType) : all.OrderByDescending(x => x.CardType),
                CardLabel => sort.SortOrder == SortOrder.Ascending ? all.OrderBy(x => x.CardLabel) : all.OrderByDescending(x => x.CardLabel),
                _ => throw new BadRequestException($"Cannot sort on property {sort.SortBy}, no such property exists")
            };
        }

        return await all.Select(x => CardCredentialMapper.ToDTO(x)).ToListAsync(cancellationToken);
    }
}
