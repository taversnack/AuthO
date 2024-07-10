using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using STSL.SmartLocker.Utils.Api.Auth;
using STSL.SmartLocker.Utils.Api.Helpers;
using STSL.SmartLocker.Utils.Data.Services.Contracts;
using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.Data.Services.Helpers;
using STSL.SmartLocker.Utils.DTO;
using System.Security.Claims;

namespace STSL.SmartLocker.Utils.Api.Controllers.V1;

[ApiController]
[ApiConventionType(typeof(DefaultApiConventions))]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/tenants")]
public sealed class TenantsController : ControllerBase
{
    private readonly ITenantService _tenantService;
    private readonly ILogger<TenantsController> _logger;

    public TenantsController(ITenantService tenantService, ILogger<TenantsController> logger)
        => (_tenantService, _logger) = (tenantService, logger);

    /// <summary>
    /// Creates a new tenant
    /// </summary>
    [HttpPost]
    [Authorize(Permissions.CreateTenants)]
    public async Task<ActionResult<TenantDTO>> Post(CreateTenantDTO dto)
    {
        var newTenant = await _tenantService.CreateTenantAsync(dto);

        return newTenant is null ? StatusCode(StatusCodes.Status500InternalServerError) : CreatedAtAction(nameof(Get), new { newTenant.Id }, newTenant);
    }

    /// <summary>
    /// Get all tenants registered to the user by page
    /// </summary>
    [HttpGet]
    [Authorize]
    [ApiConventionMethod(typeof(DefaultApiConventions), nameof(ApiConventions.Get))]
    public async Task<ActionResult<IPagingResponse<TenantDTO>>> GetMany([FromQuery] PagingRequest? page, [FromQuery] FilteredRequest? filter, [FromQuery] SortedRequest? sort)
    {
        if (User.IsInRole(Roles.SuperUser))
        {
            page ??= new PagingRequest();

            var tenants = await _tenantService.GetManyTenantsAsync(filter, sort);

            return page.ToResponse(tenants);
        }
        else if (User.IsInRole(Roles.LockerBankAdmin) || User.IsInRole(Roles.Installer) || User.IsInRole(Roles.TenantAdmin))
        {
            page ??= new PagingRequest();

            var tenants = await _tenantService.GetManyTenantsAsync(filter, sort);

            var userTenants = User.Claims
                .Where(x => x.Type == Claims.Tenants)
                .Select<Claim, Guid?>(x => Guid.TryParse(x.Value, out var parsedGuid) ? parsedGuid : null)
                .OfType<Guid>()
                .ToList();

            var intersection = tenants.IntersectBy(userTenants, x => x.Id);

            return page.ToResponse(intersection);
        }
        else
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Returns an existing tenant or 404 (not found)
    /// </summary>
    /// <param name="id">The tenant id</param>
    [HttpGet("{id}")]
    [Authorize(Permissions.ReadTenants)]
    public async Task<ActionResult<TenantDTO>> Get(Guid id)
    {
        var tenant = await _tenantService.GetTenantAsync(id);

        return tenant is null ? NotFound() : tenant;
    }

    // TODO: Finish other controller methods

    [HttpPut("{id}")]
    [Authorize(Permissions.UpdateTenants)]
    public async Task<ActionResult> Put(Guid id, [FromBody] UpdateTenantDTO dto)
    {
        await _tenantService.UpdateTenantAsync(id, dto);

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Permissions.DeleteTenants)]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _tenantService.DeleteTenantAsync(id);

        return NoContent();
    }
}
