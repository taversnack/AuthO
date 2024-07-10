using Microsoft.AspNetCore.Authentication;
using STSL.SmartLocker.Utils.Common.Exceptions;
using System.Security.Claims;

namespace STSL.SmartLocker.Utils.Api.Auth;

// NOTE: This could be changed to use a middleware factory and to inject a scoped service
// for setting the tenant Id. This could then be injected throughout request lifetime in
// controllers or services etc.
internal sealed class IsAuthorizedForTenantMiddleware
{
    private readonly RequestDelegate _next;

    public IsAuthorizedForTenantMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {

        var tenantIdParam = context.GetRouteValue("tenantId")?.ToString();

        if (tenantIdParam is not null && !context.User.IsInRole(Roles.SuperUser))
        {
            if (Guid.TryParse(tenantIdParam, out var tenantId))
            {
                if (!IsUserAuthorizedForTenant(context.User, tenantId))
                {
                    await context.ForbidAsync();
                    return;
                }
            }
            else
            {
                throw new BadRequestException("tenantId must be a valid Guid");
            }
        }

        await _next(context);
    }

    private bool IsUserAuthorizedForTenant(ClaimsPrincipal user, Guid tenantId)
        => user.HasClaim(x => (x.Type == Claims.Tenants || x.Type == Claims.Tenant) && Guid.TryParse(x.Value, out var id) && id == tenantId);
}

internal static class IApplicationBuilderExtensions
{
    /// <summary>
    /// Will check if a route contains a tenantId value, if so the User's tenants claim is checked for
    /// the matching tenant Id. <br/>
    /// If none is found, <c>403 (Forbidden)</c> is returned. <br/>
    /// <c>400 (Bad Request)</c> is returned if the Id is malformed
    /// </summary>
    /// <param name="app">The application builder</param>
    public static void UseTenantAuthorizedEndpoints(this IApplicationBuilder app)
    {
        app.UseMiddleware<IsAuthorizedForTenantMiddleware>();
    }
}