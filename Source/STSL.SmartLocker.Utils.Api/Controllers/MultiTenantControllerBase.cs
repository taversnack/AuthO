using Microsoft.AspNetCore.Mvc;
using STSL.SmartLocker.Utils.Api.Helpers;

namespace STSL.SmartLocker.Utils.Api.Controllers;

// ApiController and Route attributes are inherited but can be overridden
[ApiController]
[ApiConventionType(typeof(ApiConventions))]
[Route(BaseApiRoute + "/[controller]")]
public abstract class MultiTenantControllerBase : ControllerBase
{
    internal const string BaseApiRoute = "api/v{version:apiVersion}/tenants/{tenantId}";
    internal const string V1 = "1.0";
}
