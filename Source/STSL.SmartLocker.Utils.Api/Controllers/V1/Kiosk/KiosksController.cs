using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using STSL.SmartLocker.Utils.Api.Auth;
using STSL.SmartLocker.Utils.Common.Enum;
using STSL.SmartLocker.Utils.Data.Services.Contracts;
using STSL.SmartLocker.Utils.Data.Services.Contracts.Kiosk;
using STSL.SmartLocker.Utils.DTO;
using STSL.SmartLocker.Utils.DTO.Kiosk;
using STSL.SmartLocker.Utils.Kiosk.Printer.Contracts;

namespace STSL.SmartLocker.Utils.Api.Controllers.V1.Kiosk;

[ApiVersion(V1)]
public class KiosksController : KioskControllerBase
{
    private readonly ICardCredentialService _cardCredentialService;
    private readonly ICardHolderService _cardHolderService;
    private readonly ILockConfigService _lockConfigService;
    private readonly ILockConfigAuditService _lockConfigAuditService;
    private readonly IKioskService _kioskService;
    private readonly IEmailService _emailService;
    private readonly ILogger<KiosksController> _logger;

    public KiosksController(
        ICardCredentialService cardCredentialService,
        ICardHolderService cardHolderService,
        ILockConfigService lockConfigService,
        ILockConfigAuditService lockConfigAuditService,
        IEmailService emailService,
        IKioskService kioskService)
        => (_cardCredentialService, _cardHolderService, _lockConfigService, _lockConfigAuditService, _kioskService, _emailService)
        = (cardCredentialService, cardHolderService, lockConfigService, lockConfigAuditService, kioskService, emailService);

    [HttpPost("report-problem")]
    [Authorize]
    public async Task<ActionResult> ReportPrinterProblem([FromBody] ErrorType errorType)
    {
        try
        {
            await _emailService.SendErrorEmailAsync(errorType);
            return Ok("Successfully sent!");
        }
        catch
        {
            return Problem("Encountered one or more errors while sending the email!");
        }
    }

    /// <summary>
    /// Posts a request to recieve a one-time code via email
    /// </summary>
    /// <param name="dto">DTO with email and request time</param>
    [HttpPost("access-code-request")]
    [Authorize(Permissions.ReadCardHolders)]
    public async Task<ActionResult> PostAccessCodeRequest(CreateKioskAccessCodeDTO dto)
    {
        var tenantId = CheckClientClaims().TenantId;

        var cardHolder = await _cardHolderService.GetCardHolderByEmailAsync(tenantId, dto.Email);
        if (cardHolder is null)
        {
            return NotFound("The provided email is not recognised. Please try again");
        }

        var existingAccessCode = await _kioskService.GetAccessCodeByCardHolderIdAsync(tenantId, cardHolder.Id);
        if (existingAccessCode is not null)
        {
            return Conflict("A valid access code has already been sent to this email, it may take a few minutes to appear");
        }

        await _kioskService.CreateRecoveryCodeRequestAsync(tenantId, cardHolder);

        return Ok();
    }

    /// <summary>
    /// Creates a temporary card
    /// Temporary card gets assigned to card holder
    /// Message gets sent to Azure Service Bus to update access control system
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("card-request")]
    [Authorize(Permissions.CreateCardCredentials)]
    public async Task<ActionResult> PostTemporaryCard(CreateCardCredentialDTO dto)
    {
        var tenantId = CheckClientClaims().TenantId;

        var response = await _kioskService.PublishCreateTemporaryCardToAzureServiceBusAsync(tenantId, dto);
        if (!response)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to update access control system");
        }

        var credentials = await _cardCredentialService.GetCardCredentialByHidNumberAsync(tenantId, dto.HidNumber)
                   ?? await _cardCredentialService.CreateCardCredentialAsync(tenantId, dto);

        if (credentials is null)
        {
            return UnprocessableEntity("Unable to create or retrieve card credentials");
        }

        if (credentials.CardHolderId is null && dto.CardHolderId.HasValue)
        {
            // If temporary card already exists, it should be unassigned at this point
            credentials = await _cardCredentialService.AssignExistingTemporaryCardAsync(tenantId, credentials.Id, dto.CardHolderId.Value);
        }

        if (credentials.CardHolderId is not null)
        {
            var lockers = await _cardHolderService.GetManyLockersForCardHolderAsync(tenantId, credentials.CardHolderId.Value);

            if (lockers is null)
            {
                return Ok(); // skip locker assignment if they have no locker
            }

            // Update lockers to use temp card
            foreach (var locker in lockers)
            {
                await _kioskService.CreateKioskLockerAssignmentAsync(tenantId, locker.Id, credentials.Id, dto.CardHolderId!.Value);

                await _kioskService.UpdateLockerWithCredentialsAndCreateLeaseAsync(tenantId, locker.Id, credentials.Id, dto.CardHolderId!.Value);

                await LogAndUpdateSmartLockConfigsForLockerAsync(tenantId, locker.Id, "Credentials replaced by temporary card");
            }
        }
        return Ok();
    }

    ///<summary>
    /// Submits an access code which gets marked as used if valid
    /// </summary>
    /// <param name="dto">DTO containing One-Time code</param>
    [HttpPost("post-access-code")]
    [Authorize(Permissions.ReadCardHolders)]
    public async Task<ActionResult<KioskAccessCodeDTO>> PostAccessCode(AccessCodeDTO dto)
    {
        var tenantId = CheckClientClaims().TenantId;

        var accessCode = await _kioskService.SubmitAccessCodeAsync(tenantId, dto);

        return accessCode is null ? NotFound("Invalid code provided. Please try again or request a new code") : accessCode;
    }

    /// <summary>
    /// Returns a temporary card
    /// Temporary card gets unassigned from card holder
    /// Message gets sent to Azure Service Bus to update access control system
    /// </summary>
    /// <param name="dto">DTO to update card credentials with CardHolderId of null</param>
    [HttpPost("return-card")]
    [Authorize(Permissions.UpdateCardCredentials)]
    public async Task<ActionResult> PostReturnCard(UpdateCardCredentialDTO dto)
    {
        var tenantId = CheckClientClaims().TenantId;

        var credential = await _cardCredentialService.GetCardCredentialByHidNumberAsync(tenantId, dto.HidNumber);
        if (credential is null)
        {
            return NotFound("Unable to retrieve card credentials");
        }

        if (!await _kioskService.PublishReturnTemporaryCardToAzureServiceBusAsync(tenantId, dto))
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to update access control system");
        }

        // if the temporary card is assigned then get their lockers
        if (credential.CardHolderId is not null)
        {
            var assignments = await _kioskService.GetActiveAssignmentsForTemporaryCardOwnerAsync(tenantId, credential.CardHolderId.Value, credential.Id);

            if (assignments is null)
            {
                // No temporary card assignments
                return Ok();
            }

            foreach (var assignment in assignments)
            {
                // Re-activate user cards for locker and create new leases
                await _kioskService.EndActiveAssignmentForTemporaryCardAsync(tenantId, assignment.Id);

                await _kioskService.UpdateLockerWithCredentialsAndCreateLeaseAsync(tenantId, assignment.LockerId!.Value, assignment.ReplacedCardCredentialId!.Value, assignment.CardHolderId!.Value);

                await LogAndUpdateSmartLockConfigsForLockerAsync(tenantId, assignment.LockerId.Value, "Credentials replaced from temporary card ");
            }

            // Unassign temporary card
            await _cardCredentialService.UpdateCardCredentialAsync(tenantId, credential.Id, dto);
        }

        return Ok();
    }

    /// <summary>
    /// Ping the service bus and the victor web service. Attempt to login to the VWS API.
    /// </summary>
    /// <returns>True if login is successful, false if service bus or login fail</returns>
    [HttpGet("authenticate")]
    public async Task<ActionResult> GetAuthentication()
    {
        try
        {
            KioskContext kioskContext = CheckClientClaims();

            await _kioskService.InitializeKioskAsync(kioskContext.TenantId, kioskContext.KioskId, kioskContext.KioskName, kioskContext.LocationId);
            _logger.LogInformation($"Kiosk {kioskContext.KioskId} initialized successfully.");
            var isConnected = await _kioskService.AuthenticateServiceWorkerAsync();
            return isConnected ? Ok() : BadRequest();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex.Message);
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing the kiosk.");
            return StatusCode(500, "Internal server error.");
        }

    }


    #region Private Functions
    private async Task LogAndUpdateSmartLockConfigsForLockerAsync(Guid tenantId, Guid lockerid, string? description = null)
    {
        await _lockConfigAuditService.LogLockerConfigChangeAsync(tenantId, string.Empty, lockerid, description);
        await _lockConfigService.UpdateLockConfigByLockerAsync(tenantId, lockerid);
    }
    #endregion Private Functions
}
