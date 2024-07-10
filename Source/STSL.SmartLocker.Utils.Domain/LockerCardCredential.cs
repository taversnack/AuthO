namespace STSL.SmartLocker.Utils.Domain;

public sealed class LockerCardCredential : EntityBaseWithTenant
{
    public Guid LockerId { get; set; }
    public Guid CardCredentialId { get; set; }

    // navigation
    public Locker? Locker { get; set; }
    public CardCredential? CardCredential { get; set; }
}
