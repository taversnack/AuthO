using Microsoft.EntityFrameworkCore;
using STSL.SmartLocker.Utils.AzureServiceBus.Contracts;
using STSL.SmartLocker.Utils.Common.Data;
using STSL.SmartLocker.Utils.Common.Enum;
using STSL.SmartLocker.Utils.Common.Exceptions;
using STSL.SmartLocker.Utils.Common.Helpers;
using STSL.SmartLocker.Utils.Data.Contexts;
using STSL.SmartLocker.Utils.Data.Extensions;
using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.Data.Services.Contracts.Kiosk;
using STSL.SmartLocker.Utils.Data.Services.Mappings;
using STSL.SmartLocker.Utils.Domain;
using STSL.SmartLocker.Utils.Domain.Kiosk;
using STSL.SmartLocker.Utils.DTO;
using STSL.SmartLocker.Utils.DTO.AzureServiceBus;
using STSL.SmartLocker.Utils.DTO.Kiosk;
using STSL.SmartLocker.Utils.Kiosk.Printer.Contracts;

namespace STSL.SmartLocker.Utils.Data.Services.Kiosk;

public class KioskService : IKioskService
{
    private readonly IRepository<KioskAccessCode> _repository;
    private readonly SmartLockerDbContext _dbContext;
    private readonly IAzureServiceBusService _azureServiceBusService;
    private readonly IEmailService _emailService;

    public KioskService(
        IRepository<KioskAccessCode> repository,
        SmartLockerDbContext dbContext,
        IEmailService emailService,
        IAzureServiceBusService azureServiceBusService)
        => (_repository, _dbContext, _emailService, _azureServiceBusService)
        = (repository, dbContext, emailService, azureServiceBusService);

    #region Public Functions

    /// <summary>
    /// Sends a message to the Service Bus.
    /// This will need to be processed by service worker to pass this check
    /// Performs a keep alive or login action through service worker
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>
    /// Boolean which indicates successful request & response
    /// from the service worker through service bus
    /// </returns>
    public async Task<bool> AuthenticateServiceWorkerAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Create a CancellationTokenSource that will be canceled after 10 seconds
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(10));

            var request = new AzureServiceBusAuthenticationDTO();

            var success = await _azureServiceBusService.PublishMessageAndGetSuccessResponseAsync(request, cts.Token);
            return success;
        }
        catch (OperationCanceledException)
        {
            // timed out 
            return false;
        }
    }

    public async Task CreateRecoveryCodeRequestAsync(Guid tenantId, CardHolderDTO cardHolder, CancellationToken cancellationToken = default)
    {
        var accessCode = GenerateAccessCode();

        await SendAccessCodeByEmailAsync(cardHolder, accessCode);

        await CreateAccessCodeAsync(tenantId, cardHolder.Id, accessCode, cancellationToken);

        return;
    }

    public async Task UpdateLockerWithCredentialsAndCreateLeaseAsync(Guid tenantId, Guid lockerId, Guid cardCredentialId, Guid cardHolderId, CancellationToken cancellationToken = default)
    {
        // Get Locker
        var locker = await _dbContext.Lockers
            .ByTenant(tenantId)
            .Include(x => x.Lock)
            .Include(x => x.PermanentOwners)
            .Include(x => x.CardCredentials)
            .FirstOrDefaultAsync(x => x.Id == lockerId, cancellationToken)
            ?? throw new NotFoundException(lockerId, "Locker");

        if (locker.SecurityType != LockerSecurityType.SmartLock)
        {
            throw new BadRequestException("Cannot assign card credentials to a non smart locker");
        }

        // End old lease & create new lease
        await EndAndCreateLockerLeaseAsync(tenantId, locker, cardCredentialId, cardHolderId, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task CreateKioskLockerAssignmentAsync(Guid tenantId, Guid lockerId, Guid temporaryCardCredentialId, Guid cardHolderId, CancellationToken cancellationToken = default)
    {
        var assignmentStartTime = DateTimeOffset.UtcNow;

        // Get current locker lease
        var currentLease = await _dbContext.LockerLeases
            .ByTenant(tenantId)
            .Where(x => x.LockerId == lockerId)
            .Where(x => x.CardHolderId == cardHolderId)
            .Where(x => x.EndedAt == null)
            .FirstAsync(cancellationToken);

        _dbContext.KioskLockerAssignments.Add(new KioskLockerAssignment
        {
            TenantId = tenantId,
            LockerId = lockerId,
            CardHolderId = cardHolderId,
            TemporaryCardCredentialId = temporaryCardCredentialId,
            ReplacedCardCredentialId = currentLease.CardCredentialId, // store replaced credential for reactivation
            AssignmentDate = assignmentStartTime,
            IsTemporaryCardActive = true
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task EndActiveAssignmentForTemporaryCardAsync(Guid tenantId, Guid kioskAssignentId, CancellationToken cancellationToken = default)
    {
        var kioskAssignment = await _dbContext.KioskLockerAssignments
            .ByTenant(tenantId)
            .Where(x => x.Id == kioskAssignentId)
            .FirstOrDefaultAsync(cancellationToken);

        if (kioskAssignment is not null)
        {
            kioskAssignment.IsTemporaryCardActive = false;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<KioskLockerAssignentDTO>> GetActiveAssignmentsForTemporaryCardOwnerAsync(Guid tenantId, Guid cardHolderId, Guid temporaryCardCredentialId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.KioskLockerAssignments
            .ByTenant(tenantId)
            .Where(x => x.CardHolderId == cardHolderId)
            .Where(x => x.TemporaryCardCredentialId == temporaryCardCredentialId)
            .Where(x => x.IsTemporaryCardActive)
            .Select(x => KioskLockerAssignmentMapper.ToDTO(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> PublishCreateTemporaryCardToAzureServiceBusAsync(Guid tenantId, CreateCardCredentialDTO temporaryCard, CancellationToken cancellationToken = default)
    {
        var cardHolderWithCredentials = await _dbContext.CardHolders
            .ByTenant(tenantId)
            .Include(cardHolder => cardHolder.CardCredentials)
            .FirstOrDefaultAsync(cardHolder => cardHolder.Id == temporaryCard.CardHolderId, cancellationToken);

        var currentCardCredentials = cardHolderWithCredentials?.CardCredentials?
            .FirstOrDefault(credentials => credentials.CardType != CardType.Temporary && credentials.CardType != CardType.Master);

        if (currentCardCredentials == null)
        {
            return false;
        }

        var azureCreateTemporaryCardMessage = new AzureServiceBusCreateTemporaryCardDTO
        {
            CardHolderId = Convert.ToInt32(cardHolderWithCredentials?.UniqueIdentifier),
            CurrentCardCredentials = ConvertValuesToValidCardCredentials(currentCardCredentials.HidNumber, currentCardCredentials.SerialNumber),
            TemporaryCardCredentials = ConvertValuesToValidCardCredentials(temporaryCard.HidNumber, temporaryCard.SerialNumber),
        };

        return await _azureServiceBusService.PublishMessageAndGetSuccessResponseAsync(azureCreateTemporaryCardMessage, cancellationToken);
    }

    public async Task<bool> PublishReturnTemporaryCardToAzureServiceBusAsync(Guid tenantId, UpdateCardCredentialDTO temporaryCard, CancellationToken cancellationToken = default)
    {
        var azureReturnTemporaryCardMessage = new AzureServiceBusReturnTemporaryCardDTO
        {
            TemporaryCardCredentials = ConvertValuesToValidCardCredentials(temporaryCard.HidNumber, temporaryCard.SerialNumber),
        };

        return await _azureServiceBusService.PublishMessageAndGetSuccessResponseAsync(azureReturnTemporaryCardMessage, cancellationToken);
    }

    public async Task<KioskAccessCodeDTO?> SubmitAccessCodeAsync(Guid tenantId, AccessCodeDTO accessCode, CancellationToken cancellationToken = default)
    {
        var entity = await _repository
           .QueryingAll(tenantId)
           .Where(code => code.AccessCode == accessCode.Code)
           .Where(code => code.ExpiryDate > DateTime.UtcNow)
           .FirstOrDefaultAsync(code => !code.HasBeenUsed, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        entity.HasBeenUsed = true;

        await _repository.UpdateOneAsync(tenantId, entity.Id, entity, cancellationToken);

        return KioskAccessCodeMapper.ToDTO(entity);
    }

    public async Task<KioskAccessCodeDTO?> GetAccessCodeByCardHolderIdAsync(Guid tenantId, Guid cardHolderId, CancellationToken cancellationToken = default)
    {
        var entity = await _repository
            .QueryingAll(tenantId)
            .Where(code => code.CardHolderId == cardHolderId)
            .Where(code => code.ExpiryDate > DateTime.UtcNow)
            .FirstOrDefaultAsync(code => !code.HasBeenUsed, cancellationToken);

        return entity is null ? null : KioskAccessCodeMapper.ToDTO(entity);
    }

    #endregion Public Functions

    #region Private Functions

    private async Task EndAndCreateLockerLeaseAsync(Guid tenantId, Locker locker, Guid cardCredentialId, Guid cardHolderId, CancellationToken cancellationToken)
    {
        var leaseStartOrEndTime = DateTimeOffset.UtcNow;
        var lockerOwner = locker.PermanentOwners?.Where(x => x.CardHolderId == cardHolderId).FirstOrDefault();

        if (lockerOwner is not null)
        {
            // Get the lease for the locker
            var ownerLease = await _dbContext.LockerLeases
                .ByTenant(tenantId)
                .Where(x => x.LockerId == locker.Id)
                .Where(x => x.EndedAt == null)
                .Where(x => x.CardHolderId == lockerOwner.CardHolderId)
                .FirstOrDefaultAsync(cancellationToken);

            if (ownerLease is not null)
            {
                // End old lease 
                ownerLease.EndedAt = leaseStartOrEndTime;
            }

            // Create new lease
            _dbContext.LockerLeases.Add(new LockerLease
            {
                TenantId = tenantId,
                StartedAt = leaseStartOrEndTime,
                LockerBankBehaviour = LockerBankBehaviour.Permanent,
                CardCredentialId = cardCredentialId,
                CardHolderId = lockerOwner.CardHolderId,
                LockerId = locker.Id,
                LockId = locker.Lock?.Id,
            });
        }

        if (locker.CardCredentials is not null)
        {
            _dbContext.LockerCardCredentials.RemoveRange(locker.CardCredentials);
        }

        _dbContext.LockerCardCredentials.Add(new LockerCardCredential
        {
            TenantId = tenantId,
            LockerId = locker.Id,
            CardCredentialId = cardCredentialId
        });
    }

    private string OptTemplate(string accessCode, string firstName) => $@"
                Hi {firstName}, <br /><br />
                You have requested a temporary access code for your control card. Here is your one-time code: <strong>{accessCode}</strong> <br />
                Please note that this code will expire in 60 minutes. <br />
                If you did not request this code, please go to your nearest security team: <br />
                <ul>
                    <li>Queen Square, Albany Wing Security Desk</li>
                    <li>UCH Security Hub UCH Atrium</li>
                    <li>ID Team 250ER Ground Floor East Wing (Opposite Lifts)</li>
                </ul>
                <br />
                Regards, <br />
                GoToSecure";

    private async Task SendAccessCodeByEmailAsync(CardHolderDTO cardHolder, string accessCode)
    {

        var options = new EmailRequestDTO()
        {
            HtmlContent = OptTemplate(accessCode, cardHolder.FirstName),
            Subject = accessCode,
            ToEmail = cardHolder.Email,
            EmailType = EmailType.OneTimePasswordFromAddress
        };
        await _emailService.SendNotificationEmailAsync(options);
    }

    private async Task CreateAccessCodeAsync(Guid tenantId, Guid cardHolderId, string accessCode, CancellationToken cancellationToken = default)
    {
        var entity = new KioskAccessCode
        {
            AccessCode = accessCode,
            ExpiryDate = DateTime.UtcNow.AddMinutes(60),
            CardHolderId = cardHolderId
        };

        _ = await _repository.CreateOneAsync(tenantId, entity, cancellationToken) ?? throw new InvalidOperationException("Failed to create a new access code");
    }

    private static string GenerateAccessCode()
    {
        const int accessCodeLength = 6;

        string guidString = Guid.NewGuid().ToString("N").ToUpper();

        return guidString[..Math.Min(accessCodeLength, guidString.Length)];
    }

    private static AzureServiceBusCardCredentials ConvertValuesToValidCardCredentials(string hidNumber, string serialNumber)
    {
        var hidNumberDecimal = decimal.Parse(hidNumber);
        var serialNumberLong = serialNumber.ReverseHexStringEndiannessAndConvertToLong();

        return new(hidNumberDecimal, serialNumberLong);
    }

    public async Task InitializeKioskAsync(Guid tenantId, Guid kioskId, string kioskName, Guid locationId)
    {

        // check kiosk exists (create if not)
        var kiosk = await _dbContext.Kiosks.FindAsync(kioskId);
        if (kiosk == null)
        {
            await _dbContext.Kiosks.AddAsync(new Kiosks
            {
                KioskId = kioskId,
                Name = kioskName,
                TenantId = tenantId,
                LocationId = locationId
            });
        }
        else
        {
            if (kiosk.Name != kioskName)
            {
                kiosk.Name = kioskName;
            }
            kiosk.LocationId = locationId;
        }

        await _dbContext.SaveChangesAsync();

    }
    #endregion Private Functions
}