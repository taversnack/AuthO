using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using STSL.SmartLocker.Utils.Common.Data;
using STSL.SmartLocker.Utils.Common.Exceptions;
using STSL.SmartLocker.Utils.Common.Helpers;
using STSL.SmartLocker.Utils.Data.Contexts;
using STSL.SmartLocker.Utils.Data.Extensions;
using STSL.SmartLocker.Utils.Data.Services.Configuration;
using STSL.SmartLocker.Utils.Data.Services.Contracts;
using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.Data.Services.Helpers;
using STSL.SmartLocker.Utils.Data.Services.Mappings;
using STSL.SmartLocker.Utils.Domain;
using STSL.SmartLocker.Utils.DTO;

namespace STSL.SmartLocker.Utils.Data.Services;

public sealed class CardHolderService : ICardHolderService
{
    private readonly IRepository<CardHolder> _repository;
    private readonly IValidator<CardHolder> _validator;
    private readonly SmartLockerDbContext _dbContext;
    private readonly GlobalServiceOptions _options;

    public CardHolderService(IRepository<CardHolder> repository, IValidator<CardHolder> validator, SmartLockerDbContext dbContext, IOptions<GlobalServiceOptions> options)
        => (_repository, _validator, _dbContext, _options) = (repository, validator, dbContext, options.Value);

    public async Task<CardHolderDTO?> CreateCardHolderAsync(Guid tenantId, CreateCardHolderDTO dto, CancellationToken cancellationToken = default)
    {
        var entity = CreateCardHolderMapper.ToEntity(dto);

        _repository.ValidateOne(entity, _validator);

        var newEntity = await _repository.CreateOneAsync(tenantId, entity, cancellationToken);

        return newEntity is null ? null : CardHolderMapper.ToDTO(newEntity);
    }

    public async Task<bool> CreateManyCardHoldersAsync(Guid tenantId, IEnumerable<CreateCardHolderDTO> dtoList, CancellationToken cancellationToken = default)
    {
        var dtoCount = dtoList.TryGetNonEnumeratedCount(out var count) ? count : dtoList.Count();

        return (await CreateAndUseManyCardHoldersAsync(tenantId, dtoList, cancellationToken)).Count == dtoCount;
    }

    public async Task<IReadOnlyList<CardHolderDTO>> CreateAndUseManyCardHoldersAsync(Guid tenantId, IEnumerable<CreateCardHolderDTO> dtoList, CancellationToken cancellationToken = default)
    {
        if (!dtoList.Any())
        {
            return _options.EmptyBulkOperationIsError ? throw new BadRequestException("No data was passed") : new List<CardHolderDTO>();
        }

        var entities = dtoList.Select(CreateCardHolderMapper.ToEntity).ToList();

        _repository.ValidateMany(entities, _validator, _options.ThrowOnFirstValidationErrorForBulkOperations);

        await _repository.CreateManyAsync(tenantId, entities, cancellationToken);

        return entities.ConvertAll(CardHolderMapper.ToDTO);
    }

    public async Task AssignCardHoldersAsOwnersAndAddCardCredentialsToLockerAsync(Guid tenantId, Guid lockerId, UpdateManyCardCredentialAndCardHoldersDTO dto, CancellationToken cancellationToken = default)
    {
        if (dto.CardCredentialIds.IsNullOrEmpty() && dto.CardHolderIds.IsNullOrEmpty())
        {
            if (_options.EmptyBulkOperationIsError)
            {
                throw new BadRequestException("No data was passed");
            }
            return;
        }

        List<Guid> allCardCredentialIds = new();
        List<Guid> allCardHolderIds = new();

        var totalPotentialCardCredentialCount = dto.CardHolderIds?.Count ?? 0 + dto.CardCredentialIds?.Count ?? 0;

        if (totalPotentialCardCredentialCount > DomainConstants.MaxUserCardCredentialsPerLocker)
        {
            throw new BadRequestException("Too many card credentials assigned to a single locker");
        }

        Locker locker = await GetEntityFromDatabaseAsync<Locker>(tenantId, lockerId, cancellationToken);

        if (dto.CardHolderIds is not null)
        {
            // Using Contains() here should be fine since we are using a small number of IDs
            var cardHolderAndCardCredentialPairs = await _repository.QueryingAll(tenantId)
                .Include(x => x.CardCredentials!.Where(y => y.CardType == CardType.User).Take(1))
                .Where(x => dto.CardHolderIds.Contains(x.Id))
                .Select(x => new { cardHolderId = x.Id, cardCredential = x.CardCredentials!.FirstOrDefault() })
                .ToListAsync(cancellationToken: cancellationToken);

            // Add the locker owners to the full list
            allCardHolderIds.AddRange(cardHolderAndCardCredentialPairs.Select(x => x.cardHolderId));

            // Add the cardCredentials to the full list
            allCardCredentialIds.AddRange(cardHolderAndCardCredentialPairs.Where(x => x.cardCredential is not null).Select(x => x.cardCredential!.Id));

            // Potentially lots of individual queries
            /*
            foreach(var cardHolderId in dto.CardHolderIds) 
            {
                var cardHolder = await _repository.QueryingAll(tenantId)
                    .Include(x => x.CardCredentials.Where(y => y.CardType == CardType.User).Take(1))
                    .Where(x => x.Id == cardHolderId)
                    .FirstOrDefaultAsync(cancellationToken);

                if(cardHolder?.CardCredentials.Any() ?? false)
                {
                    allCardCredentialIds.Add(cardHolder.CardCredentials.First().Id);
                }
            }
            */
        }

        if (dto.CardCredentialIds is not null)
        {
            allCardCredentialIds.AddRange(dto.CardCredentialIds);

            var cardHolderIdsFromCardCredentials = await _dbContext.CardCredentials.ByTenant(tenantId)
                .Where(x => x.CardType == CardType.User && x.CardHolderId != null && dto.CardCredentialIds.Contains(x.Id))
                .Select(x => x.CardHolderId)
                .ToListAsync(cancellationToken);

            allCardHolderIds.AddRange(cardHolderIdsFromCardCredentials.Where(x => x is not null).Select(x => x!.Value));
        }

        _dbContext.LockerOwners.AddRange(allCardHolderIds.Distinct().Select(x => new LockerOwner { TenantId = locker.TenantId, CardHolderId = x, LockerId = locker.Id }));

        _dbContext.LockerCardCredentials.AddRange(allCardCredentialIds.Distinct().Select(x => new LockerCardCredential { TenantId = locker.TenantId, CardCredentialId = x, LockerId = locker.Id }));

        await _dbContext.SaveChangesAsync(cancellationToken: cancellationToken);
    }

    public async Task AssignCardHoldersAsLeaseUsersAndAddCardCredentialsToLockerBankAsync(Guid tenantId, Guid lockerBankId, UpdateManyCardCredentialAndCardHoldersDTO dto, CancellationToken cancellationToken = default)
    {
        if (dto.CardCredentialIds.IsNullOrEmpty() && dto.CardHolderIds.IsNullOrEmpty())
        {
            if (_options.EmptyBulkOperationIsError)
            {
                throw new BadRequestException("No data was passed");
            }
            return;
        }

        List<Guid> allCardCredentialIds = new();
        List<Guid> allCardHolderIds = new();

        var totalPotentialCardCredentialCount = dto.CardHolderIds?.Count ?? 0 + dto.CardCredentialIds?.Count ?? 0;

        if (totalPotentialCardCredentialCount > DomainConstants.MaxUserCardCredentialsPerLocker)
        {
            throw new BadRequestException("Too many card credentials assigned to a single locker bank");
        }

        LockerBank lockerBank = await GetEntityFromDatabaseAsync<LockerBank>(tenantId, lockerBankId, cancellationToken);

        if (dto.CardHolderIds is not null)
        {
            // Using Contains() here should be fine since we are using a small number of IDs
            var cardHolderAndCardCredentialPairs = await _repository.QueryingAll(tenantId)
                .Include(x => x.CardCredentials!.Where(y => y.CardType == CardType.User).Take(1))
                .Where(x => dto.CardHolderIds.Contains(x.Id))
                .Select(x => new { cardHolderId = x.Id, cardCredential = x.CardCredentials!.FirstOrDefault() })
                .ToListAsync(cancellationToken: cancellationToken);

            // Add the locker owners to the full list
            allCardHolderIds.AddRange(cardHolderAndCardCredentialPairs.Select(x => x.cardHolderId));

            // Add the cardCredentials to the full list
            allCardCredentialIds.AddRange(cardHolderAndCardCredentialPairs.Where(x => x.cardCredential is not null).Select(x => x.cardCredential!.Id));
        }

        if (dto.CardCredentialIds is not null)
        {
            allCardCredentialIds.AddRange(dto.CardCredentialIds);

            var cardHolderIdsFromCardCredentials = await _dbContext.CardCredentials.ByTenant(tenantId)
                .Where(x => x.CardType == CardType.User && x.CardHolderId != null && dto.CardCredentialIds.Contains(x.Id))
                .Select(x => x.CardHolderId)
                .ToListAsync(cancellationToken);

            allCardHolderIds.AddRange(cardHolderIdsFromCardCredentials.Where(x => x is not null).Select(x => x!.Value));
        }

        _dbContext.LockerBankLeaseUsers.AddRange(allCardHolderIds.Distinct().Select(x => new LockerBankLeaseUser { TenantId = lockerBank.TenantId, CardHolderId = x, LockerBankId = lockerBank.Id }));

        _dbContext.LockerBankUserCardCredentials.AddRange(allCardCredentialIds.Distinct().Select(x => new LockerBankUserCardCredential { TenantId = lockerBank.TenantId, CardCredentialId = x, LockerBankId = lockerBank.Id }));

        await _dbContext.SaveChangesAsync(cancellationToken: cancellationToken);
    }

    public async Task ReplaceCardHoldersAsOwnersAndReplaceCardCredentialsForLockerAsync(Guid tenantId, Guid lockerId, UpdateManyCardCredentialAndCardHoldersDTO dto, CancellationToken cancellationToken = default)
    {
        var lockerCardCredentials = await _dbContext.LockerCardCredentials
            .ByTenant(tenantId)
            .Where(x => x.LockerId == lockerId)
            .ToListAsync(cancellationToken);

        var lockerOwners = await _dbContext.LockerOwners
            .ByTenant(tenantId)
            .Where(x => x.LockerId == lockerId)
            .ToListAsync(cancellationToken);

        _dbContext.RemoveRange(lockerCardCredentials);
        _dbContext.RemoveRange(lockerOwners);

        //await _dbContext.SaveChangesAsync(cancellationToken);

        await AssignCardHoldersAsOwnersAndAddCardCredentialsToLockerAsync(tenantId, lockerId, dto, cancellationToken);
    }

    public async Task ReplaceCardHoldersAsLeaseUsersAndReplaceCardCredentialsForLockerBankAsync(Guid tenantId, Guid lockerBankId, UpdateManyCardCredentialAndCardHoldersDTO dto, CancellationToken cancellationToken = default)
    {
        var lockerBankUserCardCredentials = await _dbContext.LockerBankUserCardCredentials
            .ByTenant(tenantId)
            .Where(x => x.LockerBankId == lockerBankId)
            .ToListAsync(cancellationToken);

        var lockerBankLeaseUsers = await _dbContext.LockerBankLeaseUsers
            .ByTenant(tenantId)
            .Where(x => x.LockerBankId == lockerBankId)
            .ToListAsync(cancellationToken);

        _dbContext.RemoveRange(lockerBankUserCardCredentials);
        _dbContext.RemoveRange(lockerBankLeaseUsers);

        //await _dbContext.SaveChangesAsync(cancellationToken);

        await AssignCardHoldersAsLeaseUsersAndAddCardCredentialsToLockerBankAsync(tenantId, lockerBankId, dto, cancellationToken);
    }

    public async Task<bool> RemoveCardHolderAndTheirCardCredentialsFromLocker(Guid tenantId, Guid lockerId, Guid cardHolderId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    private async Task<T> GetEntityFromDatabaseAsync<T>(Guid tenantId, Guid entityId, CancellationToken cancellationToken = default) where T : EntityBase
    {
        var entity = await _dbContext.Set<T>().FindAsync(new object[] { entityId }, cancellationToken: cancellationToken);

        if (entity is null || entity.TenantId != tenantId)
        {
            throw new NotFoundException(entityId, nameof(T));
        }

        return entity;
    }

    public async Task<CardHolderDTO?> GetCardHolderAsync(Guid tenantId, Guid cardHolderId, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetOneAsync(tenantId, cardHolderId, cancellationToken);

        return entity is null ? null : CardHolderMapper.ToDTO(entity);
    }

    public async Task<CardHolderDTO?> GetCardHolderByEmailAsync(Guid tenantId, string email, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.QueryingAll(tenantId).FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

        return entity is null ? null : CardHolderMapper.ToDTO(entity);
    }
    public async Task<IReadOnlyList<LockerDTO>?> GetManyLockersForCardHolderAsync(Guid tenantId, Guid cardHolderId, CancellationToken cancellationToken = default)
    {
        var lockers = await _dbContext.LockerOwners
            .ByTenant(tenantId)
            .Include(x => x.Locker)
            .ThenInclude(x => x!.CurrentLease)
            .ThenInclude(x => x!.CardHolder)
            .Where(x => x.CardHolderId == cardHolderId)
            .Where( x => x.Locker != null)
            .Select(x => x.Locker)
            .ToListAsync(cancellationToken);

        return lockers is null ? null : lockers.Select(x => LockerMapper.ToDTO(x!)).ToList();
    }
    
    public async Task<IReadOnlyList<CardHolderDTO>> GetManyCardHoldersAsync(Guid tenantId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default)
        => await FilterAndSortCardHoldersAsync(_repository.QueryingAll(tenantId), filter, sort, cancellationToken);

    public async Task<IReadOnlyList<CardHolderAndCardCredentialsDTO>> GetManyCardHoldersAndTheirUserCardCredentialsAsync(Guid tenantId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default)
        => await _repository.QueryingAll(tenantId)
            .Include(x => x.CardCredentials!.Where(y => y.CardType == CardType.User))
            .FilterAndSort(filter, sort)
            .Select(x => CardHolderAndCardCredentialsMapper.ToDTO(x))
            .ToListAsync();

    public async Task<IReadOnlyList<CardHolderAndCardCredentialsDTO>> GetManyCardHoldersAndTheirSpecialCardCredentialsAsync(Guid tenantId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default)
        => await _repository.QueryingAll(tenantId)
            .Include(x => x.CardCredentials!.Where(y => y.CardType != CardType.User))
            .FilterAndSort(filter, sort)
            .Select(x => CardHolderAndCardCredentialsMapper.ToDTO(x))
            .ToListAsync();

    public async Task<IReadOnlyList<CardHolderAndCardCredentialsDTO>> GetManyLockerOwnersAndTheirUserCardCredentialsByLockerAsync(Guid tenantId, Guid lockerId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default)
        => await _dbContext.LockerCardCredentials
                .ByTenant(tenantId)
                .Where(x => x.LockerId == lockerId)
                .Include(x => x.CardCredential)
                .ThenInclude(x => x!.CardHolder)
                .GroupBy(x => x.CardCredential!.CardHolder, x => x.CardCredential)
                .Select(x => new CardHolderAndCardCredentialsDTO
                {
                    CardHolder = CardHolderMapper.ToDTO(x.Key!),
                    CardCredentials = x.Select(y => CardCredentialMapper.ToDTO(y!)).ToList()
                }).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CardHolderAndCardCredentialsDTO>> GetManyCardHoldersAndTheirSpecialCardCredentialsByLockerBankAsync(Guid tenantId, Guid lockerBankId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default)
        => await _dbContext.LockerBankSpecialCardCredentials
                .ByTenant(tenantId)
                .Where(x => x.LockerBankId == lockerBankId)
                .Include(x => x.CardCredential)
                .ThenInclude(x => x!.CardHolder)
                .GroupBy(x => x.CardCredential!.CardHolder, x => x.CardCredential)
                .Select(x => new CardHolderAndCardCredentialsDTO
                {
                    CardHolder = CardHolderMapper.ToDTO(x.Key!),
                    CardCredentials = x.Select(y => CardCredentialMapper.ToDTO(y!)).ToList()
                }).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CardHolderAndCardCredentialsDTO>> GetManyLeaseUsersAndTheirUserCardCredentialsByLockerBankAsync(Guid tenantId, Guid lockerBankId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default)
        => await _dbContext.LockerBankUserCardCredentials
                .ByTenant(tenantId)
                .Where(x => x.LockerBankId == lockerBankId)
                .Include(x => x.CardCredential)
                .ThenInclude(x => x!.CardHolder)
                .GroupBy(x => x.CardCredential!.CardHolder, x => x.CardCredential)
                .Select(x => new CardHolderAndCardCredentialsDTO
                {
                    CardHolder = CardHolderMapper.ToDTO(x.Key!),
                    CardCredentials = x.Select(y => CardCredentialMapper.ToDTO(y!)).ToList()
                }).ToListAsync(cancellationToken);

    public async Task UpdateCardHolderAsync(Guid tenantId, Guid cardHolderId, UpdateCardHolderDTO dto, CancellationToken cancellationToken = default)
    {
        var entity = UpdateCardHolderMapper.ToEntity(dto);

        _repository.ValidateOne(entity, _validator);

        await _repository.UpdateOneAsync(tenantId, cardHolderId, entity, cancellationToken);
    }

    public async Task DeleteCardHolderAsync(Guid tenantId, Guid cardHolderId, CancellationToken cancellationToken = default)
        => await _repository.DeleteOneAsync(tenantId, cardHolderId, _options.ThrowNotFoundWhenDeletingNonExistantEntity, cancellationToken);

    private static async Task<IReadOnlyList<CardHolderDTO>> FilterAndSortCardHoldersAsync(IQueryable<CardHolder> all, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default)
            => await all.FilterAndSort(filter, sort).Select(x => CardHolderMapper.ToDTO(x)).ToListAsync(cancellationToken);
}
