using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using STSL.SmartLocker.Utils.Api.Auth;
using STSL.SmartLocker.Utils.Data.Services.Contracts;
using STSL.SmartLocker.Utils.Data.Services.Helpers;
using STSL.SmartLocker.Utils.Domain;
using STSL.SmartLocker.Utils.DTO;

namespace STSL.SmartLocker.Utils.Api.Controllers.V1;

[ApiVersion(V1)]
public sealed class LockerBanksController : MultiTenantControllerBase
{
    private readonly ILockerBankService _lockerBankService;
    private readonly ILockerService _lockerService;
    private readonly ICardCredentialService _cardCredentialService;
    private readonly ICardHolderService _cardHolderService;
    private readonly ILockerBankAdminService _lockerBankAdminService;
    private readonly IMartService _martService;
    private readonly ILockConfigService _lockConfigService;
    private readonly ILockConfigAuditService _lockConfigAuditService;
    private readonly IBulkOperationService _bulkOperationService;
    private readonly IReferenceImageService<LockerBank, LockerBankReferenceImage> _referenceImageService;
    private readonly ILogger<LockerBanksController> _logger;

    public LockerBanksController(
        ILockerBankService lockerBankService,
        ILockerService lockerService,
        ICardCredentialService cardCredentialService,
        ICardHolderService cardHolderService,
        ILockerBankAdminService lockerBankAdminService,
        IMartService martService,
        ILockConfigService lockConfigService,
        ILockConfigAuditService lockConfigAuditService,
        IBulkOperationService bulkOperationService,
        IReferenceImageService<LockerBank, LockerBankReferenceImage> referenceImageService,
        ILogger<LockerBanksController> logger)
        => (_lockerBankService, _lockerService, _cardCredentialService, _cardHolderService, _lockerBankAdminService, _martService, _lockConfigService, _lockConfigAuditService, _bulkOperationService, _referenceImageService, _logger)
        = (lockerBankService, lockerService, cardCredentialService, cardHolderService, lockerBankAdminService, martService, lockConfigService, lockConfigAuditService, bulkOperationService, referenceImageService, logger);

    #region Locker Banks

    /// <summary>
    /// Creates a new locker bank
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    [HttpPost]
    [Authorize(Permissions.CreateLockerBanks)]
    public async Task<ActionResult<LockerBankDTO>> Post(Guid tenantId, CreateLockerBankDTO dto)
    {
        var newLockerBank = await _lockerBankService.CreateLockerBankAsync(tenantId, dto);

        return newLockerBank is null ? StatusCode(StatusCodes.Status500InternalServerError) : CreatedAtAction(nameof(Get), new { tenantId, newLockerBank.Id }, newLockerBank);
    }

    /// <summary>
    /// Returns all locker banks by page
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    [HttpGet]
    [Authorize(Permissions.ReadLockerBanks)]
    public async Task<ActionResult<PagingResponse<LockerBankDTO>>> GetMany(Guid tenantId, [FromQuery] PagingRequest? page, [FromQuery] FilteredRequest? filter, [FromQuery] SortedRequest? sort)
    {
        page ??= new PagingRequest();

        var lockerBanks = await _lockerBankService.GetManyLockerBanksAsync(tenantId, filter, sort);

        return page.ToResponse(lockerBanks);
    }

    /// <summary>
    /// Returns an existing locker bank or 404 (not found)
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="id">The locker bank id</param>
    [HttpGet("{id}")]
    [Authorize(Permissions.ReadLockerBanks)]
    public async Task<ActionResult<LockerBankDTO>> Get(Guid tenantId, Guid id)
    {
        var lockerBank = await _lockerBankService.GetLockerBankAsync(tenantId, id);

        return lockerBank is null ? NotFound() : lockerBank;
    }

    /// <summary>
    /// Fully updates a valid locker bank or 400 with erroneous properties
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="id">The locker bank id</param>
    [HttpPut("{id}")]
    [Authorize(Permissions.UpdateLockerBanks)]
    public async Task<ActionResult> Put(Guid tenantId, Guid id, UpdateLockerBankDTO dto)
    {
        await _lockerBankService.UpdateLockerBankAsync(tenantId, id, dto);

        await LogAndUpdateSmartLockConfigsForLockerBankAsync(tenantId, id, "Locker bank updated");

        return NoContent();
    }

    /// <summary>
    /// Deletes a locker bank if it exists, 404 otherwise
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="id">The locker bank id</param>
    [HttpDelete("{id}")]
    [Authorize(Permissions.DeleteLockerBanks)]
    public async Task<ActionResult> Delete(Guid tenantId, Guid id)
    {
        await _lockerBankService.DeleteLockerBankAsync(tenantId, id);

        return NoContent();
    }

    #endregion Locker Banks

    #region Lockers

    /// <summary>
    /// Bulk create lockers for the locker bank
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="id">The locker bank id</param>
    [HttpPost("{id}/lockers")]
    [Authorize(Permissions.CreateLockers)]
    public async Task<ActionResult> PostManyLockers(Guid tenantId, Guid id, IEnumerable<CreateLockerDTO> dtoList)
    {
        var createdAll = await _lockerService.CreateManyLockersAsync(tenantId, dtoList.Select(x => x with { LockerBankId = id }).ToList());

        return createdAll ? CreatedAtAction(nameof(GetManyLockers), new { tenantId, id }, null) : StatusCode(StatusCodes.Status500InternalServerError);
    }

    /// <summary>
    /// Returns lockers that belong to the locker bank
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="id">The locker bank id</param>
    [HttpGet("{id}/lockers")]
    [Authorize(Permissions.ReadLockers)]
    public async Task<ActionResult<PagingResponse<LockerDTO>>> GetManyLockers(Guid tenantId, Guid id, [FromQuery] PagingRequest? page, [FromQuery] FilteredRequest? filter, [FromQuery] SortedRequest? sort)
    {
        page ??= new PagingRequest();

        var lockers = await _lockerService.GetManyLockersByLockerBankAsync(tenantId, id, filter, sort);

        return page.ToResponse(lockers);
    }

    //[HttpPatch("{id}/lockers")]
    //[Authorize(Permissions.UpdateLockerBanks)]
    //public async Task<ActionResult> PatchManyLockers(Guid tenantId, Guid id, UpdateLockerBankLockersDTO dto)
    //{
    //    await _lockerBankService.UpdateAllLockersInLockerBankAsync(tenantId, id, dto);

    //    return NoContent();
    //}

    [HttpPatch("{id}/lockers")]
    [Authorize(Permissions.UpdateLockerBanks)]
    public async Task<ActionResult> PatchMoveLockersToAnotherLockerBank(Guid tenantId, Guid id, MoveLockersToLockerBankDTO dto)
    {
        await _lockerBankService.MoveManyLockersToAnotherBankAsync(tenantId, origin: id, destination: dto.Destination, lockers: dto.LockerIds);

        // TODO: [10] Only update the relevant lockers, not the whole bank!
        await LogAndUpdateSmartLockConfigsForLockerBankAsync(tenantId, dto.Destination, "Lockers moved to another locker bank");

        return NoContent();
    }

    /// <summary>
    /// Returns lockers status records for a locker bank
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="id">The locker bank id</param>
    [HttpGet("{id}/lockers-status")]
    [Authorize(Permissions.ReadLockers)]
    public async Task<ActionResult<PagingResponse<LockerStatusDTO>>> GetLockersWithStatus(Guid tenantId, Guid id, [FromQuery] PagingRequest? page, [FromQuery] FilteredRequest? filter, [FromQuery] SortedRequest? sort)
    {
        // TODO: Add filtering and sorting

        page ??= new PagingRequest();

        var lockerStatuses = await _martService.GetManyLockersWithStatusForLockerBankAsync(tenantId, id, filter, sort);

        return page.ToResponse(lockerStatuses);
    }

    /// <summary>
    /// Returns lockers and lock pairs that belong to the locker bank
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="id">The locker bank id</param>
    [HttpGet("{id}/lockers-and-locks")]
    [Authorize(Permissions.ReadLockers)]
    [Authorize(Permissions.ReadLocks)]
    public async Task<ActionResult<PagingResponse<LockerAndLockDTO>>> GetManyLockersAndLocks(Guid tenantId, Guid id, [FromQuery] PagingRequest? page, [FromQuery] FilteredRequest? filter, [FromQuery] SortedRequest? sort)
    {
        page ??= new PagingRequest();

        var lockersAndLocks = await _lockerService.GetManyLockersAndLocksByLockerBankAsync(tenantId, id, filter, sort);

        return page.ToResponse(lockersAndLocks);
    }

    /// <summary>
    /// Bulk create locker and lock pairs
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="id">The locker bank id</param>
    /// <param name="LockerAndLocks">A list of the locker and lock pairs to be created</param>
    [HttpPost("{id}/lockers-and-locks")]
    [Authorize(Permissions.CreateLockers)]
    [Authorize(Permissions.CreateLocks)]
    public async Task<ActionResult> PostManyLockersAndLocks(Guid tenantId, Guid id, List<CreateLockerAndLockDTO> LockerAndLocks)
    {
        var allCreated = await _bulkOperationService.CreateManyLockerAndLockPairsForLockerBankAsync(tenantId, id, new()
        {
            LockerAndLocks = LockerAndLocks
        });

        return allCreated ? Ok(allCreated) : BadRequest();
    }


    #endregion Lockers

    #region Admins

    /// <summary>
    /// Returns all admins for the locker bank by page
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="id">The locker bank id</param>
    [HttpGet("{id}/admins")]
    [Authorize(Permissions.ReadLockerBankAdmins)]
    public async Task<ActionResult<PagingResponse<CardHolderDTO>>> GetManyAdmins(Guid tenantId, Guid id, [FromQuery] PagingRequest? page, [FromQuery] FilteredRequest? filter, [FromQuery] SortedRequest? sort)
    {
        page ??= new PagingRequest();

        var admins = await _lockerBankAdminService.GetManyAdminsByLockerBankAsync(tenantId, id, filter, sort);

        return page.ToResponse(admins);
    }

    /// <summary>
    /// Replace admins for the locker bank
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="id">The locker bank id</param>
    /// <param name="cardHolderIds">The card holder / user ids</param>
    [HttpPut("{id}/admins")]
    [Authorize(Permissions.UpdateLockerBankAdmins)]
    public async Task<ActionResult> PutManyAdmins(Guid tenantId, Guid id, List<Guid> cardHolderIds)
    {
        await _lockerBankAdminService.ReplaceAdminsForLockerBankAsync(tenantId, id, cardHolderIds);

        return CreatedAtAction(nameof(GetManyAdmins), new { tenantId, id }, null);
    }

    /// <summary>
    /// Remove a card holder as an admin from a locker bank
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="id">The locker bank id</param>
    /// <param name="cardHolderId">The card holder id</param>
    [HttpDelete("{id}/admins/{cardHolderId}")]
    [Authorize(Permissions.DeleteLockerBankAdmins)]
    public async Task<ActionResult> DeleteAdmin(Guid tenantId, Guid id, Guid cardHolderId)
    {
        await _lockerBankAdminService.RemoveAdminFromLockerBankAsync(tenantId, lockerBankId: id, cardHolderId: cardHolderId);

        return NoContent();
    }

    #endregion Admins

    #region Bulk Operations

    /// <summary>
    /// Bulk create, locker, lock, card holder and card credential and link them all together
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="id">The locker bank id</param>
    /// <param name="LockerAndLockAndCardHolderAndCardCredentials">The data</param>
    [HttpPost("{id}/locker-and-lock-and-card-holder-and-card-credentials")]
    [Authorize(Permissions.CreateLockers)]
    [Authorize(Permissions.CreateLocks)]
    [Authorize(Permissions.CreateCardHolders)]
    [Authorize(Permissions.CreateCardCredentials)]
    public async Task<ActionResult> PostManyLockerAndLockAndCardHolderAndCardCredentials(Guid tenantId, Guid id, List<CreateLockerAndLockAndCardHolderAndCardCredentialDTO> LockerAndLockAndCardHolderAndCardCredentials)
    {
        var allCreated = await _bulkOperationService.CreateAndAssignNewCardHolderAndCardCredentialPairsToNewLockersAndLockPairs(tenantId, id, new()
        {
            LockerAndLockAndCardHolderAndCardCredentials = LockerAndLockAndCardHolderAndCardCredentials
        });

        return allCreated ? Ok(allCreated) : BadRequest();
    }

    #endregion Bulk Operations

    #region Reference Images

    /// <summary>
    /// Create a reference image for a locker bank
    /// </summary>
    /// <param name="tenantId">Tenant id</param>
    /// <param name="dto">Creation DTO for a reference image</param>
    /// <returns>Created reference image or a 500 status code</returns>
    [HttpPost("reference-images")]
    [Authorize(Permissions.UpdateLockerBanks)]
    public async Task<ActionResult> PostReferenceImage(Guid tenantId, CreateReferenceImageDTO dto)
    {
        var newReferenceImage = await _referenceImageService.CreateReferenceImageAsync(tenantId, dto, User.GetEmailFromClaim() ?? string.Empty);
        
        return newReferenceImage is null
            ? StatusCode(StatusCodes.Status500InternalServerError)
            : CreatedAtAction(nameof(GetReferenceImage), new { tenantId, newReferenceImage.Value.Id }, newReferenceImage);
    }

    /// <summary>
    /// Get a reference image for a locker bank, uses the owner Id e.g locker bank Id.
    /// This is so that tenant admins can view a locker bank reference image from the locker status' component
    /// without needing a seperate API call to first get an ILockerBankDTO to retrieve the ReferenceImageId
    /// </summary>
    /// <param name="tenantId">Tenant Id</param>
    /// <param name="id">Locker Bank Id </param>
    /// <returns>ReferenceImageDTO which can also be null</returns>
    [HttpGet("reference-images/{id}")]
    [Authorize(Permissions.ReadLockerBanks)]
    public async Task<ActionResult<ReferenceImageDTO>> GetReferenceImage(Guid tenantId, Guid id)
    {
        var referenceImage = await _referenceImageService.GetReferenceImageByOwnerIdAsync(tenantId, id);

        return referenceImage;
    }

    /// <summary>
    /// Delete a reference image 
    /// </summary>
    /// <param name="tenantId">Tenant Id</param>
    /// <param name="id">Reference Image Id</param>
    /// <returns></returns>
    [HttpDelete("reference-images/{id}")]
    [Authorize(Permissions.UpdateLockerBanks)]
    public async Task<ActionResult> DeleteReferenceImage(Guid tenantId, Guid id)
    {
        await _referenceImageService.DeleteReferenceImageAsync(tenantId, id);

        return NoContent();
    }

    #endregion Reference Images

    #region NEW OWNERSHIP MODEL

    // TODO: REPLACE ALL THE CARD CREDENTIALS METHODS WITH THESE
    
    [HttpGet("{id}/special-cards")]
    [Authorize(Permissions.ReadCardCredentials)]
    [Authorize(Permissions.ReadCardHolders)]
    [Authorize(Permissions.ReadLockerBanks)]
    public async Task<ActionResult<PagingResponse<CardHolderAndCardCredentialsDTO>>> GetSpecialCardCredentialsAndTheirCardHolders(Guid tenantId, Guid id, [FromQuery] PagingRequest? page, [FromQuery] FilteredRequest? filter, [FromQuery] SortedRequest? sort)
    {
        page ??= new PagingRequest();

        var cardHoldersAndCardCredentials = await _lockerBankService.GetCardHoldersWithSpecialCardsAssignedToLockerBankAsync(tenantId, id, filter, sort);

        return page.ToResponse(cardHoldersAndCardCredentials);
    }

    [HttpPut("{id}/special-cards")]
    [Authorize(Permissions.UpdateLockerBanks)]
    public async Task<ActionResult> PutSpecialCardCredentials(Guid tenantId, Guid id, [FromBody] List<Guid> cardCredentialIds)
    {
        await _lockerBankService.ReplaceSpecialCardsForLockerBankAsync(tenantId, id, cardCredentialIds);

        await LogAndUpdateSmartLockConfigsForLockerBankAsync(tenantId, id, "Locker bank special cards replaced");

        return NoContent();
    }

    [HttpGet("{id}/lease-users")]
    [Authorize(Permissions.ReadLeaseUsers)]
    public async Task<ActionResult<PagingResponse<CardHolderAndCardCredentialsDTO>>> GetLeaseUsersAndAssignedCardCredentials(Guid tenantId, Guid id, [FromQuery] PagingRequest? page, [FromQuery] FilteredRequest? filter, [FromQuery] SortedRequest? sort)
    {
        page ??= new PagingRequest();

        var cardCredentials = await _lockerBankService.GetLeaseUsersWithUserCardsAssignedToLockerBankAsync(tenantId, id, filter, sort);

        return page.ToResponse(cardCredentials);
    }

    [HttpPut("{id}/lease-users")]
    [Authorize(Permissions.UpdateLeaseUsers)]
    public async Task<ActionResult> PutLeaseUsers(Guid tenantId, Guid id, List<Guid> cardCredentialIds)
    {
        await _lockerBankService.ReplaceLeaseUsersForLockerBankAsync(tenantId, id, cardCredentialIds);

        await LogAndUpdateSmartLockConfigsForLockerBankAsync(tenantId, id, "Locker bank lease users replaced");

        return NoContent();
    }

    private async Task LogAndUpdateSmartLockConfigsForLockerBankAsync(Guid tenantId, Guid lockerBankid, string? description = null)
    {
        await _lockConfigAuditService.LogLockerBankConfigChangeAsync(tenantId, User.GetEmailFromClaim() ?? string.Empty, lockerBankid, description);
        await _lockConfigService.UpdateLockConfigsByLockerBankAsync(tenantId, lockerBankid);
    }

    #endregion NEW OWNERSHIP MODEL
}
