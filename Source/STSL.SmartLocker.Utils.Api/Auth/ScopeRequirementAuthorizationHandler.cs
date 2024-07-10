using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace STSL.SmartLocker.Utils.Api.Auth;

internal readonly record struct ScopeRequirement(string Issuer, string Scope) : IAuthorizationRequirement;

internal readonly struct ScopeRequirementAuthorizationHandler : IAuthorizationRequirementHandler<ScopeRequirement>
{
    public Task<bool> IsRequirementMetAsync(ClaimsPrincipal userClaims, object? resource, ScopeRequirement requirement)
        => Task.FromResult(userClaims.HasClaim(
            c => c.Type == "scope" && c.Issuer == requirement.Issuer && !string.IsNullOrEmpty(c.Value) && c.Value.Split(' ').Any(s => s == requirement.Scope)
        ));
}
