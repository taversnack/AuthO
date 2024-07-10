using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.DTO;

namespace STSL.SmartLocker.Utils.Data.Services.Contracts;

public interface ILocationService
{
    Task<LocationDTO?> CreateLocationAsync(Guid tenantId, CreateLocationDTO dto, CancellationToken cancellationToken = default);

    Task<LocationDTO?> GetLocationAsync(Guid tenantId, Guid locationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LocationDTO>> GetManyLocationsAsync(Guid tenantId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default);

    Task UpdateLocationAsync(Guid tenantId, Guid locationId, UpdateLocationDTO dto, CancellationToken cancellationToken = default);

    Task DeleteLocationAsync(Guid tenantId, Guid locationId, CancellationToken cancellationToken = default);
}
