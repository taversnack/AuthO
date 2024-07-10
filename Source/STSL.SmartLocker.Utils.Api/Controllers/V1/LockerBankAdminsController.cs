using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using STSL.SmartLocker.Utils.Api.Auth;
using STSL.SmartLocker.Utils.Data.Services.Contracts;
using STSL.SmartLocker.Utils.Data.Services.Helpers;
using STSL.SmartLocker.Utils.DTO;

namespace STSL.SmartLocker.Utils.Api.Controllers.V1;

[ApiVersion(V1)]
public class LockerBankAdminsController : MultiTenantControllerBase
{
    private readonly ILockerBankAdminService _lockerBankAdminService;
    private readonly ICardHolderService _cardHolderService;
    private readonly ILogger<LockerBanksController> _logger;

    public LockerBankAdminsController(
        ILockerBankAdminService lockerBankAdminService,
        ICardHolderService cardHolderService,
        ILogger<LockerBanksController> logger)
        => (_lockerBankAdminService, _cardHolderService, _logger)
        = (lockerBankAdminService, cardHolderService, logger);

    /// <summary>
    /// Returns all locker bank admins by page
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    [HttpGet]
    [Authorize(Permissions.ReadLockerBankAdmins)]
    public async Task<ActionResult<PagingResponse<CardHolderDTO>>> GetMany(Guid tenantId, [FromQuery] PagingRequest? page, [FromQuery] FilteredRequest? filter, [FromQuery] SortedRequest? sort)
    {
        // TODO: [10] Must return a reference to the locker bank they are admin for
        page ??= new PagingRequest();

        var lockerBanks = await _lockerBankAdminService.GetManyAdminsAsync(tenantId, filter, sort);

        return page.ToResponse(lockerBanks);
    }

    // TODO: [5] Create remaining CRUD methods for administrating locker bank admins

    /// <summary>
    /// Gets locker banks for which the current user is an admin, 
    /// regardless of super user status
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    [HttpGet("locker-banks")]
    [Authorize(Permissions.ReadLockerBanks)]
    public async Task<ActionResult<PagingResponse<LockerBankDTO>>> GetManyLockerBanks(Guid tenantId, [FromQuery] PagingRequest? page, [FromQuery] FilteredRequest? filter, [FromQuery] SortedRequest? sort)
    {
        var userEmail = User.GetEmailFromClaim();

        if (string.IsNullOrWhiteSpace(userEmail))
        {
            return Forbid();
        }

        page ??= new PagingRequest();

        var lockerBanks = await _lockerBankAdminService.GetManyLockerBanksByAdminAsync(tenantId, userEmail, filter, sort);

        return page.ToResponse(lockerBanks);
    }

    /// <summary>
    /// Gets locations of the locker banks for which the current user is an admin,
    /// regardless of super user status
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    [HttpGet("locations")]
    [Authorize(Permissions.ReadLocations)]
    public async Task<ActionResult<PagingResponse<LocationDTO>>> GetManyLocations(Guid tenantId, [FromQuery] PagingRequest? page, [FromQuery] FilteredRequest? filter, [FromQuery] SortedRequest? sort)
    {
        var userEmail = User.GetEmailFromClaim();

        if (string.IsNullOrWhiteSpace(userEmail))
        {
            return Forbid();
        }

        page ??= new PagingRequest();

        var locations = await _lockerBankAdminService.GetManyLocationsByAdminAsync(tenantId, userEmail, filter, sort);

        return page.ToResponse(locations);
    }

    /// <summary>
    /// Gets locker banks by location id for which the current user is an admin,
    /// regardless of super user status
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="locationId">The location id</param>
    [HttpGet("locations/{locationId}/locker-banks")]
    [Authorize(Permissions.ReadLockerBanks)]
    public async Task<ActionResult<PagingResponse<LockerBankDTO>>> GetManyLockerBanksByLocation(Guid tenantId, Guid locationId, [FromQuery] PagingRequest? page, [FromQuery] FilteredRequest? filter, [FromQuery] SortedRequest? sort)
    {
        var userEmail = User.GetEmailFromClaim();

        if (string.IsNullOrWhiteSpace(userEmail))
        {
            return Forbid();
        }

        page ??= new PagingRequest();

        var locations = await _lockerBankAdminService.GetManyLockerBanksByLocationByAdminAsync(tenantId, locationId, userEmail, filter, sort);

        return page.ToResponse(locations);
    }

    #region UI v2 Endpoints

    /// <summary>
    /// Gets summaries of all locker banks by locker bank admin, grouped by location
    /// regardless of super user status
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    [HttpGet("locker-bank-summaries")]
    [Authorize(Permissions.ReadLockerBanks)]
    public async Task<ActionResult<PagingResponse<LockerBankAdminSummaryDTO>>> GetManyLockerBankSummaries(Guid tenantId, [FromQuery] PagingRequest? page, [FromQuery] FilteredRequest? filter, [FromQuery] SortedRequest? sort)
    {
        var userEmail = User.GetEmailFromClaim();

        if (string.IsNullOrWhiteSpace(userEmail))
        {
            return Forbid();
        }

        page ??= new PagingRequest();

        var lockerBankAdminSummaries = await _lockerBankAdminService.GetManyLockerBankSummariesAsync(tenantId, userEmail, filter, sort);

        return page.ToResponse(lockerBankAdminSummaries);
    }

    #endregion UI v2 Endpoints
}
