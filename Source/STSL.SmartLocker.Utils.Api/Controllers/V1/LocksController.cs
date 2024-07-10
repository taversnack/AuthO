using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using STSL.SmartLocker.Utils.Api.Auth;
using STSL.SmartLocker.Utils.Data.Services.Contracts;
using STSL.SmartLocker.Utils.Data.Services.Helpers;
using STSL.SmartLocker.Utils.DTO;

namespace STSL.SmartLocker.Utils.Api.Controllers.V1;

[ApiVersion(V1)]
public class LocksController : MultiTenantControllerBase
{
    private readonly ILockService _lockService;
    private readonly ILockConfigService _lockConfigService;
    private readonly ILockConfigAuditService _lockConfigAuditService;
    private readonly ILogger<LocksController> _logger;

    public LocksController(
        ILockService lockService,
        ILockConfigService lockConfigService,
        ILockConfigAuditService lockConfigAuditService,
        ILogger<LocksController> logger)
        => (_lockService, _lockConfigService, _lockConfigAuditService, _logger)
        = (lockService, lockConfigService, lockConfigAuditService, logger);

    /// <summary>
    /// Creates a new lock
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    [HttpPost]
    [Authorize(Permissions.CreateLocks)]
    public async Task<ActionResult<LockDTO>> Post(Guid tenantId, CreateLockDTO dto)
    {

        var newLock = await _lockService.CreateLockAsync(tenantId, dto);

        if (newLock is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        await LogAndUpdateLockConfigAsync(tenantId, newLock.Id, "Lock created");

        return CreatedAtAction(nameof(Get), new { tenantId, newLock.Id }, newLock);
    }

    /// <summary>
    /// Returns all locks by page
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    [HttpGet]
    [Authorize(Permissions.ReadLocks)]
    public async Task<ActionResult<PagingResponse<LockDTO>>> GetMany(Guid tenantId, [FromQuery] PagingRequest? page, [FromQuery] FilteredRequest? filter, [FromQuery] SortedRequest? sort)
    {
        page ??= new PagingRequest();

        var locks = await _lockService.GetManyLocksAsync(tenantId, filter, sort);

        return page.ToResponse(locks);
    }

    /// <summary>
    /// Returns an existing lock or 404 (not found)
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="id">The lock id</param>
    [HttpGet("{id}")]
    [Authorize(Permissions.ReadLocks)]
    public async Task<ActionResult<LockDTO>> Get(Guid tenantId, Guid id)
    {
        // Note: '@' here is used because 'lock' is a reserved keyword in C#
        // prepending any variable with '@' allows for usage of reserved keywords as variable names
        var @lock = await _lockService.GetLockAsync(tenantId, id);

        return @lock is null ? NotFound() : @lock;
    }

    // TODO: Deprecate this, smart locks should be immutable
    /// <summary>
    /// Fully updates a valid lock or 400 with erroneous properties
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="id">The lock id</param>
    [HttpPut("{id}")]
    [Authorize(Permissions.UpdateLocks)]
    public async Task<ActionResult> Put(Guid tenantId, Guid id, UpdateLockDTO dto)
    {
        await _lockService.UpdateLockAsync(tenantId, id, dto);

        await LogAndUpdateLockConfigAsync(tenantId, id, "Lock fully updated");

        return NoContent();
    }

    /// <summary>
    /// Allows a change to a lock's lockerId
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="id">The lock id</param>
    [HttpPatch("{id}")]
    [Authorize(Permissions.UpdateLocks)]
    public async Task<ActionResult> Patch(Guid tenantId, Guid id, UpdateLockLockerIdDTO dto)
    {
        var @lock = await _lockService.GetLockAsync(tenantId, id);

        if (@lock is null)
        {
            return NotFound();
        }

        await _lockService.UpdateLockAsync(tenantId, id, new(
            SerialNumber: @lock.SerialNumber,
            InstallationDateUtc: @lock.InstallationDateUtc,
            FirmwareVersion: @lock.FirmwareVersion,
            OperatingMode: @lock.OperatingMode,
            LockerId: dto.LockerId,
            OverrideExistingLockerLockPair: true));

        await LogAndUpdateLockConfigAsync(tenantId, id, "Lock moved to different locker");

        return NoContent();
    }

    /// <summary>
    /// Deletes a lock if it exists, 404 otherwise
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="id">The lock id</param>
    [HttpDelete("{id}")]
    [Authorize(Permissions.DeleteLocks)]
    public async Task<ActionResult> Delete(Guid tenantId, Guid id)
    {
        await _lockService.DeleteLockAsync(tenantId, id);

        return NoContent();
    }

    /// <summary>
    /// Forces a lock to be updated using it's current data
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="id">The lock id</param>
    [HttpPut("{id}/config")]
    [Authorize(Permissions.UpdateLocks)]
    public async Task<ActionResult> PutConfigToLock(Guid tenantId, Guid id)
    {
        await LogAndUpdateLockConfigAsync(tenantId, id, "Lock config sent without change");

        return NoContent();
    }

    /// <summary>
    /// Gets whether a lock is pending an update (true) or any updates are complete (false)
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="id">The lock id</param>
    [HttpGet("{id}/config-pending")]
    [Authorize(Permissions.ReadLocks)]
    public async Task<ActionResult<bool>> GetConfigIsPendingStatus(Guid tenantId, Guid id)
    {
        return await _lockConfigService.IsLockUpdatePendingAsync(tenantId, id);
    }

    public readonly record struct UpdateLockLockerIdDTO(Guid? LockerId);

    private async Task LogAndUpdateLockConfigAsync(Guid tenantId, Guid lockId, string? description = null)
    {
        await _lockConfigAuditService.LogLockConfigChangeAsync(tenantId, User.GetEmailFromClaim() ?? string.Empty, lockId, description);

        await _lockConfigService.UpdateLockConfigAsync(tenantId, lockId);
    }
}
