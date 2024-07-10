using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace STSL.SmartLocker.Utils.Api.Auth;

internal interface IAuthorizationRequirementHandler<TRequirement> where TRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Handles a <typeparamref name="TRequirement"/> requirement and 
    /// </summary>
    /// <param name="userClaims">The claims principal of the current request</param>
    /// <param name="requirement">The requirement to check</param>
    /// <param name="resource">The optional resource to evaluate the requirement against</param>
    /// <returns>true if the requirement is met or false otherwise.</returns>
    Task<bool> IsRequirementMetAsync(ClaimsPrincipal userClaims, object? resource, TRequirement requirement);
}
