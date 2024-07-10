namespace STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;

public interface IReferenceImageRepository<T, E>
{
    Task<E?> CreateReferenceImageAsync(Guid tenantId, Guid entityId, E image, CancellationToken cancellationToken);
    Task<E?> GetReferenceImageAsync(Guid tenantId, Guid referenceImageId, CancellationToken cancellationToken);
    Task<E?> GetReferenceImageByOwnerIdAsync(Guid tenantId, Guid entityId, CancellationToken cancellationToken);
    Task<T> UpdateCurrentReferenceImageAsync(T entity);
    Task DeleteReferenceImageAsync(Guid tenantId, Guid referenceImageId, CancellationToken cancellationToken);
}