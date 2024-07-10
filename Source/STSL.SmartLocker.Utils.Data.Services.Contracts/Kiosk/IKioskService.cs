using STSL.SmartLocker.Utils.DTO;
using STSL.SmartLocker.Utils.DTO.Kiosk;

namespace STSL.SmartLocker.Utils.Kiosk.Printer.Contracts
{
    public interface IKioskService
    {
        Task<bool> AuthenticateServiceWorkerAsync(CancellationToken cancellationToken = default);
        Task CreateRecoveryCodeRequestAsync(Guid tenantId, CardHolderDTO cardHolder, CancellationToken cancellationToken = default);
        Task<bool> PublishCreateTemporaryCardToAzureServiceBusAsync(Guid tenantId, CreateCardCredentialDTO temporaryCard, CancellationToken cancellation = default);
        Task<bool> PublishReturnTemporaryCardToAzureServiceBusAsync(Guid tenantId, UpdateCardCredentialDTO temporaryCard, CancellationToken cancellationToken = default);
        Task<KioskAccessCodeDTO?> SubmitAccessCodeAsync(Guid tenantId, AccessCodeDTO accessCode, CancellationToken cancellationToken = default);
        Task<KioskAccessCodeDTO?> GetAccessCodeByCardHolderIdAsync(Guid tenantId, Guid cardHolderId, CancellationToken cancellationToken = default);
        Task UpdateLockerWithCredentialsAndCreateLeaseAsync(Guid tenantId, Guid lockerId, Guid cardCredentialId, Guid cardHolder, CancellationToken cancellationToken = default);
        Task CreateKioskLockerAssignmentAsync(Guid tenantId, Guid lockerId, Guid temporaryCardCredentialId, Guid cardHolderId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<KioskLockerAssignentDTO>> GetActiveAssignmentsForTemporaryCardOwnerAsync(Guid tenantId, Guid cardHolderId, Guid temporaryCardCredentialId, CancellationToken cancellationToken = default);
        Task EndActiveAssignmentForTemporaryCardAsync(Guid tenantId, Guid kioskAssignentId, CancellationToken cancellationToken = default);
        Task InitializeKioskAsync(Guid tenantId, Guid kioskId, string kioskName, Guid locationId);
    }
}
