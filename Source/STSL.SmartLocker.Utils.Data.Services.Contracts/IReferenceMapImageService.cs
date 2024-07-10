using STSL.SmartLocker.Utils.DTO;
using System.Collections.Generic;

namespace STSL.SmartLocker.Utils.Data.Services.Contracts;

public interface IReferenceImageService<T, E>
{
    Task<ReferenceImageDTO?> CreateReferenceImageAsync(Guid tenantId, CreateReferenceImageDTO dto, string userEmail, CancellationToken cancellationToken = default);
    Task<ReferenceImageDTO?> GetReferenceImageAsync(Guid tenantId, Guid referenceImageId, CancellationToken cancellationToken = default);
    Task<ReferenceImageDTO?> GetReferenceImageByOwnerIdAsync(Guid tenantId, Guid entityId, CancellationToken cancellationToken = default);
    Task UpdateReferenceImageAsync(Guid tenantId, Guid referenceImageId, UpdateReferenceImageDTO dto, CancellationToken cancellationToken = default);
    Task DeleteReferenceImageAsync(Guid tenantId, Guid referenceImageId, CancellationToken cancellationToken = default);
}
