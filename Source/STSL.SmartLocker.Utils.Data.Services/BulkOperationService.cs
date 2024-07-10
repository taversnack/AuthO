using Microsoft.Extensions.Logging;
using STSL.SmartLocker.Utils.Common.Data;
using STSL.SmartLocker.Utils.Common.Exceptions;
using STSL.SmartLocker.Utils.Data.Contexts;
using STSL.SmartLocker.Utils.Data.Extensions;
using STSL.SmartLocker.Utils.Data.Services.Contracts;
using STSL.SmartLocker.Utils.Domain;
using STSL.SmartLocker.Utils.DTO;

namespace STSL.SmartLocker.Utils.Data.Services;

public sealed class BulkOperationService : IBulkOperationService
{
    private readonly ILockerService _lockerService;
    private readonly ILockService _lockService;
    private readonly ICardHolderService _cardHolderService;
    private readonly ICardCredentialService _cardCredentialService;
    private readonly SmartLockerDbContext _dbContext;
    private readonly ILogger<BulkOperationService> _logger;

    public BulkOperationService(
        ILockerService lockerService,
        ILockService lockService,
        ICardHolderService cardHolderService,
        ICardCredentialService cardCredentialService,
        SmartLockerDbContext dbContext,
        ILogger<BulkOperationService> logger)
        => (_lockerService, _lockService, _cardHolderService, _cardCredentialService, _dbContext, _logger)
        = (lockerService, lockService, cardHolderService, cardCredentialService, dbContext, logger);

    public async Task<bool> CreateManyLockerAndLockPairsForLockerBankAsync(Guid tenantId, Guid lockerBankId, CreateBulkLockerAndLocksDTO dto, CancellationToken cancellationToken = default)
    {
        // I'm going to assume order is preserved here, but we shall see during testing!
        var createdLockers = await _lockerService.CreateAndUseManyLockersAsync(tenantId, dto.LockerAndLocks.Select(x => x.Locker with { LockerBankId = lockerBankId }), cancellationToken);

        if (createdLockers is null || createdLockers.Count == dto.LockerAndLocks.Count)
        {
            return false;
        }

        var createdLocks = await _lockService.CreateAndUseManyLocksAsync(tenantId, dto.LockerAndLocks.Select((x, i) => x.Lock with { LockerId = createdLockers[i].Id }), cancellationToken);

        return createdLockers.Count == createdLocks.Count;
    }

    public async Task<bool> CreateManyCardHolderAndCardCredentialPairsAsync(Guid tenantId, CreateBulkCardHolderAndCardCredentialsDTO dto, CancellationToken cancellationToken = default)
    {
        // I'm going to assume order is preserved here, but we shall see during testing!
        // Create transaction with dbContext so we can rollback if errors occur?
        var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var createdCardHolders = await _cardHolderService.CreateAndUseManyCardHoldersAsync(tenantId, dto.CardHolderAndCardCredentials.Select(x => x.CardHolder), cancellationToken);

        if (createdCardHolders is null || createdCardHolders.Count != dto.CardHolderAndCardCredentials.Count)
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        var createdCardCredentials = await _cardCredentialService.CreateAndUseManyCardCredentialsAsync(tenantId, dto.CardHolderAndCardCredentials.Select((x, i) => x.CardCredential with { CardHolderId = createdCardHolders[i].Id }), cancellationToken);
        
        if(createdCardHolders.Count == createdCardCredentials.Count)
        {
            await transaction.CommitAsync(cancellationToken);
            return true;
        }

        await transaction.RollbackAsync(cancellationToken);
        return false;
    }

    // Deprecate this
    public async Task<bool> CreateAndAssignNewCardHolderAndCardCredentialPairsToNewLockersAndLockPairs(Guid tenantId, Guid lockerBankId, CreateBulkLockerAndLockAndCardHolderAndCardCredentialsDTO dto, CancellationToken cancellationToken = default)
    {
        // I'm going to assume order is preserved here, but we shall see during testing!
        var createdLockers = await _lockerService.CreateAndUseManyLockersAsync(tenantId, dto.LockerAndLockAndCardHolderAndCardCredentials.Select(x => x.Locker with { LockerBankId = lockerBankId }), cancellationToken);

        if (createdLockers is null || createdLockers.Count == dto.LockerAndLockAndCardHolderAndCardCredentials.Count)
        {
            return false;
        }

        var createdLocks = await _lockService.CreateAndUseManyLocksAsync(tenantId, dto.LockerAndLockAndCardHolderAndCardCredentials.Select((x, i) => x.Lock with { LockerId = createdLockers[i].Id }), cancellationToken);

        // I'm going to assume order is preserved here, but we shall see during testing!
        var createdCardHolders = await _cardHolderService.CreateAndUseManyCardHoldersAsync(tenantId, dto.LockerAndLockAndCardHolderAndCardCredentials.Select(x => x.CardHolder), cancellationToken);

        if (createdCardHolders is null || createdCardHolders.Count == dto.LockerAndLockAndCardHolderAndCardCredentials.Count)
        {
            return false;
        }

        var createdCardCredentials = await _cardCredentialService.CreateAndUseManyCardCredentialsAsync(tenantId, dto.LockerAndLockAndCardHolderAndCardCredentials.Select((x, i) => x.CardCredential with { CardHolderId = createdCardHolders[i].Id }), cancellationToken);

        if (createdCardCredentials is null || createdCardCredentials.Count == dto.LockerAndLockAndCardHolderAndCardCredentials.Count)
        {
            return false;
        }

        var equalNumberOfEntitiesCreated = createdLockers.Count == createdLocks.Count && createdCardHolders.Count == createdCardCredentials.Count && createdLockers.Count == createdCardHolders.Count;

        if (equalNumberOfEntitiesCreated)
        {
            var leaseStartTime = DateTimeOffset.UtcNow;

            _dbContext.LockerCardCredentials.AddRange(createdCardCredentials.Select((x, i) => new LockerCardCredential
            {
                TenantId = tenantId,
                LockerId = createdLockers[i].Id,
                CardCredentialId = x.Id
            }));

            _dbContext.LockerOwners.AddRange(createdCardHolders.Select((x, i) => new LockerOwner
            {
                TenantId = tenantId,
                LockerId = createdLockers[i].Id,
                CardHolderId = x.Id
            }));

            var lockerOwnersAndCardCredentialsCreated = await _dbContext.SaveChangesAsync();
            return equalNumberOfEntitiesCreated && lockerOwnersAndCardCredentialsCreated / 2 == createdCardHolders.Count;
        }

        return false;
    }
}
