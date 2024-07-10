using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using STSL.SmartLocker.Utils.Common.Exceptions;
using STSL.SmartLocker.Utils.Data.Services.Configuration;
using STSL.SmartLocker.Utils.Data.Services.Contracts;
using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.Data.Services.Helpers;
using STSL.SmartLocker.Utils.Data.Services.Mappings;
using STSL.SmartLocker.Utils.Domain;
using STSL.SmartLocker.Utils.DTO;

namespace STSL.SmartLocker.Utils.Data.Services;

public class LocationService : ILocationService
{
    private readonly IRepository<Location> _repository;
    private readonly IValidator<Location> _validator;
    private readonly GlobalServiceOptions _options;

    public LocationService(IRepository<Location> repository, IValidator<Location> validator, IOptions<GlobalServiceOptions> options)
        => (_repository, _validator, _options) = (repository, validator, options.Value);

    public async Task<LocationDTO?> CreateLocationAsync(Guid tenantId, CreateLocationDTO dto, CancellationToken cancellationToken = default)
    {
        var entity = CreateLocationMapper.ToEntity(dto);

        _repository.ValidateOne(entity, _validator);

        var newEntity = await _repository.CreateOneAsync(tenantId, entity, cancellationToken);

        return newEntity is null ? null : LocationMapper.ToDTO(newEntity);
    }

    public async Task<LocationDTO?> GetLocationAsync(Guid tenantId, Guid locationId, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetOneAsync(tenantId, locationId, cancellationToken);

        return entity is null ? null : LocationMapper.ToDTO(entity);
    }

    public async Task<IReadOnlyList<LocationDTO>> GetManyLocationsAsync(Guid tenantId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default)
        => await _repository
            .QueryingAll(tenantId)
            .FilterAndSort(filter, sort)
            .Select(x => LocationMapper.ToDTO(x))
            .ToListAsync(cancellationToken);

    public async Task UpdateLocationAsync(Guid tenantId, Guid locationId, UpdateLocationDTO dto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetOneAsync(tenantId, locationId, cancellationToken) ?? throw new NotFoundException(locationId);

        var updatedEntity = UpdateLocationMapper.ToEntity(dto, entity);

        _repository.ValidateOne(updatedEntity, _validator);

        await _repository.UpdateOneAsync(tenantId, locationId, entity, cancellationToken);
    }

    public async Task DeleteLocationAsync(Guid tenantId, Guid locationId, CancellationToken cancellationToken = default)
        => await _repository.DeleteOneAsync(tenantId, locationId, _options.ThrowNotFoundWhenDeletingNonExistantEntity, cancellationToken);
}
