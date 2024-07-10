using Microsoft.AspNetCore.Mvc;
using STSL.SmartLocker.Utils.Api.Helpers;
using System.Security.Claims;

namespace STSL.SmartLocker.Utils.Api.Controllers.V1.Kiosk
{

    public sealed record KioskContext(Guid TenantId, Guid KioskId, string KioskName, Guid LocationId);

    // ApiController and Route attributes are inherited but can be overridden
    [ApiController]
    [ApiConventionType(typeof(ApiConventions))]
    [Route(BaseApiRoute + "/[controller]")]
    public abstract class KioskControllerBase : ControllerBase
    {
        internal const string BaseApiRoute = "api/v{version:apiVersion}";
        internal const string V1 = "1.0";
        internal const string ClaimsNamespace = "https://goto-secure.stsl.co.uk";


        /// <summary>
        /// Get claims (tenantId etc) from access token
        /// </summary>
        /// <returns>KioskContext</returns>
        protected KioskContext CheckClientClaims()
        {
            Guid? tenantId = GetCustomClientGuidClaim(User.Claims, "tenant");
            Guid? kioskId = GetCustomClientGuidClaim(User.Claims, "kiosk");
            string? kioskName = GetClientClaimValue(User.Claims, $"{ClaimsNamespace}/kiosk_name");
            Guid? locationId = GetCustomClientGuidClaim(User.Claims, "location");


            if (!tenantId.HasValue || !kioskId.HasValue || string.IsNullOrEmpty(kioskName) || !locationId.HasValue)
            {
                throw new UnauthorizedAccessException($"Bearer token for client credentials is missing required claims. tenant: {tenantId}, kiosk: {kioskId}, kiosk_name: {kioskName}, location: {locationId}");
            }

            return new KioskContext(tenantId.Value, kioskId.Value, kioskName, locationId.Value);
        }

        private static Guid? GetCustomClientGuidClaim(IEnumerable<Claim> claims, string customClaim)
        {
            string? cv = GetClientClaimValue(claims, $"{ClaimsNamespace}/{customClaim}");

            if (string.IsNullOrWhiteSpace(cv) || !Guid.TryParse(cv, out Guid res))
            {
                return null;
            }
            return res;
        }

        private static string? GetClientClaimValue(IEnumerable<Claim> claims, string type)
        {
            return claims.FirstOrDefault(c => c.Type == type)?.Value;
        }
    }
}
