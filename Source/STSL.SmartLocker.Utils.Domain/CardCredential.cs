using STSL.SmartLocker.Utils.Common.Data;

namespace STSL.SmartLocker.Utils.Domain;

public sealed class CardCredential : EntityBaseWithTenant, IUsesGuidId
{
    public Guid Id { get; set; }
    public string? SerialNumber { get; set; }
    // Make HidNumber a uint?
    public required string HidNumber { get; set; }
    public CardType CardType { get; init; }
    public string? CardLabel { get; set; }

    // TODO: Really need to map the logic of what happens to user credentials when deleted.
    // Presumably also need to remove them from locks? If so, probably need to SetNull onDelete of CardHolder
    public Guid? CardHolderId { get; set; }

    // navigation
    public CardHolder? CardHolder { get; set; }
    public List<LockerCardCredential>? LockerCardCredentials { get; set; }
    //public List<Locker> CurrentlyLeasedLockers { get; } = new List<Locker>();
    //public List<LockerLease> LockerLeaseHistory { get; } = new List<LockerLease>();
}
