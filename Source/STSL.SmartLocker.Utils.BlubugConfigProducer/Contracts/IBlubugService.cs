using STSL.SmartLocker.Utils.BlubugConfigProducer.DTO;
using STSL.SmartLocker.Utils.Common.Data;

namespace STSL.SmartLocker.Utils.BlubugConfigProducer.Contracts;

public interface IBlubugService
{
    Task<bool> UpdateLockConfigAsync(LockSerial lockSerial, UpdateLockConfigDTO lockConfigDTO, CancellationToken cancellationToken = default);
    Task<LockConfigDTO?> GetLockConfigAsync(LockSerial lockSerial, CancellationToken cancellationToken = default);
}