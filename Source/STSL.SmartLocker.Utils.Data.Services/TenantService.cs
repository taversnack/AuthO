using Microsoft.EntityFrameworkCore;
using STSL.SmartLocker.Utils.Common.Data;
using STSL.SmartLocker.Utils.Common.Exceptions;
using STSL.SmartLocker.Utils.Common.Helpers;
using STSL.SmartLocker.Utils.Data.Contexts;
using STSL.SmartLocker.Utils.Data.Services.Contracts;
using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.Data.Services.Mappings;
using STSL.SmartLocker.Utils.Domain;
using STSL.SmartLocker.Utils.DTO;
using System.Data.Common;

namespace STSL.SmartLocker.Utils.Data.Services;

public class TenantService : ITenantService
{
    private readonly SmartLockerDbContext _dbContext;
    private readonly IDatabaseExceptionHandler _exceptionHandler;

    public TenantService(SmartLockerDbContext dbContext, IDatabaseExceptionHandler exceptionHandler)
        => (_dbContext, _exceptionHandler) = (dbContext, exceptionHandler);

    public async Task<TenantDTO?> CreateTenantAsync(CreateTenantDTO dto, CancellationToken cancellationToken = default)
    {
        try
        {
            // If no default MIME Type is configured
            if (dto.Logo is not null && string.IsNullOrWhiteSpace(dto.LogoMimeType))
            {
                throw new BadRequestException("No MIME Type provided with logo");
            }

            var trackedEntity = _dbContext.Add(CreateTenantMapper.ToEntity(dto));

            await _dbContext.SaveChangesAsync(cancellationToken);

            return TenantMapper.ToDTO(trackedEntity.Entity);
        }
        catch (Exception ex)
        {
            HandleExceptions(ex);
            return null;
        }
    }

    public async Task<TenantDTO?> GetTenantAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tenant = await _dbContext.Tenants.FindAsync(new object[] { id }, cancellationToken: cancellationToken);

        return tenant is null ? null : TenantMapper.ToDTO(tenant);
    }

    public async Task<IReadOnlyList<TenantDTO>> GetManyTenantsAsync(IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default)
    {
        IQueryable<Tenant> all = _dbContext.Tenants;

        const string Name = "name";
        const string CardHolderAliasSingular = "cardholderaliassingular";
        const string CardHolderAliasPlural = "cardholderaliasplural";
        // Add HelpPortalUrl?

        if (filter is not null && !string.IsNullOrWhiteSpace(filter.FilterValue))
        {
            var filterName = filter.FilterProperties.IsNullOrEmpty() || filter.FilterProperties.Contains(Name, StringComparer.OrdinalIgnoreCase);
            var filterCardHolderAliasSingular = filter.FilterProperties.IsNullOrEmpty() || filter.FilterProperties.Contains(CardHolderAliasSingular, StringComparer.OrdinalIgnoreCase);
            var filterCardHolderAliasPlural = filter.FilterProperties.IsNullOrEmpty() || filter.FilterProperties.Contains(CardHolderAliasPlural, StringComparer.OrdinalIgnoreCase);

            all = all.Where(x =>
                (filterName && x.Name.Contains(filter.FilterValue)) ||
                (filterCardHolderAliasSingular && x.CardHolderAliasSingular.Contains(filter.FilterValue))
            );
        }

        if (sort is not null && !string.IsNullOrWhiteSpace(sort.SortBy))
        {
            all = sort.SortBy.ToLowerInvariant() switch
            {
                Name => sort.SortOrder == SortOrder.Ascending ? all.OrderBy(x => x.Name) : all.OrderByDescending(x => x.Name),
                CardHolderAliasSingular => sort.SortOrder == SortOrder.Ascending ? all.OrderBy(x => x.CardHolderAliasSingular) : all.OrderByDescending(x => x.CardHolderAliasSingular),
                CardHolderAliasPlural => sort.SortOrder == SortOrder.Ascending ? all.OrderBy(x => x.CardHolderAliasPlural) : all.OrderByDescending(x => x.CardHolderAliasPlural),
                _ => throw new BadRequestException($"Cannot sort on property {sort.SortBy}, no such property exists")
            };
        }

        return await all.Select(x => TenantMapper.ToDTO(x)).ToListAsync(cancellationToken);
    }

    public async Task UpdateTenantAsync(Guid id, UpdateTenantDTO dto, CancellationToken cancellationToken = default)
    {
        // If no default MIME Type is configured
        if (dto.Logo is not null && string.IsNullOrWhiteSpace(dto.LogoMimeType))
        {
            throw new BadRequestException("No MIME Type provided with logo");
        }

        var entity = UpdateTenantMapper.ToEntity(dto);

        var existing = await _dbContext.Tenants.FindAsync(new object[] { id }, cancellationToken: cancellationToken) ?? throw new NotFoundException(id);

        entity.TenantId = id;

        try
        {
            _dbContext.Entry(existing).CurrentValues.SetValues(entity);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            HandleExceptions(ex);
        }
    }

    public async Task DeleteTenantAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tenant = await _dbContext.Tenants.FindAsync(new object[] { id }, cancellationToken: cancellationToken) ?? throw new NotFoundException(id);

        try
        {
            // TODO: [1] Pass in global service options for throw if not found

            _dbContext.Tenants.Remove(tenant);

            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            HandleExceptions(ex);
        }
    }

    private void HandleExceptions(Exception ex)
    {
        if (ex is DbUpdateConcurrencyException)
        {
            throw new ConflictException("The entity you are trying to update has been updated by someone else, please recheck the current value and try again if necessary");
        }
        else if (ex is DbUpdateException updateException && updateException.InnerException is DbException innerException)
        {
            _exceptionHandler.HandleException(innerException);
        }
        else
        {
            throw ex;
        }
    }
}
