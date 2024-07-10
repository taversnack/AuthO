using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using STSL.SmartLocker.Utils.Common.Data;
using STSL.SmartLocker.Utils.Common.Exceptions;
using STSL.SmartLocker.Utils.Data.Contexts;
using STSL.SmartLocker.Utils.Data.Extensions;
using STSL.SmartLocker.Utils.Data.Services.Configuration;
using STSL.SmartLocker.Utils.Data.Services.Contracts;
using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.Data.Services.Helpers;
using STSL.SmartLocker.Utils.Data.Services.Mappings;
using STSL.SmartLocker.Utils.Domain;
using STSL.SmartLocker.Utils.Domain.Interfaces;
using STSL.SmartLocker.Utils.DTO;
using System.Data.Common;

namespace STSL.SmartLocker.Utils.Data.Services;

public sealed class LockerBankService : ILockerBankService
{
    private readonly IRepository<LockerBank> _repository;
    private readonly IValidator<LockerBank> _validator;
    private readonly SmartLockerDbContext _dbContext;
    private readonly IDatabaseExceptionHandler _exceptionHandler;
    private readonly GlobalServiceOptions _options;

    public LockerBankService(
        IRepository<LockerBank> repository, 
        IValidator<LockerBank> validator,
        SmartLockerDbContext dbContext,
        IDatabaseExceptionHandler exceptionHandler,
        IOptions<GlobalServiceOptions> options)
        => (_repository, _validator, _dbContext, _exceptionHandler, _options)
        = (repository, validator, dbContext, exceptionHandler, options.Value);

    public async Task<LockerBankDTO?> CreateLockerBankAsync(Guid tenantId, CreateLockerBankDTO dto, CancellationToken cancellationToken = default)
    {
        var entity = CreateLockerBankMapper.ToEntity(dto);

        _repository.ValidateOne(entity, _validator);

        var newEntity = await _repository.CreateOneAsync(tenantId, entity, cancellationToken);

        return newEntity is null ? null : LockerBankMapper.ToDTO(newEntity);
    }

    public async Task<bool> CreateManyLockerBanksAsync(Guid tenantId, IEnumerable<CreateLockerBankDTO> dtoList, CancellationToken cancellationToken = default)
    {
        if (!dtoList.Any())
        {
            return _options.EmptyBulkOperationIsError ? throw new BadRequestException("No data was passed") : true;
        }

        var entities = dtoList.Select(CreateLockerBankMapper.ToEntity).ToList();

        _repository.ValidateMany(entities, _validator, _options.ThrowOnFirstValidationErrorForBulkOperations);

        return await _repository.CreateManyAsync(tenantId, entities, cancellationToken);
    }

    public async Task<LockerBankDTO?> GetLockerBankAsync(Guid tenantId, Guid lockerBankId, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetOneAsync(tenantId, lockerBankId, cancellationToken);

        return entity is null ? null : LockerBankMapper.ToDTO(entity);
    }

    public async Task<IReadOnlyList<LockerBankDTO>> GetManyLockerBanksAsync(Guid tenantId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default)
        => await FilterAndSortLockerBanksAsync(_repository.QueryingAll(tenantId), filter, sort, cancellationToken);

    public async Task<IReadOnlyList<LockerBankDTO>> GetManyLockerBanksByLocationAsync(Guid tenantId, Guid locationId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default)
        => await FilterAndSortLockerBanksAsync(_repository.QueryingAll(tenantId).Where(x => x.LocationId == locationId), filter, sort, cancellationToken);

    public async Task UpdateLockerBankAsync(Guid tenantId, Guid lockerBankId, UpdateLockerBankDTO dto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetOneAsync(tenantId, lockerBankId, cancellationToken) ?? throw new NotFoundException(lockerBankId);

        var updatedEntity = UpdateLockerBankMapper.ToEntity(dto, entity);

        _repository.ValidateOne(updatedEntity, _validator);

        var lockerBank = await _repository.QueryingAll(tenantId)
            .Include(x => x.Lockers.Where(y => y.SecurityType == LockerSecurityType.SmartLock))
            .ThenInclude(x => x.Lock)
            .FirstOrDefaultAsync(x => x.Id == lockerBankId, cancellationToken);

        if(lockerBank is null)
        {
            throw new NotFoundException(lockerBankId, "Locker Bank");
        }

        foreach(var locker in lockerBank.Lockers)
        {
            if(locker.Lock is not null)
            {
                locker.Lock.OperatingMode = GetLockOperatingModeFromLockerBankBehaviour(dto.Behaviour);
            }
        }

        updatedEntity.Id = lockerBankId;
        updatedEntity.TenantId = tenantId;
        _dbContext.Entry(lockerBank).CurrentValues.SetValues(updatedEntity);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            if(!TryHandleDatabaseExceptions(ex))
            {
                throw;
            }
        }
    }

    private LockOperatingMode GetLockOperatingModeFromLockerBankBehaviour(LockerBankBehaviour lockerBankBehaviour) => lockerBankBehaviour switch
    {
        LockerBankBehaviour.Unset => LockOperatingMode.Installation,
        LockerBankBehaviour.Permanent => LockOperatingMode.Dedicated,
        LockerBankBehaviour.Temporary => LockOperatingMode.Shared,
        _ => throw new BadRequestException("Locker bank behaviour specified is not supported"),
    };

    public async Task UpdateAllLockersInLockerBankAsync(Guid tenantId, Guid lockerBankId, UpdateLockerBankLockersDTO dto, CancellationToken cancellationToken = default)
    {
        // NOTE: [0] If editing both aspects at same time (seems unlikely) it would probably
        // be more efficient to select data in a single query.

        if (dto.LockerBankId.HasValue)
        {
            var lockerBank = await _dbContext.LockerBanks.FindAsync(new object[] { dto.LockerBankId }, cancellationToken: cancellationToken);

            if(lockerBank is null)
            {
                throw new NotFoundException(lockerBankId, "Locker Bank");
            }

            var lockers = await _repository
                .QueryingAll(tenantId)
                .Where(x => x.Id == lockerBankId)
                .Include(x => x.Lockers)
                .ThenInclude(x => x.Lock)
                .SelectMany(x => x.Lockers)
                .ToListAsync(cancellationToken);

            foreach (var locker in lockers)
            {
                locker.LockerBankId = dto.LockerBankId.Value;
                if(locker.SecurityType == LockerSecurityType.SmartLock && locker.Lock is not null)
                {
                    locker.Lock.OperatingMode = GetLockOperatingModeFromLockerBankBehaviour(lockerBank.Behaviour);
                }
            }
        }

        if (dto.LockOperatingMode.HasValue)
        {
            var locks = await _repository
                .QueryingAll(tenantId)
                .Where(x => x.Id == lockerBankId)
                .Include(x => x.Lockers)
                .ThenInclude(x => x.Lock)
                .SelectMany(x => x.Lockers.Select(x => x.Lock))
                .OfType<Lock>()
                .ToListAsync();

            foreach (var @lock in locks)
            {
                @lock.OperatingMode = dto.LockOperatingMode.Value;
            }

        }

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            if (!TryHandleDatabaseExceptions(ex))
            {
                throw;
            }
        }
    }

    public async Task DeleteLockerBankAsync(Guid tenantId, Guid lockerBankId, CancellationToken cancellationToken = default)
        => await _repository.DeleteOneAsync(tenantId, lockerBankId, _options.ThrowNotFoundWhenDeletingNonExistantEntity, cancellationToken);

    private static async Task<IReadOnlyList<LockerBankDTO>> FilterAndSortLockerBanksAsync(IQueryable<LockerBank> all, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default)
        => await all.FilterAndSort(filter, sort).Select(x => LockerBankMapper.ToDTO(x)).ToListAsync(cancellationToken);

    public async Task ReplaceLeaseUsersForLockerBankAsync(Guid tenantId, Guid lockerBankId, IReadOnlyList<Guid> cardCredentialIds, CancellationToken cancellationToken = default)
    {
        var distinctCardCredentialIds = cardCredentialIds.Distinct().ToList();

        if (distinctCardCredentialIds.Count > DomainConstants.MaxUserCardCredentialsPerLocker)
        {
            throw new BadRequestException("Too many user card credentials assigned to a single temporary locker bank");
        }

        // Assert the locker bank is valid and is Temporary behaviour
        var lockerBank = await _dbContext.LockerBanks.ByTenant(tenantId)
            .Include(x => x.LeaseUsers)
            .Where(x => x.Id == lockerBankId)
            .FirstOrDefaultAsync(cancellationToken);

        if(lockerBank is null)
        {
            throw new NotFoundException(lockerBankId, "Locker Bank");
        }

        if(lockerBank.Behaviour != LockerBankBehaviour.Temporary)
        {
            throw new BadRequestException("Locker Bank behaviour is not set to Temporary");
        }

        // Check we are only assigning user cards
        var cardCredentials = await _dbContext.CardCredentials.ByTenant(tenantId)
            .Where(x => x.CardType == CardType.User && distinctCardCredentialIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        // Delete currently assigned user card credentials and lease users
        await DeleteAllCardCredentialsFromLockerBankAsync<LockerBankUserCardCredential>(tenantId, lockerBankId, cancellationToken);

        var currentLeaseUsers = await _dbContext.LockerBankLeaseUsers
            .ByTenant(tenantId)
            .Where(x => x.LockerBankId == lockerBankId)
            .ToListAsync(cancellationToken);

        _dbContext.RemoveRange(currentLeaseUsers);

        // Assign new card credentials and lease users
        AssignCardCredentialsToLockerBank<LockerBankUserCardCredential>(tenantId, lockerBankId, cardCredentials.Select(x => x.Id));

        var cardHolderIds = cardCredentials.Select(x => x.CardHolderId).OfType<Guid>().Distinct();

        _dbContext.LockerBankLeaseUsers.AddRange(cardHolderIds.Select(x => new LockerBankLeaseUser
        {
            TenantId = tenantId,
            LockerBankId = lockerBankId,
            CardHolderId = x
        }));

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            if (!TryHandleDatabaseExceptions(ex))
            {
                throw;
            }
        }
    }

    public async Task ReplaceSpecialCardsForLockerBankAsync(Guid tenantId, Guid lockerBankId, IReadOnlyList<Guid> cardCredentialIds, CancellationToken cancellationToken = default)
    {
        var distinctCardCredentialIds = cardCredentialIds.Distinct().ToList();

        if (distinctCardCredentialIds.Count > DomainConstants.MaxSpecialCardCredentialsPerLocker)
        {
            throw new BadRequestException("Too many special card credentials assigned to a single locker bank");
        }

        // Check we are only assigning special cards
        var cardCredentials = await _dbContext.CardCredentials.ByTenant(tenantId)
            .Where(x => x.CardType != CardType.User && distinctCardCredentialIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        await DeleteAllCardCredentialsFromLockerBankAsync<LockerBankSpecialCardCredential>(tenantId, lockerBankId, cancellationToken);

        AssignCardCredentialsToLockerBank<LockerBankSpecialCardCredential>(tenantId, lockerBankId, cardCredentials.Select(x => x.Id));

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            if (!TryHandleDatabaseExceptions(ex))
            {
                throw;
            }
        }
    }

    public async Task<IReadOnlyList<CardHolderAndCardCredentialsDTO>> GetLeaseUsersWithUserCardsAssignedToLockerBankAsync(Guid tenantId, Guid lockerBankId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default)
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

    public async Task<IReadOnlyList<CardHolderAndCardCredentialsDTO>> GetCardHoldersWithSpecialCardsAssignedToLockerBankAsync(Guid tenantId, Guid lockerBankId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default)
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

    private async Task DeleteAllCardCredentialsFromLockerBankAsync<T>(Guid tenantId, Guid lockerBankId, CancellationToken cancellationToken = default) where T : EntityBase, ILockerBankCardCredential
    {
        var cards = await _dbContext.Set<T>()
            .ByTenant(tenantId)
            .Where(x => x.LockerBankId == lockerBankId)
            .ToListAsync(cancellationToken);

        _dbContext.RemoveRange(cards);
    }

    private void AssignCardCredentialsToLockerBank<T>(Guid tenantId, Guid lockerBankId, IEnumerable<Guid> cardCredentialIds) where T : EntityBase, ILockerBankCardCredential, new()
    {
        if(!cardCredentialIds.Any())
        {
            return;
        }

        var entities = cardCredentialIds.Select(cardCredentialId => new T()
        {
            TenantId = tenantId,
            LockerBankId = lockerBankId,
            CardCredentialId = cardCredentialId,
        });

        _dbContext.Set<T>().AddRange(entities);
    }

    private bool TryHandleDatabaseExceptions(Exception ex)
    {
        if (ex is DbUpdateConcurrencyException)
        {
            throw new ConflictException("The data you are trying to update has been updated by someone else, please recheck the current value and try again if necessary");
        }
        else if (ex is DbUpdateException updateException && updateException.InnerException is DbException innerException)
        {
            _exceptionHandler.HandleException(innerException);
            return true;
        }
        return false;
    }

    public async Task MoveManyLockersToAnotherBankAsync(Guid tenantId, Guid origin, Guid destination, List<Guid> lockers, CancellationToken cancellationToken = default)
    {
        if(!await _repository.ExistsAsync(tenantId, destination, cancellationToken))
        {
            throw new NotFoundException(destination, "Locker Bank");
        }
        try
        {
            var allLockersInOrigin = await _dbContext.Lockers
                .ByTenant(tenantId)
                .Where(x => x.LockerBankId == origin)
                .ToListAsync(cancellationToken);

            var lockersToBeMoved = allLockersInOrigin.IntersectBy(lockers, x => x.Id);

            foreach(var locker in lockersToBeMoved)
            {
                locker.LockerBankId = destination;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            if (!TryHandleDatabaseExceptions(ex))
            {
                throw;
            }
        }
    }
}
