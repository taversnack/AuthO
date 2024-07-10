using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using STSL.SmartLocker.Utils.Api.Auth;
using STSL.SmartLocker.Utils.Data.Services.Contracts;
using STSL.SmartLocker.Utils.DTO;

namespace STSL.SmartLocker.Utils.Api.Controllers.V1;

[ApiVersion(V1)]
public class BulkOperationsController : MultiTenantControllerBase
{
    private readonly IBulkOperationService _bulkOperationService;
    private readonly ILogger<BulkOperationsController> _logger;

    public BulkOperationsController(
        IBulkOperationService bulkOperationService,
        ILogger<BulkOperationsController> logger)
        => (_bulkOperationService, _logger)
        = (bulkOperationService, logger);

    [HttpPost("card-holder-and-credentials")]
    [Authorize(Permissions.CreateCardHolders)]
    [Authorize(Permissions.CreateCardCredentials)]
    public async Task<ActionResult> PostManyCardHolderAndCardCredentials(Guid tenantId, CreateBulkCardHolderAndCardCredentialsDTO cardHolderAndCardCredentials)
    {
        var allCreated = await _bulkOperationService.CreateManyCardHolderAndCardCredentialPairsAsync(tenantId, cardHolderAndCardCredentials);

        return allCreated ? Ok(allCreated) : BadRequest();
    }

}
