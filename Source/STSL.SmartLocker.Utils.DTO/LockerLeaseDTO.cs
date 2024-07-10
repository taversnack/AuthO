using STSL.SmartLocker.Utils.Common.Data;

namespace STSL.SmartLocker.Utils.DTO;

public sealed record LockerLeaseDTO(
    Guid Id, 
    DateTimeOffset? StartedAt, 
    DateTimeOffset? EndedAt, 
    LockerBankBehaviour LockerBankBehaviour, 
    bool EndedByMasterCard,
    CardCredentialDTO? CardCredential,
    CardHolderDTO? CardHolder,
    LockerDTO? Locker,
    LockDTO? Lock);

public readonly record struct CreatePermanentLockerLeaseDTO(
    Guid CardHolderId, 
    Guid LockerId, 
    DateTimeOffset StartedAt, 
    Guid? CardCredentialId = null, 
    Guid? LockId = null);