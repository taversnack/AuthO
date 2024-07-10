using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using STSL.SmartLocker.Utils.Api.Auth;
using STSL.SmartLocker.Utils.Data.Services.Contracts;
using STSL.SmartLocker.Utils.Data.Services.Helpers;
using STSL.SmartLocker.Utils.Domain;
using STSL.SmartLocker.Utils.DTO;

namespace STSL.SmartLocker.Utils.Api.Controllers.V1;

[ApiVersion(V1)]
public sealed class LocationsController : MultiTenantControllerBase
{
    private readonly ILocationService _locationService;
    private readonly ILockerBankService _lockerBankService;
    private readonly ILogger<LocationsController> _logger;
    private readonly IReferenceImageService<Location, LocationReferenceImage> _referenceImageService;

    public LocationsController(ILocationService locationService, ILockerBankService lockerBankService, ILogger<LocationsController> logger, IReferenceImageService<Location, LocationReferenceImage> referenceImageService)
        => (_locationService, _lockerBankService, _logger, _referenceImageService) = (locationService, lockerBankService, logger, referenceImageService);

    #region Locations

    /// <summary>
    /// Creates a new location
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    [HttpPost]
    [Authorize(Permissions.CreateLocations)]
    public async Task<ActionResult<LocationDTO>> Post(Guid tenantId, CreateLocationDTO dto)
    {
        var newLocation = await _locationService.CreateLocationAsync(tenantId, dto);

        return newLocation is null ? StatusCode(StatusCodes.Status500InternalServerError) : CreatedAtAction(nameof(Get), new { tenantId, newLocation.Id }, newLocation);
    }

    /// <summary>
    /// Returns all locations by page
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    [HttpGet]
    [Authorize(Permissions.ReadLocations)]
    public async Task<ActionResult<PagingResponse<LocationDTO>>> GetMany(Guid tenantId, [FromQuery] PagingRequest? page, [FromQuery] FilteredRequest? filter, [FromQuery] SortedRequest? sort)
    {
        page ??= new PagingRequest();

        var locations = await _locationService.GetManyLocationsAsync(tenantId, filter, sort);

        return page.ToResponse(locations);
    }

    /// <summary>
    /// Returns an existing location or 404 (not found)
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="id">The location id</param>
    [HttpGet("{id}")]
    [Authorize(Permissions.ReadLocations)]
    public async Task<ActionResult<LocationDTO>> Get(Guid tenantId, Guid id)
    {
        var location = await _locationService.GetLocationAsync(tenantId, id);

        return location is null ? NotFound() : location;
    }

    /// <summary>
    /// Fully updates a valid location or 400 with erroneous properties
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="id">The location id</param>

    [HttpPut("{id}")]
    [Authorize(Permissions.UpdateLocations)]
    public async Task<ActionResult> Put(Guid tenantId, Guid id, UpdateLocationDTO dto)
    {
        await _locationService.UpdateLocationAsync(tenantId, id, dto);

        return NoContent();
    }

    /// <summary>
    /// Deletes a location if it exists, 404 otherwise
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="id">The location id</param>
    [HttpDelete("{id}")]
    [Authorize(Permissions.DeleteLocations)]
    public async Task<ActionResult> Delete(Guid tenantId, Guid id)
    {
        await _locationService.DeleteLocationAsync(tenantId, id);

        return NoContent();
    }

    #endregion Locations

    #region LockerBanks

    /// <summary>
    /// Bulk create locker banks for the location
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="id">The location id</param>
    [HttpPost("{id}/locker-banks")]
    [Authorize(Permissions.CreateLockerBanks)]
    public async Task<ActionResult> PostManyLockerBanks(Guid tenantId, Guid id, IEnumerable<CreateLockerBankDTO> dtoList)
    {
        var createdAll = await _lockerBankService.CreateManyLockerBanksAsync(tenantId, dtoList.Select(x => x with { LocationId = id }).ToList());

        return createdAll ? CreatedAtAction(nameof(GetManyLockerBanks), new { tenantId, id }, null) : StatusCode(StatusCodes.Status500InternalServerError);
    }

    /// <summary>
    /// Returns locker banks that belong to the location
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="id">The location id</param>
    [HttpGet("{id}/locker-banks")]
    [Authorize(Permissions.ReadLockerBanks)]
    public async Task<ActionResult<PagingResponse<LockerBankDTO>>> GetManyLockerBanks(Guid tenantId, Guid id, [FromQuery] PagingRequest? page, [FromQuery] FilteredRequest? filter, [FromQuery] SortedRequest? sort)
    {
        page ??= new PagingRequest();

        var locations = await _lockerBankService.GetManyLockerBanksByLocationAsync(tenantId, id, filter, sort);

        return page.ToResponse(locations);
    }

    #endregion LockerBanks

    #region Reference Images

    /// <summary>
    /// Create a reference image for a location
    /// </summary>
    /// <param name="tenantId">Tenant id</param>
    /// <param name="dto">Creation DTO for a reference image</param>
    /// <returns>Created reference image or a 500 status code</returns>
    [HttpPost("reference-images")]
    [Authorize(Permissions.UpdateLocations)]
    public async Task<ActionResult> PostReferenceImage(Guid tenantId, CreateReferenceImageDTO dto)
    {
        var newReferenceImage = await _referenceImageService.CreateReferenceImageAsync(tenantId, dto, User.GetEmailFromClaim() ?? string.Empty);
        return newReferenceImage is null 
            ? StatusCode(StatusCodes.Status500InternalServerError) 
            : CreatedAtAction(nameof(GetReferenceImage), new { tenantId, newReferenceImage.Value.Id }, newReferenceImage);
    }

    /// <summary>
    /// Get a reference image for a location, uses the owner Id e.g Location Id
    /// This is so that tenant admins can view a location reference image from the locker status' component
    /// without needing a seperate API call to first get an ILocationDTO to retrieve the ReferenceImageId
    /// </summary>
    /// <param name="tenantId">Tenant Id</param>
    /// <param name="id">Location Id</param>
    /// <returns>ReferenceImageDTO which can also be null</returns>
    [HttpGet("reference-images/{id}")]
    [Authorize(Permissions.ReadLocations)]
    public async Task<ActionResult<ReferenceImageDTO?>> GetReferenceImage(Guid tenantId, Guid id)
    {
        var referenceImage = await _referenceImageService.GetReferenceImageByOwnerIdAsync(tenantId, id);

        return referenceImage;
    }

    /// <summary>
    /// Delete a reference image 
    /// </summary>
    /// <param name="tenantId">Tenant Id</param>
    /// <param name="id">Reference Image Id</param>
    [HttpDelete("reference-images/{id}")]
    [Authorize(Permissions.UpdateLocations)]
    public async Task<ActionResult> DeleteReferenceImage(Guid tenantId, Guid id)
    {
        await _referenceImageService.DeleteReferenceImageAsync(tenantId, id);

        return NoContent();
    }

    #endregion Reference Images
}
