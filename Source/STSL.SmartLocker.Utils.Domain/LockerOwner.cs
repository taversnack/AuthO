namespace STSL.SmartLocker.Utils.Domain;

public sealed class LockerOwner : EntityBaseWithTenant
{
    public Guid LockerId { get; set; }
    public Guid CardHolderId { get; set; }

    // navigation
    public Locker? Locker { get; set; }
    public CardHolder? CardHolder { get; set; }
}