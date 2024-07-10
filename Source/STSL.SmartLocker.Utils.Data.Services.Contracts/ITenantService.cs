using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.DTO;

namespace STSL.SmartLocker.Utils.Data.Services.Contracts;

public interface ITenantService
{
    Task<TenantDTO?> CreateTenantAsync(CreateTenantDTO dto, CancellationToken cancellationToken = default);
    Task<TenantDTO?> GetTenantAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TenantDTO>> GetManyTenantsAsync(IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default);
    Task UpdateTenantAsync(Guid id, UpdateTenantDTO dto, CancellationToken cancellationToken = default);
    Task DeleteTenantAsync(Guid id, CancellationToken cancellationToken = default);

}
