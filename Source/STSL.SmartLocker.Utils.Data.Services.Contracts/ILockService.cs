using STSL.SmartLocker.Utils.Common.Data;
using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.DTO;

namespace STSL.SmartLocker.Utils.Data.Services.Contracts;

public interface ILockService
{
    Task<LockDTO?> CreateLockAsync(Guid tenantId, CreateLockDTO dto, CancellationToken cancellationToken = default);
    Task<bool> CreateManyLocksAsync(Guid tenantId, IEnumerable<CreateLockDTO> dtoList, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LockDTO>> CreateAndUseManyLocksAsync(Guid tenantId, IEnumerable<CreateLockDTO> dtoList, CancellationToken cancellationToken = default);

    Task<LockDTO?> GetLockAsync(Guid tenantId, Guid lockId, CancellationToken cancellationToken = default);
    Task<LockDTO?> GetLockByLockerIdAsync(Guid tenantId, Guid lockerId, CancellationToken cancellationToken = default);
    Task<LockDTO?> GetLockBySerialNumberAsync(Guid tenantId, LockSerial lockSerial, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LockDTO>> GetManyLocksAsync(Guid tenantId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default);

    Task UpdateLockAsync(Guid tenantId, Guid lockId, UpdateLockDTO dto, CancellationToken cancellationToken = default);

    Task DeleteLockAsync(Guid tenantId, Guid lockId, CancellationToken cancellationToken = default);
}
