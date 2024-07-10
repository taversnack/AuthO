using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using STSL.SmartLocker.Utils.Api.Auth;
using STSL.SmartLocker.Utils.Common.Data;
using STSL.SmartLocker.Utils.Data.Services.Contracts;
using STSL.SmartLocker.Utils.Data.Services.Helpers;
using STSL.SmartLocker.Utils.DTO;

namespace STSL.SmartLocker.Utils.Api.Controllers.V1;

[ApiVersion(V1)]
public sealed class CardHoldersController : MultiTenantControllerBase
{
    private readonly ICardHolderService _cardHolderService;
    private readonly ICardCredentialService _cardCredentialService;
    private readonly ILockerBankAdminService _lockerBankAdminService;
    private readonly ILockerLeaseService _lockerLeaseService;
    private readonly ILockConfigService _lockConfigService;
    private readonly ILockConfigAuditService _lockConfigAuditService;
    private readonly ILogger<CardHoldersController> _logger;

    public CardHoldersController(
        ICardHolderService cardHolderService,
        ICardCredentialService cardCredentialService,
        ILockerBankAdminService lockerBankAdminService,
        ILockerLeaseService lockerLeaseService,
        ILockConfigService lockConfigService,
        ILockConfigAuditService lockConfigAuditService,
        ILogger<CardHoldersController> logger)
        => (_cardHolderService, _cardCredentialService, _lockerBankAdminService, _lockerLeaseService, _lockConfigService, _lockConfigAuditService, _logger)
        = (cardHolderService, cardCredentialService, lockerBankAdminService, lockerLeaseService, lockConfigService, lockConfigAuditService, logger);

    #region Card Holders

    /// <summary>
    /// Creates a new card holder
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    [HttpPost]
    [Authorize(Permissions.CreateCardHolders)]
    public async Task<ActionResult<CardHolderDTO>> Post(Guid tenantId, CreateCardHolderDTO dto)
    {
        var newCardHolder = await _cardHolderService.CreateCardHolderAsync(tenantId, dto);

        return newCardHolder is null ? StatusCode(StatusCodes.Status500InternalServerError) : CreatedAtAction(nameof(Get), new { tenantId, newCardHolder.Id }, newCardHolder);
    }

    /// <summary>
    /// Returns all card holders by page
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    [HttpGet]
    [Authorize(Permissions.ReadCardHolders)]
    public async Task<ActionResult<PagingResponse<CardHolderDTO>>> GetMany(Guid tenantId, [FromQuery] PagingRequest? page, [FromQuery] FilteredRequest? filter, [FromQuery] SortedRequest? sort)
    {
        page ??= new PagingRequest();

        var cardHolders = await _cardHolderService.GetManyCardHoldersAsync(tenantId, filter, sort);

        return page.ToResponse(cardHolders);
    }

    /// <summary>
    /// Returns an existing card holder or 404 (not found)
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="id">The card holder id</param>
    [HttpGet("{id}")]
    [Authorize(Permissions.ReadCardHolders)]
    public async Task<ActionResult<CardHolderDTO>> Get(Guid tenantId, Guid id)
    {
        var cardHolder = await _cardHolderService.GetCardHolderAsync(tenantId, id);

        return cardHolder is null ? NotFound() : cardHolder;
    }

    /// <summary>
    /// Fully updates a valid card holder or 400 with erroneous properties
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="id">The card holder id</param>
    [HttpPut("{id}")]
    [Authorize(Permissions.UpdateCardHolders)]
    public async Task<ActionResult> Put(Guid tenantId, Guid id, UpdateCardHolderDTO dto)
    {
        await _cardHolderService.UpdateCardHolderAsync(tenantId, id, dto);

        return NoContent();
    }

    /// <summary>
    /// Deletes a card holder if it exists, 404 otherwise
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="id">The card holder id</param>
    [HttpDelete("{id}")]
    [Authorize(Permissions.DeleteCardHolders)]
    public async Task<ActionResult> Delete(Guid tenantId, Guid id)
    {
        var (affectedLockerBanks, affectedLockers) = await GetLockerBanksAndLockersWhereCardHolderHasCardCredentialsAssignedAsync(tenantId, id);

        await _cardHolderService.DeleteCardHolderAsync(tenantId, id);

        await LogAndUpdateLockerBanksAndLockersWhereCardHolderHasChangedAsync(tenantId, affectedLockerBanks, affectedLockers);

        return NoContent();
    }

    private async Task<(IReadOnlyList<LockerBankDTO>, IReadOnlyList<LockerDTO>)> GetLockerBanksAndLockersWhereCardHolderHasCardCredentialsAssignedAsync(Guid tenantId, Guid cardHolderId, CancellationToken cancellationToken = default)
    {
        var affectedLockerBanks = await _cardCredentialService.GetLockerBanksWhereCardHolderHasCardCredentialsAssignedAsync(tenantId, cardHolderId);
        var affectedLockers = await _cardCredentialService.GetLockersWhereCardHolderHasCardCredentialsAssignedAsync(tenantId, cardHolderId);

        return (affectedLockerBanks, affectedLockers);
    }


    private async Task LogAndUpdateLockerBanksAndLockersWhereCardHolderHasChangedAsync(Guid tenantId, IReadOnlyList<LockerBankDTO> affectedLockerBanks, IReadOnlyList<LockerDTO> affectedLockers, CancellationToken cancellationToken = default)
    {
        var userEmail = User.GetEmailFromClaim() ?? string.Empty;

        foreach (var lockerBank in affectedLockerBanks)
        {
            await _lockConfigAuditService.LogLockerBankConfigChangeAsync(tenantId, userEmail, lockerBank.Id, "Card holder deleted");

            await _lockConfigService.UpdateLockConfigsByLockerBankAsync(tenantId, lockerBank.Id);
        }

        var lockersNotInAlreadyUpdatedBanks = affectedLockers.ExceptBy(affectedLockerBanks.Select(x => x.Id), x => x.LockerBankId);

        foreach (var locker in lockersNotInAlreadyUpdatedBanks)
        {
            await _lockConfigAuditService.LogLockerConfigChangeAsync(tenantId, userEmail, locker.Id, "Card holder deleted");

            await _lockConfigService.UpdateLockConfigByLockerAsync(tenantId, locker.Id);
        }
    }

    #endregion Card Holders

    #region Card Credentials

    /// <summary>
    /// Bulk create card credential for the card holder
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="id">The card holder id</param>
    [HttpPost("{id}/card-credentials")]
    [Authorize(Permissions.CreateCardCredentials)]
    public async Task<ActionResult> PostManyCardCredentials(Guid tenantId, Guid id, IEnumerable<CreateCardCredentialDTO> dtoList)
    {
        var createdAll = await _cardCredentialService.CreateManyCardCredentialsAsync(tenantId, dtoList.Select(x => x with { CardHolderId = id }).ToList());

        return createdAll ? CreatedAtAction(nameof(GetManyCardCredentials), new { tenantId, id }, null) : StatusCode(StatusCodes.Status500InternalServerError);
    }

    /// <summary>
    /// Returns card credentials that belong to the card holder
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="id">The card holder id</param>
    [HttpGet("{id}/card-credentials")]
    [Authorize(Permissions.ReadCardCredentials)]
    public async Task<ActionResult<PagingResponse<CardCredentialDTO>>> GetManyCardCredentials(Guid tenantId, Guid id, [FromQuery] PagingRequest? page, [FromQuery] FilteredRequest? filter, [FromQuery] SortedRequest? sort, [FromQuery] CardType? cardType = null)
    {
        page ??= new PagingRequest();

        var cardCredentials = await _cardCredentialService.GetManyCardCredentialsByCardHolderAsync(tenantId, id, filter, sort, cardType);

        return page.ToResponse(cardCredentials);
    }

    /// <summary>
    /// Returns a card credential for a card holder by C-Cure lookup or 404 (not found)
    /// <param name="tenantId">The tenant id</param>
    /// <param name="id">The card holder id</param>
    /// </summary>
    [HttpGet("card-credentials-ccure/{id}")]
    [Authorize(Permissions.ReadCardCredentials)]
    public async Task<ActionResult<CardCredentialDTO>> GetCardCredentialFromCCure(Guid tenantId, Guid id)
    {
        var cardHolder = await _cardHolderService.GetCardHolderAsync(tenantId, id);

        if (cardHolder is null || cardHolder.UniqueIdentifier is null)
        {
           return NotFound();
        }

        // Do a lookup to C-Cure, if a card credential is found, save to the database and return for locker assignment
        var cardCredential = await _cardCredentialService.GetCardCredentialFromCCureAsync(tenantId, cardHolder.Id, cardHolder.UniqueIdentifier);

        return cardCredential is null ? NotFound() : cardCredential;
    }
    #endregion Card Credentials

    #region With Card Credentials

    /// <summary>
    /// Returns user card credentials for every card holder
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    [HttpGet("with-user-card-credentials")]
    [Authorize(Permissions.ReadCardCredentials)]
    public async Task<ActionResult<PagingResponse<CardHolderAndCardCredentialsDTO>>> GetManyCardHoldersWithUserCardCredentials(Guid tenantId, [FromQuery] PagingRequest? page, [FromQuery] FilteredRequest? filter, [FromQuery] SortedRequest? sort)
    {
        page ??= new PagingRequest();

        var cardHoldersAndTheirUserCardCredentials = await _cardHolderService.GetManyCardHoldersAndTheirUserCardCredentialsAsync(tenantId, filter, sort);

        return page.ToResponse(cardHoldersAndTheirUserCardCredentials);
    }

    /// <summary>
    /// Returns special card credentials for every card holder
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    [HttpGet("with-special-card-credentials")]
    [Authorize(Permissions.ReadCardCredentials)]
    public async Task<ActionResult<PagingResponse<CardHolderAndCardCredentialsDTO>>> GetManyCardHoldersWithSpecialCardCredentials(Guid tenantId, [FromQuery] PagingRequest? page, [FromQuery] FilteredRequest? filter, [FromQuery] SortedRequest? sort)
    {
        page ??= new PagingRequest();

        var cardHoldersAndTheirUserCardCredentials = await _cardHolderService.GetManyCardHoldersAndTheirSpecialCardCredentialsAsync(tenantId, filter, sort);

        return page.ToResponse(cardHoldersAndTheirUserCardCredentials);
    }

    #endregion With Card Credentials

    #region Locker Leases

    /// <summary>
    /// Returns all locker lease history by page for this card holder
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="id">The card holder id</param>
    [HttpGet("{id}/locker-leases")]
    //[Authorize(Permissions.ReadLockerLeases)]
    public async Task<ActionResult<PagingResponse<LockerLeaseDTO>>> GetLockerLeaseHistory(Guid tenantId, Guid id, [FromQuery] PagingRequest? page, [FromQuery] FilteredRequest? filter, [FromQuery] SortedRequest? sort)
    {
        page ??= new PagingRequest();

        var lockers = await _lockerLeaseService.GetLockerLeaseHistoryByCardHolderAsync(tenantId, id, filter, sort);

        return page.ToResponse(lockers);
    }

    #endregion Locker Leases
}
