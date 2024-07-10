using STSL.SmartLocker.Utils.Data.Services.Contracts;
using System.Security.Claims;

namespace STSL.SmartLocker.Utils.Api.Auth;

internal static class LockerBankAdminAuthorizationExtensions
{
    // Assuming locker bank Ids are not added to the user data as provided by the Auth provider:

    // NOTE: Another way to do this without requiring additional claims;
    // First time we lookup users from their userinfo endpoint, (this may cause some latency)
    // we retrieve their email and store it alonside the value of the 'sub' along with the 'azp' claims.
    // Repeat requests can use the sub and trusted azp to lookup the user.

    // Alternatively you can do a quick hack as used below and add a namespaced email claim to the access token.
    // and just lookup based on that.
    public static async Task<bool> IsUserAuthorizedForBankAsync(this ILockerBankAdminService lockerBankAdminService, ClaimsPrincipal User, Guid tenantId, Guid lockerBankId)
    {
        if (User.HasClaim(Claims.Roles, Roles.SuperUser))
        {
            return true;
        }

        if (!User.HasClaim(Claims.Roles, Roles.LockerBankAdmin))
        {
            return false;
        }

        var userEmail = User.GetEmailFromClaim();

        return userEmail is not null && await lockerBankAdminService.IsCardHolderAdminForLockerBankAsync(tenantId, lockerBankId, userEmail);
    }

    public static string? GetEmailFromClaim(this ClaimsPrincipal User) => User.Claims.FirstOrDefault(x => x.Type == Claims.Email)?.Value;
}
