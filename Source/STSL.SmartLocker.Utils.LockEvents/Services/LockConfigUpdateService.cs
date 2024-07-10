using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using STSL.SmartLocker.Utils.Common.Data;
using STSL.SmartLocker.Utils.Common.Helpers;
using STSL.SmartLocker.Utils.Data.Contexts;
using STSL.SmartLocker.Utils.Domain;
using STSL.SmartLocker.Utils.LockEvents.Contracts;
using STSL.SmartLocker.Utils.LockEvents.DTO;
using STSL.SmartLocker.Utils.Messages;

namespace STSL.SmartLocker.Utils.LockEvents.Services;

public sealed class LockConfigUpdateService : ILockConfigUpdateService
{
    private readonly SmartLockerDbContext _dbContext;
    private readonly ILogger<LockConfigUpdateService> _logger;

    public LockConfigUpdateService(SmartLockerDbContext dbContext, ILogger<LockConfigUpdateService> logger)
        => (_dbContext, _logger) = (dbContext, logger);

    public async Task HandleUpdatesFromParsedMessageAsync(BluBugMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            var auditType = (LockAuditType?)message.AuditTypeCode ?? LockAuditType.None;
            if (!CouldTypeCodeAffectDatabase(auditType) || message.AuditSubject.IsNullOrWhiteSpace())
            {
                return;
            }

            // Verify required dto data & dispatch to event handler functions
            if (auditType == LockAuditType.LockedByUserCard)
            {
                await HandleLockerLeasedEventAsync(new
                (
                    LockSerial: new(message.OriginAddress),
                    CardSerial: message.AuditSubject,
                    ServerTimeStamp: message.ServerTimestamp,
                    LockTimeStamp: message.OriginTimestamp
                ), cancellationToken);
            }
            else if (auditType == LockAuditType.OpenedByUserCard)
            {
                await HandleLockerLeaseEndedEventAsync(new
                (
                    LockSerial: new(message.OriginAddress),
                    CardSerial: message.AuditSubject,
                    ServerTimeStamp: message.ServerTimestamp,
                    LockTimeStamp: message.OriginTimestamp
                ), cancellationToken);
            }
            else if (auditType == LockAuditType.OpenedByBrokenOpen)
            {
                await HandleLockerLeaseEndedByBrokenOpenEventAsync(new
                (
                    LockSerial: new(message.OriginAddress),
                    ServerTimeStamp: message.ServerTimestamp,
                    LockTimeStamp: message.OriginTimestamp
                ), cancellationToken);
            }
            else if (auditType == LockAuditType.OpenedByMasterCard && !message.AuditObject.IsNullOrWhiteSpace())
            {
                await HandleLockerLeaseEndedByMasterCardEventAsync(new
                (
                    LockSerial: new(message.OriginAddress),
                    LeaseCardSerial: message.AuditObject,
                    MasterCardSerial: message.AuditSubject,
                    ServerTimeStamp: message.ServerTimestamp,
                    LockTimeStamp: message.OriginTimestamp
                ), cancellationToken);
            }
        }
        catch (Exception exception)
        {
            // TODO: Have a table of the messageIds that need to be handled / retried. Just before the SaveChanges above we attempt to mark the message as complete / remove it from queue
            _logger.LogError(exception, "An exception occurred in HandleUpdatesFromParsedMessagesAsync while processing message");
        }
    }

    private async Task HandleLockerLeasedEventAsync(LockerLeasedEventDTO message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to handle locker leased event");

        var cardCredential = await _dbContext.CardCredentials
            .Include(x => x.CardHolder)
            .Where(x => x.SerialNumber == message.CardSerial)
            .FirstOrDefaultAsync(cancellationToken);

        // Card credential is not in our system.. this should never happen unless our data is not in sync or a manual update has been made to a lock
        if (cardCredential is null)
        {
            _logger.LogError("Card credential is not in our database yet it was used to access a lock that is!\nLock serial: {lockSerial}\nCard serial: {cardSerial}", message.LockSerial.Value, message.CardSerial);
            return;
        }

        var @lock = await _dbContext.Locks
            .Include(x => x.Locker)
            .ThenInclude(x => x!.LockerBank)
            .Include(x => x.Locker)
            .ThenInclude(x => x!.CurrentLease)
            .Include(x => x.Locker)
            .ThenInclude(x => x!.LeaseHistory!.Where(y => y.StartedAt == null || y.EndedAt == null))
            .Where(x => message.LockSerial == x.SerialNumber)
            .FirstOrDefaultAsync(cancellationToken);

        // Lock is not in our database, could be a test lock or the lock is not in a temporary / shift bank so we don't care.
        if (@lock is null || @lock.Locker?.LockerBank?.Behaviour != LockerBankBehaviour.Temporary)
        {
            _logger.LogInformation("The lock was null or the locker bank didn't have a temporary behaviour");

            return;
        }

        // TODO: Check the timestamps and whether there is already a lease on the locker.
        // We need to store a lease in the LockerLease table even if the locker already has a lease currently
        // since messages do not necessarily arrive in the exact order.
        // Therefore we should create a lease on every lock event.
        // Every unlock event should find the matching lease and end it as well as finding the latest unfinished lease for the locker.
        // This will ensure correct state where we get messages in the order
        // 1: person A starts lease at 12:00 on lock 1 -- locker state: leased by person A (locker was vacant)
        // 2: person B starts lease at 12:31 on lock 1 -- locker state: leased by person B (B lease started later than current lease A)
        // 3: person A ends lease at 12:30 on lock 1 -- locker state: leased by person B (found unfinished LockerLease by A, finish it)
        // 4: person B ends lease at 12:50 on lock 1 -- locker state: vacant (found unfinished LockerLease by B, finish it, is current lease so set to vacant)

        // User locked the locker, they are now the lease holder
        //@lock.Locker.LeasedByCardHolderId = cardCredential.CardHolderId;
        //@lock.Locker.LeasedByCardCredentialId = cardCredential.Id;
        var leaseBeganAt = message.LockTimeStamp ?? message.ServerTimeStamp;
        var shouldCreateNewLease = false;

        var leaseHistoryWithEndButNoStart = @lock.Locker.LeaseHistory?.Where(y => y.StartedAt == null).OrderBy(x => x.EndedAt?.UtcTicks ?? 0).ToList();

        if (leaseHistoryWithEndButNoStart.IsNullOrEmpty())
        {
            shouldCreateNewLease = true;
        }
        else
        {
            //leaseHistoryWithEndButNoStart.RemoveFirst(y => y.Id == @lock.Locker.CurrentLeaseId);

            // TODO: Check lease history contains a lease that has ended but not started with a matching card credential
            // if so, match up the lease and set currentLeaseId to the latest lease in leaseHistory.
            // Otherwise create a new lease as below
            var earliestLeaseWithMatchingCard = leaseHistoryWithEndButNoStart.Find(x => x.CardCredentialId == cardCredential.Id && x.EndedAt >= leaseBeganAt);

            if (earliestLeaseWithMatchingCard is null)
            {
                shouldCreateNewLease = true;
            }
            else
            {
                earliestLeaseWithMatchingCard.StartedAt = leaseBeganAt;
            }
        }

        if (shouldCreateNewLease)
        {
            // This lease is a lease with a start but no end, add it to the history; we will eventually
            // get a lease end event that matches up
            LockerLease newLockerLease = new()
            {
                Id = Guid.NewGuid(),
                TenantId = @lock.TenantId,
                StartedAt = leaseBeganAt,
                LockerBankBehaviour = LockerBankBehaviour.Temporary,
                CardCredentialId = cardCredential.Id,
                CardHolderId = cardCredential.CardHolderId,
                LockerId = @lock.LockerId,
                LockId = @lock.Id,
            };

            _dbContext.LockerLeases.Add(newLockerLease);

            if (@lock.Locker.CurrentLeaseId is null || (@lock.Locker.CurrentLease?.StartedAt?.UtcTicks ?? 0) < leaseBeganAt.UtcTicks)
            {
                @lock.Locker.CurrentLeaseId = newLockerLease.Id;
            }
        }
        else if(@lock.Locker.CurrentLeaseId is null)
        {
            var leaseHistoryWithStartButNoEnd = @lock.Locker.LeaseHistory?.Where(y => y.EndedAt == null).OrderBy(x => x.StartedAt?.UtcTicks ?? 0).ToList();

            var latestLease = leaseHistoryWithStartButNoEnd.IsNullOrEmpty() ? null : leaseHistoryWithStartButNoEnd[^1];

            @lock.Locker.CurrentLeaseId = latestLease?.Id;
        }


        _logger.LogInformation("Locker locked. Lease started.\n" +
            "Lock Serial: {lockSerial}\n" +
            "Card Serial: {cardSerial}\n" +
            "Locker Label: {lockerLabel}\n" +
            "Locker Service Tag: {lockerServiceTag}\n" +
            "Timestamp: {timestamp}",
            message.LockSerial.Value,
            message.CardSerial,
            @lock.Locker.Label,
            @lock.Locker.ServiceTag,
            message.LockTimeStamp ?? message.ServerTimeStamp);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task HandleLockerLeaseEndedEventAsync(LockerLeaseEndedEventDTO message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to handle locker lease ended event");

        await EndLockerLeaseAsync(message.LockSerial, message.LockTimeStamp ?? message.ServerTimeStamp, message.CardSerial, cancellationToken);

        _logger.LogInformation("Locker unlocked. Lease ended.\n" +
            "Lock Serial: {lockSerial}\n" +
            "Card Serial: {cardSerial}\n" +
            "Timestamp: {timestamp}",
            message.LockSerial.Value,
            message.CardSerial,
            message.LockTimeStamp ?? message.ServerTimeStamp);
    }

    private async Task HandleLockerLeaseEndedByBrokenOpenEventAsync(LockerLeaseEndedByBrokenOpenEventDTO message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to handle locker lease ended by broken open event");

        await EndLockerLeaseAsync(message.LockSerial, message.LockTimeStamp ?? message.ServerTimeStamp, cancellationToken: cancellationToken);

        _logger.LogInformation("Locker unlocked by broken open. Lease ended.\n" +
            "Lock Serial: {lockSerial}\n" +
            "Timestamp: {timestamp}",
            message.LockSerial.Value,
            message.LockTimeStamp ?? message.ServerTimeStamp);
    }

    private async Task HandleLockerLeaseEndedByMasterCardEventAsync(LockerLeaseEndedByMasterCardEventDTO message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to handle locker lease ended by master card event");

        await EndLockerLeaseAsync(message.LockSerial, message.LockTimeStamp ?? message.ServerTimeStamp, message.LeaseCardSerial, cancellationToken);

        _logger.LogInformation("Locker unlocked by master card. Lease ended.\n" +
            "Lock Serial: {lockSerial}\n" +
            "Lease Card Serial: {cardSerial}\n" +
            "Master Card Serial: {cardSerial}\n" +
            "Timestamp: {timestamp}",
            message.LockSerial.Value,
            message.LeaseCardSerial,
            message.MasterCardSerial,
            message.LockTimeStamp ?? message.ServerTimeStamp);
    }

    private async Task EndLockerLeaseAsync(LockSerial lockSerial, DateTimeOffset leaseEndTimeStamp, string? cardSerial = null, CancellationToken cancellationToken = default)
    {
        var cardCredential = cardSerial is null ? null : await _dbContext.CardCredentials
            .Include(x => x.CardHolder)
            .Where(x => x.SerialNumber == cardSerial)
            .FirstOrDefaultAsync(cancellationToken);

        // Card credential is not in our system.. this should never happen unless our data is not in sync or a manual update has been made to a lock
        if (cardSerial is not null && cardCredential is null)
        {
            _logger.LogError("Card credential is not in our database, perhaps it was deleted!\n" +
                "Lock serial: {lockSerial}\n" +
                "Card serial: {cardSerial}",
                lockSerial,
                cardSerial);
            return;
        }

        // LeaseHistory will contain leases that haven't ended yet (in an ideal world, that would only be the currentLease)
        // but since messages aren't guaranteed to arrive in order we must check for other unfinished leases in case
        // the current lease does not match up to the correct user.
        var @lock = await _dbContext.Locks
            .Include(x => x.Locker)
            .ThenInclude(x => x!.LockerBank)
            .Include(x => x.Locker)
            .ThenInclude(x => x!.CurrentLease)
            .Include(x => x.Locker)
            .ThenInclude(x => x!.LeaseHistory!.Where(y => y.EndedAt == null))
            .Where(x => lockSerial == x.SerialNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (@lock is null || @lock.Locker?.LockerBank?.Behaviour != LockerBankBehaviour.Temporary)
        {
            _logger.LogInformation("The lock was null or the locker bank didn't have a temporary behaviour");

            return;
        }

        var leaseHistoryWithStartButNoEnd = @lock.Locker.LeaseHistory?.OrderBy(x => x.StartedAt?.ToUnixTimeMilliseconds() ?? 0).ToList();

        //leaseHistoryWithStartButNoEnd?.RemoveFirst(y => y.Id == @lock.Locker.CurrentLeaseId);

        // CurrentLease Id doesn't match this card; there could be unfinished leases in the history that do match
        var earliestLeaseWithMatchingCard = cardSerial is null ? 
            leaseHistoryWithStartButNoEnd?.FirstOrDefault() 
            : 
            leaseHistoryWithStartButNoEnd?.Find(x => x.CardCredentialId == cardCredential?.Id && x.StartedAt < leaseEndTimeStamp);

        if (earliestLeaseWithMatchingCard is null)
        {
            // This lease is a lease with an end but no start, add it to the history; we will eventually
            // hopefully get a lease start event that matches up
            LockerLease newLockerLease = new()
            {
                Id = Guid.NewGuid(),
                TenantId = @lock.TenantId,
                EndedAt = leaseEndTimeStamp,
                EndedByMasterCard = true,
                LockerBankBehaviour = LockerBankBehaviour.Temporary,
                CardCredentialId = cardCredential?.Id,
                CardHolderId = cardCredential?.CardHolderId,
                LockerId = @lock.LockerId,
                LockId = @lock.Id,
            };

            _dbContext.LockerLeases.Add(newLockerLease);
        }
        else
        {
            earliestLeaseWithMatchingCard.EndedAt = leaseEndTimeStamp;
            leaseHistoryWithStartButNoEnd?.RemoveFirst(x => x.Id == earliestLeaseWithMatchingCard.Id);
        }

        // Do we do a check here to find out if we have a more recently started lease in history and reassign the current lease to it?
        // Something like this
        
        var latestLease = leaseHistoryWithStartButNoEnd.IsNullOrEmpty() ? null : leaseHistoryWithStartButNoEnd[^1];
        //if (@lock.Locker.CurrentLeaseId == null || (@lock.Locker.CurrentLease?.StartedAt?.UtcTicks ?? 0) < (latestLease?.StartedAt?.UtcTicks ?? 0))
        //{
        // Set the CurrentLeaseId for the locker to be the latest lease. Should be null if no other unfinished leases
        @lock.Locker.CurrentLeaseId = latestLease?.Id;
        //}

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static bool CouldTypeCodeAffectDatabase(LockAuditType? lockAuditType) => lockAuditType switch
    {
        LockAuditType.LockedByUserCard or
        LockAuditType.OpenedByUserCard or 
        LockAuditType.OpenedByBrokenOpen or
        LockAuditType.OpenedByMasterCard => true,
        // TODO: Add welcome card types and handler functions
        _ => false
    };
}
