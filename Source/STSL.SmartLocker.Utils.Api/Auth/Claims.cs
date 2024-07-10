namespace STSL.SmartLocker.Utils.Api.Auth;

internal static class Claims
{
    public const string ClaimsNamespace = "https://goto-secure.stsl.co.uk";

    public const string Tenants = ClaimsNamespace + "/tenants";
    public const string Tenant = ClaimsNamespace + "/tenant";
    public const string Roles = ClaimsNamespace + "/roles";
    public const string Email = ClaimsNamespace + "/email";
    public const string Permissions = "permissions";
}
