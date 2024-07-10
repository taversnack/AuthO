using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using STSL.SmartLocker.Utils.Api.Auth;
using STSL.SmartLocker.Utils.Common.Data;
using STSL.SmartLocker.Utils.Data.Services.Contracts;
using STSL.SmartLocker.Utils.Data.Services.Helpers;
using STSL.SmartLocker.Utils.DTO;

namespace STSL.SmartLocker.Utils.Api.Controllers.V1;

[ApiVersion(V1)]
public class CardCredentialsController : MultiTenantControllerBase
{
    private readonly ICardCredentialService _cardCredentialService;
    private readonly ILockConfigService _lockConfigService;
    private readonly ILockConfigAuditService _lockConfigAuditService;
    private readonly ILogger<CardCredentialsController> _logger;

    public CardCredentialsController(
        ICardCredentialService cardCredentialService,
        ILockConfigService lockConfigService,
        ILockConfigAuditService lockConfigAuditService,
        ILogger<CardCredentialsController> logger)
        => (_cardCredentialService, _lockConfigService, _lockConfigAuditService, _logger)
        = (cardCredentialService, lockConfigService, lockConfigAuditService, logger);

    /// <summary>
    /// Creates a new card credential
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    [HttpPost]
    [Authorize(Permissions.CreateCardCredentials)]
    public async Task<ActionResult<CardCredentialDTO>> Post(Guid tenantId, CreateCardCredentialDTO dto)
    {
        var newCardCredential = await _cardCredentialService.CreateCardCredentialAsync(tenantId, dto);

        return newCardCredential is null ? StatusCode(StatusCodes.Status500InternalServerError) : CreatedAtAction(nameof(Get), new { tenantId, newCardCredential.Id }, newCardCredential);
    }

    /// <summary>
    /// Returns all card credentials by page
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    [HttpGet]
    [Authorize(Permissions.ReadCardCredentials)]
    public async Task<ActionResult<PagingResponse<CardCredentialDTO>>> GetMany(Guid tenantId, [FromQuery] PagingRequest? page, [FromQuery] FilteredRequest? filter, [FromQuery] SortedRequest? sort, [FromQuery] CardType? cardType = null)
    {
        page ??= new PagingRequest();

        var cardCredentials = await _cardCredentialService.GetManyCardCredentialsAsync(tenantId, filter, sort, cardType);

        return page.ToResponse(cardCredentials);
    }

    /// <summary>
    /// Returns an existing card credential or 404 (not found)
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="id">The card credential id</param>
    [HttpGet("{id}")]
    [Authorize(Permissions.ReadCardCredentials)]
    public async Task<ActionResult<CardCredentialDTO>> Get(Guid tenantId, Guid id)
    {
        var cardCredential = await _cardCredentialService.GetCardCredentialAsync(tenantId, id);

        return cardCredential is null ? NotFound() : cardCredential;
    }

    /// <summary>
    /// Fully updates a valid card credential or 400 with erroneous properties
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="id">The card credential id</param>
    [HttpPut("{id}")]
    [Authorize(Permissions.UpdateCardCredentials)]
    public async Task<ActionResult> Put(Guid tenantId, Guid id, UpdateCardCredentialDTO dto)
    {
        var cardCredential = await _cardCredentialService.GetCardCredentialAsync(tenantId, id);

        if (cardCredential is null)
        {
            return NotFound();
        }

        // Don't bother checking for lock updates if the SerialNumber hasn't changed.
        // May likely improve performance by avoiding extra queries.
        if (string.Equals(cardCredential.SerialNumber, dto.SerialNumber, StringComparison.OrdinalIgnoreCase))
        {
            await _cardCredentialService.UpdateCardCredentialAsync(tenantId, id, dto);
        }
        else
        {
            var (affectedLockerBanks, affectedLockers) = await GetLockerBanksAndLockersWhereCardCredentialIsAssignedAsync(tenantId, id);

            await _cardCredentialService.UpdateCardCredentialAsync(tenantId, id, dto);

            await LogAndUpdateLockerBanksAndLockersWhereCardCredentialHasChangedAsync(tenantId, affectedLockerBanks, affectedLockers);
        }

        return NoContent();
    }

    /// <summary>
    /// Deletes a card credential if it exists, 404 otherwise
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="id">The card credential id</param>
    [HttpDelete("{id}")]
    [Authorize(Permissions.DeleteCardCredentials)]
    public async Task<ActionResult> Delete(Guid tenantId, Guid id)
    {
        // TODO: Come up with some stored procedure for this type of update.
        // Really need to put some work into mapping the flows & best way to update =

        // Check locker banks and lockers where card is assigned
        // (must be done before delete since cascade will delete from many to many tables)
        var (affectedLockerBanks, affectedLockers) = await GetLockerBanksAndLockersWhereCardCredentialIsAssignedAsync(tenantId, id);

        await _cardCredentialService.DeleteCardCredentialAsync(tenantId, id);

        await LogAndUpdateLockerBanksAndLockersWhereCardCredentialHasChangedAsync(tenantId, affectedLockerBanks, affectedLockers);

        return NoContent();
    }

    private async Task<(IReadOnlyList<LockerBankDTO>, IReadOnlyList<LockerDTO>)> GetLockerBanksAndLockersWhereCardCredentialIsAssignedAsync(Guid tenantId, Guid cardCredentialId, CancellationToken cancellationToken = default)
    {
        var affectedLockerBanks = await _cardCredentialService.GetLockerBanksWhereCardCredentialIsAssignedAsync(tenantId, cardCredentialId);
        var affectedLockers = await _cardCredentialService.GetLockersWhereCardCredentialIsAssignedAsync(tenantId, cardCredentialId);

        return (affectedLockerBanks, affectedLockers);
    }

    private async Task LogAndUpdateLockerBanksAndLockersWhereCardCredentialHasChangedAsync(Guid tenantId, IReadOnlyList<LockerBankDTO> affectedLockerBanks, IReadOnlyList<LockerDTO> affectedLockers, CancellationToken cancellationToken = default)
    {
        var userEmail = User.GetEmailFromClaim() ?? string.Empty;

        foreach (var lockerBank in affectedLockerBanks)
        {
            await _lockConfigAuditService.LogLockerBankConfigChangeAsync(tenantId, userEmail, lockerBank.Id, "Card credential changed or deleted");

            await _lockConfigService.UpdateLockConfigsByLockerBankAsync(tenantId, lockerBank.Id);
        }

        var lockersNotInAlreadyUpdatedBanks = affectedLockers.ExceptBy(affectedLockerBanks.Select(x => x.Id), x => x.LockerBankId);

        foreach (var locker in lockersNotInAlreadyUpdatedBanks)
        {
            await _lockConfigAuditService.LogLockerConfigChangeAsync(tenantId, userEmail, locker.Id, "Card credential changed or deleted");

            await _lockConfigService.UpdateLockConfigByLockerAsync(tenantId, locker.Id);
        }
    }
}
