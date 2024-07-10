using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using STSL.SmartLocker.Utils.Api.Auth;
using STSL.SmartLocker.Utils.Data.Services.Contracts;
using STSL.SmartLocker.Utils.Data.Services.Helpers;
using STSL.SmartLocker.Utils.DTO;

namespace STSL.SmartLocker.Utils.Api.Controllers.V1;

[ApiVersion(V1)]
public sealed class LockersController : MultiTenantControllerBase
{
    private readonly ILockerService _lockerService;
    private readonly ICardCredentialService _cardCredentialService;
    private readonly ICardHolderService _cardHolderService;
    private readonly ILockService _lockService;
    private readonly ILockerLeaseService _lockerLeaseService;
    private readonly IMartService _martService;
    private readonly ILockConfigService _lockConfigService;
    private readonly ILockConfigAuditService _lockConfigAuditService;
    private readonly ILogger<LockersController> _logger;

    public LockersController(
        ILockerService lockerService,
        ICardCredentialService cardCredentialService,
        ICardHolderService cardHolderService,
        ILockService lockService,
        ILockerLeaseService lockerLeaseService,
        IMartService martService,
        ILockConfigService lockConfigService,
        ILockConfigAuditService lockConfigAuditService,
        ILogger<LockersController> logger)
        => (_lockerService, _cardCredentialService, _cardHolderService, _lockService, _lockerLeaseService, _martService, _lockConfigService, _lockConfigAuditService, _logger)
        = (lockerService, cardCredentialService, cardHolderService, lockService, lockerLeaseService, martService, lockConfigService, lockConfigAuditService, logger);

    #region Lockers

    /// <summary>
    /// Creates a new locker
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    [HttpPost]
    [Authorize(Permissions.CreateLockers)]
    public async Task<ActionResult<LockerDTO>> Post(Guid tenantId, CreateLockerDTO dto)
    {
        var newLocker = await _lockerService.CreateLockerAsync(tenantId, dto);

        return newLocker is null ? StatusCode(StatusCodes.Status500InternalServerError) : CreatedAtAction(nameof(Get), new { tenantId, newLocker.Id }, newLocker);
    }

    /// <summary>
    /// Returns all lockers by page
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    [HttpGet]
    [Authorize(Permissions.ReadLockers)]
    public async Task<ActionResult<PagingResponse<LockerDTO>>> GetMany(Guid tenantId, [FromQuery] PagingRequest? page, [FromQuery] FilteredRequest? filter, [FromQuery] SortedRequest? sort)
    {
        page ??= new PagingRequest();

        var lockers = await _lockerService.GetManyLockersAsync(tenantId, filter, sort);

        return page.ToResponse(lockers);
    }

    /// <summary>
    /// Returns an existing locker or 404 (not found)
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="id">The locker id</param>
    [HttpGet("{id}")]
    [Authorize(Permissions.ReadLockers)]
    public async Task<ActionResult<LockerDTO>> Get(Guid tenantId, Guid id)
    {
        var locker = await _lockerService.GetLockerAsync(tenantId, id);

        return locker is null ? NotFound() : locker;
    }

    /// <summary>
    /// Fully updates a valid locker or 400 with erroneous properties
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="id">The locker id</param>
    [HttpPut("{id}")]
    [Authorize(Permissions.UpdateLockers)]
    public async Task<ActionResult> Put(Guid tenantId, Guid id, UpdateLockerDTO dto)
    {
        await _lockerService.UpdateLockerAsync(tenantId, id, dto);

        return NoContent();
    }

    /// <summary>
    /// Deletes a locker if it exists, 404 otherwise
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="id">The locker id</param>
    [HttpDelete("{id}")]
    [Authorize(Permissions.DeleteLockers)]
    public async Task<ActionResult> Delete(Guid tenantId, Guid id)
    {
        await _lockerService.DeleteLockerAsync(tenantId, id);

        return NoContent();
    }

    /// <summary>
    /// Returns the audit records for a locker
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="id">The locker id</param>
    [HttpGet("{id}/audits")]
    [Authorize(Permissions.ReadLockers)]
    public async Task<ActionResult<PagingResponse<AuditRecordsForLockerDTO>>> GetAudits(Guid tenantId, Guid id, [FromQuery] PagingRequest? page)
    {
        page ??= new PagingRequest();

        return Ok(await _martService.GetPagedAuditRecordsForLockerAsync(tenantId, id, page));
    }

    #endregion Lockers

    #region Locks

    /// <summary>
    /// Gets the lock currently associated with a locker if there is one (null if not)
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="id">The locker id</param>
    [HttpGet("{id}/lock")]
    [Authorize(Permissions.ReadLocks)]
    public async Task<ActionResult<LockDTO?>> GetLock(Guid tenantId, Guid id)
    {
        // TODO: This should really probably return not found if null
        return await _lockService.GetLockByLockerIdAsync(tenantId, id);

        //var @lock = await _lockService.GetLockByLockerIdAsync(tenantId, id);

        //return @lock is null ? NotFound() : @lock;
    }


    /// <summary>
    /// Gets whether a locker's lock is pending an update (true) or any updates are complete (false).
    /// Will always return false when the locker has no associated lock.
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="id">The locker id</param>
    [HttpGet("{id}/lock/config-pending")]
    [Authorize(Permissions.ReadLocks)]
    public async Task<ActionResult<bool>> GetConfigIsPendingStatus(Guid tenantId, Guid id)
    {
        return await _lockConfigService.IsLockUpdatePendingByLockerAsync(tenantId, id);
    }

    #endregion Locks

    #region Bulk And Misc

    /// <summary>
    /// Returns all lockers by page
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    [HttpGet("with-location-locker-bank-lock")]
    [Authorize(Permissions.ReadLocations)]
    [Authorize(Permissions.ReadLockerBanks)]
    [Authorize(Permissions.ReadLockers)]
    [Authorize(Permissions.ReadLocks)]
    public async Task<ActionResult<GlobalLockerSearchResultDTO>> GetManyWithLockAndLocationAndLockerBankDetails(Guid tenantId, [FromQuery] PagingRequest? page, [FromQuery] FilteredRequest? filter, [FromQuery] SortedRequest? sort)
        => await _lockerService.GetManyLockersWithLockAndLocationAndLockerBankDetailsAsync(tenantId, page, filter, sort);

    #endregion Bulk And Misc

    #region Leases

    /// <summary>
    /// Returns all locker lease history by page for this locker
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="id">The locker id</param>
    [HttpGet("{id}/leases")]
    //[Authorize(Permissions.ReadLeaseUsers)]
    [Authorize(Permissions.ReadLockers)]
    public async Task<ActionResult<PagingResponse<LockerLeaseDTO>>> GetLockerLeaseHistoryByLocker(Guid tenantId, Guid id, [FromQuery] PagingRequest? page, [FromQuery] FilteredRequest? filter, [FromQuery] SortedRequest? sort)
    {
        page ??= new PagingRequest();

        var lockers = await _lockerLeaseService.GetLockerLeaseHistoryByLockerAsync(tenantId, id, filter, sort);

        return page.ToResponse(lockers);
    }

    // TODO: Remove the following 2 methods after testing / move to separate lease controller
    /// <summary>
    /// Returns all locker lease history by page for all lockers
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    [HttpGet("leases")]
    [Authorize(Permissions.ReadLeaseUsers)]
    [Authorize(Permissions.ReadLockers)]
    public async Task<ActionResult<PagingResponse<LockerLeaseDTO>>> GetLockerLeaseHistory(Guid tenantId, [FromQuery] PagingRequest? page, [FromQuery] FilteredRequest? filter, [FromQuery] SortedRequest? sort)
    {
        page ??= new PagingRequest();

        var lockers = await _lockerLeaseService.GetLockerLeaseHistoryAsync(tenantId, filter, sort);

        return page.ToResponse(lockers);
    }

    /// <summary>
    /// Returns a single locker lease by id
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="id">The locker lease id</param>
    [HttpGet("leases/{id}")]
    [Authorize(Permissions.ReadLeaseUsers)]
    [Authorize(Permissions.ReadLockers)]
    public async Task<ActionResult<LockerLeaseDTO>> GetLockerLease(Guid tenantId, Guid id)
    {
        var locker = await _lockerLeaseService.GetLockerLeaseAsync(tenantId, id);

        return locker is null ? NotFound() : locker;
    }

    #endregion Leases

    #region Owners
    
    [HttpGet("{id}/owners")]
    [Authorize(Permissions.ReadLeaseUsers)]
    public async Task<ActionResult<PagingResponse<CardHolderAndCardCredentialsDTO>>> GetLockerOwnersWithAssignedCardCredentials(Guid tenantId, Guid id, [FromQuery] PagingRequest? page, [FromQuery] FilteredRequest? filter, [FromQuery] SortedRequest? sort)
    {
        page ??= new PagingRequest();

        var cardCredentials = await _lockerService.GetOwnersWithCardsAssignedToLockerAsync(tenantId, id, filter, sort);

        return page.ToResponse(cardCredentials);
    }

    [HttpPut("{id}/owners/card-credentials")]
    [Authorize(Permissions.UpdateLeaseUsers)]
    public async Task<ActionResult> PutLockerOwnersViaCardCredentials(Guid tenantId, Guid id, List<Guid> cardCredentialIds)
    {
        await _lockerService.ReplaceOwnersForLockerByCardCredentialsAsync(tenantId, id, cardCredentialIds);

        await LogAndUpdateSmartLockConfigsForLockerAsync(tenantId, id, "Locker owners replaced");

        return NoContent();
    }

    [HttpPut("{id}/owners/card-holders")]
    [Authorize(Permissions.UpdateLeaseUsers)]
    public async Task<ActionResult> PutLockerOwners(Guid tenantId, Guid id, List<Guid> cardHolderIds)
    {
        await _lockerService.ReplaceOwnersForLockerByCardHoldersAsync(tenantId, id, cardHolderIds);

        return NoContent();
    }

    private async Task LogAndUpdateSmartLockConfigsForLockerAsync(Guid tenantId, Guid lockerid, string? description = null)
    {
        await _lockConfigAuditService.LogLockerConfigChangeAsync(tenantId, User.GetEmailFromClaim() ?? string.Empty, lockerid, description);
        await _lockConfigService.UpdateLockConfigByLockerAsync(tenantId, lockerid);
    }
   
    #endregion Owners
}
