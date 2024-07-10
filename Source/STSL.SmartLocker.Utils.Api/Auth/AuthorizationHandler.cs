using Microsoft.AspNetCore.Authorization;

namespace STSL.SmartLocker.Utils.Api.Auth;

internal sealed class AuthorizationHandler : IAuthorizationHandler
{
    public async Task HandleAsync(AuthorizationHandlerContext context)
    {
        var requirements = context.PendingRequirements;

        foreach (var requirement in requirements)
        {
            if (requirement is ScopeRequirement scopeRequirement)
            {
                await ProcessHandlerResult<ScopeRequirementAuthorizationHandler, ScopeRequirement>(context, scopeRequirement);
            }
        }
    }

    private async Task ProcessHandlerResult<T, R>(AuthorizationHandlerContext context, R requirement) where T : IAuthorizationRequirementHandler<R>, new() where R : IAuthorizationRequirement
    {
        var result = await new T().IsRequirementMetAsync(context.User, context.Resource, requirement);
        if (result)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail(new(this, $"{typeof(T).Name} handler failed for requirement {typeof(R).Name}"));
        }
    }
}