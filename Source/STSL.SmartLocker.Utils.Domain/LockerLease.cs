using STSL.SmartLocker.Utils.Common.Data;

namespace STSL.SmartLocker.Utils.Domain;

public sealed class LockerLease : EntityBaseWithTenant, IUsesGuidId
{
    public Guid Id { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    public LockerBankBehaviour LockerBankBehaviour { get; set; }
    public bool EndedByMasterCard { get; set; } = false;
    
    public Guid? CardCredentialId { get; set; }
    public Guid? CardHolderId { get; set; }
    public Guid? LockerId { get; set; }
    public Guid? LockId { get; set; }

    // navigation
    public CardCredential? CardCredential { get; set; }
    public CardHolder? CardHolder { get; set; }
    public Locker? Locker { get; set; }
    public Lock? Lock { get; set; }
}
