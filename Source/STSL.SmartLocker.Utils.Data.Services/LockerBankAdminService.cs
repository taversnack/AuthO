using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using STSL.SmartLocker.Utils.Common.Exceptions;
using STSL.SmartLocker.Utils.Data.Contexts;
using STSL.SmartLocker.Utils.Data.Extensions;
using STSL.SmartLocker.Utils.Data.Services.Configuration;
using STSL.SmartLocker.Utils.Data.Services.Contracts;
using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.Data.Services.Helpers;
using STSL.SmartLocker.Utils.Data.Services.Mappings;
using STSL.SmartLocker.Utils.Data.SqlServer.Contexts;
using STSL.SmartLocker.Utils.Domain;
using STSL.SmartLocker.Utils.DTO;
using System.Data.Common;

namespace STSL.SmartLocker.Utils.Data.Services;

public sealed class LockerBankAdminService : ILockerBankAdminService
{
    private readonly SmartLockerDbContext _context;
    private readonly SmartLockerSqlServerDbContext _sqlContext;
    private readonly IValidator<LockerBankAdmin> _validator;
    private readonly IDatabaseExceptionHandler _exceptionHandler;
    private readonly GlobalServiceOptions _options;

    public LockerBankAdminService(
        SmartLockerDbContext context,
        SmartLockerSqlServerDbContext sqlContext,
        IValidator<LockerBankAdmin> validator,
        IDatabaseExceptionHandler exceptionHandler,
        IOptions<GlobalServiceOptions> options)
        => (_context, _sqlContext, _validator, _exceptionHandler, _options)
        = (context, sqlContext, validator, exceptionHandler, options.Value);

    public async Task<LockerBankAdminDTO?> CreateLockerBankAdminAsync(Guid tenantId, CreateLockerBankAdminDTO dto, CancellationToken cancellationToken = default)
    {
        var entity = CreateLockerBankAdminMapper.ToEntity(dto);

        entity.TenantId = tenantId;

        var validationResult = _validator.Validate(entity);

        if (validationResult.IsInvalid)
        {
            throw new BadRequestException("Locker bank admin invalid", validationResult.ErrorsDictionary);
        }

        try
        {
            var newEntity = _context.Add(entity);

            await _context.SaveChangesAsync(cancellationToken);

            return newEntity.Entity is null ? null : LockerBankAdminMapper.ToDTO(newEntity.Entity);
        }
        catch (DbException ex)
        {
            _exceptionHandler.HandleException(ex);
            return null;
        }
    }

    public async Task<bool> CreateManyLockerBankAdminsAsync(Guid tenantId, IEnumerable<CreateLockerBankAdminDTO> dtoList, CancellationToken cancellationToken = default)
    {
        if (!dtoList.Any())
        {
            return _options.EmptyBulkOperationIsError ? throw new BadRequestException("No data was passed") : true;
        }

        var entities = dtoList.Select(CreateLockerBankAdminMapper.ToEntity).ToList();

        foreach (var entity in entities)
        {
            entity.TenantId = tenantId;
        }

        ValidateManyLockerBankAdmins(entities);

        try
        {
            _context.AddRange(entities);

            return await _context.SaveChangesAsync(cancellationToken) == entities.Count;
        }
        catch (DbException ex)
        {
            _exceptionHandler.HandleException(ex);
            return false;
        }
    }

    public async Task ReplaceAdminsForLockerBankAsync(Guid tenantId, Guid lockerBankId, IReadOnlyList<Guid> cardHolderIds, CancellationToken cancellationToken = default)
    {
        var newAdmins = cardHolderIds.Select(x => new LockerBankAdmin {
            TenantId = tenantId,
            LockerBankId = lockerBankId,
            CardHolderId = x
        }).ToList();

        // No point validating currently.. entity is literally just 3 FKs
        //ValidateManyLockerBankAdmins(newAdmins);

        try
        {
            // NOTE: Maybe best to do replaces as a transaction so all changes are committed or reverted to previous state.
            //var transaction = _context.Database.BeginTransaction();

            var existingAdmins = await _context.LockerBankAdmins
                .ByTenant(tenantId)
                .Where(x => x.LockerBankId == lockerBankId)
                .ToListAsync(cancellationToken);

            _context.LockerBankAdmins.RemoveRange(existingAdmins);

            _context.AddRange(newAdmins);

            await _context.SaveChangesAsync(cancellationToken);

            //await transaction.CommitAsync();
        }
        catch (DbException ex)
        {
            _exceptionHandler.HandleException(ex);
        }
    }

    public async Task<IReadOnlyList<CardHolderDTO>> GetManyAdminsAsync(Guid tenantId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default)
        => await _context.LockerBankAdmins
                .ByTenant(tenantId)
                .Include(x => x.CardHolder)
                .Select(x => x.CardHolder)
                .OfType<CardHolder>()
                .FilterAndSort(filter, sort)
                .Select(x => CardHolderMapper.ToDTO(x))
                .ToListAsync(cancellationToken);

    public async Task RemoveAdminFromLockerBankAsync(Guid tenantId, Guid lockerBankId, Guid cardHolderId, CancellationToken cancellationToken = default)
    {
        var admin = await _context.LockerBankAdmins.FindAsync(new object[] { tenantId, lockerBankId, cardHolderId }, cancellationToken: cancellationToken);

        if (admin is null)
        {
            throw new NotFoundException("No admin found with matching id for locker bank");
        }

        try
        {
            _context.Remove(admin);

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbException ex)
        {
            _exceptionHandler.HandleException(ex);
        }
    }

    public async Task<IReadOnlyList<LocationDTO>> GetManyLocationsByAdminAsync(Guid tenantId, Guid adminId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default)
        => await _context.LockerBankAdmins
            .ByTenant(tenantId)
            .Where(x => x.CardHolderId == adminId)
            .Include(x => x.LockerBank)
            .ThenInclude(x => x!.Location)
            .Select(x => x.LockerBank!.Location)
            .OfType<Location>()
            .Distinct()
            .FilterAndSort(filter, sort)
            .Select(x => LocationMapper.ToDTO(x))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<LocationDTO>> GetManyLocationsByAdminAsync(Guid tenantId, string adminEmail, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default)
        => await _context.LockerBankAdmins
            .ByTenant(tenantId)
            .Include(x => x.CardHolder)
            .Where(x => x.CardHolder != null && x.CardHolder.Email == adminEmail)
            .Include(x => x.LockerBank)
            .ThenInclude(x => x!.Location)
            .Select(x => x.LockerBank!.Location)
            .OfType<Location>()
            .Distinct()
            .FilterAndSort(filter, sort)
            .Select(x => LocationMapper.ToDTO(x))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<LockerBankDTO>> GetManyLockerBanksByAdminAsync(Guid tenantId, Guid adminId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default)
        => await _context.LockerBankAdmins
            .ByTenant(tenantId)
            .Where(x => x.CardHolderId == adminId)
            .Include(x => x.LockerBank)
            .Select(x => x.LockerBank)
            .OfType<LockerBank>()
            .FilterAndSort(filter, sort)
            .Select(x => LockerBankMapper.ToDTO(x))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<LockerBankDTO>> GetManyLockerBanksByAdminAsync(Guid tenantId, string adminEmail, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default)
        => await _context.LockerBankAdmins
            .ByTenant(tenantId)
            .Include(x => x.CardHolder)
            .Where(x => x.CardHolder != null && x.CardHolder.Email == adminEmail)
            .Include(x => x.LockerBank)
            .Select(x => x.LockerBank)
            .OfType<LockerBank>()
            .FilterAndSort(filter, sort)
            .Select(x => LockerBankMapper.ToDTO(x))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<LockerBankDTO>> GetManyLockerBanksByLocationByAdminAsync(Guid tenantId, Guid locationId, Guid adminId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default)
        => await _context.LockerBankAdmins
            .ByTenant(tenantId)
            .Where(x => x.CardHolderId == adminId)
            .Include(x => x.LockerBank)
            .Select(x => x.LockerBank)
            .OfType<LockerBank>()
            .Where(x => x.LocationId == locationId)
            .FilterAndSort(filter, sort)
            .Select(x => LockerBankMapper.ToDTO(x))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<LockerBankDTO>> GetManyLockerBanksByLocationByAdminAsync(Guid tenantId, Guid locationId, string adminEmail, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default)
        => await _context.LockerBankAdmins
            .ByTenant(tenantId)
            .Include(x => x.CardHolder)
            .Where(x => x.CardHolder != null && x.CardHolder.Email == adminEmail)
            .Include(x => x.LockerBank)
            .Select(x => x.LockerBank)
            .OfType<LockerBank>()
            .Where(x => x.LocationId == locationId)
            .FilterAndSort(filter, sort)
            .Select(x => LockerBankMapper.ToDTO(x))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CardHolderDTO>> GetManyAdminsByLockerBankAsync(Guid tenantId, Guid lockerBankId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default)
        => await _context.LockerBankAdmins
            .ByTenant(tenantId)
            .Where(x => x.LockerBankId == lockerBankId)
            .Include(x => x.CardHolder)
            .Select(x => x.CardHolder)
            .OfType<CardHolder>()
            .FilterAndSort(filter, sort)
            .Select(x => CardHolderMapper.ToDTO(x))
            .ToListAsync(cancellationToken);

    public async Task<bool> IsCardHolderAdminForLockerBankAsync(Guid tenantId, Guid lockerBankId, Guid cardHolderId, CancellationToken cancellationToken = default)
        => await _context.LockerBankAdmins
            .ByTenant(tenantId)
            .FirstOrDefaultAsync(x => x.CardHolderId == cardHolderId && x.LockerBankId == lockerBankId, cancellationToken) is not null;

    public async Task<bool> IsCardHolderAdminForLockerBankAsync(Guid tenantId, Guid lockerBankId, string cardHolderEmail, CancellationToken cancellationToken = default)
        => await _context.LockerBankAdmins
            .ByTenant(tenantId)
            .Where(x => x.LockerBankId == lockerBankId)
            .Include(x => x.CardHolder)
            .FirstOrDefaultAsync(x => x.CardHolder != null && x.CardHolder.Email == cardHolderEmail, cancellationToken) is not null;

    public async Task<IReadOnlyList<LockerBankAdminSummaryDTO>> GetManyLockerBankSummariesAsync(Guid tenantId, string adminEmail, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default)
    {
        /*TODO: Refactor this */

        // Get locker banks for admin, & the location information for each locker bank
        var locationsAndBanks = await _context.LockerBankAdmins
            .ByTenant(tenantId)
            .Include(x => x.CardHolder)
            .Where(x => x.CardHolder != null && x.CardHolder.Email == adminEmail)
            .Include(x => x.LockerBank)
            .ThenInclude(x => x!.Location)
            .Select(x => new
            {
                LocationId = x.LockerBank!.LocationId,
                LocationName = x.LockerBank.Location!.Name,
                LocationDescription = x.LockerBank.Location.Description,
                LockerBankId = x.LockerBank.Id
            })
            .ToListAsync(cancellationToken);

        // Group locker banks by location
        var groupedByLocation = locationsAndBanks
            .GroupBy(x => new { x.LocationId, x.LocationName, x.LocationDescription })
            .Select(g => new
            {
                g.Key.LocationId,
                g.Key.LocationName,
                g.Key.LocationDescription,
                LockerBankIds = g.Select(x => x.LockerBankId).ToList()
            })
            .ToList();

        // Get a list of ids here for LINQ-SQL Translation
        var allLockerBankIds = groupedByLocation.SelectMany(g => g.LockerBankIds).Distinct().ToList();

        // Query the View with list of LockerBankIds
        var lockerBankSummaries = await _sqlContext.LockerBankSummaries_View
          .Where(summary => allLockerBankIds.Contains(summary.LockerBankId))
          .Select(x => LockerBankSummaryMapper.ToDTO(x))
          .ToListAsync(cancellationToken);

        // Final In-Memory Mapping
        var finalResults = groupedByLocation
            .Select(g => new LockerBankAdminSummaryDTO(
                LocationId: g.LocationId,
                LocationName: g.LocationName,
                LocationDescription: g.LocationDescription,
                LockerBankSummaries: lockerBankSummaries
                    .Where(summary => g.LockerBankIds.Contains(summary.Id))
                    .ToList()
                ))
        .ToList();

        return finalResults;
    }

    private void ValidateManyLockerBankAdmins(IReadOnlyList<LockerBankAdmin> entities)
    {
        var validationErrors = ValidationResult.CreateIndexedResults();
        foreach (var (entity, index) in entities.Select((entity, index) => (entity, index)))
        {
            var validationResult = _validator.Validate(entity);
            if (validationResult.IsInvalid)
            {
                if (_options.ThrowOnFirstValidationErrorForBulkOperations)
                {
                    throw new BadRequestException("Locker bank admin invalid", validationResult.ErrorsDictionary);
                }
                else
                {
                    validationErrors.Add(index, validationResult.ErrorsDictionary);
                }
            }
        }
        if (validationErrors.Any())
        {
            throw new BadRequestException("There is one or more invalid Locker bank admins", validationErrors);
        }
    }
}
