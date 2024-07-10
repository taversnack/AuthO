using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using STSL.SmartLocker.Utils.BlubugConfigProducer.Contracts;
using STSL.SmartLocker.Utils.BlubugConfigProducer.DTO;
using STSL.SmartLocker.Utils.Common.Data;
using STSL.SmartLocker.Utils.Common.Exceptions;
using STSL.SmartLocker.Utils.Common.Helpers;
using STSL.SmartLocker.Utils.Data.Contexts;
using STSL.SmartLocker.Utils.Data.Extensions;
using STSL.SmartLocker.Utils.Data.Services.Contracts;
using STSL.SmartLocker.Utils.Domain;

namespace STSL.SmartLocker.Utils.Data.Services;

public sealed class LockConfigService : ILockConfigService
{
    private readonly SmartLockerDbContext _context;
    // Keep a very lightweight database record when locks are pending update.
    // If system fails, all queued locks can be updated on resume.
    //private readonly IRepository<LockConfigUpdates> _lockConfigUpdateRepository;
    private readonly IBlubugService _blubugService;
    private readonly ILogger<LockConfigService> _logger;

    private readonly bool _throwIfUpdatingLockerWithoutLock = false;

    public LockConfigService(
        SmartLockerDbContext context,
        IBlubugService blubugService,
        ILogger<LockConfigService> logger)
        => (_context, _blubugService, _logger)
        = (context, blubugService, logger);

    public async Task UpdateLockConfigAsync(Guid tenantId, Guid lockId, CancellationToken cancellationToken = default)
    {
        if (!await CheckTenantAllowsLockUpdatesAsync(tenantId, cancellationToken))
        {
            // TODO: Log or do something useful here
            return;
        }

        var @lock = await _context.Locks
            .ByTenant(tenantId)
            .Include(x => x.Locker)
            .ThenInclude(x => x!.CardCredentials!)
            .ThenInclude(x => x.CardCredential)
            .Include(x => x.Locker)
            .ThenInclude(x => x!.LockerBank)
            .ThenInclude(x => x!.SpecialCardCredentials)
            .ThenInclude(x => x.CardCredential)
            .Include(x => x.Locker)
            .ThenInclude(x => x!.LockerBank)
            .ThenInclude(x => x!.UserCardCredentials)
            .ThenInclude(x => x.CardCredential)
            .FirstOrDefaultAsync(x => x.Id == lockId, cancellationToken);

        if (@lock is null || @lock.Locker is null)
        {
            if (_throwIfUpdatingLockerWithoutLock)
            {
                throw new BadRequestException("The locker is not found or not linked to a lock");
            }
            return;
        }

        if (@lock.Locker.SecurityType == LockerSecurityType.SmartLock)
        {
            SendLockUpdateToBlubug(@lock, cancellationToken);
        }
    }

    public async Task UpdateLockConfigByLockerAsync(Guid tenantId, Guid lockerId, CancellationToken cancellationToken = default)
    {
        if (!await CheckTenantAllowsLockUpdatesAsync(tenantId, cancellationToken))
        {
            // TODO: Log or do something useful here
            return;
        }

        var @lock = await _context.Locks
            .ByTenant(tenantId)
            .Include(x => x.Locker)
            .ThenInclude(x => x!.CardCredentials!)
            .ThenInclude(x => x.CardCredential)
            .Include(x => x.Locker)
            .ThenInclude(x => x!.LockerBank)
            .ThenInclude(x => x!.SpecialCardCredentials)
            .ThenInclude(x => x.CardCredential)
            .Include(x => x.Locker)
            .ThenInclude(x => x!.LockerBank)
            .ThenInclude(x => x!.UserCardCredentials)
            .ThenInclude(x => x.CardCredential)
            .FirstOrDefaultAsync(x => x.LockerId == lockerId, cancellationToken);

        if (@lock is null || @lock.Locker is null)
        {
            if (_throwIfUpdatingLockerWithoutLock)
            {
                throw new BadRequestException("The locker is not found or not linked to a lock");
            }
            return;
        }

        if(@lock.Locker.SecurityType == LockerSecurityType.SmartLock)
        {
            SendLockUpdateToBlubug(@lock, cancellationToken);
        }
    }

    public async Task UpdateLockConfigsByLockerBankAsync(Guid tenantId, Guid lockerBankId, CancellationToken cancellationToken = default)
    {
        if (!await CheckTenantAllowsLockUpdatesAsync(tenantId, cancellationToken))
        {
            // TODO: Log or do something useful here
            return;
        }

        var locks = await _context.Locks
            .ByTenant(tenantId)
            .Include(x => x.Locker)
            .ThenInclude(x => x!.CardCredentials!)
            .ThenInclude(x => x.CardCredential)
            .Include(x => x.Locker)
            .ThenInclude(x => x!.LockerBank)
            .ThenInclude(x => x!.SpecialCardCredentials)
            .ThenInclude(x => x.CardCredential)
            .Include(x => x.Locker)
            .ThenInclude(x => x!.LockerBank)
            .ThenInclude(x => x!.UserCardCredentials)
            .ThenInclude(x => x.CardCredential)
            .Where(x => x.Locker!.LockerBankId == lockerBankId && x.Locker.SecurityType == LockerSecurityType.SmartLock)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        foreach (var @lock in locks)
        {
            SendLockUpdateToBlubug(@lock, cancellationToken);
        }
    }

    private async Task<bool> CheckTenantAllowsLockUpdatesAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var tenant = await _context.Tenants.FindAsync(new object[] { tenantId }, cancellationToken: cancellationToken);

        if (tenant is null)
        {
            throw new NotFoundException(tenantId, nameof(Tenant));
        }

        return tenant.AllowLockUpdates;
    }

    private void SendLockUpdateToBlubug(Lock @lock, CancellationToken cancellationToken = default)
    {
        var dto = MapLockToDTO(@lock);

        //var lockUpdate = _context.LockConfigUpdates.Add(...);
        //var lockUpdateId = lockUpdate.Entity.Id;

        var lockSerial = @lock.SerialNumber;

        _ = _blubugService.UpdateLockConfigAsync(@lock.SerialNumber, dto, cancellationToken)
            .ContinueWith(async success =>
            {
                if (await success)
                {
                    _logger.LogInformation("Update reached blubug for lock {lockSerial}", lockSerial);
                    //var found = await _context.Locks.FindAsync(lockUpdateId);

                    //if(found is not null)
                    //{
                    //    _context.Locks.Remove(found);
                    //    await _context.SaveChangesAsync();
                    //}
                }
                else
                {
                    _logger.LogCritical("Update did not reach blubug for lock {lockSerial}", lockSerial);
                    // RetryPushingLockConfig(@lock, _options.LockConfigFailureRetries)
                    // recursively calls self with RetryPushingLockConfig(@lock, retries - 1)
                }
            }, TaskContinuationOptions.RunContinuationsAsynchronously);
    }

    private static UpdateLockConfigDTO MapLockToDTO(Lock @lock)
    {
        var Cards = new List<LockConfigCardDTO>();

        if (@lock.Locker is not null)
        {
            if(!@lock.Locker.CardCredentials.IsNullOrEmpty())
            {
                Cards.AddRange(@lock.Locker.CardCredentials
                    .Where(x => !(x.CardCredential?.SerialNumber.IsNullOrWhiteSpace() ?? true))
                    .Select(x => new LockConfigCardDTO
                    (
                        SerialNumber: x.CardCredential!.SerialNumber!,
                        // When sending the config update, change temporary cards to be user cards so they are accepted by the lock
                        CardType: x.CardCredential!.CardType == CardType.Temporary ? CardType.User : x.CardCredential!.CardType
                    )));
            }

            // In theory lockers should never exist in a state where they
            // are not in a locker bank but anything's possible in this project..!
            if (@lock.Locker.LockerBank is not null)
            {
                Cards.AddRange(
                    @lock.Locker!.LockerBank.SpecialCardCredentials
                    .Where(x => !string.IsNullOrWhiteSpace(x.CardCredential?.SerialNumber))
                    .Select(x => new LockConfigCardDTO
                    (
                        SerialNumber: x.CardCredential!.SerialNumber!,
                        CardType: x.CardCredential!.CardType
                    ))
                );

                Cards.AddRange(
                    @lock.Locker!.LockerBank.UserCardCredentials
                    .Where(x => !string.IsNullOrWhiteSpace(x.CardCredential?.SerialNumber))
                    .Select(x => new LockConfigCardDTO
                    (
                        SerialNumber: x.CardCredential!.SerialNumber!,
                        // When sending the config update, change temporary cards to be user cards so they are accepted by the lock
                        CardType: x.CardCredential!.CardType == CardType.Temporary ? CardType.User : x.CardCredential!.CardType
                    ))
                );
            }
        }

        return new()
        {
            OperatingMode = @lock.OperatingMode,
            Cards = Cards,
        };
    }

    // TODO: Write these if / when needed
    public Task ResetAllLockConfigsToFactorySettingsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task RefreshAllLockConfigsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> IsLockUpdatePendingAsync(Guid tenantId, Guid lockId, CancellationToken cancellationToken = default)
    {
        var @lock = await _context.Locks.FindAsync(new object[] { lockId }, cancellationToken: cancellationToken);

        if (@lock is null || @lock.TenantId != tenantId)
        {
            throw new NotFoundException(lockId, "Lock");
        }

        var lockConfig = await _blubugService.GetLockConfigAsync(@lock.SerialNumber, cancellationToken);

        return lockConfig?.Pending is not null;
    }

    public async Task<bool> IsLockUpdatePendingByLockerAsync(Guid tenantId, Guid lockerId, CancellationToken cancellationToken = default)
    {
        var locker = await _context.Lockers
            .ByTenant(tenantId)
            .Include(x => x.Lock)
            .FirstOrDefaultAsync(x => x.Id == lockerId) ?? throw new NotFoundException(lockerId, "Lock");

        if (locker.Lock is null)
        {
            return false;
        }

        var lockConfig = await _blubugService.GetLockConfigAsync(locker.Lock.SerialNumber, cancellationToken);

        return lockConfig?.Pending is not null;
    }
}
