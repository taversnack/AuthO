using STSL.SmartLocker.Utils.Common.Data;

namespace STSL.SmartLocker.Utils.Domain;

// TODO: Does it make sense to have LockerLease as a separate domain data object?

public sealed class Locker : EntityBaseWithTenant, IUsesGuidId
{
    public Guid Id { get; set; }
    public required string Label { get; set; }
    public string? ServiceTag { get; set; }
    /// <summary>
    /// An absolute time the scheduler uses the Location TimeZone to calculate expiry.
    /// </summary>
    public DateTimeOffset? AbsoluteLeaseExpiry { get; set; }

    public Guid LockerBankId { get; set; }
    public Guid? CurrentLeaseId { get; set; }
    public LockerSecurityType SecurityType { get; set; } = LockerSecurityType.SmartLock;

    // navigation
    public Lock? Lock { get; set; }
    public LockerBank? LockerBank { get; set; }
    public LockerLease? CurrentLease { get; set; }

    /// <summary>
    /// A list of the permanent owners assigned to this locker
    /// </summary>
    public List<LockerOwner>? PermanentOwners { get; set; }
    /// <summary>
    /// For permanent behaviour locker banks; this is the user card credentials assigned to a single locker
    /// and does not include locker bank level; master, security sweep or tag cards.
    /// For temporary behaviour locker banks; this list should always be empty as all cards are set at a bank level.
    /// </summary>
    public List<LockerCardCredential>? CardCredentials { get; set; }
    public List<LockerLease>? LeaseHistory { get; set; }
}
