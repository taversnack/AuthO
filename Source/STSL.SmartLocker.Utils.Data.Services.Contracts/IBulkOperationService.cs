using STSL.SmartLocker.Utils.DTO;

namespace STSL.SmartLocker.Utils.Data.Services.Contracts;

public interface IBulkOperationService
{
    Task<bool> CreateManyLockerAndLockPairsForLockerBankAsync(Guid tenantId, Guid lockerBankId, CreateBulkLockerAndLocksDTO dto, CancellationToken cancellationToken = default);
    Task<bool> CreateManyCardHolderAndCardCredentialPairsAsync(Guid tenantId, CreateBulkCardHolderAndCardCredentialsDTO dto, CancellationToken cancellationToken = default);

    Task<bool> CreateAndAssignNewCardHolderAndCardCredentialPairsToNewLockersAndLockPairs(Guid tenantId, Guid lockerBankId, CreateBulkLockerAndLockAndCardHolderAndCardCredentialsDTO dto, CancellationToken cancellationToken = default);
}
