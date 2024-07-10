namespace STSL.SmartLocker.Utils.Data.Services.Contracts;

public interface ILockConfigService
{
    Task UpdateLockConfigAsync(Guid tenantId, Guid lockId, CancellationToken cancellationToken = default);
    Task UpdateLockConfigByLockerAsync(Guid tenantId, Guid lockerId, CancellationToken cancellationToken = default);
    Task UpdateLockConfigsByLockerBankAsync(Guid tenantId, Guid lockerBankId, CancellationToken cancellationToken = default);

    Task ResetAllLockConfigsToFactorySettingsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task RefreshAllLockConfigsAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<bool> IsLockUpdatePendingAsync(Guid tenantId, Guid lockId, CancellationToken cancellationToken = default);
    Task<bool> IsLockUpdatePendingByLockerAsync(Guid tenantId, Guid lockerId, CancellationToken cancellationToken = default);

    //Task<LockConfigDTO?> GetLockConfigPendingPropertiesAsync(Guid tenantId, Guid lockId, CancellationToken token = default);
    //Task<LockConfigDTO?> GetLockConfigPendingPropertiesByLockerAsync(Guid tenantId, Guid lockerId, CancellationToken token = default);
}
