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

public class LockerService : ILockerService
{
    private readonly IRepository<Locker> _repository;
    private readonly IValidator<Locker> _validator;
    private readonly SmartLockerDbContext _dbContext;
    private readonly GlobalServiceOptions _options;

    public LockerService(IRepository<Locker> repository, IValidator<Locker> validator, SmartLockerDbContext dbContext, IOptions<GlobalServiceOptions> options)
        => (_repository, _validator, _dbContext, _options) = (repository, validator, dbContext, options.Value);

    public async Task<LockerDTO?> CreateLockerAsync(Guid tenantId, CreateLockerDTO dto, CancellationToken cancellationToken = default)
    {
        var entity = CreateLockerMapper.ToEntity(dto);

        _repository.ValidateOne(entity, _validator);

        var newEntity = await _repository.CreateOneAsync(tenantId, entity, cancellationToken);

        return newEntity is null ? null : LockerMapper.ToDTO(newEntity);
    }

    public async Task<bool> CreateManyLockersAsync(Guid tenantId, IEnumerable<CreateLockerDTO> dtoList, CancellationToken cancellationToken = default)
    {
        var dtoCount = dtoList.TryGetNonEnumeratedCount(out var count) ? count : dtoList.Count();

        return (await CreateAndUseManyLockersAsync(tenantId, dtoList, cancellationToken)).Count == dtoCount;
    }

    public async Task<IReadOnlyList<LockerDTO>> CreateAndUseManyLockersAsync(Guid tenantId, IEnumerable<CreateLockerDTO> dtoList, CancellationToken cancellationToken = default)
    {
        if (!dtoList.Any())
        {
            return _options.EmptyBulkOperationIsError ? throw new BadRequestException("No data was passed") : new List<LockerDTO>();
        }

        var entities = dtoList.Select(CreateLockerMapper.ToEntity).ToList();

        _repository.ValidateMany(entities, _validator, _options.ThrowOnFirstValidationErrorForBulkOperations);

        await _repository.CreateManyAsync(tenantId, entities, cancellationToken);

        return entities.ConvertAll(LockerMapper.ToDTO);
    }

    public async Task<LockerDTO?> GetLockerAsync(Guid tenantId, Guid lockerId, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetOneAsync(tenantId, lockerId, cancellationToken);

        return entity is null ? null : LockerMapper.ToDTO(entity);
    }

    public async Task<IReadOnlyList<LockerDTO>> GetManyLockersAsync(Guid tenantId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default)
        => await FilterAndSortLockersAsync(_repository.QueryingAll(tenantId), filter, sort, cancellationToken);

    public async Task<IReadOnlyList<LockerDTO>> GetManyLockersByLockerBankAsync(Guid tenantId, Guid lockerBankId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default)
        => await FilterAndSortLockersAsync(_repository.QueryingAll(tenantId).Where(x => x.LockerBankId == lockerBankId), filter, sort, cancellationToken);

    public async Task<IReadOnlyList<LockerAndLockDTO>> GetManyLockersAndLocksByLockerBankAsync(Guid tenantId, Guid lockerBankId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default)
        => await MapFilterAndSortLockersAndLocksAsync(
            _repository.QueryingAll(tenantId)
                .Where(x => x.LockerBankId == lockerBankId)
                .Include(x => x.Lock),
            filter, sort, cancellationToken);

    public async Task<IReadOnlyList<CardHolderDTO>> GetManyCardHoldersByLockerAsync(Guid tenantId, Guid lockerId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default)
        => await _repository.QueryingAll(tenantId)
            .Where(locker => locker.Id == lockerId)
            .Include(locker => locker.CardCredentials!)
            .ThenInclude(lockerCardCredential => lockerCardCredential.CardCredential)
            .ThenInclude(cardCredential => cardCredential!.CardHolder)
            .SelectMany(locker => locker.CardCredentials!)
            .Select(lockerCardCredential => lockerCardCredential.CardCredential!.CardHolder)
            .OfType<CardHolder>()
            .FilterAndSort(filter, sort)
            .Select(x => CardHolderMapper.ToDTO(x))
            .ToListAsync(cancellationToken);

    public async Task<GlobalLockerSearchResultDTO> GetManyLockersWithLockAndLocationAndLockerBankDetailsAsync(Guid tenantId, IPagingRequest? page = null, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default)
    {
        var lockers = await _repository.QueryingAll(tenantId)
            .FilterAndSortLockerAndLocks(filter, sort)
            .Include(x => x.LockerBank)
            .ThenInclude(x => x!.Location)
            .Include(x => x.Lock)
            //.AsSplitQuery())
            .ToListAsync(cancellationToken);

        var lockerBanks = lockers.Select(x => x.LockerBank).OfType<LockerBank>().DistinctBy(x => x.Id).ToList();

        var locationDTOs = lockerBanks.Select(x => x.Location).OfType<Location>().DistinctBy(x => x.Id).ToList().ConvertAll(LocationMapper.ToDTO);

        page ??= new PagingRequest();

        return new (
            Locations: locationDTOs,
            LockerBanks: lockerBanks.ConvertAll(LockerBankMapper.ToDTO),
            LockerAndLocks: page.ToResponse(lockers.ConvertAll(LockerAndLockMapper.ToDTO))
        );
    }

    public async Task UpdateLockerAsync(Guid tenantId, Guid lockerId, UpdateLockerDTO dto, CancellationToken cancellationToken = default)
    {
        var entity = UpdateLockerMapper.ToEntity(dto);

        _repository.ValidateOne(entity, _validator);

        await _repository.UpdateOneAsync(tenantId, lockerId, entity, cancellationToken);
    }

    public async Task<bool> UpdateManyLockersAsync(Guid tenantId, UpdateManyLockersDTO dto, CancellationToken cancellationToken = default)
    {
        var entities = dto.Lockers.Select(UpdateLockerMapper.ToEntity).ToList();

        _repository.ValidateMany(entities, _validator);

        return await _repository.UpdateManyAsync(tenantId, entities, cancellationToken) == entities.Count;
    }

    public async Task DeleteLockerAsync(Guid tenantId, Guid lockerId, CancellationToken cancellationToken = default)
        => await _repository.DeleteOneAsync(tenantId, lockerId, _options.ThrowNotFoundWhenDeletingNonExistantEntity, cancellationToken);

    private static async Task<IReadOnlyList<LockerDTO>> FilterAndSortLockersAsync(IQueryable<Locker> all, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default)
    {
        const string Label = "label";
        const string ServiceTag = "servicetag";

        if (filter is not null && !string.IsNullOrWhiteSpace(filter.FilterValue))
        {
            var filterLabel = filter.FilterProperties.IsNullOrEmpty() || filter.FilterProperties.Contains(Label, StringComparer.OrdinalIgnoreCase);
            var filterServiceTag = filter.FilterProperties.IsNullOrEmpty() || filter.FilterProperties.Contains(ServiceTag, StringComparer.OrdinalIgnoreCase);

            all = all.Where(x =>
                (filterLabel && x.Label.Contains(filter.FilterValue)) ||
                (filterServiceTag && x.ServiceTag != null && x.ServiceTag.Contains(filter.FilterValue))
            );
        }

        if (sort is not null && !string.IsNullOrWhiteSpace(sort.SortBy))
        {
            all = sort.SortBy.ToLowerInvariant() switch
            {
                Label => sort.SortOrder == SortOrder.Ascending ? all.OrderBy(x => x.Label) : all.OrderByDescending(x => x.Label),
                ServiceTag => sort.SortOrder == SortOrder.Ascending ? all.OrderBy(x => x.ServiceTag) : all.OrderByDescending(x => x.ServiceTag),
                _ => throw new BadRequestException($"Cannot sort on property {sort.SortBy}, no such property exists")
            };
        }

        return await all.Select(x => LockerMapper.ToDTO(x)).ToListAsync(cancellationToken);
    }

    private static async Task<IReadOnlyList<LockerAndLockDTO>> MapFilterAndSortLockersAndLocksAsync(IQueryable<Locker> all, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default)
        => await all.FilterAndSortLockerAndLocks(filter, sort).Select(x => LockerAndLockMapper.ToDTO(x)).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CardHolderAndCardCredentialsDTO>> GetOwnersWithCardsAssignedToLockerAsync(Guid tenantId, Guid lockerId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default)
    {
        var locker = await _repository.GetOneAsync(tenantId, lockerId, cancellationToken);

        if (locker is null)
        {
            throw new NotFoundException(lockerId, "Locker");
        }

        if (locker.SecurityType == LockerSecurityType.SmartLock)
        {
            return await _dbContext.LockerCardCredentials.ByTenant(tenantId)
                .Include(x => x.CardCredential)
                .ThenInclude(x => x!.CardHolder)
                .Where(x => x.LockerId == lockerId)
                .GroupBy(x => x.CardCredential!.CardHolder, x => x.CardCredential)
                .Select(x => new CardHolderAndCardCredentialsDTO
                {
                    CardHolder = CardHolderMapper.ToDTO(x.Key!),
                    CardCredentials = x.Select(y => CardCredentialMapper.ToDTO(y!)).ToList()
                }).ToListAsync(cancellationToken);
        }
        else
        {
            return await _dbContext.LockerOwners.ByTenant(tenantId)
                .Include(x => x.CardHolder)
                .Where(x => x.LockerId == lockerId)
                .Select(x => new CardHolderAndCardCredentialsDTO
                {
                    CardHolder = CardHolderMapper.ToDTO(x.CardHolder!)
                }).ToListAsync(cancellationToken);
        }
    }

    public async Task ReplaceOwnersForLockerByCardCredentialsAsync(Guid tenantId, Guid lockerId, IReadOnlyList<Guid> cardCredentialIds, CancellationToken cancellationToken = default)
    {
        // NOTE: This should probably be passed in
        var leaseStartOrEndTime = DateTimeOffset.UtcNow;

        var distinctCardCredentialIds = cardCredentialIds.Distinct().ToList();

        if (distinctCardCredentialIds.Count > DomainConstants.MaxUserCardCredentialsPerLocker)
        {
            throw new BadRequestException("Too many card credentials assigned to a single smart locker");
        }

        // Get current owners so we can start permanent leases for new owners and end leases for previous owners.
        // No action for existing owners in the new list of owners.
        var locker = await _dbContext.Lockers.ByTenant(tenantId)
            .Include(x => x.Lock)
            .Include(x => x.PermanentOwners)
            .Include(x => x.CardCredentials)
            .FirstOrDefaultAsync(x => x.Id == lockerId, cancellationToken);

        if (locker is null)
        {
            throw new NotFoundException(lockerId, "Locker");
        }

        if(locker.SecurityType != LockerSecurityType.SmartLock)
        {
            throw new BadRequestException("Cannot assign card credentials to a non smart locker");
        }

        var cardCredentials = await _dbContext.CardCredentials.ByTenant(tenantId)
            .Where(x => (x.CardType == CardType.User || x.CardType == CardType.Temporary) && distinctCardCredentialIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        var cardCredentialsAreNullOrEmpty = cardCredentials.IsNullOrEmpty();

        var existingPermanentOwnersAreEmpty = locker.PermanentOwners.IsNullOrEmpty();

        if(!existingPermanentOwnersAreEmpty)
        {
            // Find all old permanent owners
            var oldOwnerIds = locker.PermanentOwners?.Where(x => !cardCredentials.Exists(y => y.CardHolderId == x.CardHolderId)).Select(x => x.CardHolderId).ToList();

            if(!oldOwnerIds.IsNullOrEmpty())
            {
                var oldOwnerLeases = await _dbContext.LockerLeases.ByTenant(tenantId)
                    .Where(x => x.LockerId == locker.Id && x.EndedAt == null && x.CardHolderId.HasValue && oldOwnerIds.Contains(x.CardHolderId.Value))
                    .ToListAsync(cancellationToken);

                // End permanent leases for old owners
                foreach (var lease in oldOwnerLeases)
                {
                    lease.EndedAt = leaseStartOrEndTime;
                }
            }
        }

        if (!cardCredentialsAreNullOrEmpty)
        {
            // Find all new permanent owners
            var newOwnerIds = cardCredentials.Where(x => !locker.PermanentOwners?.Exists(y => y.CardHolderId == x.CardHolderId) ?? false);

            // Start permanent leases for new owners
            _dbContext.LockerLeases.AddRange(newOwnerIds.Select(x => new LockerLease
            {
                TenantId = tenantId,
                StartedAt = leaseStartOrEndTime,
                LockerBankBehaviour = LockerBankBehaviour.Permanent,
                CardCredentialId = x.Id,
                CardHolderId = x.CardHolderId,
                LockerId = locker.Id,
                LockId = locker.Lock?.Id,
            }));
        }

        if(locker.CardCredentials is not null)
        {
            _dbContext.LockerCardCredentials.RemoveRange(locker.CardCredentials);
        }

        _dbContext.LockerCardCredentials.AddRange(cardCredentials.Select(x => new LockerCardCredential
        {
            TenantId = tenantId,
            LockerId = locker.Id,
            CardCredentialId = x.Id
        }));

        if(!locker.PermanentOwners.IsNullOrEmpty())
        {
            _dbContext.LockerOwners.RemoveRange(locker.PermanentOwners);
        }

        _dbContext.LockerOwners.AddRange(cardCredentials.DistinctBy(x => x.CardHolderId).Select(x => x.CardHolderId).OfType<Guid>().Select(x => new LockerOwner
        {
            TenantId = tenantId,
            LockerId = locker.Id,
            CardHolderId = x
        }));

        await _dbContext.SaveChangesAsync();
    }

    public async Task ReplaceOwnersForLockerByCardHoldersAsync(Guid tenantId, Guid lockerId, IReadOnlyList<Guid> cardHolderIds, CancellationToken cancellationToken = default)
    {
        // NOTE: This should probably be passed in
        var leaseStartOrEndTime = DateTimeOffset.UtcNow;

        var distinctCardHolderIds = cardHolderIds.Distinct().ToList();

        if (distinctCardHolderIds.Count > 1)
        {
            throw new BadRequestException("Too many card credentials assigned to a single non-smart locker");
        }

        // Get current owners so we can start permanent leases for new owners and end leases for previous owners.
        // No action for existing owners in the new list of owners.
        var locker = await _dbContext.Lockers.ByTenant(tenantId)
            .Include(x => x.PermanentOwners)
            .FirstOrDefaultAsync(x => x.Id == lockerId, cancellationToken);

        if (locker is null)
        {
            throw new NotFoundException(lockerId, "Locker");
        }

        if (locker.SecurityType == LockerSecurityType.SmartLock)
        {
            throw new BadRequestException("Cannot assign card holders to a smart locker, use card credentials instead");
        }

        var cardHoldersAreNullOrEmpty = distinctCardHolderIds.IsNullOrEmpty();

        var existingPermanentOwnersAreEmpty = locker.PermanentOwners.IsNullOrEmpty();

        if(!existingPermanentOwnersAreEmpty)
        {
            // Find all old permanent owners
            var oldOwnerIds = locker.PermanentOwners?.Where(x => !distinctCardHolderIds.Exists(y => y == x.CardHolderId)).Select(x => x.CardHolderId).ToList();

            if (!oldOwnerIds.IsNullOrEmpty())
            {
                var oldOwnerLeases = await _dbContext.LockerLeases.ByTenant(tenantId)
                    .Where(x => x.LockerId == locker.Id && x.EndedAt == null && x.CardHolderId.HasValue && oldOwnerIds.Contains(x.CardHolderId.Value))
                    .ToListAsync(cancellationToken);

                // End permanent leases for old owners
                foreach (var lease in oldOwnerLeases)
                {
                    lease.EndedAt = leaseStartOrEndTime;
                }
            }
        }

        if (!cardHoldersAreNullOrEmpty)
        {
            // Find all new permanent owners
            var newOwnerIds = distinctCardHolderIds.Where(x => !locker.PermanentOwners?.Exists(y => y.CardHolderId == x) ?? false);

            // Start permanent leases for new owners
            _dbContext.LockerLeases.AddRange(newOwnerIds.Select(x => new LockerLease
            {
                TenantId = tenantId,
                StartedAt = leaseStartOrEndTime,
                LockerBankBehaviour = LockerBankBehaviour.Permanent,
                CardHolderId = x,
                LockerId = locker.Id,
            }));
        }

        if(!locker.PermanentOwners.IsNullOrEmpty())
        {
            _dbContext.LockerOwners.RemoveRange(locker.PermanentOwners);
        }

        _dbContext.LockerOwners.AddRange(distinctCardHolderIds.Select(x => new LockerOwner
        {
            TenantId = tenantId,
            LockerId = locker.Id,
            CardHolderId = x
        }));


        await _dbContext.SaveChangesAsync();
    }
}
